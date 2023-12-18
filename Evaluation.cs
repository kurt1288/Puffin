using System.Runtime.CompilerServices;

namespace Puffin
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new(80, 119),
         new(285, 343),
         new(317, 355),
         new(402, 617),
         new(824, 1141),
         new(0, 0),
      };

      public static readonly Score[] PST =
      {
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 18,  96), new( 44,  84), new( 28,  82), new( 60,  27), new( 38,  28), new( 24,  42), new(-52,  88), new(-83, 104),
         new( -8,  34), new(  9,  36), new( 33,  -6), new( 36, -42), new( 43, -46), new( 73, -25), new( 49,  14), new(  6,  17),
         new(-23,  18), new(  0,  11), new( -1, -10), new(  3, -27), new( 21, -27), new( 17, -20), new( 18,   0), new( -3,  -4),
         new(-31,   2), new( -7,   4), new( -7, -14), new(  7, -19), new(  9, -20), new(  4, -16), new(  7,  -4), new(-14, -14),
         new(-29,  -3), new(-11,   1), new(-11, -16), new( -5, -12), new(  4, -11), new( -2, -14), new( 20,  -8), new( -2, -20),
         new(-31,   0), new(-11,   3), new(-21, -10), new(-16,  -7), new( -5,  -1), new(-14,  -1), new(  7,  -1), new(-39,  -9),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-158, -68), new(-121, -11), new(-69,   6), new(-41,   0), new(-11,   3), new(-66, -20), new(-90,  -8), new(-107, -90),
         new(-31, -10), new(-10,   4), new( 15,   6), new( 32,   6), new( 13,   1), new( 68, -11), new( -8,   0), new(  5, -24),
         new(-10,  -2), new( 27,   5), new( 43,  25), new( 46,  29), new( 73,  18), new( 88,   6), new( 33,   4), new(  9,  -8),
         new( -8,   9), new(  5,  23), new( 30,  36), new( 53,  37), new( 21,  43), new( 50,  37), new(  0,  30), new( 15,   5),
         new(-19,   9), new( -3,  14), new( 15,  35), new( 17,  37), new( 28,  39), new( 19,  27), new( 14,  15), new( -8,   1),
         new(-35,  -8), new(-14,   6), new(  2,  13), new( 10,  30), new( 22,  26), new(  8,   9), new(  4,   0), new(-16,  -5),
         new(-41, -12), new(-33,   0), new(-19,   5), new( -1,   6), new( -1,   5), new(  2,   0), new(-15,  -9), new(-14,  -5),
         new(-83, -18), new(-26, -24), new(-39,  -5), new(-20,  -4), new(-18,  -2), new(-10, -12), new(-21, -20), new(-52, -29),

         new(-29,   2), new(-58,  14), new(-54,   6), new(-97,  19), new(-89,  13), new(-73,   5), new(-36,   1), new(-62,   2),
         new(-30,  -3), new(  0,  -2), new( -9,   1), new(-26,   2), new(-10,  -3), new(-13,  -4), new(-23,   6), new(-27,  -7),
         new(-12,  12), new(  9,   3), new(  7,   4), new( 14,  -1), new( -1,   3), new( 26,   9), new( 11,   3), new( -6,  14),
         new(-17,   8), new( -3,  10), new(  2,   8), new( 13,  19), new( 11,  11), new(  5,  11), new( -2,   6), new(-25,   9),
         new(-15,   2), new(-16,  11), new(-10,  14), new( 13,  13), new(  8,  10), new( -1,   8), new(-11,   7), new( -4,  -6),
         new(-12,   3), new( -2,   9), new(  3,   9), new(  0,   9), new(  6,  12), new(  5,   7), new(  3,   3), new(  7,  -7),
         new( -4,   2), new(  6,  -3), new(  8, -10), new( -5,   5), new(  3,   2), new( 16,  -4), new( 25,  -2), new(  3, -10),
         new(-11,  -6), new(  4,   3), new( -2,  -2), new(-13,   4), new( -5,   4), new( -8,   8), new(  4,  -9), new(  4, -23),

         new(  0,  25), new(-16,  34), new(-19,  44), new(-20,  40), new( -7,  33), new( 11,  28), new( 10,  27), new( 29,  20),
         new(-14,  28), new(-19,  40), new( -1,  43), new( 14,  33), new( -2,  35), new( 20,  24), new( 16,  20), new( 45,   8),
         new(-23,  25), new(  0,  25), new( -3,  26), new( -1,  23), new( 27,  10), new( 20,   8), new( 60,   1), new( 29,   0),
         new(-27,  28), new(-17,  24), new(-15,  30), new( -9,  25), new( -7,  12), new( -5,   8), new( -1,  11), new(  0,   5),
         new(-38,  22), new(-40,  24), new(-30,  25), new(-20,  22), new(-18,  18), new(-36,  18), new(-17,   9), new(-25,   6),
         new(-38,  18), new(-36,  16), new(-25,  15), new(-21,  18), new(-14,  12), new(-20,   4), new(  2, -11), new(-14,  -8),
         new(-38,  12), new(-30,  14), new(-12,  13), new(-12,  14), new( -8,   7), new( -4,   0), new(  8,  -8), new(-21,   0),
         new(-17,  12), new(-17,  12), new( -5,  16), new(  0,  12), new(  4,   6), new( -1,   7), new(  1,  -1), new(-12,   0),

         new(-46,  23), new(-52,  33), new(-32,  47), new( -6,  30), new(-16,  34), new(-10,  35), new( 34, -15), new(-17,  21),
         new(-16,   8), new(-32,  29), new(-33,  59), new(-44,  75), new(-53,  92), new(-10,  47), new(-20,  44), new( 29,  29),
         new( -4,  13), new(-11,  22), new(-13,  52), new(-12,  53), new( -6,  58), new( 15,  41), new( 28,  17), new( 13,  19),
         new(-21,  32), new(-13,  37), new(-19,  43), new(-24,  61), new(-18,  60), new(-11,  52), new( -3,  51), new( -4,  38),
         new(-12,  22), new(-21,  42), new(-18,  40), new( -8,  49), new( -9,  49), new(-11,  42), new( -3,  35), new(  0,  29),
         new(-15,  10), new( -6,  20), new( -4,  29), new( -8,  31), new( -2,  41), new(  0,  30), new(  7,  15), new(  0,  11),
         new( -8,   5), new( -4,   6), new(  4,   5), new(  8,  15), new(  7,  19), new( 18, -14), new( 21, -38), new( 29, -60),
         new( -9,   0), new(-11,   1), new(  0,   5), new(  8,  19), new(  6,   2), new( -7,  -4), new(  2, -21), new(  1, -28),

         new(  5, -101), new( 18, -54), new( 31, -38), new(-47,  -1), new(-26, -13), new(  1, -10), new( 30, -15), new( 39, -107),
         new(-73, -14), new(-16,  13), new(-49,  25), new( 41,  11), new( -3,  29), new( -2,  41), new( 32,  28), new( -7,   0),
         new(-98,   1), new( 22,  19), new(-43,  38), new(-63,  50), new(-25,  49), new( 48,  41), new( 22,  38), new(-22,   9),
         new(-72,  -6), new(-62,  24), new(-82,  44), new(-124,  56), new(-115,  57), new(-79,  51), new(-79,  41), new(-122,  17),
         new(-78, -16), new(-61,  11), new(-88,  35), new(-123,  52), new(-115,  50), new(-77,  36), new(-85,  24), new(-135,  12),
         new(-30, -25), new( -2,  -3), new(-50,  17), new(-61,  29), new(-51,  28), new(-52,  18), new(-17,   0), new(-49,  -8),
         new( 54, -24), new( 24,   2), new(  9,  -6), new(-16,   2), new(-13,   4), new( -6,  -1), new( 34,   0), new( 33, -17),
         new( 19, -55), new( 42, -29), new( 16,  -5), new(-35, -25), new(  0, -16), new(  0, -30), new( 40, -26), new( 37, -61),
      };

      public static readonly Score[] KnightMobility =
      {
         new(-24, 3),
         new(-10, 6),
         new(-3, 11),
         new(0, 8),
         new(2, 13),
         new(1, 18),
         new(0, 20),
         new(0, 20),
         new(1, 18),
      };

      public static readonly Score[] BishopMobility =
      {
         new(-25, -43),
         new(-18, -27),
         new(-10, -20),
         new(-8, -9),
         new(-2, 4),
         new(3, 18),
         new(7, 23),
         new(9, 30),
         new(9, 38),
         new(9, 38),
         new(10, 37),
         new(12, 38),
         new(10, 44),
         new(30, 32),
      };

      public static readonly Score[] RookMobility =
      {
         new(-32, -10),
         new(-24, 5),
         new(-20, 5),
         new(-15, 8),
         new(-16, 14),
         new(-10, 15),
         new(-7, 17),
         new(-2, 19),
         new(0, 27),
         new(4, 30),
         new(7, 33),
         new(11, 37),
         new(10, 43),
         new(13, 46),
         new(11, 45),
      };

      public static readonly Score[] QueenMobility =
      {
         new(-8, -16),
         new(-11, -78),
         new(-19, -23),
         new(-17, -9),
         new(-16, -4),
         new(-13, 0),
         new(-9, 4),
         new(-11, 24),
         new(-10, 31),
         new(-9, 35),
         new(-9, 45),
         new(-10, 51),
         new(-10, 58),
         new(-10, 63),
         new(-9, 66),
         new(-9, 70),
         new(-6, 73),
         new(-10, 83),
         new(-7, 87),
         new(-5, 86),
         new(5, 81),
         new(7, 82),
         new(9, 80),
         new(14, 79),
         new(36, 59),
         new(88, 40),
         new(63, 52),
         new(63, 63),
      };

      public static Score[] KingAttackWeights =
      {
         new(0, 0),
         new(12, -6),
         new(20, -6),
         new(23, -6),
         new(15, 11),
      };

      public static Score[] PawnShield =
      {
         new(-29, -14),
         new(10, -21),
         new(46, -36),
         new(74, -61),
      };

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      {
         new(0, 0),
         new(-9, -3),
         new(-15, 3),
         new(-14, 26),
         new(7, 51),
         new(0, 115),
         new(26, 94),
      };

      public static int Evaluate(Board board)
      {
         Score white = board.MaterialValue[(int)Color.White];
         Score black = board.MaterialValue[(int)Color.Black];

         ulong whiteKingZone = Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)];
         ulong blackKingZone = Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)];
         Score[] kingAttacks = { new(), new() };
         int[] kingAttacksCount = { 0, 0 };

         white += Pawns(board, Color.White);
         black += Pawns(board, Color.Black);
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

      private static Score Pawns(Board board, Color color)
      {
         Score score = new();
         Bitboard friendlyPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color];
         Bitboard enemyPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color ^ 1];

         while (!friendlyPawns.IsEmpty())
         {
            int square = friendlyPawns.GetLSB();
            friendlyPawns.ClearLSB();
            int rank = color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3);

            // Passed pawns
            if ((Constants.PassedPawnMasks[(int)color][square] & enemyPawns.Value) == 0)
            {
               score += PassedPawn[rank - 1];
            }
         }

         return score;
      }
   }
}
