using System.Runtime.CompilerServices;

namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(58, 85),
         new Score(228, 198),
         new Score(252, 206),
         new Score(320, 367),
         new Score(687, 708),
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
         new Score(36, 72),
         new Score(39, 63),
         new Score(24, 69),
         new Score(38, 31),
         new Score(27, 29),
         new Score(18, 33),
         new Score(13, 34),
         new Score(1, 62),
         new Score(-7, 40),
         new Score(-10, 46),
         new Score(9, 32),
         new Score(19, 10),
         new Score(17, 12),
         new Score(23, 8),
         new Score(23, 26),
         new Score(10, 25),
         new Score(-10, 0),
         new Score(0, -5),
         new Score(0, -13),
         new Score(3, -20),
         new Score(14, -22),
         new Score(9, -20),
         new Score(11, -9),
         new Score(-1, -9),
         new Score(-23, -8),
         new Score(-9, -10),
         new Score(-7, -22),
         new Score(0, -23),
         new Score(1, -25),
         new Score(-4, -19),
         new Score(-2, -13),
         new Score(-11, -15),
         new Score(-23, -14),
         new Score(-11, -11),
         new Score(-12, -22),
         new Score(-12, -14),
         new Score(-4, -19),
         new Score(-9, -19),
         new Score(9, -15),
         new Score(-10, -18),
         new Score(-21, -12),
         new Score(-11, -10),
         new Score(-15, -19),
         new Score(-19, -21),
         new Score(-6, -16),
         new Score(3, -19),
         new Score(16, -17),
         new Score(-15, -16),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(0, 0),
         new Score(-49, -28),
         new Score(-2, 0),
         new Score(-3, 0),
         new Score(6, -2),
         new Score(-1, 0),
         new Score(0, -17),
         new Score(-1, -13),
         new Score(-11, -31),
         new Score(-8, -12),
         new Score(5, -2),
         new Score(3, 8),
         new Score(9, -1),
         new Score(5, -3),
         new Score(40, -11),
         new Score(-6, -8),
         new Score(-4, -9),
         new Score(-1, 0),
         new Score(15, 5),
         new Score(24, 10),
         new Score(25, 17),
         new Score(55, 3),
         new Score(41, 3),
         new Score(12, -4),
         new Score(2, -3),
         new Score(-1, 1),
         new Score(7, 12),
         new Score(20, 15),
         new Score(41, 10),
         new Score(21, 11),
         new Score(37, 11),
         new Score(10, 4),
         new Score(8, -5),
         new Score(-14, 8),
         new Score(-4, 0),
         new Score(11, 16),
         new Score(4, 17),
         new Score(15, 16),
         new Score(11, 13),
         new Score(-2, 6),
         new Score(-5, -8),
         new Score(-26, -6),
         new Score(-14, 0),
         new Score(1, 3),
         new Score(1, 10),
         new Score(10, 12),
         new Score(3, 0),
         new Score(0, 0),
         new Score(-15, 0),
         new Score(-18, -22),
         new Score(-10, -7),
         new Score(-9, -6),
         new Score(-7, 0),
         new Score(-7, 2),
         new Score(-6, 2),
         new Score(-11, -6),
         new Score(-14, -5),
         new Score(-44, -13),
         new Score(-24, -36),
         new Score(-38, -7),
         new Score(-28, -7),
         new Score(-19, -8),
         new Score(-20, -6),
         new Score(-24, -20),
         new Score(-20, -19),
         new Score(-18, 2),
         new Score(-4, -2),
         new Score(-12, 4),
         new Score(-21, -1),
         new Score(-10, -2),
         new Score(-6, -1),
         new Score(-6, -1),
         new Score(-11, -4),
         new Score(-8, -11),
         new Score(1, -3),
         new Score(-4, 0),
         new Score(-4, 1),
         new Score(11, -5),
         new Score(2, -2),
         new Score(-8, 2),
         new Score(-10, -10),
         new Score(-10, 1),
         new Score(9, 1),
         new Score(2, 4),
         new Score(22, -6),
         new Score(7, 5),
         new Score(35, 0),
         new Score(17, -3),
         new Score(12, -2),
         new Score(-5, -1),
         new Score(3, 9),
         new Score(8, 10),
         new Score(23, 8),
         new Score(18, 8),
         new Score(14, 9),
         new Score(4, 2),
         new Score(-5, -3),
         new Score(-9, -1),
         new Score(-3, 0),
         new Score(4, 4),
         new Score(16, 7),
         new Score(6, 6),
         new Score(3, 3),
         new Score(3, 2),
         new Score(-5, -13),
         new Score(0, -8),
         new Score(1, 0),
         new Score(-4, 9),
         new Score(5, 0),
         new Score(6, 4),
         new Score(3, -1),
         new Score(5, -2),
         new Score(4, -12),
         new Score(-3, -14),
         new Score(-5, -4),
         new Score(6, -6),
         new Score(-13, 4),
         new Score(-4, 2),
         new Score(4, -5),
         new Score(9, -5),
         new Score(7, -19),
         new Score(-15, -11),
         new Score(-4, -12),
         new Score(-19, -13),
         new Score(-23, -1),
         new Score(-18, -7),
         new Score(-16, -9),
         new Score(8, -9),
         new Score(-7, -12),
         new Score(11, 1),
         new Score(14, -2),
         new Score(14, 8),
         new Score(16, 8),
         new Score(4, 2),
         new Score(17, -2),
         new Score(3, 0),
         new Score(15, -1),
         new Score(4, 5),
         new Score(1, 7),
         new Score(19, 6),
         new Score(20, 9),
         new Score(9, 8),
         new Score(33, 0),
         new Score(22, -5),
         new Score(31, -9),
         new Score(3, -3),
         new Score(3, 1),
         new Score(5, 4),
         new Score(4, 4),
         new Score(15, -4),
         new Score(26, -5),
         new Score(33, -5),
         new Score(29, -8),
         new Score(-1, -2),
         new Score(-5, 4),
         new Score(2, 6),
         new Score(-3, 2),
         new Score(9, -2),
         new Score(1, -3),
         new Score(6, -9),
         new Score(8, -10),
         new Score(-19, -2),
         new Score(-15, 2),
         new Score(-6, 0),
         new Score(-3, 1),
         new Score(-13, -2),
         new Score(-24, 1),
         new Score(-4, -2),
         new Score(-6, -11),
         new Score(-23, -5),
         new Score(-14, -5),
         new Score(-17, -8),
         new Score(-17, -3),
         new Score(-15, 0),
         new Score(-12, -10),
         new Score(0, -19),
         new Score(-8, -16),
         new Score(-28, -3),
         new Score(-22, -1),
         new Score(-7, -7),
         new Score(-12, -1),
         new Score(-9, -13),
         new Score(-14, -9),
         new Score(-7, -9),
         new Score(-23, -1),
         new Score(-18, -6),
         new Score(-16, -4),
         new Score(-6, -3),
         new Score(-3, -1),
         new Score(-1, -7),
         new Score(-8, -11),
         new Score(-4, -3),
         new Score(-22, -5),
         new Score(1, -4),
         new Score(-7, 1),
         new Score(-18, 19),
         new Score(11, -1),
         new Score(-1, 2),
         new Score(11, 3),
         new Score(4, 8),
         new Score(5, -2),
         new Score(-7, -6),
         new Score(-15, -2),
         new Score(-2, 11),
         new Score(-4, 9),
         new Score(-3, 11),
         new Score(6, 12),
         new Score(3, -1),
         new Score(12, -7),
         new Score(-4, -17),
         new Score(-8, -3),
         new Score(3, 8),
         new Score(7, 7),
         new Score(9, 6),
         new Score(24, 14),
         new Score(19, -5),
         new Score(13, 0),
         new Score(-19, -2),
         new Score(-10, 1),
         new Score(-9, 17),
         new Score(-5, 13),
         new Score(-2, 14),
         new Score(8, 2),
         new Score(4, 5),
         new Score(2, 3),
         new Score(-13, 0),
         new Score(-12, 6),
         new Score(-5, -6),
         new Score(-9, 13),
         new Score(-4, 7),
         new Score(4, -13),
         new Score(3, -5),
         new Score(2, -8),
         new Score(-9, -7),
         new Score(-9, -4),
         new Score(-4, -8),
         new Score(-9, -8),
         new Score(-10, 4),
         new Score(-3, 0),
         new Score(-1, -4),
         new Score(2, -29),
         new Score(-6, -13),
         new Score(-9, -20),
         new Score(-5, -13),
         new Score(-3, -15),
         new Score(-8, -12),
         new Score(5, -43),
         new Score(0, -35),
         new Score(-11, -15),
         new Score(-17, -6),
         new Score(-21, -6),
         new Score(-17, -20),
         new Score(-9, -13),
         new Score(-16, -12),
         new Score(-16, -20),
         new Score(0, -20),
         new Score(-15, -5),
         new Score(-12, -13),
         new Score(-4, 1),
         new Score(-1, -12),
         new Score(13, 6),
         new Score(-11, -7),
         new Score(0, -2),
         new Score(3, -3),
         new Score(-6, -7),
         new Score(-22, -5),
         new Score(2, 3),
         new Score(-20, 6),
         new Score(-7, 13),
         new Score(-7, 16),
         new Score(8, 17),
         new Score(-2, 9),
         new Score(1, -2),
         new Score(1, 5),
         new Score(1, 10),
         new Score(-4, 11),
         new Score(-12, 17),
         new Score(3, 17),
         new Score(8, 20),
         new Score(11, 19),
         new Score(0, 4),
         new Score(-13, 0),
         new Score(2, 11),
         new Score(-25, 18),
         new Score(-8, 21),
         new Score(-10, 19),
         new Score(-18, 23),
         new Score(-16, 17),
         new Score(-19, 4),
         new Score(8, -9),
         new Score(-9, 7),
         new Score(-23, 13),
         new Score(-16, 18),
         new Score(-30, 20),
         new Score(-22, 19),
         new Score(-12, 8),
         new Score(-18, -2),
         new Score(0, -13),
         new Score(0, -5),
         new Score(-27, 5),
         new Score(-23, 11),
         new Score(-9, 10),
         new Score(-18, 6),
         new Score(0, 0),
         new Score(-15, -9),
         new Score(29, -20),
         new Score(14, -7),
         new Score(0, -2),
         new Score(-14, 1),
         new Score(-9, 2),
         new Score(-7, 0),
         new Score(17, -9),
         new Score(22, -20),
         new Score(23, -33),
         new Score(40, -27),
         new Score(23, -15),
         new Score(-20, -13),
         new Score(3, -23),
         new Score(-18, -13),
         new Score(33, -29),
         new Score(27, -42),
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

         return ((total.Mg * board.Phase) + (total.Eg * (24 - board.Phase))) / 24;
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
