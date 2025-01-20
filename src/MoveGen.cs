using static Puffin.Constants;
using static Puffin.Attacks.Attacks;

namespace Puffin
{
   internal sealed class MoveGen
   {
      public static void GenerateAll(Board board, ref MoveList list)
      {
         Generate(ref list, board, MoveGenType.Quiet);
         Generate(ref list, board, MoveGenType.Noisy);
      }

      public static void Generate(ref MoveList moveList, Board board, MoveGenType genType)
      {         
         ulong targets = genType == MoveGenType.Noisy ? board.ColorBoard(board.SideToMove ^ (Color)1).Value : ~board.ColorBoard(Color.Both).Value;

         // If double check, only the king can move so skip generating other moves
         if (board.Checkers.CountBits() > 1)
         {
            GenerateKingMoves(ref moveList, board, targets, genType);
            return;
         }

         ulong evasions = board.Checkers ? BetweenBB[board.KingSquares[(int)board.SideToMove]][board.Checkers.GetLSB()] | SquareBB[board.Checkers.GetLSB()] : 0xffffffffffffffff;

         if (genType == MoveGenType.Quiet)
         {
            GeneratePawnPushes(ref moveList, board, targets & evasions);
            GenerateCastling(ref moveList, board);
         }
         else
         {
            GeneratePawnAttacks(ref moveList, board, targets & evasions);
            GenerateEnPassant(ref moveList, board);
            GeneratePawnPromotions(ref moveList, board, evasions);
         }

         GeneratePieceMoves(ref moveList, board, targets & evasions, genType);
         GenerateKingMoves(ref moveList, board, targets, genType);
      }

      private static void GeneratePieceMoves(ref MoveList moveList, Board board, ulong targets, MoveGenType type)
      {
         ulong occupied = board.ColorBoard(Color.Both).Value;
         Bitboard nonPawns = board.ColorBoard(board.SideToMove) & ~(board.PieceBoard(PieceType.Pawn) | board.PieceBoard(PieceType.King));

         while (nonPawns)
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Bitboard moves = board.Squares[from].Type switch
            {
               PieceType.Knight => new(KnightAttacks[from]),
               PieceType.Bishop => new(GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(GetRookAttacks(from, occupied)),
               PieceType.Queen => new(GetQueenAttacks(from, occupied)),
               _ => throw new Exception($"Unable to get moves for piece {board.Squares[from].Type}"),
            };

            moves &= targets;
            while (moves)
            {
               moveList.Add(new Move(from, moves.GetLSB(), type == MoveGenType.Quiet ? MoveFlag.Quiet : MoveFlag.Capture));
               moves.ClearLSB();
            }
         }
      }

      private static void GenerateKingMoves(ref MoveList moveList, Board board, ulong targets, MoveGenType type)
      {
         int kingSq = board.KingSquares[(int)board.SideToMove];
         Bitboard moves = new(KingAttacks[kingSq] & targets);

         while (moves)
         {
            moveList.Add(new Move(kingSq, moves.GetLSB(), type == MoveGenType.Quiet ? MoveFlag.Quiet : MoveFlag.Capture));
            moves.ClearLSB();
         }
      }

      // Only generates QUIET pawn moves (no promotions, attacks, en passant, etc.)
      private static void GeneratePawnPushes(ref MoveList moveList, Board board, ulong targets)
      {
         ulong pawns = board.ColorPieceBB(board.SideToMove, PieceType.Pawn).Value;
         ulong empty = ~board.ColorBoard(Color.Both).Value;
         int up = board.SideToMove == Color.White ? -8 : 8;

         Bitboard pushSquares = new Bitboard(board.SideToMove == Color.White ? pawns >> 8 : pawns << 8)
            & empty
            & (~RANK_MASKS[board.SideToMove == Color.White ? (int)Rank.Rank_8 : (int)Rank.Rank_1]);

         Bitboard doublePushSquares = board.SideToMove == Color.White
            ? ((pushSquares & RANK_MASKS[2]) >> 8) & empty
            : ((pushSquares & RANK_MASKS[5]) << 8) & empty;

         pushSquares &= targets;
         while (pushSquares)
         {
            int square = pushSquares.GetLSB();
            pushSquares.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.Quiet));
         }

