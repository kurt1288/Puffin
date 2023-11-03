using System.Runtime.CompilerServices;

namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(82, 98),
         new Score(284, 239),
         new Score(316, 241),
         new Score(404, 426),
         new Score(819, 852),
         new Score(0, 0),
      };

      public static readonly Score[] PST =
      {
         new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0),
         new Score( 21,  88), new Score( 29,  78), new Score(  4,  84), new Score( 16,  44), new Score( 21,  37), new Score( 22,  33), new Score( -2,  32), new Score(  3,  52),
         new Score( -5,  42), new Score(  5,  43), new Score( 13,  24), new Score( 24,  10), new Score( 17,  10), new Score( 15,  13), new Score( 17,  28), new Score(  6,  30),
         new Score(-13,   0), new Score(  0,  -7), new Score( -5, -13), new Score(  1, -20), new Score( 19, -26), new Score(  4, -22), new Score(  7,  -5), new Score(  0,  -7),
         new Score(-25,  -7), new Score(-10, -11), new Score( -4, -25), new Score(  3, -28), new Score(  7, -29), new Score( -1, -21), new Score( -4, -12), new Score(-14, -18),
         new Score(-23, -14), new Score(-12, -14), new Score(-13, -25), new Score(-12, -17), new Score(  0, -22), new Score( -7, -22), new Score(  9, -15), new Score( -7, -21),
         new Score(-20, -14), new Score(-10, -14), new Score(-17, -21), new Score(-20, -10), new Score( -4, -13), new Score( 11, -21), new Score( 20, -18), new Score(-15, -18),
         new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0),

         new Score(-45, -24), new Score(  4,  -7), new Score(-14,   6), new Score(  4,  -2), new Score( -9,  10), new Score(  4,  -2), new Score( -2,  -6), new Score(-22, -28),
         new Score(-20,  -1), new Score( -3,   1), new Score(  8,   5), new Score( -3,  -1), new Score(  0, -11), new Score( 13,   2), new Score(  4,  -5), new Score( -4,  -9),
         new Score( -6,   0), new Score( 13,   3), new Score( 25,   7), new Score( 15,  10), new Score( 34,   2), new Score( 40,  -3), new Score( 27,  -1), new Score(  4,  -1),
         new Score(-10,  -2), new Score(  7,  10), new Score( 17,  12), new Score( 41,   9), new Score( 24,  13), new Score( 34,  11), new Score(  7,   8), new Score( 12,   0),
         new Score(-15,   0), new Score(  0,   2), new Score( 13,  15), new Score( 13,  11), new Score( 21,  12), new Score( 11,  13), new Score(  8,  -2), new Score(-10,  -1),
         new Score(-28,  -6), new Score(-17,   1), new Score(  1,   0), new Score( 10,   3), new Score( 21,   5), new Score( 10,  -6), new Score(  1,   0), new Score(-13,   2),
         new Score(-30,   2), new Score(-14, -13), new Score(-16,   0), new Score( -2,  -3), new Score( -3,   3), new Score( -3,  -9), new Score( -5, -16), new Score(-13,   0),
         new Score(-31, -13), new Score(-20, -15), new Score(-24,  -4), new Score(-14, -17), new Score(-18,  -2), new Score(-11,  -6), new Score(-18, -10), new Score(-15, -23),

         new Score(-12, -10), new Score( -7,  -8), new Score(-13,  -3), new Score( -8,   3), new Score( -2,   3), new Score(-10,  -3), new Score(  3,  -3), new Score(-10,  -8),
         new Score(-14,  -8), new Score( -6,   1), new Score(-13,   1), new Score(-15,   0), new Score(  5,  -4), new Score(  0,  -9), new Score(  5,   0), new Score( -2,  -4),
         new Score(  0,   1), new Score(  1,   4), new Score( -4,   1), new Score( 20,  -6), new Score(  2,   2), new Score( 16,   2), new Score( 14,  -2), new Score(  8,   1),
         new Score(-14,   0), new Score( -4,   7), new Score(  8,   1), new Score( 15,   7), new Score(  8,   7), new Score( 12,   4), new Score( -3,   5), new Score( -7,   1),
         new Score(-15,  -4), new Score( -9,   5), new Score( -2,   0), new Score(  7,   2), new Score(  8,   0), new Score(  4,  -2), new Score( -7,   2), new Score( -3,  -8),
         new Score( -8,  -3), new Score(  2,  -3), new Score(  0,   6), new Score(  3,   3), new Score(  8,   2), new Score(  0,  -1), new Score(  5,  -6), new Score( -1,  -4),
         new Score( -4,  -4), new Score(  3,  -5), new Score(  8,  -4), new Score(-10,  -1), new Score(  0,   2), new Score(  8,  -4), new Score( 20,  -3), new Score(  3,  -8),
         new Score( -4,  -5), new Score(  9,  -6), new Score( -6,  -3), new Score( -9,   0), new Score(  0,  -5), new Score(-10,   2), new Score(  0,  -4), new Score( -2, -14),

         new Score(  3,   2), new Score( -9,   7), new Score(  2,   1), new Score(  9,  12), new Score(  9,   4), new Score( -3,   2), new Score( -3,  -4), new Score( 15,  -1),
         new Score( -6,   7), new Score(  4,   8), new Score( 12,  10), new Score(  7,  10), new Score( 13,   9), new Score( 14,  11), new Score( 14,  -4), new Score( 27,  -3),
         new Score( -7,   1), new Score(  3,   0), new Score(  0,   2), new Score( -3,   0), new Score( 11,   0), new Score(  5,   3), new Score( 19,  -3), new Score( 10,  -5),
         new Score( -8,  -3), new Score( -6,   3), new Score(  2,   6), new Score( -3,   3), new Score(  5,  -3), new Score( -2,  -3), new Score(  3,  -5), new Score(  3, -15),
         new Score(-13,   5), new Score(-11,   1), new Score(-13,   7), new Score(  0,  -1), new Score( -7,  -5), new Score(-22,   6), new Score( -2,  -8), new Score( -9,  -1),
         new Score(-12,  -5), new Score(-13,  -2), new Score(-17,  -4), new Score( -5,  -3), new Score( -8,   0), new Score( -2, -12), new Score(  3, -14), new Score(-12,  -7),
         new Score(-21,  -5), new Score(-18,   2), new Score( -2,  -2), new Score( -7,   0), new Score( -1,  -6), new Score(  0, -10), new Score( -3, -10), new Score(-16, -11),
         new Score(-12,  -4), new Score( -7,  -8), new Score(  2,  -2), new Score(  9,  -6), new Score( 12, -13), new Score( 10, -14), new Score( -1,  -7), new Score(-18,  -7),

         new Score( -4,  -1), new Score(-11,   1), new Score( -7,   6), new Score( 12,   0), new Score(  6,   7), new Score(  0,  10), new Score(  4,  -5), new Score(  0,   8),
         new Score(-11,   3), new Score(-22,   2), new Score( -3,   1), new Score( -6,   6), new Score( -7,   4), new Score(  6,   1), new Score( -3,   5), new Score( 17,  -1),
         new Score(-12,   6), new Score( -9,   4), new Score( -7,   9), new Score( 15,  -3), new Score(  1,   6), new Score( 10,   8), new Score( 19,  -2), new Score( 26,  -5),
         new Score(-15,  -2), new Score(-12,  -4), new Score(-12,   6), new Score(-11,  12), new Score(  4,   4), new Score( 10,   1), new Score(  6,  11), new Score(  5,  12),
         new Score(-20,   6), new Score(-22,   2), new Score(-14,  -2), new Score( -1,   6), new Score( -7,   8), new Score( -5,  16), new Score(  5,  -7), new Score(  8,   0),
         new Score(-10,  -5), new Score( -8,  -4), new Score( -1,  -4), new Score( -6,   1), new Score( -5,   0), new Score( -2,   5), new Score(  7,  -6), new Score(  7, -11),
         new Score(-11,  -7), new Score( -5, -13), new Score(  0, -10), new Score(  7, -17), new Score(  3, -13), new Score(  5, -12), new Score( 13, -36), new Score(  9, -11),
         new Score(-13,   1), new Score(-10,  -8), new Score( -5, -11), new Score(  4,  -6), new Score( -2, -16), new Score(-11, -11), new Score(  5,   8), new Score(-11,  -2),

         new Score( -9, -15), new Score(  5,  -1), new Score(  1,   5), new Score(  0,   3), new Score( -9,  -5), new Score( -2,   0), new Score(  0,   3), new Score( -3, -10),
         new Score(-10,   0), new Score( -3,   1), new Score( -8,   8), new Score( -8,  12), new Score( -4,  14), new Score(  0,  14), new Score( -5,   7), new Score( 10,   0),
         new Score(  9,  -3), new Score(-10,  10), new Score( -1,  10), new Score( -7,  17), new Score(  6,  18), new Score(  3,  20), new Score(  4,  16), new Score(  3,   8),
         new Score(-10,  -1), new Score(  1,   9), new Score(-21,  22), new Score(-10,  20), new Score(-13,  23), new Score( -1,  20), new Score( -6,  17), new Score( -6,   1),
         new Score(  8,  -6), new Score( -8,   3), new Score(-12,  16), new Score(-18,  20), new Score(-21,  23), new Score(-17,  20), new Score(-19,   7), new Score(-21,  -3),
         new Score( -1, -23), new Score( -4,  -4), new Score(-15,   6), new Score(-20,   9), new Score( -7,   9), new Score(-29,   5), new Score( -4,   1), new Score(-18,  -4),
         new Score( 30, -23), new Score( 13,  -8), new Score(  3,  -1), new Score(-15,   1), new Score(-14,   2), new Score( -8,  -1), new Score( 12, -10), new Score( 11, -17),
         new Score(  2, -35), new Score( 46, -32), new Score( 28, -14), new Score(-31, -10), new Score(  0, -20), new Score(-21, -15), new Score( 32, -32), new Score( 19, -46),
      };

      public static readonly Score[] KnightMobility =
      {
         new Score(-14, -12),
         new Score(-14, -8),
         new Score(-13, -6),
         new Score(-4, -14),
         new Score(5, -8),
         new Score(6, -6),
         new Score(4, 0),
         new Score(7, 2),
         new Score(13, 3),
      };

      public static readonly Score[] BishopMobility =
      {
         new Score(-26, -33),
         new Score(-22, -15),
         new Score(-16, -20),
         new Score(-8, -19),
         new Score(0, -10),
         new Score(7, -5),
         new Score(12, -3),
         new Score(16, 0),
         new Score(16, 7),
         new Score(16, 6),
         new Score(15, 7),
         new Score(6, 13),
         new Score(4, 8),
         new Score(1, 7),
      };

      public static readonly Score[] RookMobility =
      {
         new Score(-28, -25),
         new Score(-21, -14),
         new Score(-17, -14),
         new Score(-10, -14),
         new Score(-11, -14),
         new Score(-5, -13),
         new Score(-2, -11),
         new Score(1, -5),
         new Score(9, -2),
         new Score(10, 2),
         new Score(15, 4),
         new Score(18, 3),
         new Score(22, 7),
         new Score(16, 10),
         new Score(10, 12),
      };

      public static readonly Score[] QueenMobility =
      {
         new Score(-6, -4),
         new Score(-14, 4),
         new Score(-24, -13),
         new Score(-23, -7),
         new Score(-18, -3),
         new Score(-13, -19),
         new Score(-3, -28),
         new Score(-6, -16),
         new Score(-4, -13),
         new Score(-3, -13),
         new Score(1, -15),
         new Score(3, -9),
         new Score(2, -8),
         new Score(2, -3),
         new Score(1, -1),
         new Score(4, 6),
         new Score(8, 0),
         new Score(0, 10),
         new Score(5, 11),
         new Score(9, 10),
         new Score(5, 11),
         new Score(4, 12),
         new Score(-4, 13),
         new Score(10, 13),
         new Score(8, 4),
         new Score(10, 0),
         new Score(2, 4),
         new Score(2, 6),
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
