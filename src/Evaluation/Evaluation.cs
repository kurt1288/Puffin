using System.Runtime.CompilerServices;
using static Puffin.Constants;
using static Puffin.Attacks.Attacks;

namespace Puffin.Evaluation
{
   internal readonly struct EvalInfo(ulong[] mobilitySquares, ulong[] kingZones)
   {
      internal readonly ulong[] MobilitySquares = mobilitySquares;
      internal readonly ulong[] KingZones = kingZones;
      internal readonly int[] KingAttacksCount = [0, 0];
      internal readonly Score[] KingAttacksWeight = [new(), new()];
   }

   internal static class Evaluation
   {
      public static int Evaluate(Board board)
      {
         EvalInfo info = InitEval(board);
         
         // Material and PST score
         Score score = board.MaterialScore[(int)Color.White] - board.MaterialScore[(int)Color.Black];
         
         // Piece evaluation
         score += EvaluatePieces(board, info);

         if (board.SideToMove == Color.Black)
         {
            score *= -1;
         }

         return (score.Mg * board.Phase + score.Eg * (24 - board.Phase)) / 24;
      }

      private static EvalInfo InitEval(Board board)
      {
         // Mobility squares: All squares not attacked by enemy pawns minus own blocked pawns.
         Bitboard occ = board.ColorBoard(Color.Both);
         Bitboard blackPawns = board.ColorPieceBB(Color.Black, PieceType.Pawn);
         Bitboard whitePawns = board.ColorPieceBB(Color.White, PieceType.Pawn);

         EvalInfo info = new(
            [
               ~PawnAnyAttacks(blackPawns.Value, Color.Black) ^ (occ.Shift(Direction.Up) & whitePawns).Value,
               ~PawnAnyAttacks(whitePawns.Value, Color.White) ^ (occ.Shift(Direction.Down) & blackPawns).Value,
            ],
            [
               KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)],
               KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)],
            ]
         );

         return info;
      }

      private static Score EvaluatePieces(Board board, EvalInfo info)
      {
         Score score = new();

         if (board.ColorPieceBB(Color.White, PieceType.Bishop).CountBits() >= 2)
         {
            score += EvalTerms.BishopPair;
         }
         if (board.ColorPieceBB(Color.Black, PieceType.Bishop).CountBits() >= 2)
         {
            score -= EvalTerms.BishopPair;
         }

         score += EvalPawns(board, Color.White) - EvalPawns(board, Color.Black);
         score += EvalKnights(board, info, Color.White) - EvalKnights(board, info, Color.Black);
         score += EvalBishops(board, info, Color.White) - EvalBishops(board, info, Color.Black);
         score += EvalRooks(board, info, Color.White) - EvalRooks(board, info, Color.Black);
         score += EvalQueens(board, info, Color.White) - EvalQueens(board, info, Color.Black);
         score += EvalKings(board, info, Color.White) - EvalKings(board, info, Color.Black);

         return score;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static int GetPieceValue(PieceType piece, Board board)
      {
         Score value = EvalTerms.PieceValues[(int)(piece == PieceType.Null ? PieceType.Pawn : piece)];
         return (value.Mg * board.Phase + value.Eg * (24 - board.Phase)) / 24;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Score GetPSTScore(Piece piece, int square)
      {
         if (piece.Color == Color.Black)
         {
            square ^= 56;
         }

         return EvalTerms.PST[(int)piece.Type * 64 + square];
      }

      // Used for debugging and verification of lazy eval during board updates
      public static Score Material(Board board, Color color)
      {
         Bitboard us = board.ColorBoard(color);
         Score score = new();
         while (us)
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Squares[square];
            score += EvalTerms.PieceValues[(int)piece.Type];
            score += GetPSTScore(piece, square);
         }
         return score;
      }

      private static Score EvalPawns(Board board, Color color)
      {
         Score score = new();
         Bitboard pawns = board.ColorPieceBB(color, PieceType.Pawn);

         score += EvalTerms.DefendedPawn[(pawns & PawnAnyAttacks(pawns.Value, color)).CountBits()];
         score += EvalTerms.ConnectedPawn[(pawns & pawns.RightShift()).CountBits()];

         // Enemy non-pawn pieces that can be attacked with a pawn push
         ulong pawnShift = (pawns.Shift(color == Color.White ? Direction.Down : Direction.Up) & ~board.ColorBoard(Color.Both).Value).Value;
         ulong enemyPieces = (board.ColorBoard(color ^ (Color)1) ^ board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn)).Value;
         score += EvalTerms.PawnPushThreats * new Bitboard(PawnAnyAttacks(pawnShift, color) & enemyPieces).CountBits();

         // Enemy non-pawn pieces that are attacked
         score += EvalTerms.PawnAttacks * new Bitboard(PawnAnyAttacks(pawns.Value, color) & enemyPieces).CountBits();

         while (pawns)
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();
            int rank = (color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1;

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value) == 0)
            {
               score += EvalTerms.PassedPawn[rank];

               if (rank < 4)
               {
                  continue;
               }

               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)] * EvalTerms.FriendlyKingPawnDistance;
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)] * EvalTerms.EnemyKingPawnDistance;

               // Free to advance (no enemy non-pawn pieces ahead)
               if ((ForwardMask[(int)color][square] & board.ColorBoard(color ^ (Color)1).Value) == 0)
               {
                  score += EvalTerms.FreeAdvancePawn;
               }
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & board.ColorPieceBB(color, PieceType.Pawn).Value) == 0)
            {
               // Penalty is based on file
               score -= EvalTerms.IsolatedPawn[square & 7];
            }
         }

         return score;
      }

      private static Score EvalKnights(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard knightsBB = board.ColorPieceBB(color, PieceType.Knight);

         while (knightsBB)
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += EvalTerms.KnightMobility[new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits()];

            if ((KnightAttacks[square] & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalBishops(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard bishopBB = board.ColorPieceBB(color, PieceType.Bishop);

         while (bishopBB)
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            ulong moves = GetBishopAttacks(square, board.ColorBoard(Color.Both).Value);
            score += EvalTerms.BishopMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            // Pawns on same color squares
            bool isLSqBishop = ((WHITE_SQUARES >> square) & 1) == 1;
            Bitboard samePawns = board.ColorPieceBB(color, PieceType.Pawn);
            samePawns &= isLSqBishop ? WHITE_SQUARES : DARK_SQUARES;

            score -= EvalTerms.SameColorBishopPawns[samePawns.CountBits()];

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalRooks(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard rookBB = board.ColorPieceBB(color, PieceType.Rook);

         while (rookBB)
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            ulong moves = GetRookAttacks(square, board.ColorBoard(Color.Both).Value);
            score += EvalTerms.RookMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((FILE_MASKS[square & 7] & board.ColorPieceBB(color, PieceType.Pawn).Value) == 0)
            {
               if ((FILE_MASKS[square & 7] & board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value) == 0)
               {
                  score += EvalTerms.RookOpenFile;
               }
               else
               {
                  score += EvalTerms.RookHalfOpenFile;
               }
            }

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalQueens(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard queenBB = board.ColorPieceBB(color, PieceType.Queen);

         while (queenBB)
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            ulong moves = GetQueenAttacks(square, board.ColorBoard(Color.Both).Value);
            score += EvalTerms.QueenMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalKings(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard kingBB = board.ColorPieceBB(color, PieceType.King);

         while (kingBB)
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & SquareBB[kingSq]) != 0)
            {
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x7070000000000 : 0xe0e00000000000) : (ulong)(kingSq % 8 < 3 ? 0x70700 : 0xe0e000);

               Bitboard pawns = board.ColorPieceBB(color, PieceType.Pawn) & pawnSquares;
               score += EvalTerms.PawnShield[Math.Min(pawns.CountBits(), 3)];

               if ((board.ColorPieceBB(color, PieceType.Pawn).Value & FILE_MASKS[kingSq & 7]) == 0)
               {
                  score -= (board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value & FILE_MASKS[kingSq & 7]) == 0
                     ? EvalTerms.KingOpenFile
                     : EvalTerms.KingHalfOpenFile;
               }
            }

            if (info.KingAttacksCount[(int)color ^ 1] >= 2)
            {
               score -= info.KingAttacksWeight[(int)color ^ 1];
            }
         }

         return score;
      }
   }
}
