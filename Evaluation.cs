namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(111, 91),
         new Score(292, 293),
         new Score(332, 317),
         new Score(500, 480),
         new Score(900, 953),
         new Score(10000, 10000),
      };

      public static readonly Score[] KnightMobility =
      {
         new Score(-32, -43),
         new Score(-30, -42),
         new Score(-22, -32),
         new Score(-11, -17),
         new Score(21, -2),
         new Score(22, 8),
         new Score(19, 18),
         new Score(25, 18),
         new Score(28, 18),
      };

      public static readonly Score[] BishopMobility =
      {
         new Score(-31, -37),
         new Score(-25, -31),
         new Score(-13, -16),
         new Score(-3, -11),
         new Score(-11, 3),
         new Score(21, 13),
         new Score(20, 24),
         new Score(27, 24),
         new Score(26, 24),
         new Score(29, 24),
         new Score(31, 24),
         new Score(30, 34),
         new Score(-8, 24),
         new Score(3, 39),
      };

      public static readonly Score[] RookMobility =
      {
         new Score(-26, -36),
         new Score(-14, -21),
         new Score(-9, -26),
         new Score(-14, -21),
         new Score(15, -11),
         new Score(19, -6),
         new Score(25, 0),
         new Score(29, 0),
         new Score(32, 14),
         new Score(20, 19),
         new Score(24, 19),
         new Score(34, 24),
         new Score(27, 19),
         new Score(21, 29),
         new Score(13, 44),
      };

      public static readonly Score[] QueenMobility =
      {
         new Score(-18, -24),
         new Score(-23, -34),
         new Score(-28, -40),
         new Score(-26, -29),
         new Score(-14, -24),
         new Score(-9, -14),
         new Score(14, -9),
         new Score(23, -4),
         new Score(26, 0),
         new Score(26, 5),
         new Score(26, 10),
         new Score(13, 10),
         new Score(28, 16),
         new Score(26, 16),
         new Score(-32, 26),
         new Score(-19, 16),
         new Score(-29, 26),
         new Score(-36, 16),
         new Score(-16, 26),
         new Score(3, 16),
         new Score(8, 26),
         new Score(6, 16),
         new Score(3, 31),
         new Score(0, 41),
         new Score(0, 16),
         new Score(0, 26),
         new Score(0, 41),
         new Score(0, 26),
      };

      public static int Evaluate(Board board)
      {
         Score white = board.MaterialValue[(int)Color.White];
         Score black = board.MaterialValue[(int)Color.Black];
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

      private static Score Knights(Board board, Color color)
      {
         Bitboard knightsBB = new(board.PieceBB[(int)PieceType.Knight].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         Score score = new();
         while (!knightsBB.IsEmpty())
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += KnightMobility[new Bitboard(Attacks.KnightAttacks[square] & ~us).CountBits()];
         }
         return score;
      }

      private static Score Bishops(Board board, Color color)
      {
         Bitboard bishopBB = new(board.PieceBB[(int)PieceType.Bishop].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!bishopBB.IsEmpty())
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            score += BishopMobility[new Bitboard(Attacks.GetBishopAttacks(square, occupied) & ~us).CountBits()];
         }
         return score;
      }

      private static Score Rooks(Board board, Color color)
      {
         Bitboard rookBB = new(board.PieceBB[(int)PieceType.Rook].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!rookBB.IsEmpty())
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            score += RookMobility[new Bitboard(Attacks.GetRookAttacks(square, occupied) & ~us).CountBits()];
         }
         return score;
      }
      private static Score Queens(Board board, Color color)
      {
         Bitboard queenBB = new(board.PieceBB[(int)PieceType.Queen].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!queenBB.IsEmpty())
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            score += QueenMobility[new Bitboard(Attacks.GetQueenAttacks(square, occupied) & ~us).CountBits()];
         }
         return score;
      }
   }
}
