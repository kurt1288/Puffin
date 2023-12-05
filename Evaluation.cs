using System.Runtime.CompilerServices;

namespace Puffin
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new(76, 148),
         new(287, 337),
         new(318, 349),
         new(405, 612),
         new(831, 1127),
         new(0, 0),
      };

      public static readonly Score[] PST =
      {
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 53, 150), new( 80, 141), new( 61, 141), new( 92,  89), new( 74,  87), new( 60,  98), new(-19, 147), new(-50, 160),
         new(-18,  85), new(  0,  94), new( 29,  60), new( 35,  38), new( 39,  28), new( 61,  14), new( 38,  61), new( -5,  59),
         new(-27,  12), new( -2,   1), new( -1, -17), new(  3, -27), new( 21, -36), new( 16, -33), new( 16, -13), new( -7, -13),
         new(-35, -13), new( -9, -16), new( -8, -34), new(  5, -36), new(  9, -39), new(  3, -39), new(  6, -26), new(-16, -33),
         new(-33, -20), new(-12, -18), new(-12, -35), new( -6, -25), new(  4, -31), new( -2, -35), new( 18, -29), new( -4, -40),
         new(-34, -16), new(-12, -14), new(-21, -27), new(-16, -18), new( -5, -17), new(-15, -20), new(  5, -21), new(-42, -28),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-157, -70), new(-124,  -8), new(-68,   5), new(-41,   0), new(-10,   3), new(-64, -21), new(-92,  -4), new(-106, -91),
         new(-28, -14), new( -9,   5), new( 15,   6), new( 32,   8), new( 13,   1), new( 69, -11), new( -7,   0), new(  7, -28),
         new(-10,   0), new( 26,   7), new( 45,  25), new( 46,  29), new( 74,  18), new( 86,   9), new( 33,   3), new(  9,  -7),
         new( -8,   8), new(  5,  22), new( 30,  35), new( 52,  37), new( 20,  44), new( 49,  37), new(  0,  30), new( 15,   6),
         new(-19,   8), new( -3,  13), new( 15,  35), new( 17,  36), new( 28,  37), new( 19,  28), new( 14,  16), new( -8,   1),
         new(-35,  -8), new(-14,   5), new(  2,  12), new( 10,  29), new( 22,  26), new(  8,   8), new(  4,   1), new(-16,  -5),
         new(-41, -16), new(-32,  -1), new(-18,   4), new( -1,   6), new( -1,   5), new(  2,  -1), new(-15,  -9), new(-14,  -6),
         new(-82, -22), new(-26, -27), new(-38,  -7), new(-20,  -4), new(-18,  -2), new(-10, -12), new(-21, -21), new(-51, -35),

         new(-29,   2), new(-56,  11), new(-52,   6), new(-97,  19), new(-91,  14), new(-72,   5), new(-36,  -2), new(-62,   1),
         new(-28,  -8), new(  1,  -3), new( -8,   0), new(-25,   1), new(-10,  -3), new(-11,  -5), new(-22,   6), new(-25,  -9),
         new(-12,  13), new(  9,   3), new(  7,   6), new( 15,  -1), new(  0,   4), new( 27,   8), new( 12,   2), new( -5,  13),
         new(-17,   6), new( -4,  11), new(  3,   8), new( 12,  20), new( 11,  12), new(  4,  12), new( -2,   6), new(-24,   8),
         new(-15,   0), new(-16,  11), new(-10,  15), new( 13,  11), new(  8,  11), new( -1,   8), new(-11,   6), new( -4,  -7),
         new(-12,   3), new( -2,   8), new(  3,   8), new(  0,   8), new(  6,  11), new(  5,   7), new(  3,   3), new(  7,  -8),
         new( -3,   1), new(  6,  -4), new(  8, -11), new( -5,   5), new(  3,   2), new( 16,  -5), new( 25,  -2), new(  3, -12),
         new(-11,  -6), new(  4,   3), new( -2,  -4), new(-13,   2), new( -5,   3), new( -8,   8), new(  5, -10), new(  3, -21),

         new(  0,  23), new(-15,  32), new(-18,  42), new(-19,  38), new( -7,  32), new( 14,  23), new( 15,  21), new( 29,  17),
         new(-13,  24), new(-18,  37), new( -1,  41), new( 15,  31), new( -1,  33), new( 20,  22), new( 16,  17), new( 46,   3),
         new(-22,  22), new(  0,  24), new( -3,  25), new( -1,  22), new( 27,  10), new( 18,   8), new( 59,   1), new( 29,   0),
         new(-26,  24), new(-17,  22), new(-15,  29), new( -9,  24), new( -7,  11), new( -6,   7), new( -2,   8), new(  0,   3),
         new(-37,  17), new(-40,  22), new(-29,  22), new(-20,  20), new(-18,  16), new(-36,  16), new(-17,   6), new(-23,   1),
         new(-38,  14), new(-35,  13), new(-24,  12), new(-21,  15), new(-14,  10), new(-19,   1), new(  2, -14), new(-13, -12),
         new(-38,   9), new(-29,  11), new(-12,  11), new(-12,  13), new( -7,   5), new( -4,  -2), new(  8, -11), new(-21,  -4),
         new(-16,   9), new(-17,  11), new( -5,  14), new(  0,  10), new(  4,   5), new( -1,   5), new(  1,  -3), new(-12,  -2),

         new(-45,  20), new(-52,  32), new(-32,  47), new( -4,  27), new(-11,  29), new(-13,  36), new( 33, -15), new(-16,  17),
         new(-15,   2), new(-30,  25), new(-33,  58), new(-42,  73), new(-52,  91), new(-10,  47), new(-18,  38), new( 31,  23),
         new( -4,  11), new(-11,  21), new(-12,  52), new(-12,  54), new( -5,  59), new( 15,  41), new( 28,  15), new( 14,  15),
         new(-20,  26), new(-13,  34), new(-19,  42), new(-24,  61), new(-18,  60), new(-10,  50), new( -1,  47), new( -3,  34),
         new(-11,  18), new(-21,  39), new(-18,  39), new( -8,  48), new( -9,  48), new(-10,  40), new( -2,  32), new(  0,  25),
         new(-15,   9), new( -6,  19), new( -4,  27), new( -8,  29), new( -2,  37), new(  0,  28), new(  7,  14), new(  0,   9),
         new( -7,   1), new( -4,   4), new(  5,   3), new(  8,  14), new(  7,  18), new( 18, -16), new( 21, -37), new( 29, -62),
         new( -8,  -1), new(-11,   0), new(  0,   4), new(  9,  18), new(  6,   0), new( -7,  -5), new(  3, -22), new(  2, -32),

         new(  6, -99), new( 13, -52), new( 36, -40), new(-52,  -1), new(-20, -15), new(  1,  -9), new( 22, -13), new( 34, -105),
         new(-70, -12), new(-13,  14), new(-55,  27), new( 46,  10), new( -5,  32), new(  0,  43), new( 23,  33), new(-16,   3),
         new(-101,   5), new( 18,  22), new(-51,  42), new(-65,  52), new(-27,  52), new( 43,  45), new( 15,  43), new(-28,  14),
         new(-74,  -4), new(-66,  27), new(-84,  46), new(-124,  58), new(-113,  58), new(-80,  53), new(-84,  45), new(-128,  21),
         new(-72, -16), new(-60,  12), new(-84,  34), new(-116,  50), new(-113,  49), new(-76,  36), new(-85,  25), new(-132,  12),
         new(-32, -25), new(  0,  -5), new(-48,  15), new(-59,  26), new(-51,  25), new(-51,  17), new(-16,   0), new(-49, -10),
         new( 53, -27), new( 25,  -1), new( 10,  -8), new(-15,   0), new(-12,   2), new( -5,  -4), new( 35,  -1), new( 34, -19),
         new( 20, -56), new( 42, -30), new( 17,  -7), new(-35, -26), new(  0, -17), new(  0, -32), new( 40, -27), new( 37, -61),
      };

      public static readonly Score[] KnightMobility =
      {
         new(-25, 7),
         new(-10, 7),
         new(-3, 11),
         new(0, 7),
         new(2, 11),
         new(1, 17),
         new(0, 18),
         new(0, 20),
         new(1, 16),
      };

      public static readonly Score[] BishopMobility =
      {
         new(-25, -45),
         new(-18, -27),
         new(-10, -20),
         new(-8, -9),
         new(-2, 3),
         new(3, 17),
         new(7, 21),
         new(10, 28),
         new(9, 36),
         new(10, 35),
         new(10, 35),
         new(13, 35),
         new(11, 40),
         new(33, 25),
      };

      public static readonly Score[] RookMobility =
      {
         new(-32, -11),
         new(-23, 4),
         new(-20, 3),
         new(-15, 6),
         new(-16, 12),
         new(-10, 13),
         new(-7, 14),
         new(-2, 16),
         new(0, 25),
         new(4, 28),
         new(8, 29),
         new(13, 31),
         new(12, 37),
         new(13, 43),
         new(14, 40),
      };

      public static readonly Score[] QueenMobility =
      {
         new(-8, -16),
         new(-11, -79),
         new(-18, -24),
         new(-16, -9),
         new(-15, -4),
         new(-12, -1),
         new(-9, 3),
         new(-11, 23),
         new(-9, 29),
         new(-8, 33),
         new(-8, 42),
         new(-9, 49),
         new(-9, 55),
         new(-9, 60),
         new(-8, 63),
         new(-8, 67),
         new(-5, 70),
         new(-9, 79),
         new(-5, 82),
         new(-4, 81),
         new(6, 75),
         new(9, 76),
         new(11, 74),
         new(16, 72),
         new(38, 51),
         new(91, 30),
         new(61, 46),
         new(61, 57),
      };

      public static Score[] KingAttackWeights =
      {
         new(0, 0),
         new(13, -7),
         new(20, -6),
         new(23, -6),
         new(15, 11),
      };

      public static Score[] PawnShield =
      {
         new(-30, -14),
         new(9, -20),
         new(47, -39),
         new(76, -66),
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

         return (total.Mg * board.Phase + total.Eg * (24 - board.Phase)) / 24;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static Score GetPSTScore(Piece piece, int square)
      {
         if (piece.Color == Color.Black)
         {
            square ^= 56;
         }

         return PST[(int)piece.Type * 64 + square];
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
