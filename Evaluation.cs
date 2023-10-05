using System.Runtime.CompilerServices;

namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(94, 90),
         new Score(300, 270),
         new Score(325, 294),
         new Score(500, 444),
         new Score(900, 892),
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
         new Score(-1, 69),
         new Score(3, 77),
         new Score(0, 56),
         new Score(0, 49),
         new Score(0, 35),
         new Score(-1, 37),
         new Score(-1, 21),
         new Score(2, 38),
         new Score(-2, 34),
         new Score(0, 39),
         new Score(7, 35),
         new Score(6, 22),
         new Score(5, 20),
         new Score(5, 25),
         new Score(1, 35),
         new Score(3, 25),
         new Score(2, 1),
         new Score(4, 1),
         new Score(5, -6),
         new Score(17, -9),
         new Score(21, 0),
         new Score(6, -5),
         new Score(3, 3),
         new Score(2, -1),
         new Score(-1, -17),
         new Score(-5, -9),
         new Score(10, -17),
         new Score(15, -9),
         new Score(16, -10),
         new Score(0, -14),
         new Score(-9, -7),
         new Score(-7, -19),
         new Score(-15, -20),
         new Score(-14, -9),
         new Score(-11, -19),
         new Score(-21, -16),
         new Score(-15, -11),
         new Score(-5, -17),
         new Score(-11, -1),
         new Score(2, -19),
         new Score(3, -18),
         new Score(4, -9),
         new Score(-14, -18),
         new Score(-13, -29),
         new Score(-9, -15),
         new Score(6, -5),
         new Score(13, 3),
         new Score(0, -21),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, -37),
         new Score(0, -5),
         new Score(0, 4),
         new Score(0, -12),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, -3),
         new Score(0, -15),
         new Score(-1, -7),
         new Score(-1, -6),
         new Score(-5, 11),
         new Score(0, 0),
         new Score(0, 0),
         new Score(2, 8),
         new Score(-2, -11),
         new Score(2, -6),
         new Score(2, -1),
         new Score(1, 13),
         new Score(0, 23),
         new Score(9, 35),
         new Score(4, 32),
         new Score(0, 27),
         new Score(5, 7),
         new Score(3, -2),
         new Score(0, -3),
         new Score(1, 9),
         new Score(12, 27),
         new Score(12, 39),
         new Score(14, 32),
         new Score(5, 33),
         new Score(1, 15),
         new Score(5, 1),
         new Score(-8, -6),
         new Score(0, 0),
         new Score(1, 14),
         new Score(7, 17),
         new Score(4, 23),
         new Score(4, 17),
         new Score(3, 4),
         new Score(-4, -6),
         new Score(-8, -24),
         new Score(-5, -12),
         new Score(16, 3),
         new Score(0, 7),
         new Score(0, 15),
         new Score(14, 5),
         new Score(4, 3),
         new Score(-9, -16),
         new Score(-4, -36),
         new Score(-7, -13),
         new Score(-1, -14),
         new Score(-10, -6),
         new Score(-5, -7),
         new Score(-3, -10),
         new Score(-6, -15),
         new Score(-6, -18),
         new Score(-7, -31),
         new Score(-18, -31),
         new Score(-6, -22),
         new Score(-1, -21),
         new Score(-10, -18),
         new Score(-5, -17),
         new Score(-17, -25),
         new Score(1, -20),
         new Score(0, -9),
         new Score(0, -11),
         new Score(0, -3),
         new Score(0, -2),
         new Score(0, 4),
         new Score(0, -14),
         new Score(0, -1),
         new Score(0, -13),
         new Score(-1, -13),
         new Score(0, -1),
         new Score(0, -1),
         new Score(0, 2),
         new Score(0, 0),
         new Score(-1, 0),
         new Score(4, 7),
         new Score(2, -8),
         new Score(-2, -4),
         new Score(1, 6),
         new Score(0, 5),
         new Score(10, 9),
         new Score(-3, 4),
         new Score(0, 17),
         new Score(2, 14),
         new Score(0, 10),
         new Score(0, -7),
         new Score(0, 7),
         new Score(-2, 11),
         new Score(2, 21),
         new Score(2, 20),
         new Score(1, 14),
         new Score(7, 7),
         new Score(1, 2),
         new Score(-7, -7),
         new Score(-2, 0),
         new Score(4, 9),
         new Score(2, 15),
         new Score(3, 13),
         new Score(9, 5),
         new Score(0, 2),
         new Score(-3, -7),
         new Score(1, -3),
         new Score(6, 0),
         new Score(0, 2),
         new Score(16, 9),
         new Score(14, 9),
         new Score(-6, 5),
         new Score(3, 0),
         new Score(0, 0),
         new Score(0, -2),
         new Score(-10, -4),
         new Score(10, 2),
         new Score(-13, -5),
         new Score(-9, 0),
         new Score(5, 0),
         new Score(-5, 7),
         new Score(0, -3),
         new Score(1, -20),
         new Score(1, -5),
         new Score(-16, -21),
         new Score(-8, -16),
         new Score(-7, -14),
         new Score(-15, -17),
         new Score(0, -4),
         new Score(-1, -14),
         new Score(0, 12),
         new Score(0, 14),
         new Score(0, 12),
         new Score(0, 13),
         new Score(0, 14),
         new Score(0, 14),
         new Score(0, -1),
         new Score(0, 6),
         new Score(0, 14),
         new Score(5, 15),
         new Score(0, 20),
         new Score(0, 20),
         new Score(0, 21),
         new Score(0, 12),
         new Score(0, 8),
         new Score(1, 10),
         new Score(0, 10),
         new Score(4, 9),
         new Score(2, 13),
         new Score(0, 14),
         new Score(-1, 10),
         new Score(0, 8),
         new Score(0, 15),
         new Score(-2, 6),
         new Score(-2, 3),
         new Score(-2, 2),
         new Score(4, 12),
         new Score(1, 12),
         new Score(1, 7),
         new Score(-1, 1),
         new Score(0, 2),
         new Score(-1, 1),
         new Score(-1, 0),
         new Score(3, -3),
         new Score(-3, 0),
         new Score(-3, 3),
         new Score(-1, -4),
         new Score(0, -6),
         new Score(0, -3),
         new Score(2, -3),
         new Score(-2, -14),
         new Score(-2, -9),
         new Score(-2, -10),
         new Score(2, -9),
         new Score(-4, -10),
         new Score(-3, -14),
         new Score(0, -8),
         new Score(2, -13),
         new Score(-4, -20),
         new Score(0, -12),
         new Score(3, -5),
         new Score(-3, -7),
         new Score(-1, -11),
         new Score(-3, -10),
         new Score(0, -5),
         new Score(-3, -18),
         new Score(-9, -16),
         new Score(-2, -13),
         new Score(11, -4),
         new Score(11, 1),
         new Score(12, -1),
         new Score(13, -6),
         new Score(-2, -7),
         new Score(-14, -26),
         new Score(1, -1),
         new Score(0, -1),
         new Score(0, 3),
         new Score(0, 6),
         new Score(0, 11),
         new Score(0, 12),
         new Score(0, 3),
         new Score(0, 6),
         new Score(5, -12),
         new Score(-2, -9),
         new Score(0, 0),
         new Score(0, 5),
         new Score(0, 10),
         new Score(0, 16),
         new Score(0, 10),
         new Score(3, 16),
         new Score(6, -9),
         new Score(0, -2),
         new Score(0, 8),
         new Score(1, 8),
         new Score(0, 15),
         new Score(0, 29),
         new Score(1, 21),
         new Score(-2, 20),
         new Score(2, -7),
         new Score(-1, -3),
         new Score(-2, 6),
         new Score(-2, 5),
         new Score(-2, 11),
         new Score(1, 14),
         new Score(0, 8),
         new Score(1, 11),
         new Score(-6, -11),
         new Score(-1, -2),
         new Score(-2, -4),
         new Score(-4, 6),
         new Score(-2, 5),
         new Score(-2, 1),
         new Score(4, 7),
         new Score(3, 0),
         new Score(-3, -7),
         new Score(0, -7),
         new Score(-2, -2),
         new Score(-1, -5),
         new Score(-2, 0),
         new Score(-1, 2),
         new Score(3, 5),
         new Score(2, -3),
         new Score(4, -13),
         new Score(-6, -12),
         new Score(1, -4),
         new Score(9, -1),
         new Score(3, -7),
         new Score(2, -7),
         new Score(4, -5),
         new Score(3, -6),
         new Score(0, -7),
         new Score(-2, -16),
         new Score(-5, -20),
         new Score(0, -10),
         new Score(-5, -18),
         new Score(-4, -16),
         new Score(-1, -26),
         new Score(0, -5),
         new Score(0, -9),
         new Score(0, -6),
         new Score(0, -5),
         new Score(0, 2),
         new Score(0, -3),
         new Score(0, -2),
         new Score(0, 6),
         new Score(0, -6),
         new Score(0, -3),
         new Score(0, 2),
         new Score(0, 3),
         new Score(0, 12),
         new Score(0, 8),
         new Score(0, 12),
         new Score(0, 0),
         new Score(0, -2),
         new Score(0, 0),
         new Score(0, 3),
         new Score(0, 23),
         new Score(0, 17),
         new Score(0, 10),
         new Score(0, 22),
         new Score(0, 22),
         new Score(0, 3),
         new Score(0, -7),
         new Score(0, 9),
         new Score(0, 14),
         new Score(0, 25),
         new Score(0, 21),
         new Score(0, 15),
         new Score(0, 10),
         new Score(0, -2),
         new Score(0, 0),
         new Score(0, 5),
         new Score(0, 14),
         new Score(0, 21),
         new Score(0, 18),
         new Score(0, 11),
         new Score(0, 3),
         new Score(0, -5),
         new Score(0, -14),
         new Score(0, -1),
         new Score(0, 0),
         new Score(0, 3),
         new Score(0, 9),
         new Score(0, 0),
         new Score(2, -1),
         new Score(0, -13),
         new Score(0, 2),
         new Score(-1, -1),
         new Score(-2, -4),
         new Score(-5, -6),
         new Score(-10, -4),
         new Score(-5, -3),
         new Score(-2, 0),
         new Score(2, -8),
         new Score(0, -15),
         new Score(7, 6),
         new Score(7, 2),
         new Score(-4, -26),
         new Score(-16, -20),
         new Score(-9, -23),
         new Score(20, 0),
         new Score(3, -13),
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

         return total.Eg + board.Phase / 24 * (total.Mg - total.Eg);
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
