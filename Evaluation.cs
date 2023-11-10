using System.Runtime.CompilerServices;

namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new Score(84, 99),
         new Score(294, 249),
         new Score(328, 250),
         new Score(422, 440),
         new Score(837, 849),
         new Score(0, 0),
      };

      public static readonly Score[] PST =
      {
         new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0),
         new Score( 35,  87), new Score( 33,  81), new Score( 20,  75), new Score( 29,  35), new Score( 25,  38), new Score( 23,  41), new Score( 12,  40), new Score( -8,  52),
         new Score( -1,  40), new Score(  6,  37), new Score( 17,  26), new Score( 21,  14), new Score( 17,  14), new Score( 21,  15), new Score( 25,  22), new Score( 14,  26),
         new Score(-13,   7), new Score(  5,  -6), new Score(  1, -13), new Score(  6, -24), new Score( 19, -26), new Score( 12, -24), new Score(  8,  -6), new Score( -2,  -6),
         new Score(-28, -10), new Score( -5, -12), new Score( -4, -23), new Score(  7, -28), new Score(  8, -28), new Score(  0, -24), new Score( -3, -15), new Score(-19, -18),
         new Score(-25, -13), new Score( -9, -13), new Score(-14, -25), new Score(-10, -16), new Score(  0, -18), new Score( -8, -21), new Score( 10, -17), new Score( -6, -24),
         new Score(-21, -14), new Score( -8, -14), new Score(-21, -20), new Score(-17, -10), new Score( -6, -13), new Score( -2, -21), new Score(  7, -17), new Score(-31, -19),
         new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0), new Score(  0,   0),

         new Score(-24, -32), new Score( -8,  -4), new Score( -1,  -4), new Score(-10,   2), new Score( -1,  -6), new Score(  2,  -6), new Score(-11,  -7), new Score( -3, -29),
         new Score(-16,  -3), new Score(-13,   0), new Score(  3,   3), new Score(  9,   0), new Score( -4,  -4), new Score( 16,   3), new Score(  3,  -3), new Score( -1,  -8),
         new Score(  2,   3), new Score(  9,  11), new Score( 24,  11), new Score( 17,  12), new Score( 31,   3), new Score( 16,   1), new Score( -1,   1), new Score( 12,  -8),
         new Score(-12,   7), new Score(  3,   4), new Score( 20,   7), new Score( 44,  12), new Score( 18,  16), new Score( 32,  10), new Score( -6,  10), new Score(  5,   6),
         new Score(-17,   3), new Score( -8,   1), new Score( 20,   7), new Score( 15,  11), new Score( 26,  16), new Score( 17,  15), new Score(  9,   1), new Score( -9,   3),
         new Score(-24,  -8), new Score(-15,   2), new Score(  5,  -3), new Score(  5,   3), new Score( 20,   3), new Score( 11,  -9), new Score( -1,   1), new Score(-13,  -3),
         new Score(-29,  -6), new Score(-11,  -5), new Score(-11,  -3), new Score( -3,   0), new Score( -1,   1), new Score( -6,  -4), new Score( -8,  -7), new Score( -9, -11),
         new Score(-18, -20), new Score(-24, -16), new Score(-21,  -7), new Score(-15,  -5), new Score( -8,   0), new Score(-16,  -8), new Score(-14, -10), new Score(-18, -11),

         new Score(  2,   0), new Score(  0,  -3), new Score( -9,  -1), new Score( -2,  -1), new Score( -9,  -3), new Score( -4,  -7), new Score( -4,   0), new Score( -1,  -9),
         new Score(-12, -11), new Score(  0,   0), new Score( -6,  -2), new Score( -7,   0), new Score( -3,   0), new Score(  5, -10), new Score( -5,   1), new Score(-11,  -1),
         new Score( -8,   5), new Score(  0,   5), new Score(  0,   7), new Score( 14,  -1), new Score(  6,   1), new Score( 14,   4), new Score(  0,   1), new Score(  2,   4),
         new Score(-12,  -3), new Score(  2,   4), new Score( -7,   7), new Score( 20,   7), new Score(  9,   2), new Score(  0,   7), new Score(  6,  -3), new Score(-17,   0),
         new Score( -8,  -6), new Score( -8,   1), new Score(  1,   6), new Score( 13,  11), new Score(  4,   9), new Score(  5,   1), new Score( -7,   2), new Score( -4,  -7),
         new Score(-12,   0), new Score(  2,   0), new Score( -1,   4), new Score(  7,  -1), new Score( 10,   1), new Score(  4,   0), new Score(  1,  -4), new Score(  4,  -8),
         new Score(  0,  -4), new Score(  1,  -8), new Score(  7,  -7), new Score( -8,   4), new Score(  1,   3), new Score(  9,  -2), new Score( 24,  -5), new Score(  6, -14),
         new Score(-11,  -2), new Score(  8, -10), new Score( -7, -11), new Score(-19,   0), new Score( -2,  -6), new Score( -9,   0), new Score(  4,  -3), new Score( -7, -14),

         new Score(  1,   5), new Score(-10,   3), new Score( -8,  11), new Score(  0,  12), new Score(  2,   6), new Score( -8,  12), new Score( 13,  -6), new Score(  9,   0),
         new Score(  0,   3), new Score(  3,   9), new Score(  3,  15), new Score( 17,  11), new Score(  2,   7), new Score(  9,  10), new Score(  9,   0), new Score( 10,   0),
         new Score( -3,   0), new Score(  5,   2), new Score(  4,   6), new Score(  9,  -2), new Score( 12,   2), new Score(  6,   1), new Score(  8,  -2), new Score( 20,  -9),
         new Score( -7,   2), new Score( -7,   3), new Score(  9,   3), new Score( -2,   3), new Score(  1,  -3), new Score( -1,  -2), new Score(  2,   1), new Score(  4, -10),
         new Score(-20,  10), new Score(-15,  -2), new Score( -9,   2), new Score( -5,   0), new Score(-12,   0), new Score(-13,   0), new Score( -9,  -4), new Score(-11,  -6),
         new Score(-15,  -3), new Score(-13,  -8), new Score(-12,  -8), new Score(-12,  -1), new Score( -8,   0), new Score( -7, -12), new Score(  3, -17), new Score(-12,  -8),
         new Score(-21,  -2), new Score(-10,  -1), new Score( -3,  -2), new Score(  1,  -3), new Score(  1,  -5), new Score(  1, -12), new Score(  8, -17), new Score( -9, -11),
         new Score(-10,  -4), new Score( -8,  -5), new Score(  8,  -4), new Score( 11,  -6), new Score( 13, -12), new Score( 11, -15), new Score( -1, -10), new Score(-17,  -3),

         new Score( -1,  -5), new Score( -3,   1), new Score(-14,   0), new Score(  0,  -2), new Score(  0,   1), new Score(  0,   5), new Score( 15,   0), new Score(  6,  -5),
         new Score(-17,   1), new Score(-13,  -6), new Score( -8,  13), new Score(  4,   0), new Score(  0,   0), new Score(  0,   6), new Score(  0,  -9), new Score( 12,   3),
         new Score(  4, -19), new Score(-10,  -2), new Score( -3,  20), new Score(  5,   7), new Score(  2,   7), new Score(  9,   4), new Score( 13, -14), new Score(  7, -15),
         new Score( -9,   0), new Score( -5,  -5), new Score( -9,   7), new Score( -9,  13), new Score( -3,   8), new Score(  4,  13), new Score(  1, -12), new Score(  4, -10),
         new Score(-17,  -2), new Score( -7,   6), new Score(-13,  12), new Score( -3,  14), new Score(-10,  13), new Score(  3,  -9), new Score(  3,  -7), new Score(  0,   1),
         new Score(-10,  -6), new Score( -5,   2), new Score(  0,   2), new Score( -3,   5), new Score( -4,  12), new Score(  0,   8), new Score(  5,  -1), new Score(  4, -16),
         new Score( -1,   1), new Score( -5,  -6), new Score(  1,  -5), new Score(  9,  -7), new Score(  6,  -2), new Score(  7,  -5), new Score(  5, -19), new Score(  8, -24),
         new Score( -8,   2), new Score(-17,   3), new Score( -4,  -4), new Score(  7,   2), new Score( -2,  -3), new Score(-14,   0), new Score( -3, -17), new Score( -7, -18),

         new Score( -2, -10), new Score(  1,  -3), new Score(  1, -19), new Score( -1,   3), new Score( -2,  -4), new Score( -6,   0), new Score(  3,   2), new Score( -7, -15),
         new Score( -6,  -6), new Score( -2,   1), new Score(  4,   5), new Score( -5,  11), new Score(  0,  14), new Score(  6,   7), new Score(  0,   6), new Score( -5,  -5),
         new Score( -7,  -3), new Score( 11,   8), new Score(  2,  19), new Score(  0,  19), new Score(  8,  17), new Score( 16,  16), new Score(  9,  15), new Score(  0,   2),
         new Score( -1,  -2), new Score(  2,   8), new Score(-17,  19), new Score( -7,  21), new Score(  7,  19), new Score(-12,  18), new Score( -4,  15), new Score(-20,   0),
         new Score( 13,  -6), new Score( -3,  -1), new Score( -6,  13), new Score( -5,  15), new Score(-24,  26), new Score(-15,  18), new Score(-13,   2), new Score( -8,  -7),
         new Score(-10, -17), new Score(  6,  -7), new Score( -9,   1), new Score(-12,   5), new Score(  2,   6), new Score(-20,   1), new Score( -2,  -7), new Score(-12, -12),
         new Score( 24, -11), new Score( 11,   2), new Score( 14, -10), new Score(-10,  -3), new Score(-14,  -2), new Score( -4,  -6), new Score( 18,   3), new Score( 10, -11),
         new Score(  7, -18), new Score( 25, -13), new Score(  5,   0), new Score(-25, -18), new Score(-10,  -7), new Score(-12, -23), new Score( 22, -21), new Score(  6, -34),
      };

      public static readonly Score[] KnightMobility =
      {
         new Score(-12, -1),
         new Score(-18, -4),
         new Score(-11, -12),
         new Score(-4, -17),
         new Score(8, -9),
         new Score(7, -3),
         new Score(8, -2),
         new Score(7, 6),
         new Score(12, 5),
      };

      public static readonly Score[] BishopMobility =
      {
         new Score(-24, -23),
         new Score(-20, -27),
         new Score(-13, -20),
         new Score(-6, -14),
         new Score(-2, -9),
         new Score(10, -3),
         new Score(12, 0),
         new Score(15, 3),
         new Score(14, 9),
         new Score(12, 12),
         new Score(14, 7),
         new Score(11, 11),
         new Score(6, -5),
         new Score(1, 7),
      };

      public static readonly Score[] RookMobility =
      {
         new Score(-27, -18),
         new Score(-19, -12),
         new Score(-16, -12),
         new Score(-10, -11),
         new Score(-10, -17),
         new Score(-3, -12),
         new Score(0, -13),
         new Score(5, -8),
         new Score(10, -1),
         new Score(11, 2),
         new Score(12, 7),
         new Score(19, 4),
         new Score(14, 9),
         new Score(20, 7),
         new Score(11, 12),
      };

      public static readonly Score[] QueenMobility =
      {
         new Score(-4, 0),
         new Score(-15, -9),
         new Score(-26, -12),
         new Score(-13, -7),
         new Score(-18, -4),
         new Score(-13, -11),
         new Score(-2, -16),
         new Score(-4, -11),
         new Score(-1, -16),
         new Score(0, -14),
         new Score(1, -11),
         new Score(4, -6),
         new Score(4, -4),
         new Score(0, -1),
         new Score(3, 0),
         new Score(4, 0),
         new Score(2, 6),
         new Score(5, 2),
         new Score(7, 3),
         new Score(0, 13),
         new Score(1, 7),
         new Score(15, 5),
         new Score(6, 3),
         new Score(0, 17),
         new Score(1, 6),
         new Score(0, -1),
         new Score(9, 9),
         new Score(1, -5),
      };

      public static Score[] KingAttackWeights =
      {
         new Score(0, 0),
         new Score(18, -8),
         new Score(20, -8),
         new Score(21, -7),
         new Score(14, 16),
      };

      public static Score[] PawnShield =
      {
         new Score(-19, -17),
         new Score(0, -12),
         new Score(28, -24),
         new Score(38, -33),
      };

      public static int Evaluate(Board board)
      {
         Score white = board.MaterialValue[(int)Color.White];
         Score black = board.MaterialValue[(int)Color.Black];

         ulong whiteKingZone = Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)];
         ulong blackKingZone = Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)];
         Score[] kingAttacks = { new(), new() };
         int[] kingAttacksCount = { 0, 0 };

         white += Knights(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Knights(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Bishops(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Bishops(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Rooks(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Rooks(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Queens(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Queens(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Kings(board, Color.White, ref kingAttacks, ref kingAttacksCount);
         black += Kings(board, Color.Black, ref kingAttacks, ref kingAttacksCount);

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

      private static Score Knights(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard knightsBB = new(board.PieceBB[(int)PieceType.Knight].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         Score score = new();

         while (!knightsBB.IsEmpty())
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += KnightMobility[new Bitboard(Attacks.KnightAttacks[square] & ~us).CountBits()];

            if ((Attacks.KnightAttacks[square] & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Knight] * new Bitboard(Attacks.KnightAttacks[square] & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Bishops(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard bishopBB = new(board.PieceBB[(int)PieceType.Bishop].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Score score = new();

         while (!bishopBB.IsEmpty())
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            ulong moves = Attacks.GetBishopAttacks(square, occupied);
            score += BishopMobility[new Bitboard(moves & ~us).CountBits()];

            if ((moves & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Rooks(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard rookBB = new(board.PieceBB[(int)PieceType.Rook].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Score score = new();

         while (!rookBB.IsEmpty())
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            ulong moves = Attacks.GetRookAttacks(square, occupied);
            score += RookMobility[new Bitboard(moves & ~us).CountBits()];

            if ((moves & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Queens(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard queenBB = new(board.PieceBB[(int)PieceType.Queen].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;
         Score score = new();

         while (!queenBB.IsEmpty())
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            ulong moves = Attacks.GetQueenAttacks(square, occupied);
            score += QueenMobility[new Bitboard(moves & ~us).CountBits()];

            if ((moves & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Kings(Board board, Color color, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Score score = new();
         Bitboard kingBB = new(board.PieceBB[(int)PieceType.King].Value & board.ColorBB[(int)color].Value);
         int kingSq = kingBB.GetLSB();
         ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

         if ((kingSquares & Constants.SquareBB[kingSq]) != 0)
         {
            ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x007000000000000 : 0x000E0000000000000) : (ulong)(kingSq % 8 < 3 ? 0x700 : 0xE000);

            Bitboard pawns = new(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & pawnSquares);
            score += PawnShield[Math.Min(pawns.CountBits(), 3)];
         }

         if (kingAttacksCount[(int)color ^ 1] >= 2)
         {
            score -= kingAttacks[(int)color ^ 1];
         }

         return score;
      }
   }
}
