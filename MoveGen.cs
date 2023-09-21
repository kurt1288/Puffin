namespace Skookum
{
   internal class MoveGen
   {
      readonly Board Board;

      public MoveGen(Board board)
      {
         Board = board;
      }

      public MoveList GenerateAll(MoveList moveList)
      {
         GenerateQuiet(moveList);
         GenerateNoisy(moveList);

         return moveList;
      }

      public void GenerateQuiet(MoveList moveList)
      {
         GeneratePawnPushes(moveList);
         GenerateCastling(moveList);

         ulong occupied = Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value;
         ulong empty = ~occupied;
         Bitboard nonPawns = new(Board.ColorBB[(int)Board.SideToMove].Value & ~Board.PieceBB[(int)PieceType.Pawn].Value);

         while (!nonPawns.IsEmpty())
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Piece piece = Board.Mailbox[from];

            Bitboard quiets = piece.Type switch
            {
               PieceType.Knight => new(Attacks.KnightAttacks[from]),
               PieceType.Bishop => new(Attacks.GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(Attacks.GetRookAttacks(from, occupied)),
               PieceType.Queen => new(Attacks.GetQueenAttacks(from, occupied)),
               PieceType.King => new(Attacks.KingAttacks[from]),
               _ => throw new Exception($"Unable to get moves for piece {piece}"),
            };

            quiets.And(empty);
            while (!quiets.IsEmpty())
            {
               int to = quiets.GetLSB();
               quiets.ClearLSB();
               moveList.Add(new Move(from, to, MoveFlag.Quiet), piece, null);
            }
         }
      }

      public void GenerateNoisy(MoveList moveList)
      {
         GeneratePawnAttacks(moveList);
         GenerateEnPassant(moveList);
         GeneratePawnPromotions(moveList);

         ulong occupied = Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value;
         ulong opponent = Board.ColorBB[(int)Board.SideToMove ^ 1].Value;
         Bitboard nonPawns = new(Board.ColorBB[(int)Board.SideToMove].Value & ~Board.PieceBB[(int)PieceType.Pawn].Value);

         while (!nonPawns.IsEmpty())
         {
            int from = nonPawns.GetLSB();
            nonPawns.ClearLSB();

            Piece piece = Board.Mailbox[from];

            Bitboard attacks = piece.Type switch
            {
               PieceType.Knight => new(Attacks.KnightAttacks[from]),
               PieceType.Bishop => new(Attacks.GetBishopAttacks(from, occupied)),
               PieceType.Rook => new(Attacks.GetRookAttacks(from, occupied)),
               PieceType.Queen => new(Attacks.GetQueenAttacks(from, occupied)),
               PieceType.King => new(Attacks.KingAttacks[from]),
               _ => throw new Exception($"Unable to get attacks for piece {piece}"),
            };

            attacks.And(opponent);
            while (!attacks.IsEmpty())
            {
               int to = attacks.GetLSB();
               attacks.ClearLSB();
               moveList.Add(new Move(from, to, MoveFlag.Capture), piece, Board.Mailbox[to]);
            }
         }
      }

      // Only generates QUIET pawn moves (no promotions, attacks, en passant, etc.)
      public void GeneratePawnPushes(MoveList moveList)
      {
         ulong pawns = Board.PieceBB[(int)PieceType.Pawn].Value & Board.ColorBB[(int)Board.SideToMove].Value;
         ulong empty = ~(Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value);
         Bitboard targets;
         Bitboard doubleTargets;
         int up;

         if (Board.SideToMove == Color.White)
         {
            targets = new((pawns >> 8) & empty & ~Constants.RANK_MASKS[(int)Rank.Rank_8]);
            doubleTargets = new(((targets.Value & Constants.RANK_MASKS[(int)Rank.Rank_3]) >> 8) & empty);
            up = -8;
         }
         else
         {
            targets = new((pawns << 8) & empty & ~Constants.RANK_MASKS[(int)Rank.Rank_1]);
            doubleTargets = new(((targets.Value & Constants.RANK_MASKS[(int)Rank.Rank_6]) << 8) & empty);
            up = 8;
         }

         while (!targets.IsEmpty())
         {
            int square = targets.GetLSB();
            targets.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.Quiet), null, null);
         }

         while (!doubleTargets.IsEmpty())
         {
            int square = doubleTargets.GetLSB();
            doubleTargets.ClearLSB();
            moveList.Add(new Move(square - (2 * up), square, MoveFlag.DoublePawnPush), null, null);
         }
      }

      public void GeneratePawnAttacks(MoveList moveList)
      {
         ulong pawns = Board.PieceBB[(int)PieceType.Pawn].Value & Board.ColorBB[(int)Board.SideToMove].Value;
         Bitboard rightTargets;
         Bitboard leftTargets;
         int upRight;
         int upLeft;

         if (Board.SideToMove == Color.White)
         {
            rightTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.H]) >> 7 & Board.ColorBB[(int)Board.SideToMove ^ 1].Value);
            leftTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.A]) >> 9 & Board.ColorBB[(int)Board.SideToMove ^ 1].Value);
            upRight = -7;
            upLeft = -9;
         }
         else
         {
            rightTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.A]) << 7 & Board.ColorBB[(int)Board.SideToMove ^ 1].Value);
            leftTargets = new((pawns & ~Constants.FILE_MASKS[(int)File.H]) << 9 & Board.ColorBB[(int)Board.SideToMove ^ 1].Value);
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
               moveList.Add(new Move(from, to, MoveFlag.KnightPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
               moveList.Add(new Move(from, to, MoveFlag.BishopPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
               moveList.Add(new Move(from, to, MoveFlag.RookPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
               moveList.Add(new Move(from, to, MoveFlag.QueenPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
            }
            else
            {
               moveList.Add(new Move(from, to, MoveFlag.Capture), Board.Mailbox[from], Board.Mailbox[to]);
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
               moveList.Add(new Move(from, to, MoveFlag.KnightPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
               moveList.Add(new Move(from, to, MoveFlag.BishopPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
               moveList.Add(new Move(from, to, MoveFlag.RookPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
               moveList.Add(new Move(from, to, MoveFlag.QueenPromotionCapture), Board.Mailbox[from], Board.Mailbox[to]);
            }
            else
            {
               moveList.Add(new Move(from, to, MoveFlag.Capture), Board.Mailbox[from], Board.Mailbox[to]);
            }
         }
      }

      // Pawn pushes to promotions (no attacks)
      public void GeneratePawnPromotions(MoveList moveList)
      {
         ulong pawns = Board.PieceBB[(int)PieceType.Pawn].Value & Board.ColorBB[(int)Board.SideToMove].Value;
         ulong empty = ~(Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value);
         int up;
         Bitboard targets;

         if (Board.SideToMove == Color.White)
         {
            targets = new((pawns >> 8) & empty & Constants.RANK_MASKS[(int)Rank.Rank_8]);
            up = -8;
         }
         else
         {
            targets = new((pawns << 8) & empty & Constants.RANK_MASKS[(int)Rank.Rank_1]);
            up = 8;
         }

         while (!targets.IsEmpty())
         {
            int square = targets.GetLSB();
            targets.ClearLSB();
            moveList.Add(new Move(square - up, square, MoveFlag.KnightPromotion), null, null);
            moveList.Add(new Move(square - up, square, MoveFlag.BishopPromotion), null, null);
            moveList.Add(new Move(square - up, square, MoveFlag.RookPromotion), null, null);
            moveList.Add(new Move(square - up, square, MoveFlag.QueenPromotion), null, null);
         }
      }

      public void GenerateEnPassant(MoveList moveList)
      {
         if (Board.En_Passant != Square.Null)
         {
            Bitboard attackers = new(Attacks.PawnAttacks[(int)Board.SideToMove ^ 1][(int)Board.En_Passant]
               & Board.PieceBB[(int)PieceType.Pawn].Value
               & Board.ColorBB[(int)Board.SideToMove].Value);

            while (!attackers.IsEmpty())
            {
               int square = attackers.GetLSB();
               attackers.ClearLSB();
               moveList.Add(new Move(square, (int)Board.En_Passant, MoveFlag.EPCapture), Board.Mailbox[square], new Piece(PieceType.Pawn, (Color)((int)Board.SideToMove ^ 1)));
            }
         }
      }

      public void GenerateCastling(MoveList moveList)
      {
         if (Board.SideToMove == Color.White)
         {
            Bitboard whiteCastle = new(Board.CastleSquares & Constants.RANK_MASKS[(int)Rank.Rank_1]);
            int kingSquare = 63 - Board.PieceBB[(int)PieceType.King].GetMSB();

            while (!whiteCastle.IsEmpty())
            {
               int square = whiteCastle.GetLSB();
               whiteCastle.ClearLSB();

               if (kingSquare < square && (Constants.BetweenBB[kingSquare][square] & (Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.G1, MoveFlag.KingCastle), null, null);
               }
               else if (kingSquare > square && (Constants.BetweenBB[kingSquare][square] & (Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.C1, MoveFlag.QueenCastle), null, null);
               }
            }
         }
         else
         {
            Bitboard blackCastle = new(Board.CastleSquares & Constants.RANK_MASKS[(int)Rank.Rank_8]);
            int kingSquare = Board.PieceBB[(int)PieceType.King].GetLSB();

            while (!blackCastle.IsEmpty())
            {
               int square = blackCastle.GetLSB();
               blackCastle.ClearLSB();

               if (kingSquare < square && (Constants.BetweenBB[kingSquare][square] & (Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.G8, MoveFlag.KingCastle), null, null);
               }
               else if (kingSquare > square && (Constants.BetweenBB[kingSquare][square] & (Board.ColorBB[(int)Color.White].Value | Board.ColorBB[(int)Color.Black].Value)) == 0)
               {
                  moveList.Add(new Move(kingSquare, (int)Square.C8, MoveFlag.QueenCastle), null, null);
               }
            }
         }
      }
   }
}
