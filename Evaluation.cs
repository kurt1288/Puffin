using System.Runtime.CompilerServices;

namespace Puffin
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new(72, 110),
         new(289, 338),
         new(321, 356),
         new(410, 619),
         new(831, 1145),
         new(0, 0),
      };

      public static readonly Score[] PST =
      {
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 21,  99), new( 45,  87), new( 22,  86), new( 54,  30), new( 33,  32), new( 21,  46), new(-52,  91), new(-79, 108),
         new( -1,  37), new(  3,  41), new( 29,  -2), new( 36, -39), new( 42, -43), new( 71, -22), new( 48,  18), new( 11,  22),
         new(-22,  20), new( -6,   8), new( -7,  -9), new( -2, -28), new( 15, -27), new( 15, -20), new( 11,  -1), new(  0,  -2),
         new(-26,   2), new(-14,   1), new( -8, -16), new(  2, -24), new(  6, -23), new(  1, -18), new(  2,  -7), new(-11, -15),
         new(-30,  -5), new(-17,  -8), new(-17, -17), new(-11, -14), new( -1, -13), new( -7, -15), new( 12, -18), new( -5, -22),
         new(-24,   0), new( -9,   4), new(-14,  -8), new(-12,  -9), new( -2,  -1), new( -9,  -1), new( 11,  -2), new(-33, -10),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-147, -51), new(-124, -10), new(-69,   4), new(-39,  -2), new( -9,   0), new(-61, -22), new(-89,  -5), new(-96, -72),
         new(-25,  -8), new( -9,   2), new( 12,   3), new( 29,   4), new( 13,   0), new( 65, -15), new( -3,   0), new( 14, -23),
         new( -8,  -3), new( 20,   3), new( 26,  23), new( 32,  26), new( 57,  16), new( 74,   4), new( 28,   1), new( 15,  -8),
         new( -4,   9), new(  0,  17), new( 20,  32), new( 45,  33), new( 16,  38), new( 39,  34), new( -4,  27), new( 19,   7),
         new(-11,  11), new( -4,  11), new(  7,  31), new( 14,  32), new( 22,  35), new( 17,  24), new( 20,  13), new(  1,   9),
         new(-29,  -5), new(-12,   0), new( -2,   8), new(  2,  25), new( 16,  21), new(  4,   5), new(  5,  -1), new( -7,   1),
         new(-33,  -5), new(-23,   0), new(-14,   1), new(  4,   1), new(  5,   0), new(  3,  -4), new( -8,  -5), new( -8,   6),
         new(-71,  -4), new(-20, -13), new(-30,  -4), new(-10,  -1), new( -7,   1), new( -4,  -8), new(-15, -10), new(-40,  -8),

         new(-32,  -1), new(-63,   9), new(-59,   3), new(-99,  15), new(-93,   9), new(-79,   0), new(-38,  -2), new(-69,  -2),
         new(-29,  -8), new(  0,  -5), new(-12,  -1), new(-24,   0), new(-12,  -4), new(-12,  -5), new(-26,   3), new(-23, -11),
         new(-13,   7), new(  6,   0), new(  4,   5), new( 10,  -1), new( -2,   4), new( 24,   9), new(  9,   2), new( -7,  11),
         new(-20,   3), new( -1,   9), new(  0,   8), new( 14,  20), new( 12,  13), new(  3,  12), new(  0,   5), new(-26,   7),
         new(-12,   0), new(-18,  10), new( -7,  16), new( 16,  15), new( 10,  13), new( -1,  10), new(-11,   8), new(  0,  -8),
         new(-13,   0), new(  1,  10), new(  3,  10), new(  2,  13), new(  8,  17), new(  7,   8), new(  4,   3), new(  5,  -8),
         new( -1,   2), new(  5,  -8), new(  9,  -8), new( -2,   4), new(  7,   3), new( 14,  -4), new( 24,  -5), new(  5, -10),
         new( -9,  -8), new(  9,   4), new( -1,  -2), new(-11,   1), new( -3,   0), new( -7,   8), new(  4, -10), new(  6, -23),

         new( -6,  26), new(-20,  35), new(-25,  45), new(-28,  42), new(-14,  35), new(  8,  28), new(  3,  30), new( 22,  21),
         new(-19,  28), new(-22,  40), new( -6,  43), new( 10,  34), new( -4,  36), new( 16,  24), new( 14,  20), new( 40,   8),
         new(-23,  26), new(  4,  26), new(  1,  28), new(  8,  25), new( 34,  12), new( 27,   9), new( 68,   2), new( 31,   2),
         new(-26,  29), new(-11,  27), new( -8,  32), new( -2,  28), new(  1,  15), new(  0,  11), new(  7,  13), new(  2,   7),
         new(-36,  23), new(-37,  27), new(-26,  26), new(-16,  24), new(-16,  21), new(-34,  21), new(-11,  11), new(-20,   8),
         new(-39,  18), new(-32,  16), new(-23,  15), new(-22,  18), new(-14,  14), new(-18,   5), new(  5,  -9), new(-14,  -7),
         new(-38,  10), new(-29,  14), new(-10,  11), new( -9,  11), new( -5,   4), new( -4,  -1), new( 10,  -9), new(-22,  -2),
         new(-19,   8), new(-15,  10), new( -3,  14), new(  3,   9), new(  7,   3), new(  0,   4), new(  4,  -3), new(-14,  -3),

         new(-48,  19), new(-57,  33), new(-37,  50), new(-13,  34), new(-20,  35), new(-13,  35), new( 29, -14), new(-20,  18),
         new(-18,   4), new(-36,  30), new(-39,  62), new(-50,  80), new(-57,  94), new(-13,  49), new(-25,  46), new( 31,  24),
         new( -6,  14), new(-12,  26), new(-14,  58), new(-13,  59), new( -6,  64), new( 17,  45), new( 29,  22), new( 15,  19),
         new(-22,  32), new(-13,  41), new(-20,  48), new(-24,  68), new(-17,  68), new(-11,  59), new( -1,  58), new( -5,  41),
         new(-11,  20), new(-23,  45), new(-18,  47), new( -9,  56), new(-10,  57), new(-11,  49), new( -3,  40), new(  1,  30),
         new(-15,   5), new( -5,  21), new( -5,  32), new( -8,  38), new( -3,  47), new(  1,  31), new(  8,  15), new(  3,   7),
         new( -5,  -2), new( -3,   0), new(  5,   3), new( 10,  11), new( 10,  13), new( 17, -16), new( 21, -42), new( 28, -65),
         new( -7,  -9), new( -5,  -7), new(  3,   0), new( 11,  11), new( 10,  -5), new( -3, -13), new(  4, -27), new(  3, -38),

         new(  4, -102), new( 18, -55), new( 32, -39), new(-50,  -1), new(-24, -13), new(  2, -11), new( 32, -15), new( 45, -109),
         new(-76, -15), new(-16,  12), new(-55,  26), new( 45,  10), new( -3,  29), new( -1,  40), new( 33,  27), new( -2,  -1),
         new(-101,   0), new( 19,  19), new(-44,  38), new(-62,  49), new(-22,  49), new( 52,  40), new( 24,  39), new(-21,   9),
         new(-74,  -7), new(-61,  24), new(-83,  45), new(-122,  57), new(-113,  57), new(-79,  52), new(-76,  41), new(-120,  17),
         new(-79, -17), new(-61,  11), new(-86,  34), new(-122,  52), new(-114,  49), new(-76,  35), new(-83,  23), new(-135,  11),
         new(-30, -27), new( -2,  -5), new(-50,  16), new(-63,  28), new(-54,  28), new(-53,  18), new(-19,   0), new(-51,  -9),
         new( 56, -26), new( 22,   2), new(  8,  -7), new(-15,   0), new(-14,   3), new( -8,  -2), new( 33,   1), new( 35, -17),
         new( 18, -55), new( 43, -30), new( 17,  -5), new(-33, -26), new(  0, -15), new(  0, -31), new( 40, -26), new( 36, -60),
      };

      public static readonly Score[] KnightMobility =
      {
         new(-36, -51),
         new(-18, -16),
         new(-8, 4),
         new(-2, 14),
         new(3, 22),
         new(8, 29),
         new(15, 28),
         new(21, 25),
         new(26, 17),
      };

      public static readonly Score[] BishopMobility =
      {
         new(-27, -42),
         new(-15, -26),
         new(-6, -8),
         new(-1, 4),
         new(4, 14),
         new(8, 25),
         new(11, 29),
         new(12, 33),
         new(14, 37),
         new(18, 34),
         new(25, 31),
         new(32, 29),
         new(23, 41),
         new(37, 20),
      };

      public static readonly Score[] RookMobility =
      {
         new(-33, -15),
         new(-23, 3),
         new(-20, 2),
         new(-13, 6),
         new(-15, 13),
         new(-8, 16),
         new(-4, 22),
         new(1, 24),
         new(7, 27),
         new(11, 31),
         new(16, 34),
         new(16, 40),
         new(18, 44),
         new(20, 42),
         new(9, 46),
      };

      public static readonly Score[] QueenMobility =
      {
         new(-22, -43),
         new(-19, -85),
         new(-26, -20),
         new(-23, 2),
         new(-22, 19),
         new(-18, 22),
         new(-15, 38),
         new(-15, 48),
         new(-12, 55),
         new(-11, 58),
         new(-10, 64),
         new(-7, 69),
         new(-6, 70),
         new(-6, 75),
         new(-5, 76),
         new(-3, 80),
         new(-5, 88),
         new(-3, 87),
         new(3, 85),
         new(16, 76),
         new(19, 78),
         new(66, 50),
         new(58, 54),
         new(77, 36),
         new(135, 25),
         new(99, 23),
         new(55, 52),
         new(37, 42),
      };

      public static Score[] KingAttackWeights =
      {
         new(0, 0),
         new(12, -6),
         new(20, -5),
         new(24, -6),
         new(14, 12),
      };

      public static Score[] PawnShield =
      {
         new(-25, -14),
         new(10, -25),
         new(44, -39),
         new(79, -54),
      };

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      {
         new(0, 0),
         new(-6, 1),
         new(-12, 10),
         new(-12, 33),
         new(8, 56),
         new(-1, 119),
         new(24, 99),
      };

      public static Score[] DefendedPawn = [
         new(-17, -28),
         new(-6, -12),
         new(6, 3),
         new(16, 24),
         new(24, 41),
         new(30, 54),
         new(38, 52),
         new(0, 0),
      ];

      public static int Evaluate(Board board)
      {
         Score white = board.MaterialValue[(int)Color.White];
         Score black = board.MaterialValue[(int)Color.Black];

         ulong whiteKingZone = Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)];
         ulong blackKingZone = Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)];
         Score[] kingAttacks = { new(), new() };
         int[] kingAttacksCount = { 0, 0 };
         ulong[] mobilitySquares = [0, 0];

         white += Pawns(board, Color.White, ref mobilitySquares);
         black += Pawns(board, Color.Black, ref mobilitySquares);
         white += Knights(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         black += Knights(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         white += Bishops(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         black += Bishops(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         white += Rooks(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         black += Rooks(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         white += Queens(board, Color.White, blackKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
         black += Queens(board, Color.Black, whiteKingZone, ref kingAttacks, ref kingAttacksCount, ref mobilitySquares);
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

      private static Score Knights(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount, ref ulong[] mobilitySquares)
      {
         Bitboard knightsBB = new(board.PieceBB[(int)PieceType.Knight].Value & board.ColorBB[(int)color].Value);
         ulong us = board.ColorBB[(int)color].Value;
         Score score = new();

         while (!knightsBB.IsEmpty())
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += KnightMobility[new Bitboard(Attacks.KnightAttacks[square] & ~us & mobilitySquares[(int)color]).CountBits()];

            if ((Attacks.KnightAttacks[square] & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Knight] * new Bitboard(Attacks.KnightAttacks[square] & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Bishops(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount, ref ulong[] mobilitySquares)
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
            score += BishopMobility[new Bitboard(moves & ~us & mobilitySquares[(int)color]).CountBits()];

            if ((moves & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Rooks(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount, ref ulong[] mobilitySquares)
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
            score += RookMobility[new Bitboard(moves & ~us & mobilitySquares[(int)color]).CountBits()];

            if ((moves & kingZone) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & kingZone).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score Queens(Board board, Color color, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount, ref ulong[] mobilitySquares)
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
            score += QueenMobility[new Bitboard(moves & ~us & mobilitySquares[(int)color]).CountBits()];

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

      private static Score Pawns(Board board, Color color, ref ulong[] mobilitySquares)
      {
         Score score = new();
         Bitboard friendlyPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color];
         Bitboard pawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color];
         Bitboard enemyPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color ^ 1];
         int defender = 0;

         while (!pawns.IsEmpty())
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();
            int rank = color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3);
            mobilitySquares[(int)color ^ 1] |= Attacks.PawnAttacks[(int)color][square];

            // Passed pawns
            if ((Constants.PassedPawnMasks[(int)color][square] & enemyPawns.Value) == 0)
            {
               score += PassedPawn[rank - 1];
            }

            if ((Attacks.PawnAttacks[(int)color][square] & friendlyPawns.Value) != 0)
            {
               defender++;
            }
         }

         score += DefendedPawn[defender];

         mobilitySquares[(int)color ^ 1] = ~mobilitySquares[(int)color ^ 1];

         return score;
      }
   }
}
