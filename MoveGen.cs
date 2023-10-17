namespace Skookum
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
         Bitboard nonPawns = new(board.ColorBB[(int)board.SideToMove].Value & ~board.PieceBB[(int)PieceType.Pawn].Value);

         while (!nonPawns.IsEmpty())
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Piece piece = board.Mailbox[from];

            Bitboard quiets = piece.Type switch
            {
               PieceType.Knight => new(Attacks.KnightAttacks[from]),
               PieceType.Bishop => new(Attacks.GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(Attacks.GetRookAttacks(from, occupied)),
               PieceType.Queen => new(Attacks.GetQueenAttacks(from, occupied)),
               PieceType.King => new(Attacks.KingAttacks[from]),
               _ => throw new Exception($"Unable to get moves for piece {piece}"),
            };

            quiets.And(~occupied);
            while (!quiets.IsEmpty())
            {
               int to = quiets.GetLSB();
               quiets.ClearLSB();
               moveList.Add(new Move(from, to, MoveFlag.Quiet));
            }
         }
      }

      public static void GenerateNoisy(MoveList moveList, Board board)
      {
         GeneratePawnAttacks(moveList, board);
         GenerateEnPassant(moveList, board);
         GeneratePawnPromotions(moveList, board);

         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Bitboard nonPawns = new(board.ColorBB[(int)board.SideToMove].Value & ~board.PieceBB[(int)PieceType.Pawn].Value);

         while (!nonPawns.IsEmpty())
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Piece piece = board.Mailbox[from];

            Bitboard attacks = piece.Type switch
            {
               PieceType.Knight => new(Attacks.KnightAttacks[from]),
               PieceType.Bishop => new(Attacks.GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(Attacks.GetRookAttacks(from, occupied)),
               PieceType.Queen => new(Attacks.GetQueenAttacks(from, occupied)),
               PieceType.King => new(Attacks.KingAttacks[from]),
               _ => throw new Exception($"Unable to get attacks for piece {piece}"),
            };

            attacks.And(board.ColorBB[(int)board.SideToMove ^ 1].Value);
            while (!attacks.IsEmpty())
            {
               int to = attacks.GetLSB();
               attacks.ClearLSB();
               moveList.Add(new Move(from, to, MoveFlag.Capture));
            }
         }
      }

      // Only generates QUIET pawn moves (no promotions, attacks, en passant, etc.)
      public static void GeneratePawnPushes(MoveList moveList, Board board)
      {
         ulong pawns = board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)board.SideToMove].Value;
         ulong empty = ~(board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value);
         Bitboard targets = new((pawns >> 8) & empty & ~Constants.RANK_MASKS[(int)Rank.Rank_8]);
         Bitboard doubleTargets = new(((targets.Value & Constants.RANK_MASKS[(int)Rank.Rank_3]) >> 8) & empty);
         int up = -8;

         if (board.SideToMove == Color.Black)
         {
            targets = new((pawns << 8) & empty & ~Constants.RANK_MASKS[(int)Rank.Rank_1]);
            doubleTargets = new(((targets.Value & Constants.RANK_MASKS[(int)Rank.Rank_6]) << 8) & empty);
            up = 8;
         }

         while (!targets.IsEmpty())
         {
            int square = targets.GetLSB();
            targets.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.Quiet));
         }

         while (!doubleTargets.IsEmpty())
         {
            int square = doubleTargets.GetLSB();
            doubleTargets.ClearLSB();
            moveList.Add(new Move(square - (2 * up), square, MoveFlag.DoublePawnPush));
         }
      }

      public static void GeneratePawnAttacks(MoveList moveList, Board board)
      {
         ulong pawns = board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)board.SideToMove].Value;
         Bitboard rightTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.H]) >> 7 & board.ColorBB[(int)board.SideToMove ^ 1].Value);
         Bitboard leftTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.A]) >> 9 & board.ColorBB[(int)board.SideToMove ^ 1].Value);
         int upRight = -7;
         int upLeft = -9;

         if (board.SideToMove == Color.Black)
         {
            rightTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.A]) << 7 & board.ColorBB[(int)board.SideToMove ^ 1].Value);
            leftTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.H]) << 9 & board.ColorBB[(int)board.SideToMove ^ 1].Value);
            upRight = 7;
            upLeft = 9;
         }

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
         Bitboard targets = new((pawns >> 8) & empty & Constants.RANK_MASKS[(int)Rank.Rank_8]);
         int up = -8;

         if (board.SideToMove == Color.Black)
         {
            targets = new((pawns << 8) & empty & Constants.RANK_MASKS[(int)Rank.Rank_1]);
            up = 8;
         }

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
         if (board.En_Passant != Square.Null)
         {
            Bitboard attackers = new(Attacks.PawnAttacks[(int)board.SideToMove ^ 1][(int)board.En_Passant]
               & board.PieceBB[(int)PieceType.Pawn].Value
               & board.ColorBB[(int)board.SideToMove].Value);

            while (!attackers.IsEmpty())
            {
               int square = attackers.GetLSB();
               attackers.ClearLSB();
               moveList.Add(new Move(square, (int)board.En_Passant, MoveFlag.EPCapture));
            }
         }
      }

      public static void GenerateCastling(MoveList moveList, Board board)
      {
         if (board.SideToMove == Color.White)
         {
            Bitboard whiteCastle = new(board.CastleSquares & Constants.RANK_MASKS[(int)Rank.Rank_1]);
            int kingSquare = 63 - board.PieceBB[(int)PieceType.King].GetMSB();

            while (!whiteCastle.IsEmpty())
            {
               int square = whiteCastle.GetLSB();
               whiteCastle.ClearLSB();

               if (kingSquare < square && (Constants.BetweenBB[kingSquare][square] & (board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.G1, MoveFlag.KingCastle));
               }
               else if (kingSquare > square && (Constants.BetweenBB[kingSquare][square] & (board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.C1, MoveFlag.QueenCastle));
               }
            }
         }
         else
         {
            Bitboard blackCastle = new(board.CastleSquares & Constants.RANK_MASKS[(int)Rank.Rank_8]);
            int kingSquare = board.PieceBB[(int)PieceType.King].GetLSB();

            while (!blackCastle.IsEmpty())
            {
               int square = blackCastle.GetLSB();
               blackCastle.ClearLSB();

               if (kingSquare < square && (Constants.BetweenBB[kingSquare][square] & (board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.G8, MoveFlag.KingCastle));
               }
               else if (kingSquare > square && (Constants.BetweenBB[kingSquare][square] & (board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.C8, MoveFlag.QueenCastle));
               }
            }
         }
      }
   }
}
