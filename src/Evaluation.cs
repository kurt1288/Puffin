using System.Runtime.CompilerServices;
using static Puffin.Constants;
using static Puffin.Attacks.Attacks;

namespace Puffin
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
            score += BishopPair;
         }
         if (board.ColorPieceBB(Color.Black, PieceType.Bishop).CountBits() >= 2)
         {
            score -= BishopPair;
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
         Score value = PieceValues[(int)(piece == PieceType.Null ? PieceType.Pawn : piece)];
         return (value.Mg * board.Phase + value.Eg * (24 - board.Phase)) / 24;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Score GetPSTScore(Piece piece, int square)
      {
         if (piece.Color == Color.Black)
         {
            square ^= 56;
         }

         return PST[(int)piece.Type * 64 + square];
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
            score += PieceValues[(int)piece.Type];
            score += GetPSTScore(piece, square);
         }
         return score;
      }

      private static Score EvalPawns(Board board, Color color)
      {
         Score score = new();
         Bitboard pawns = board.ColorPieceBB(color, PieceType.Pawn);

         score += DefendedPawn[(pawns & PawnAnyAttacks(pawns.Value, color)).CountBits()];
         score += ConnectedPawn[(pawns & pawns.RightShift()).CountBits()];

         // Enemy non-pawn pieces that can be attacked with a pawn push
         ulong pawnShift = (pawns.Shift(color == Color.White ? Direction.Down : Direction.Up) & ~board.ColorBoard(Color.Both).Value).Value;
         ulong enemyPieces = (board.ColorBoard(color ^ (Color)1) ^ board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn)).Value;
         score += PawnPushThreats * new Bitboard(PawnAnyAttacks(pawnShift, color) & enemyPieces).CountBits();

         // Enemy non-pawn pieces that are attacked
         score += PawnAttacks * new Bitboard(PawnAnyAttacks(pawns.Value, color) & enemyPieces).CountBits();

         while (pawns)
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();
            int rank = (color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1;

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value) == 0)
            {
               score += PassedPawn[rank];

               if (rank < 4)
               {
                  continue;
               }

               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)] * FriendlyKingPawnDistance;
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)] * EnemyKingPawnDistance;

               // Free to advance (no enemy non-pawn pieces ahead)
               if ((ForwardMask[(int)color][square] & board.ColorBoard(color ^ (Color)1).Value) == 0)
               {
                  score += FreeAdvancePawn;
               }
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & board.ColorPieceBB(color, PieceType.Pawn).Value) == 0)
            {
               // Penalty is based on file
               score -= IsolatedPawn[square & 7];
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
            score += KnightMobility[new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits()];

            if ((KnightAttacks[square] & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & info.KingZones[(int)color ^ 1]).CountBits();
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
            score += BishopMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
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
            score += RookMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((FILE_MASKS[square & 7] & board.ColorPieceBB(color, PieceType.Pawn).Value) == 0)
            {
               if ((FILE_MASKS[square & 7] & board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value) == 0)
               {
                  score += RookOpenFile;
               }
               else
               {
                  score += RookHalfOpenFile;
               }
            }

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
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
            score += QueenMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
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
               score += PawnShield[Math.Min(pawns.CountBits(), 3)];

               if ((board.ColorPieceBB(color, PieceType.Pawn).Value & FILE_MASKS[kingSq & 7]) == 0)
               {
                  score -= (board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value & FILE_MASKS[kingSq & 7]) == 0
                     ? KingOpenFile
                     : KingHalfOpenFile;
               }
            }

            if (info.KingAttacksCount[(int)color ^ 1] >= 2)
            {
               score -= info.KingAttacksWeight[(int)color ^ 1];
            }
         }

         return score;
      }

      public static readonly Score[] PieceValues = [
         new(61, 102),
         new(300, 341),
         new(312, 352),
         new(407, 628),
         new(846, 1160),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 15,  72), new( 20,  68), new( 13,  80), new( 55,  35), new( 25,  39), new( 24,  37), new(-74,  82), new(-86,  90),
         new(  6,  29), new( -3,  43), new( 21,   9), new( 29, -19), new( 39, -22), new( 71, -15), new( 39,  24), new( 19,  21),
         new(-14,  13), new(-10,  11), new( -8,  -4), new( -5, -17), new( 12, -12), new( 17, -16), new(  0,   7), new(  4,  -1),
         new(-18,   1), new(-18,   3), new( -8, -12), new(  0, -19), new(  0, -14), new( 10, -20), new( -9,  -2), new( -5, -12),
         new(-25,  -6), new(-19,  -6), new(-14, -13), new(-10, -10), new(  0,  -7), new(-25,  -8), new(-16,  -6), new(-28, -11),
         new(-18,  -1), new(-10,  -1), new( -6,  -6), new( -2,  -6), new(  3,   3), new(  3,  -3), new(  0,  -6), new(-25,  -8),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-122, -28), new(-117,   0), new(-79,  12), new(-45,   1), new(-21,   6), new(-72, -14), new(-90,  -1), new(-85, -48),
         new(-19,   1), new( -9,   6), new( 12,   2), new( 27,   2), new( 14,  -3), new( 54, -16), new( -1,   3), new( 10, -14),
         new( -7,   1), new( 14,   2), new( 25,  15), new( 30,  16), new( 57,   5), new( 72,  -6), new( 30,  -5), new( 15,  -7),
         new(  1,  14), new( 12,  12), new( 25,  22), new( 46,  25), new( 26,  26), new( 52,  19), new( 20,  15), new( 34,   5),
         new( -4,  15), new(  0,   9), new(  7,  24), new( 13,  26), new( 17,  30), new( 19,  17), new( 28,   7), new( 11,  12),
         new(-21,   1), new(-12,   2), new( -6,   5), new( -7,  20), new(  5,  18), new(  0,   2), new(  9,   0), new( -5,   5),
         new(-26,   1), new(-21,   5), new(-18,   0), new( -5,   2), new( -6,   1), new( -4,  -1), new( -2,   1), new( -4,  15),
         new(-59,   2), new(-20,  -3), new(-35,  -3), new(-18,  -1), new(-14,   3), new(-10,  -4), new(-16,   4), new(-30,   5),

         new(-35,   7), new(-70,  14), new(-66,  10), new(-107,  19), new(-98,  18), new(-88,   4), new(-53,   5), new(-73,   1),
         new(-32,  -4), new(-12,   0), new(-22,   1), new(-33,   6), new(-14,  -5), new(-21,   0), new(-34,   3), new(-38,  -1),
         new(-16,  11), new( -2,   1), new(  0,   6), new(  6,  -3), new( -1,   3), new( 35,   3), new( 12,   3), new(  6,   8),
         new(-20,   6), new(  8,   5), new(  4,   3), new( 17,  18), new( 15,   7), new( 15,   8), new( 16,   1), new(-16,   7),
         new( -6,   2), new(-15,   8), new( -2,  12), new( 15,  11), new(  9,  12), new(  1,   6), new( -8,   8), new( 16, -11),
         new( -7,   2), new(  9,   4), new(  0,   7), new(  2,  10), new(  6,  14), new(  7,   8), new(  9,   0), new( 11,  -2),
         new( 11,   6), new( -1,  -6), new( 10, -10), new(-10,   2), new( -3,   5), new(  7,  -3), new( 20,   0), new(  9,  -4),
         new( -1,  -1), new( 14,   3), new( -3,   0), new(-13,   0), new( -4,  -2), new(-10,  12), new(  6,  -4), new( 16, -14),

         new(-15,  29), new(-23,  34), new(-30,  42), new(-34,  38), new(-20,  32), new(  0,  29), new(  0,  31), new( 16,  23),
         new(-26,  28), new(-26,  38), new(-14,  41), new( -1,  32), new(-12,  32), new( 10,  22), new( 16,  17), new( 30,   9),
         new(-32,  28), new( -7,  25), new( -8,  26), new( -6,  22), new( 20,  10), new( 26,   6), new( 64,   1), new( 32,   0),
         new(-28,  30), new( -8,  23), new( -7,  27), new( -8,  23), new(  0,  12), new( 16,   4), new( 26,   8), new( 11,   5),
         new(-33,  22), new(-33,  22), new(-22,  18), new(-17,  17), new(-19,  16), new(-16,  12), new(  7,   5), new( -6,   4),
         new(-34,  12), new(-27,   9), new(-23,   5), new(-22,   6), new(-11,   1), new( -4,  -5), new( 27, -18), new(  3, -13),
         new(-32,   1), new(-29,   6), new(-17,   3), new(-14,   1), new( -8,  -4), new(  0, -10), new( 17, -18), new(-17,  -8),
         new(-16,   6), new(-16,   4), new(-12,   9), new( -3,   1), new(  2,  -3), new(  0,   0), new(  3,  -5), new( -8,  -5),

         new(-39,  37), new(-53,  58), new(-35,  77), new(-16,  71), new(-19,  72), new( -9,  66), new( 36,  14), new(-16,  47),
         new(-16,  18), new(-40,  45), new(-41,  81), new(-55, 105), new(-50, 119), new(-12,  77), new(-18,  65), new( 36,  49),
         new(-13,  26), new(-19,  32), new(-21,  66), new(-18,  76), new( -5,  87), new( 27,  73), new( 37,  43), new( 34,  46),
         new(-19,  36), new( -4,  36), new(-12,  48), new(-21,  72), new(-15,  88), new(  2,  77), new( 15,  75), new(  9,  60),
         new( -6,  21), new(-17,  46), new(-15,  48), new(-13,  64), new(-15,  66), new( -6,  60), new(  2,  55), new( 12,  49),
         new( -7,   5), new( -2,  22), new( -9,  36), new(-10,  33), new( -6,  39), new(  2,  33), new( 15,  20), new( 11,  14),
         new( -6,   0), new( -7,   2), new( -1,   3), new(  2,   8), new(  1,  11), new(  8, -14), new( 13, -36), new( 24, -52),
         new(-13,  -3), new(-10,  -4), new( -4,  -1), new(  1,   9), new(  0,  -5), new(-12,  -5), new( -8, -11), new(  2, -32),

         new(-12, -79), new(  7, -42), new(  1, -25), new(-76,   5), new(-41,  -8), new( -9,   1), new( 55,  -1), new( 68, -77),
         new(-81,  -8), new(-26,  15), new(-65,  25), new( 24,  12), new(-18,  28), new(-14,  43), new( 39,  33), new( 56,   9),
         new(-100,   3), new(  9,  17), new(-56,  34), new(-86,  47), new(-37,  46), new( 46,  36), new( 34,  34), new(  0,   7),
         new(-61,  -8), new(-64,  17), new(-94,  38), new(-142,  50), new(-128,  51), new(-79,  45), new(-69,  34), new(-115,  17),
         new(-65, -20), new(-65,   4), new(-83,  23), new(-127,  41), new(-117,  40), new(-71,  27), new(-77,  18), new(-128,   9),
         new(-13, -34), new( 20, -15), new(-33,   3), new(-49,  14), new(-44,  15), new(-41,  10), new( -9,  -3), new(-39, -12),
         new( 52, -21), new( 20,   1), new( 23, -15), new( -7,  -7), new( -6,  -3), new(  7,  -7), new( 29,   4), new( 26,  -7),
         new( 13, -42), new( 36, -24), new( 13,  -6), new(-29, -28), new(  3, -14), new(  6, -30), new( 35, -21), new( 33, -50),
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-96, -122),
         new(-44, -49),
         new(-21, -12),
         new(-13, 6),
         new(-2, 17),
         new(2, 29),
         new(11, 30),
         new(18, 34),
         new(29, 28),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-57, -108),
         new(-38, -70),
         new(-24, -21),
         new(-14, -1),
         new(-4, 8),
         new(4, 20),
         new(10, 29),
         new(14, 33),
         new(17, 38),
         new(21, 38),
         new(24, 40),
         new(32, 36),
         new(33, 41),
         new(40, 30),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-99, -156),
         new(-27, -90),
         new(-27, -24),
         new(-20, -7),
         new(-15, 2),
         new(-10, 9),
         new(-9, 17),
         new(-5, 23),
         new(-2, 25),
         new(2, 30),
         new(5, 35),
         new(5, 41),
         new(8, 43),
         new(14, 42),
         new(18, 40),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-20, -2),
         new(-70, -23),
         new(-53, -50),
         new(-54, -108),
         new(-32, -60),
         new(-33, 11),
         new(-22, 3),
         new(-19, 27),
         new(-17, 43),
         new(-13, 59),
         new(-8, 59),
         new(-6, 66),
         new(-3, 75),
         new(0, 75),
         new(1, 82),
         new(3, 87),
         new(4, 93),
         new(4, 100),
         new(3, 110),
         new(8, 108),
         new(13, 111),
         new(17, 108),
         new(29, 110),
         new(53, 93),
         new(40, 117),
         new(136, 60),
         new(66, 95),
         new(37, 89),
      ];

      public static Score RookHalfOpenFile = new(12, 4);
      public static Score RookOpenFile = new(29, 5);
      public static Score KingOpenFile = new(71, -9);
      public static Score KingHalfOpenFile = new(25, -16);

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(10, -5),
         new(13, -1),
         new(26, -9),
         new(18, 12),
      ];

      public static Score[] PawnShield =
      [
         new(-26, -14),
         new(5, -21),
         new(41, -31),
         new(75, -47),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(-3, 12),
         new(-7, 17),
         new(-9, 39),
         new(-14, 6),
         new(-29, 84),
         new(6, 91),
      ];

      public static Score[] DefendedPawn = [
         new(-25, -27),
         new(-8, -13),
         new(6, 1),
         new(19, 21),
         new(30, 42),
         new(42, 52),
         new(35, 47),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-15, -15),
         new(-4, 0),
         new(4, 4),
         new(12, 19),
         new(21, 21),
         new(24, 76),
         new(46, -4),
         new(-5, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(0, 5),
         new(1, 13),
         new(10, 8),
         new(7, 10),
         new(11, 13),
         new(7, 5),
         new(0, 13),
         new(4, 4),
      ];

      public static Score FriendlyKingPawnDistance = new(9, -12);
      public static Score EnemyKingPawnDistance = new(-4, 19);
      public static Score BishopPair = new(24, 64);
      public static Score PawnPushThreats = new(19, 0);
      public static Score PawnAttacks = new(46, 7);
      public static Score FreeAdvancePawn = new(-17, 54);
   }
}
