using System.Runtime.CompilerServices;

namespace Puffin
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = {
         new(77, 118),
         new(288, 338),
         new(319, 353),
         new(408, 616),
         new(829, 1141),
         new(0, 0),
      };

      public static readonly Score[] PST =
      {
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 20,  95), new( 42,  84), new( 21,  82), new( 51,  27), new( 30,  29), new( 19,  42), new(-52,  87), new(-82, 104),
         new( -7,  34), new(  2,  38), new( 26,  -5), new( 31, -42), new( 36, -45), new( 67, -24), new( 43,  15), new(  6,  18),
         new(-24,  19), new( -5,  11), new( -7,  -9), new( -3, -27), new( 14, -27), new( 13, -20), new( 11,   1), new( -4,  -3),
         new(-28,   2), new(-12,   5), new( -9, -15), new(  3, -20), new(  6, -21), new(  0, -16), new(  3,  -3), new(-12, -14),
         new(-28,  -2), new( -9,   0), new(-12, -15), new( -7, -11), new(  4, -11), new( -4, -14), new( 20,  -8), new( -2, -19),
         new(-26,   0), new( -9,   3), new(-16, -11), new( -9,  -7), new(  0,  -1), new(-12,  -1), new(  8,  -1), new(-35, -10),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-145, -49), new(-121,  -9), new(-68,   4), new(-38,  -2), new( -9,   1), new(-60, -22), new(-90,  -5), new(-94, -71),
         new(-24,  -6), new( -8,   4), new( 12,   2), new( 28,   3), new( 12,  -1), new( 65, -15), new( -2,   0), new( 13, -21),
         new( -8,  -3), new( 18,   1), new( 24,  22), new( 31,  25), new( 56,  15), new( 73,   3), new( 25,   0), new( 13,  -7),
         new( -3,   9), new(  0,  17), new( 19,  31), new( 43,  32), new( 13,  37), new( 38,  33), new( -4,  26), new( 20,   7),
         new(-11,  13), new( -4,  10), new(  6,  30), new( 14,  30), new( 22,  34), new( 17,  23), new( 20,  13), new(  1,  11),
         new(-29,  -4), new(-12,   0), new( -2,   6), new(  2,  24), new( 17,  20), new(  5,   2), new(  5,  -2), new( -6,   1),
         new(-33,  -4), new(-22,   1), new(-14,   0), new(  5,   1), new(  5,   0), new(  3,  -4), new( -8,  -4), new( -7,   7),
         new(-71,  -1), new(-19, -12), new(-30,  -4), new( -9,   0), new( -7,   1), new( -4,  -6), new(-14,  -9), new(-40,  -8),

         new(-31,  -1), new(-63,  10), new(-56,   3), new(-99,  15), new(-93,   9), new(-77,   0), new(-41,  -3), new(-65,  -2),
         new(-30,  -8), new( -1,  -5), new(-12,   0), new(-24,   0), new(-13,  -4), new(-13,  -6), new(-25,   3), new(-25, -11),
         new(-14,   8), new(  5,   0), new(  3,   4), new(  9,  -2), new( -4,   3), new( 22,   8), new(  6,   1), new( -9,  11),
         new(-20,   4), new( -1,   8), new(  0,   7), new( 13,  19), new( 11,  12), new(  3,  11), new(  0,   5), new(-28,   7),
         new(-13,   0), new(-19,   9), new( -8,  15), new( 15,  13), new( 10,  11), new( -2,   8), new(-11,   7), new(  1,  -9),
         new(-13,   1), new(  1,  10), new(  3,  10), new(  2,  11), new(  8,  16), new(  6,   8), new(  4,   3), new(  6,  -6),
         new(  0,   3), new(  5,  -6), new(  9,  -8), new( -2,   4), new(  7,   2), new( 16,  -5), new( 24,  -3), new(  7,  -9),
         new(-10,  -5), new( 10,   3), new( -1,  -1), new(-12,   2), new( -3,   0), new( -6,   8), new(  6, -10), new(  7, -21),

         new( -6,  26), new(-20,  35), new(-25,  45), new(-27,  41), new(-13,  35), new(  8,  28), new(  6,  28), new( 23,  21),
         new(-18,  28), new(-22,  40), new( -5,  43), new( 10,  34), new( -5,  36), new( 16,  25), new( 14,  21), new( 41,   8),
         new(-23,  27), new(  3,  26), new(  1,  28), new(  7,  25), new( 34,  12), new( 27,   9), new( 65,   2), new( 31,   1),
         new(-26,  29), new(-12,  26), new( -9,  32), new( -3,  28), new(  1,  15), new( -1,  11), new(  5,  12), new(  1,   6),
         new(-37,  22), new(-37,  26), new(-26,  26), new(-16,  23), new(-16,  21), new(-35,  21), new(-12,  11), new(-22,   7),
         new(-39,  18), new(-32,  15), new(-22,  14), new(-21,  18), new(-14,  13), new(-17,   4), new(  4, -10), new(-14,  -7),
         new(-39,  10), new(-29,  14), new(-11,  11), new( -8,  12), new( -5,   5), new( -4,   0), new(  8,  -8), new(-22,  -1),
         new(-19,   9), new(-15,  10), new( -3,  14), new(  3,   9), new(  7,   3), new(  0,   4), new(  2,  -3), new(-15,  -2),

         new(-48,  19), new(-58,  33), new(-38,  49), new(-14,  34), new(-22,  35), new(-15,  35), new( 27, -14), new(-19,  16),
         new(-18,   4), new(-37,  30), new(-39,  62), new(-50,  79), new(-58,  95), new(-13,  48), new(-25,  46), new( 30,  25),
         new( -7,  14), new(-14,  25), new(-15,  57), new(-14,  58), new( -7,  62), new( 15,  45), new( 26,  21), new( 13,  19),
         new(-22,  31), new(-14,  40), new(-21,  48), new(-25,  66), new(-18,  66), new(-11,  57), new( -2,  56), new( -5,  40),
         new(-11,  19), new(-24,  44), new(-18,  45), new( -9,  54), new(-10,  56), new(-12,  48), new( -3,  39), new(  2,  29),
         new(-15,   6), new( -5,  21), new( -5,  30), new( -9,  37), new( -3,  45), new(  1,  30), new(  8,  14), new(  3,   6),
         new( -5,  -3), new( -3,   0), new(  4,   3), new( 11,   9), new( 10,  13), new( 18, -17), new( 21, -42), new( 30, -66),
         new( -8,  -6), new( -5,  -5), new(  4,  -1), new( 12,  11), new( 10,  -6), new( -2, -12), new(  5, -27), new(  4, -37),

         new(  5, -101), new( 18, -54), new( 31, -38), new(-53,   0), new(-25, -13), new(  1, -11), new( 31, -15), new( 45, -108),
         new(-78, -14), new(-18,  14), new(-53,  26), new( 43,  11), new( -2,  29), new( -2,  41), new( 33,  28), new( -5,  -1),
         new(-101,   1), new( 19,  19), new(-44,  39), new(-63,  50), new(-24,  49), new( 51,  40), new( 23,  38), new(-21,   9),
         new(-74,  -6), new(-62,  24), new(-82,  44), new(-123,  56), new(-113,  57), new(-78,  51), new(-77,  41), new(-121,  16),
         new(-77, -16), new(-62,  11), new(-87,  35), new(-122,  52), new(-114,  50), new(-77,  36), new(-84,  23), new(-133,  11),
         new(-30, -26), new( -1,  -4), new(-50,  17), new(-62,  29), new(-53,  28), new(-52,  18), new(-17,   0), new(-50,  -8),
         new( 53, -24), new( 22,   2), new(  9,  -6), new(-15,   1), new(-13,   3), new( -7,  -1), new( 32,   1), new( 32, -16),
         new( 18, -55), new( 42, -29), new( 16,  -5), new(-33, -25), new(  0, -15), new(  1, -31), new( 39, -26), new( 36, -61),
      };

      public static readonly Score[] KnightMobility =
      {
         new(-38, -51),
         new(-20, -17),
         new(-8, 3),
         new(-2, 12),
         new(3, 21),
         new(8, 30),
         new(15, 29),
         new(22, 26),
         new(27, 18),
      };

      public static readonly Score[] BishopMobility =
      {
         new(-28, -48),
         new(-17, -28),
         new(-7, -11),
         new(-1, 3),
         new(5, 14),
         new(9, 26),
         new(12, 31),
         new(13, 34),
         new(14, 39),
         new(18, 36),
         new(24, 34),
         new(31, 32),
         new(24, 45),
         new(35, 23),
      };

      public static readonly Score[] RookMobility =
      {
         new(-33, -14),
         new(-23, 3),
         new(-20, 3),
         new(-14, 6),
         new(-15, 13),
         new(-8, 16),
         new(-4, 22),
         new(1, 24),
         new(7, 27),
         new(11, 31),
         new(15, 34),
         new(15, 40),
         new(17, 45),
         new(19, 42),
         new(6, 46),
      };

      public static readonly Score[] QueenMobility =
      {
         new(-23, -47),
         new(-19, -89),
         new(-26, -23),
         new(-23, -1),
         new(-22, 15),
         new(-18, 18),
         new(-15, 34),
         new(-14, 45),
         new(-12, 52),
         new(-10, 56),
         new(-9, 62),
         new(-7, 67),
         new(-5, 68),
         new(-6, 74),
         new(-5, 76),
         new(-3, 80),
         new(-4, 88),
         new(-3, 86),
         new(3, 84),
         new(15, 76),
         new(18, 79),
         new(67, 49),
         new(58, 54),
         new(76, 35),
         new(135, 25),
         new(99, 23),
         new(57, 51),
         new(36, 41),
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
         new(-28, -15),
         new(10, -22),
         new(47, -37),
         new(77, -62),
      };

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      {
         new(0, 0),
         new(-9, -4),
         new(-15, 3),
         new(-14, 26),
         new(7, 51),
         new(-1, 115),
         new(20, 95),
      };

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
         Bitboard enemyPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color ^ 1];

         while (!friendlyPawns.IsEmpty())
         {
            int square = friendlyPawns.GetLSB();
            friendlyPawns.ClearLSB();
            int rank = color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3);
            mobilitySquares[(int)color ^ 1] |= Attacks.PawnAttacks[(int)color][square];

            // Passed pawns
            if ((Constants.PassedPawnMasks[(int)color][square] & enemyPawns.Value) == 0)
            {
               score += PassedPawn[rank - 1];
            }
         }

         mobilitySquares[(int)color ^ 1] = ~mobilitySquares[(int)color ^ 1];

         return score;
      }
   }
}
