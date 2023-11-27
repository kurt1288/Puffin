namespace Puffin
{
   internal sealed class MoveGen
   {
      public static MoveList GenerateAll(Board board)
      {
         MoveList moveList = new();
         GenerateQuiet(moveList, board);
         GenerateNoisy(moveList, board);

         return moveList;
      }

      public static void GenerateQuiet(MoveList moveList, Board board)
      {
         GeneratePawnPushes(moveList, board);
         GenerateCastling(moveList, board);

         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Bitboard nonPawns = board.ColorBB[(int)board.SideToMove] & ~board.PieceBB[(int)PieceType.Pawn].Value;

         while (!nonPawns.IsEmpty())
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Bitboard quiets = board.Mailbox[from].Type switch
            {
               PieceType.Knight => new(Attacks.KnightAttacks[from]),
               PieceType.Bishop => new(Attacks.GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(Attacks.GetRookAttacks(from, occupied)),
               PieceType.Queen => new(Attacks.GetQueenAttacks(from, occupied)),
               PieceType.King => new(Attacks.KingAttacks[from]),
               _ => throw new Exception($"Unable to get moves for piece {board.Mailbox[from].Type}"),
            };

            quiets &= ~occupied;
            while (!quiets.IsEmpty())
            {
               moveList.Add(new Move(from, quiets.GetLSB(), MoveFlag.Quiet));
               quiets.ClearLSB();
            }
         }
      }

      public static void GenerateNoisy(MoveList moveList, Board board)
      {
         GeneratePawnAttacks(moveList, board);
         GenerateEnPassant(moveList, board);
         GeneratePawnPromotions(moveList, board);

         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Bitboard nonPawns = board.ColorBB[(int)board.SideToMove] & ~board.PieceBB[(int)PieceType.Pawn].Value;

         while (!nonPawns.IsEmpty())
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Bitboard attacks = board.Mailbox[from].Type switch
            {
               PieceType.Knight => new(Attacks.KnightAttacks[from]),
               PieceType.Bishop => new(Attacks.GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(Attacks.GetRookAttacks(from, occupied)),
               PieceType.Queen => new(Attacks.GetQueenAttacks(from, occupied)),
               PieceType.King => new(Attacks.KingAttacks[from]),
               _ => throw new Exception($"Unable to get attacks for piece {board.Mailbox[from].Type}"),
            };

            attacks &= board.ColorBB[(int)board.SideToMove ^ 1];
            while (!attacks.IsEmpty())
            {
               moveList.Add(new Move(from, attacks.GetLSB(), MoveFlag.Capture));
               attacks.ClearLSB();
            }
         }
      }

      // Only generates QUIET pawn moves (no promotions, attacks, en passant, etc.)
      public static void GeneratePawnPushes(MoveList moveList, Board board)
      {
         ulong pawns = board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)board.SideToMove].Value;
         ulong empty = ~(board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value);
         int up = board.SideToMove == Color.White ? -8 : 8;
         int startRank = board.SideToMove == Color.White ? 6 : 1;
         Bitboard targets = board.SideToMove == Color.White ? new(pawns >> 8) : new(pawns << 8);
         targets &= empty & ~Constants.RANK_MASKS[board.SideToMove == Color.White ? (int)Rank.Rank_8 : (int)Rank.Rank_1];

         while (!targets.IsEmpty())
         {
            int square = targets.GetLSB();
            targets.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.Quiet));

            if (square - up >> 3 == startRank && (Constants.SquareBB[square + up] & empty) != 0)
            {
               moveList.Add(new Move(square - up, square + up, MoveFlag.DoublePawnPush));
            }
         }
      }

      public static void GeneratePawnAttacks(MoveList moveList, Board board)
      {
         ulong pawns = board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)board.SideToMove].Value;
         Bitboard rightTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.H]) >> 7);
         Bitboard leftTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.A]) >> 9);
         int upRight = board.SideToMove == Color.White ? -7 : 7;
         int upLeft = board.SideToMove == Color.White ? -9 : 9;

         if (board.SideToMove == Color.Black)
         {
            rightTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.A]) << 7);
            leftTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.H]) << 9);
         }

         rightTargets &= board.ColorBB[(int)board.SideToMove ^ 1];
         leftTargets &= board.ColorBB[(int)board.SideToMove ^ 1];

         while (!rightTargets.IsEmpty())
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

         while (!leftTargets.IsEmpty())
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
      public static void GeneratePawnPromotions(MoveList moveList, Board board)
      {
         ulong pawns = board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)board.SideToMove].Value;
         ulong empty = ~(board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value);
         Bitboard targets = new(pawns >> 8 & Constants.RANK_MASKS[(int)Rank.Rank_8]);
         int up = board.SideToMove == Color.White ? -8 : 8;

         if (board.SideToMove == Color.Black)
         {
            targets = new(pawns << 8 & Constants.RANK_MASKS[(int)Rank.Rank_1]);
         }

         targets &= empty;

         while (!targets.IsEmpty())
         {
            int square = targets.GetLSB();
            targets.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.KnightPromotion));
            moveList.Add(new Move(square - up, square, MoveFlag.BishopPromotion));
            moveList.Add(new Move(square - up, square, MoveFlag.RookPromotion));
            moveList.Add(new Move(square - up, square, MoveFlag.QueenPromotion));
         }
      }

      public static void GenerateEnPassant(MoveList moveList, Board board)
      {
         Bitboard attackers = board.En_Passant != Square.Null
            ? new(Attacks.PawnAttacks[(int)board.SideToMove ^ 1][(int)board.En_Passant]
               & board.PieceBB[(int)PieceType.Pawn].Value
               & board.ColorBB[(int)board.SideToMove].Value)
            : new();

         while (!attackers.IsEmpty())
         {
            int square = attackers.GetLSB();
            attackers.ClearLSB();
            moveList.Add(new Move(square, (int)board.En_Passant, MoveFlag.EPCapture));
         }
      }

      public static void GenerateCastling(MoveList moveList, Board board)
      {
         Bitboard castleSquares = new(board.CastleSquares & (board.SideToMove == Color.White ? Constants.RANK_MASKS[(int)Rank.Rank_1] : Constants.RANK_MASKS[(int)Rank.Rank_8]));
         int kingSquare = board.SideToMove == Color.White ? board.PieceBB[(int)PieceType.King].GetMSB() : board.PieceBB[(int)PieceType.King].GetLSB();

         while (!castleSquares.IsEmpty())
         {
            int square = castleSquares.GetLSB();
            castleSquares.ClearLSB();

            if ((Constants.BetweenBB[kingSquare][square] & (board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value)) == 0)
            {
               if (kingSquare < square)
               {
                  moveList.Add(new Move(kingSquare, board.SideToMove == Color.White ? (int)Square.G1 : (int)Square.G8, MoveFlag.KingCastle));
               }
               else if (kingSquare > square)
               {
                  moveList.Add(new Move(kingSquare, board.SideToMove == Color.White ? (int)Square.C1 : (int)Square.C8, MoveFlag.QueenCastle));
               }
            }
         }
      }
   }
}
