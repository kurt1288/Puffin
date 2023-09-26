using System.Runtime.CompilerServices;

namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(153, 90),
         new Score(300, 279),
         new Score(325, 304),
         new Score(500, 458),
         new Score(900, 913),
         new Score(0, 0),
      };

      public static readonly Score[] PST =
      {
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(-3, 71),
         new Score(1, 83),
         new Score(44, 67),
         new Score(0, 70),
         new Score(0, 64),
         new Score(3, 57),
         new Score(52, 58),
         new Score(2, 63),
         new Score(-10, 45),
         new Score(-30, 55),
         new Score(30, 43),
         new Score(30, 33),
         new Score(43, 32),
         new Score(44, 39),
         new Score(47, 50),
         new Score(30, 37),
         new Score(30, 4),
         new Score(30, 6),
         new Score(30, -3),
         new Score(30, -7),
         new Score(30, 6),
         new Score(30, -5),
         new Score(-30, 8),
         new Score(30, 2),
         new Score(30, -14),
         new Score(-30, -5),
         new Score(30, -15),
         new Score(30, -6),
         new Score(30, -10),
         new Score(30, -15),
         new Score(-30, -3),
         new Score(-30, -18),
         new Score(-31, -17),
         new Score(-30, -5),
         new Score(-30, -16),
         new Score(-30, -15),
         new Score(-30, -9),
         new Score(-30, -17),
         new Score(-30, 3),
         new Score(30, -17),
         new Score(29, -14),
         new Score(30, -8),
         new Score(-30, -17),
         new Score(-30, -24),
         new Score(-30, -8),
         new Score(30, -5),
         new Score(30, 7),
         new Score(30, -20),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, -97),
         new Score(0, -38),
         new Score(0, -14),
         new Score(-3, -9),
         new Score(3, -2),
         new Score(0, -39),
         new Score(0, -10),
         new Score(-3, -51),
         new Score(10, -22),
         new Score(50, 1),
         new Score(37, 15),
         new Score(9, 24),
         new Score(20, -3),
         new Score(40, 27),
         new Score(19, 10),
         new Score(35, -17),
         new Score(-7, -2),
         new Score(31, 26),
         new Score(31, 39),
         new Score(31, 38),
         new Score(30, 44),
         new Score(54, 38),
         new Score(49, 26),
         new Score(16, -2),
         new Score(51, -3),
         new Score(-30, 12),
         new Score(30, 29),
         new Score(30, 48),
         new Score(30, 36),
         new Score(31, 43),
         new Score(30, 16),
         new Score(30, 20),
         new Score(-30, -17),
         new Score(-38, 7),
         new Score(30, 19),
         new Score(30, 19),
         new Score(30, 29),
         new Score(-30, 25),
         new Score(30, 7),
         new Score(-30, -7),
         new Score(-30, -35),
         new Score(-30, -12),
         new Score(30, 5),
         new Score(30, 12),
         new Score(30, 21),
         new Score(30, 5),
         new Score(30, 1),
         new Score(-30, -24),
         new Score(-30, -45),
         new Score(-30, -27),
         new Score(-30, -23),
         new Score(-30, -7),
         new Score(-30, -7),
         new Score(-30, -9),
         new Score(-30, -23),
         new Score(-30, -23),
         new Score(-39, -39),
         new Score(-30, -39),
         new Score(-30, -34),
         new Score(-30, -39),
         new Score(-30, -34),
         new Score(-30, -26),
         new Score(-30, -36),
         new Score(-30, -34),
         new Score(0, -12),
         new Score(0, -28),
         new Score(0, -10),
         new Score(0, -21),
         new Score(0, -23),
         new Score(-3, -21),
         new Score(-7, -22),
         new Score(0, -43),
         new Score(-31, -19),
         new Score(30, 6),
         new Score(30, 5),
         new Score(3, 3),
         new Score(46, 6),
         new Score(46, 4),
         new Score(36, 8),
         new Score(54, -26),
         new Score(42, 0),
         new Score(30, 10),
         new Score(22, 9),
         new Score(30, 20),
         new Score(36, 6),
         new Score(34, 21),
         new Score(40, 11),
         new Score(30, 21),
         new Score(-30, -3),
         new Score(-30, 11),
         new Score(-30, 16),
         new Score(30, 32),
         new Score(30, 26),
         new Score(30, 18),
         new Score(30, 9),
         new Score(-30, -1),
         new Score(-30, -5),
         new Score(-30, 3),
         new Score(30, 10),
         new Score(30, 21),
         new Score(30, 18),
         new Score(30, 9),
         new Score(-30, 4),
         new Score(30, -11),
         new Score(-30, 0),
         new Score(30, 0),
         new Score(-30, 7),
         new Score(30, 15),
         new Score(30, 11),
         new Score(-30, 3),
         new Score(30, 2),
         new Score(-30, -4),
         new Score(30, -9),
         new Score(-30, -5),
         new Score(30, 4),
         new Score(-30, -4),
         new Score(-30, 2),
         new Score(30, 1),
         new Score(-30, 10),
         new Score(30, -2),
         new Score(-30, -33),
         new Score(30, -4),
         new Score(-30, -23),
         new Score(-39, -23),
         new Score(-30, -26),
         new Score(-30, -22),
         new Score(-21, -13),
         new Score(-30, -25),
         new Score(35, 29),
         new Score(3, 24),
         new Score(0, 29),
         new Score(27, 28),
         new Score(0, 33),
         new Score(0, 33),
         new Score(3, 24),
         new Score(9, 28),
         new Score(41, 24),
         new Score(30, 29),
         new Score(4, 35),
         new Score(3, 37),
         new Score(16, 31),
         new Score(2, 32),
         new Score(23, 19),
         new Score(11, 23),
         new Score(-9, 13),
         new Score(18, 20),
         new Score(1, 24),
         new Score(4, 26),
         new Score(30, 24),
         new Score(-3, 20),
         new Score(34, 21),
         new Score(11, 20),
         new Score(30, 17),
         new Score(-30, 15),
         new Score(0, 20),
         new Score(-10, 18),
         new Score(-30, 13),
         new Score(16, 9),
         new Score(-43, 2),
         new Score(41, 2),
         new Score(30, 3),
         new Score(-42, 3),
         new Score(-56, 1),
         new Score(-39, 6),
         new Score(-12, -5),
         new Score(-30, -5),
         new Score(4, -5),
         new Score(-7, -8),
         new Score(-30, -17),
         new Score(-10, -11),
         new Score(-30, -4),
         new Score(-29, -3),
         new Score(-30, -4),
         new Score(-30, -6),
         new Score(-31, -7),
         new Score(-14, -18),
         new Score(-30, -21),
         new Score(-38, -12),
         new Score(30, 1),
         new Score(-29, 0),
         new Score(-30, -9),
         new Score(-30, -3),
         new Score(30, -10),
         new Score(-30, -21),
         new Score(-30, -14),
         new Score(-30, -9),
         new Score(30, 0),
         new Score(30, 3),
         new Score(30, 2),
         new Score(30, -3),
         new Score(-30, -4),
         new Score(-30, -25),
         new Score(4, -13),
         new Score(0, 3),
         new Score(0, 18),
         new Score(0, 29),
         new Score(-3, 25),
         new Score(0, 31),
         new Score(-2, 35),
         new Score(8, 23),
         new Score(-29, -12),
         new Score(-29, -11),
         new Score(-39, 5),
         new Score(-38, 23),
         new Score(11, 36),
         new Score(32, 30),
         new Score(12, 33),
         new Score(22, 50),
         new Score(-22, -10),
         new Score(-20, 5),
         new Score(0, 23),
         new Score(32, 22),
         new Score(39, 27),
         new Score(1, 43),
         new Score(31, 39),
         new Score(23, 37),
         new Score(38, -15),
         new Score(-28, -8),
         new Score(-48, -2),
         new Score(-28, 8),
         new Score(-24, 17),
         new Score(-36, 32),
         new Score(-28, 18),
         new Score(32, 21),
         new Score(-28, -10),
         new Score(-29, -1),
         new Score(-28, 0),
         new Score(-28, 12),
         new Score(-29, 8),
         new Score(-28, 4),
         new Score(32, 12),
         new Score(22, 2),
         new Score(-28, -7),
         new Score(-28, -5),
         new Score(-28, -3),
         new Score(-28, -2),
         new Score(-28, 0),
         new Score(-28, 6),
         new Score(32, 5),
         new Score(32, 0),
         new Score(-28, -22),
         new Score(-28, -8),
         new Score(32, -2),
         new Score(32, -1),
         new Score(32, -1),
         new Score(-28, -5),
         new Score(-28, -16),
         new Score(57, -20),
         new Score(-35, -26),
         new Score(-29, -30),
         new Score(-28, -25),
         new Score(-28, -10),
         new Score(-28, -17),
         new Score(-29, -22),
         new Score(-28, -42),
         new Score(-12, -34),
         new Score(0, -35),
         new Score(0, -23),
         new Score(0, -4),
         new Score(0, 22),
         new Score(0, 0),
         new Score(0, 4),
         new Score(0, 19),
         new Score(0, -55),
         new Score(0, -9),
         new Score(0, 14),
         new Score(0, 24),
         new Score(0, 31),
         new Score(0, 31),
         new Score(0, 32),
         new Score(0, 12),
         new Score(0, 12),
         new Score(0, 10),
         new Score(0, 35),
         new Score(0, 41),
         new Score(0, 35),
         new Score(0, 40),
         new Score(0, 39),
         new Score(0, 30),
         new Score(0, 1),
         new Score(0, 1),
         new Score(0, 28),
         new Score(0, 33),
         new Score(0, 35),
         new Score(0, 35),
         new Score(-9, 31),
         new Score(0, 27),
         new Score(0, 3),
         new Score(0, 0),
         new Score(0, 16),
         new Score(-3, 25),
         new Score(0, 28),
         new Score(-3, 26),
         new Score(-6, 22),
         new Score(-3, 2),
         new Score(0, -11),
         new Score(-7, -19),
         new Score(-25, 3),
         new Score(-8, 6),
         new Score(-9, 8),
         new Score(-12, 12),
         new Score(-25, 10),
         new Score(-30, 1),
         new Score(-39, -10),
         new Score(-23, 3),
         new Score(-58, -4),
         new Score(-30, 5),
         new Score(-34, 1),
         new Score(-30, 1),
         new Score(-29, 0),
         new Score(-29, 2),
         new Score(-29, -4),
         new Score(-41, -24),
         new Score(31, 14),
         new Score(30, 6),
         new Score(-30, -29),
         new Score(-30, -20),
         new Score(-30, -24),
         new Score(31, 4),
         new Score(-29, -10),
      };

      //public static readonly Score[] KnightMobility =
      //{
      //   new Score(-32, -43),
      //   new Score(-30, -42),
      //   new Score(-22, -32),
      //   new Score(-11, -17),
      //   new Score(21, -2),
      //   new Score(22, 8),
      //   new Score(19, 18),
      //   new Score(25, 18),
      //   new Score(28, 18),
      //};

      //public static readonly Score[] BishopMobility =
      //{
      //   new Score(-31, -37),
      //   new Score(-25, -31),
      //   new Score(-13, -16),
      //   new Score(-3, -11),
      //   new Score(-11, 3),
      //   new Score(21, 13),
      //   new Score(20, 24),
      //   new Score(27, 24),
      //   new Score(26, 24),
      //   new Score(29, 24),
      //   new Score(31, 24),
      //   new Score(30, 34),
      //   new Score(-8, 24),
      //   new Score(3, 39),
      //};

      //public static readonly Score[] RookMobility =
      //{
      //   new Score(-26, -36),
      //   new Score(-14, -21),
      //   new Score(-9, -26),
      //   new Score(-14, -21),
      //   new Score(15, -11),
      //   new Score(19, -6),
      //   new Score(25, 0),
      //   new Score(29, 0),
      //   new Score(32, 14),
      //   new Score(20, 19),
      //   new Score(24, 19),
      //   new Score(34, 24),
      //   new Score(27, 19),
      //   new Score(21, 29),
      //   new Score(13, 44),
      //};

      //public static readonly Score[] QueenMobility =
      //{
      //   new Score(-18, -24),
      //   new Score(-23, -34),
      //   new Score(-28, -40),
      //   new Score(-26, -29),
      //   new Score(-14, -24),
      //   new Score(-9, -14),
      //   new Score(14, -9),
      //   new Score(23, -4),
      //   new Score(26, 0),
      //   new Score(26, 5),
      //   new Score(26, 10),
      //   new Score(13, 10),
      //   new Score(28, 16),
      //   new Score(26, 16),
      //   new Score(-32, 26),
      //   new Score(-19, 16),
      //   new Score(-29, 26),
      //   new Score(-36, 16),
      //   new Score(-16, 26),
      //   new Score(3, 16),
      //   new Score(8, 26),
      //   new Score(6, 16),
      //   new Score(3, 31),
      //   new Score(0, 41),
      //   new Score(0, 16),
      //   new Score(0, 26),
      //   new Score(0, 41),
      //   new Score(0, 26),
      //};

      public static int Evaluate(Board board)
      {
         Score white = board.MaterialValue[(int)Color.White];
         Score black = board.MaterialValue[(int)Color.Black];
         //white += Knights(board, Color.White);
         //black += Knights(board, Color.Black);
         //white += Bishops(board, Color.White);
         //black += Bishops(board, Color.Black);
         //white += Rooks(board, Color.White);
         //black += Rooks(board, Color.Black);
         //white += Queens(board, Color.White);
         //black += Queens(board, Color.Black);
         Score total = white - black;

         if (board.SideToMove == Color.Black)
         {
            total *= -1;
         }

         return total.Eg + Math.Clamp(board.Phase, 0, 24) / 24 * (total.Mg - total.Eg);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Score GetPSTScore(Piece piece, int square)
      {
         if (piece.Color == Color.Black)
         {
            square ^= 56;
         }

         return PST[((int)piece.Type * 64) + square];
      }

      // Used for debugging and verification of lazy eval during board updates
      public static Score Material(Board board, Color color)
      {
         Bitboard us = new(board.ColorBB[(int)color].Value);
         Score score = new();
         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Mailbox[square];
            score += PieceValues[(int)piece.Type];
            score += GetPSTScore(piece, square);
         }
         return score;
      }

      //private static Score Knights(Board board, Color color)
      //{
      //   Bitboard knightsBB = new(board.PieceBB[(int)PieceType.Knight].Value & board.ColorBB[(int)color].Value);
      //   ulong us = board.ColorBB[(int)color].Value;
      //   Score score = new();
      //   while (!knightsBB.IsEmpty())
      //   {
      //      int square = knightsBB.GetLSB();
      //      knightsBB.ClearLSB();
      //      score += KnightMobility[new Bitboard(Attacks.KnightAttacks[square] & ~us).CountBits()];
      //   }
      //   return score;
      //}

      //private static Score Bishops(Board board, Color color)
      //{
      //   Bitboard bishopBB = new(board.PieceBB[(int)PieceType.Bishop].Value & board.ColorBB[(int)color].Value);
      //   ulong us = board.ColorBB[(int)color].Value;
      //   ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
      //   Score score = new();
      //   while (!bishopBB.IsEmpty())
      //   {
      //      int square = bishopBB.GetLSB();
      //      bishopBB.ClearLSB();
      //      score += BishopMobility[new Bitboard(Attacks.GetBishopAttacks(square, occupied) & ~us).CountBits()];
      //   }
      //   return score;
      //}

      //private static Score Rooks(Board board, Color color)
      //{
      //   Bitboard rookBB = new(board.PieceBB[(int)PieceType.Rook].Value & board.ColorBB[(int)color].Value);
      //   ulong us = board.ColorBB[(int)color].Value;
      //   ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
      //   Score score = new();
      //   while (!rookBB.IsEmpty())
      //   {
      //      int square = rookBB.GetLSB();
      //      rookBB.ClearLSB();
      //      score += RookMobility[new Bitboard(Attacks.GetRookAttacks(square, occupied) & ~us).CountBits()];
      //   }
      //   return score;
      //}
      //private static Score Queens(Board board, Color color)
      //{
      //   Bitboard queenBB = new(board.PieceBB[(int)PieceType.Queen].Value & board.ColorBB[(int)color].Value);
      //   ulong us = board.ColorBB[(int)color].Value;
      //   ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
      //   Score score = new();
      //   while (!queenBB.IsEmpty())
      //   {
      //      int square = queenBB.GetLSB();
      //      queenBB.ClearLSB();
      //      score += QueenMobility[new Bitboard(Attacks.GetQueenAttacks(square, occupied) & ~us).CountBits()];
      //   }
      //   return score;
      //}
   }
}