         doublePushSquares &= targets;
         while (doublePushSquares)
         {
            int square = doublePushSquares.GetLSB();
            doublePushSquares.ClearLSB();
            moveList.Add(new Move(square - (up * 2), square, MoveFlag.DoublePawnPush));
         }
      }

      private static void GeneratePawnAttacks(ref MoveList moveList, Board board, ulong targets)
      {
         ulong pawns = board.ColorPieceBB(board.SideToMove, PieceType.Pawn).Value;
         Bitboard rightTargets = new((pawns & ~FILE_MASKS[(int)File.H]) >> 7);
         Bitboard leftTargets = new((pawns & ~FILE_MASKS[(int)File.A]) >> 9);
         int upRight = board.SideToMove == Color.White ? -7 : 7;
         int upLeft = board.SideToMove == Color.White ? -9 : 9;

         if (board.SideToMove == Color.Black)
         {
            rightTargets = new((pawns & ~FILE_MASKS[(int)File.A]) << 7);
            leftTargets = new((pawns & ~FILE_MASKS[(int)File.H]) << 9);
         }

         rightTargets &= targets;
         leftTargets &= targets;

         while (rightTargets)
         {
            int to = rightTargets.GetLSB();
            rightTargets.ClearLSB();
            int from = to - upRight;

            if (to / 8 == (int)Rank.Rank_8 || to / 8 == (int)Rank.Rank_1)
            {
               // Pawn attack to promotion
               moveList.Add(new Move(from, to, MoveFlag.KnightPromotionCapture));
               moveList.Add(new Move(from, to, MoveFlag.BishopPromotionCapture));
               moveList.Add(new Move(from, to, MoveFlag.RookPromotionCapture));
               moveList.Add(new Move(from, to, MoveFlag.QueenPromotionCapture));
            }
            else
            {
               moveList.Add(new Move(from, to, MoveFlag.Capture));
            }
         }

         while (leftTargets)
         {
            int to = leftTargets.GetLSB();
            leftTargets.ClearLSB();
            int from = to - upLeft;

            if (to / 8 == (int)Rank.Rank_8 || to / 8 == (int)Rank.Rank_1)
            {
               // Pawn attack to promotion
               moveList.Add(new Move(from, to, MoveFlag.KnightPromotionCapture));
               moveList.Add(new Move(from, to, MoveFlag.BishopPromotionCapture));
               moveList.Add(new Move(from, to, MoveFlag.RookPromotionCapture));
               moveList.Add(new Move(from, to, MoveFlag.QueenPromotionCapture));
            }
            else
            {
               moveList.Add(new Move(from, to, MoveFlag.Capture));
            }
         }
      }

      // Pawn pushes to promotions (no attacks)
      private static void GeneratePawnPromotions(ref MoveList moveList, Board board, ulong targets)
      {
         ulong pawns = board.ColorPieceBB(board.SideToMove, PieceType.Pawn).Value;
         ulong empty = ~board.ColorBoard(Color.Both).Value;
         Bitboard squares = new(pawns >> 8 & RANK_MASKS[(int)Rank.Rank_8]);
         int up = board.SideToMove == Color.White ? -8 : 8;

         if (board.SideToMove == Color.Black)
         {
            squares = new(pawns << 8 & RANK_MASKS[(int)Rank.Rank_1]);
         }

         squares &= empty & targets;

         while (squares)
         {
            int square = squares.GetLSB();
            squares.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.KnightPromotion));
            moveList.Add(new Move(square - up, square, MoveFlag.BishopPromotion));
            moveList.Add(new Move(square - up, square, MoveFlag.RookPromotion));
            moveList.Add(new Move(square - up, square, MoveFlag.QueenPromotion));
         }
      }

      private static void GenerateEnPassant(ref MoveList moveList, Board board)
      {
         Bitboard attackers = board.EnPassant != Square.Null
            ? new(PawnAttacks[(int)board.SideToMove ^ 1][(int)board.EnPassant]
               & board.ColorPieceBB(board.SideToMove, PieceType.Pawn).Value)
            : new();

         while (attackers)
         {
            int square = attackers.GetLSB();
            attackers.ClearLSB();
            moveList.Add(new Move(square, (int)board.EnPassant, MoveFlag.EPCapture));
         }
      }

      public static void GenerateCastling(ref MoveList moveList, Board board)
      {
         Bitboard rookSquares = new(board.CastleSquares & (board.SideToMove == Color.White ? RANK_MASKS[(int)Rank.Rank_1] : RANK_MASKS[(int)Rank.Rank_8]));

         if (board.InCheck || !rookSquares)
         {
            return;
         }

         int kingSquare = board.SideToMove == Color.White ? board.PieceBoard(PieceType.King).GetMSB() : board.PieceBoard(PieceType.King).GetLSB();
         ulong occupied = board.ColorBoard(Color.Both).Value;
         int kingCastleSquare = board.SideToMove == Color.White ? (int)Square.G1 : (int)Square.G8;
         int queenCastleSquare = board.SideToMove == Color.White ? (int)Square.C1 : (int)Square.C8;
         int oppositeColor = (int)board.SideToMove ^ 1;

         while (rookSquares)
         {
            int rookSquare = rookSquares.GetLSB();
            rookSquares.ClearLSB();

            if ((BetweenBB[kingSquare][rookSquare] & occupied) != 0)
            {
               continue;
            }

            int targetSquare = (kingSquare < rookSquare) ? kingCastleSquare : queenCastleSquare;
            int pathSquare = (kingSquare < rookSquare) ? targetSquare - 1 : targetSquare + 1;

            // Check the path of the king to make sure it isn't moving through check
            if (!board.IsAttacked(pathSquare, oppositeColor))
            {
               moveList.Add(new Move(
                  kingSquare,
                  targetSquare,
                  kingSquare < rookSquare ? MoveFlag.KingCastle : MoveFlag.QueenCastle
               ));
            }
         }
      }
   }
}
