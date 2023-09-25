namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(211, 58), new Score(232, 211), new Score(275, 223), new Score(349, 320), new Score(508, 615), new Score(10000, 10000),
      };

      public static readonly Score[] KnightMobility =
      {
         new Score(0, 0), new Score(0, 0), new Score(-41, -58), new Score(-32, -40), new Score(-32, -14),
         new Score(0, 0), new Score(-26, -14), new Score(0, 0), new Score(31, 3),
      };

      public static readonly Score[] BishopMobility =
      {
         new Score(0, 0), new Score(-3, -22), new Score(-32, -71), new Score(-29, -40), new Score(-32, -13), new Score(-35, -13), new Score(-26, -13),
         new Score(-26, -13), new Score(22, 0), new Score(25, 0), new Score(28, 4), new Score(31, 4), new Score(21, 4), new Score(65, 4),
      };

      public static readonly Score[] RookMobility =
      {
         new Score(0, 0), new Score(0, 0), new Score(0, -92), new Score(0, -42), new Score(6, -11), new Score(-48, -11), new Score(-6, -11),
         new Score(-38, -2), new Score(27, -11), new Score(-33, -11), new Score(-33, -11), new Score(-18, -6), new Score(27, -2), new Score(-24, -11),
         new Score(21, 6),
      };

      public static readonly Score[] QueenMobility =
      {
         new Score(0, 0), new Score(0, 0), new Score(0, 0), new Score(0, -6), new Score(0, 7), new Score(-3, -18), new Score(0, 16), new Score(-6, 7),
         new Score(9, 25), new Score(0, 7), new Score(33, 43), new Score(-46, 25), new Score(-27, 25), new Score(-24, 25), new Score(-36, 25),
         new Score(-24, 20), new Score(-39, 11), new Score(-36, 12), new Score(-24, 12), new Score(35, 11), new Score(26, 25), new Score(26, 25),
         new Score(29, 25), new Score(23, 25), new Score(26, 25), new Score(8, 29), new Score(6, 16), new Score(-3, 29),
      };

      public static int Evaluate(Board board)
      {
         Score white = Material(board, Color.White);
         Score black = Material(board, Color.Black);
         white += Knights(board, Color.White);
         black += Knights(board, Color.Black);
         white += Bishops(board, Color.White);
         black += Bishops(board, Color.Black);
         white += Rooks(board, Color.White);
         black += Rooks(board, Color.Black);
         white += Queens(board, Color.White);
         black += Queens(board, Color.Black);
         Score total = white - black;

         if (board.SideToMove == Color.Black)
         {
            total *= -1;
         }

         return total.Eg + Math.Clamp(board.Phase, 0, 24) / 24 * (total.Mg - total.Eg);
      }

      private static Score Material(Board board, Color color)
      {
         Bitboard us = new(board.ColorBB[(int)color].Value);
         Score score = new();

         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Mailbox[square];

            score += PieceValues[(int)piece.Type];
         }

         return score;
      }

      private static Score Knights(Board board, Color color)
      {
         Bitboard us = new(board.PieceBB[(int)PieceType.Knight].Value & board.ColorBB[(int)color].Value);
         Score score = new();
         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            score += KnightMobility[new Bitboard(Attacks.KnightAttacks[square]).CountBits()];
         }
         return score;
      }

      private static Score Bishops(Board board, Color color)
      {
         Bitboard us = new(board.PieceBB[(int)PieceType.Bishop].Value & board.ColorBB[(int)color].Value);
         Bitboard them = new(board.ColorBB[(int)color ^ 1].Value);
         Score score = new();
         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            score += BishopMobility[new Bitboard(Attacks.GetBishopAttacks(square, them.Value)).CountBits()];
         }
         return score;
      }

      private static Score Rooks(Board board, Color color)
      {
         Bitboard us = new(board.PieceBB[(int)PieceType.Rook].Value & board.ColorBB[(int)color].Value);
         Bitboard them = new(board.ColorBB[(int)color ^ 1].Value);
         Score score = new();
         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            score += RookMobility[new Bitboard(Attacks.GetRookAttacks(square, them.Value)).CountBits()];
         }
         return score;
      }
      private static Score Queens(Board board, Color color)
      {
         Bitboard us = new(board.PieceBB[(int)PieceType.Queen].Value & board.ColorBB[(int)color].Value);
         Bitboard them = new(board.ColorBB[(int)color ^ 1].Value);
         Score score = new();
         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            score += QueenMobility[new Bitboard(Attacks.GetQueenAttacks(square, them.Value)).CountBits()];
         }
         return score;
      }
   }
}
