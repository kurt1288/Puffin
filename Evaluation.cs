using System.Runtime.CompilerServices;
using static Puffin.Constants;
using static Puffin.Attacks;

namespace Puffin
{
   internal static class Evaluation
   {
      public static int Evaluate(Board board)
      {
         Score score = board.MaterialValue[(int)Color.White] - board.MaterialValue[(int)Color.Black];
         Score[] kingAttacks = [new(), new()];
         int[] kingAttacksCount = [0, 0];
         ulong[] mobilitySquares = [0, 0];
         int[] kingSquares = [
            board.GetSquareByPiece(PieceType.King, Color.White),
            board.GetSquareByPiece(PieceType.King, Color.Black)
         ];
         ulong[] kingZones = [
            KingAttacks[kingSquares[(int)Color.White]],
            KingAttacks[kingSquares[(int)Color.Black]]
         ];
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;

         Pawns(board, kingSquares, ref mobilitySquares, ref score);
         Knights(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount);
         Bishops(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, occupied);
         Rooks(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, occupied);
         Queens(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, occupied);
         Kings(board, ref score, ref kingAttacks, ref kingAttacksCount);

         if (board.SideToMove == Color.Black)
         {
            score *= -1;
         }

         return (score.Mg * board.Phase + score.Eg * (24 - board.Phase)) / 24;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static int GetPieceValue(PieceType piece, Board board)
      {
         Score value = PieceValues[(int)(piece == PieceType.Null ? PieceType.Pawn : piece)];
         return (value.Mg * board.Phase + value.Eg * (24 - board.Phase)) / 24;
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
         while (us)
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Mailbox[square];
            score += PieceValues[(int)piece.Type];
            score += GetPSTScore(piece, square);
         }
         return score;
      }

      private static void Knights(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard knightsBB = board.PieceBB[(int)PieceType.Knight];

         while (knightsBB)
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            // * (1 - 2 * (int)color) evaluates to 1 when color is white and to -1 when color is black (so that black score is subtracted)
            score += KnightMobility[new Bitboard(KnightAttacks[square] & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

            if ((KnightAttacks[square] & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }
      }

      private static void Bishops(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied)
      {
         Bitboard bishopBB = board.PieceBB[(int)PieceType.Bishop];

         while (bishopBB)
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = GetBishopAttacks(square, occupied);
            score += BishopMobility[new Bitboard(moves & ~(board.ColorBB[(int)color].Value & board.PieceBB[(int)PieceType.Pawn].Value) & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

            if ((moves & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }
      }

      private static void Rooks(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied)
      {
         Bitboard rookBB = board.PieceBB[(int)PieceType.Rook];

         while (rookBB)
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = GetRookAttacks(square, occupied);
            score += RookMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

            if ((moves & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }
      }

      private static void Queens(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied)
      {
         Bitboard queenBB = board.PieceBB[(int)PieceType.Queen];

         while (queenBB)
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = GetQueenAttacks(square, occupied);
            score += QueenMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

            if ((moves & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }
      }

      private static void Kings(Board board, ref Score score, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard kingBB = board.PieceBB[(int)PieceType.King];

         while (kingBB)
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            Color color = board.Mailbox[kingSq].Color;
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & SquareBB[kingSq]) != 0)
            {
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x7070000000000 : 0xe0e00000000000) : (ulong)(kingSq % 8 < 3 ? 0x70700 : 0xe0e000);

               Bitboard pawns = new(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & pawnSquares);
               score += PawnShield[Math.Min(pawns.CountBits(), 3)] * (1 - 2 * (int)color);
            }

            if (kingAttacksCount[(int)color ^ 1] >= 2)
            {
               score -= kingAttacks[(int)color ^ 1] * (1 - 2 * (int)color);
            }
         }
      }

      private static void Pawns(Board board, int[] kingSquares, ref ulong[] mobilitySquares, ref Score score)
      {
         Bitboard pawns = board.PieceBB[(int)PieceType.Pawn];
         Bitboard[] colorPawns = [pawns & board.ColorBB[(int)Color.White], pawns & board.ColorBB[(int)Color.Black]];
         int[] defended = [
            (colorPawns[(int)Color.White] & WhitePawnAttacks(colorPawns[(int)Color.White].Value)).CountBits(),
            (colorPawns[(int)Color.Black] & BlackPawnAttacks(colorPawns[(int)Color.Black].Value)).CountBits(),
         ];
         int[] connected = [
            (colorPawns[(int)Color.White] & colorPawns[(int)Color.White].RightShift()).CountBits(),
            (colorPawns[(int)Color.Black] & colorPawns[(int)Color.Black].RightShift()).CountBits(),
         ];

         while (pawns)
         {
            int square = pawns.GetLSB();
            Color color = board.Mailbox[square].Color;
            pawns.ClearLSB();
            mobilitySquares[(int)color ^ 1] |= PawnAttacks[(int)color][square];

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & colorPawns[(int)color ^ 1].Value) == 0)
            {
               score += PassedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1] * (1 - 2 * (int)color);
               score += TaxiDistance[square][kingSquares[(int)color]] * FriendlyKingPawnDistance * (1 - 2 * (int)color);
               score += TaxiDistance[square][kingSquares[(int)color ^ 1]] * EnemyKingPawnDistance * (1 - 2 * (int)color);
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & colorPawns[(int)color].Value) == 0)
            {
               // Penalty is based on file
               score -= IsolatedPawn[square & 7] * (1 - 2 * (int)color);
            }
         }

         score += DefendedPawn[defended[(int)Color.White]] - DefendedPawn[defended[(int)Color.Black]];
         score += ConnectedPawn[connected[(int)Color.White]] - ConnectedPawn[connected[(int)Color.Black]];
         mobilitySquares[(int)Color.White] = ~mobilitySquares[(int)Color.White];
         mobilitySquares[(int)Color.Black] = ~mobilitySquares[(int)Color.Black];
      }

      public static readonly Score[] PieceValues = [
         new(63, 107),
         new(292, 345),
         new(322, 364),
         new(412, 630),
         new(837, 1159),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 19, 108), new( 38,  98), new( 21, 101), new( 53,  51), new( 33,  50), new( 17,  59), new(-68, 101), new(-90, 121), 
         new(  3,  36), new(  6,  42), new( 33,   1), new( 39, -34), new( 51, -41), new( 77, -24), new( 45,  14), new( 21,  19), 
         new(-18,  19), new( -5,  10), new( -2,  -6), new(  2, -23), new( 22, -21), new( 19, -20), new(  7,  -1), new(  6,  -3), 
         new(-23,   0), new(-15,   0), new( -5, -14), new(  5, -20), new(  9, -19), new(  6, -19), new( -2,  -9), new( -3, -16), 
         new(-29,  -5), new(-19,  -7), new(-15, -13), new(-10, -11), new(  1,  -9), new(-29,  -8), new(-16, -11), new(-26, -13), 
         new(-23,  -1), new(-13,  -2), new(-12,  -6), new(  0,  -8), new(  0,   3), new( -6,  -2), new( -3, -11), new(-26, -11), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-151, -52), new(-121,  -9), new(-73,   6), new(-42,  -1), new( -8,   0), new(-65, -21), new(-93,  -4), new(-102, -72), 
         new(-27,  -8), new(-11,   2), new( 10,   2), new( 32,   2), new( 12,   0), new( 63, -16), new( -6,   0), new( 10, -22), 
         new( -8,  -3), new( 21,   1), new( 28,  22), new( 35,  25), new( 59,  15), new( 78,   2), new( 27,  -1), new( 13,  -8), 
         new( -4,   9), new(  0,  16), new( 22,  30), new( 45,  32), new( 18,  36), new( 41,  32), new( -2,  26), new( 20,   6), 
         new(-11,  12), new( -3,   9), new(  8,  30), new( 14,  31), new( 22,  34), new( 17,  24), new( 22,  12), new(  1,  10), 
         new(-31,  -4), new(-11,   1), new( -1,   7), new(  3,  25), new( 16,  21), new(  4,   3), new(  7,  -2), new( -9,   2), 
         new(-33,  -5), new(-24,   1), new(-13,   0), new(  3,   0), new(  3,   0), new(  2,  -3), new( -9,  -3), new( -7,   7), 
         new(-70,  -6), new(-21, -12), new(-31,  -4), new(-12,   0), new( -7,   1), new( -6,  -7), new(-16,  -7), new(-42,  -5), 

         new(-33,   2), new(-62,   9), new(-59,   5), new(-96,  14), new(-94,  13), new(-79,   0), new(-43,   1), new(-69,  -3), 
         new(-29,  -9), new( -5,  -3), new(-13,   0), new(-25,   3), new(-13,  -4), new(-14,  -3), new(-31,   3), new(-27,  -8), 
         new(-15,   8), new(  4,   0), new(  3,   6), new( 11,  -2), new( -2,   4), new( 26,   7), new(  4,   3), new( -7,  10), 
         new(-21,   2), new( -1,   8), new(  0,   7), new( 15,  21), new( 12,  12), new(  2,  12), new(  0,   4), new(-28,   8), 
         new( -9,   0), new(-19,   8), new( -7,  16), new( 14,  15), new( 10,  13), new( -2,   8), new(-13,   8), new( 10, -12), 
         new(-10,   1), new(  6,  10), new(  0,  12), new(  0,  14), new(  6,  17), new(  5,   8), new( 10,   1), new(  7,  -5), 
         new(  6,   4), new(  2,  -6), new(  9,  -7), new( -5,   4), new(  0,   5), new( 10,  -5), new( 20,  -1), new(  7, -11), 
         new( -3,  -7), new( 17,   3), new(  3,   0), new(-11,   2), new(  0,  -1), new( -1,  10), new(  9, -10), new( 13, -20), 

         new( -6,  27), new(-20,  35), new(-24,  44), new(-27,  40), new(-14,  34), new(  5,  28), new( -2,  33), new( 18,  23), 
         new(-18,  27), new(-23,  40), new( -6,  42), new( 11,  33), new( -5,  34), new( 20,  21), new( 14,  19), new( 41,   7), 
         new(-20,  26), new(  3,  25), new(  3,  27), new(  8,  23), new( 35,  11), new( 32,   7), new( 67,   1), new( 32,   2), 
         new(-25,  28), new(-14,  27), new( -7,  30), new( -2,  27), new(  2,  14), new(  1,  10), new(  7,  13), new(  0,   8), 
         new(-35,  22), new(-37,  27), new(-23,  25), new(-16,  23), new(-14,  20), new(-31,  19), new( -9,  11), new(-20,   9), 
         new(-39,  18), new(-31,  16), new(-22,  14), new(-21,  17), new(-12,  11), new(-15,   3), new(  8, -10), new(-13,  -7), 
         new(-38,   9), new(-30,  14), new(-10,  11), new( -8,  11), new( -4,   4), new( -2,  -2), new( 10, -10), new(-24,  -2), 
         new(-18,   8), new(-15,  10), new( -4,  13), new(  4,   8), new(  9,   2), new( -2,   4), new(  4,  -1), new(-14,  -2), 

         new(-47,  20), new(-56,  35), new(-37,  52), new(-13,  37), new(-20,  37), new(-14,  38), new( 32, -12), new(-20,  21), 
         new(-16,   4), new(-38,  33), new(-39,  66), new(-51,  86), new(-58, 100), new(-11,  48), new(-25,  50), new( 33,  24), 
         new( -7,  16), new(-11,  27), new(-14,  60), new(-13,  62), new( -4,  66), new( 18,  48), new( 26,  24), new( 15,  23), 
         new(-21,  33), new(-13,  43), new(-17,  49), new(-23,  72), new(-16,  72), new(-10,  61), new(  0,  61), new( -5,  43), 
         new( -9,  21), new(-21,  45), new(-17,  50), new( -8,  59), new(-10,  62), new( -9,  52), new( -1,  42), new(  4,  31), 
         new(-12,   4), new( -4,  23), new( -5,  35), new( -8,  41), new( -2,  49), new(  1,  34), new( 11,  17), new(  4,   7), 
         new( -5,  -3), new( -3,   1), new(  5,   4), new( 11,  13), new( 10,  16), new( 18, -14), new( 19, -38), new( 27, -64), 
         new( -8,  -5), new( -4,  -6), new(  2,   3), new( 10,  15), new( 11,  -5), new( -3, -11), new(  1, -21), new(  1, -35), 

         new(  0, -76), new( 20, -40), new( 22, -26), new(-58,   7), new(-24,  -7), new( -5,  -1), new( 31,  -3), new( 51, -82), 
         new(-70,  -6), new(-16,  15), new(-52,  23), new( 45,   8), new( -2,  23), new( -1,  40), new( 29,  32), new( -1,  11), 
         new(-90,   0), new( 26,  12), new(-40,  27), new(-59,  38), new(-18,  38), new( 58,  31), new( 33,  31), new(-12,   6), 
         new(-55, -17), new(-44,   8), new(-76,  28), new(-121,  41), new(-111,  41), new(-68,  37), new(-57,  27), new(-101,   7), 
         new(-61, -27), new(-42,  -3), new(-69,  17), new(-109,  35), new(-103,  34), new(-61,  21), new(-65,  11), new(-119,   4), 
         new(-18, -29), new( 16, -14), new(-33,   5), new(-46,  16), new(-37,  16), new(-38,  10), new( -7,  -3), new(-41,  -8), 
         new( 44, -18), new( 14,   3), new( 19,  -8), new( -5,  -2), new( -3,   0), new(  7,  -3), new( 27,   5), new( 24,  -6), 
         new(  7, -35), new( 36, -21), new( 10,  -1), new(-29, -18), new( -3, -12), new(  5, -23), new( 34, -16), new( 28, -44), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-34, -52),
         new(-17, -18),
         new(-7, 2),
         new(-2, 12),
         new(2, 21),
         new(6, 30),
         new(13, 29),
         new(18, 26),
         new(24, 17),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-50, -50),
         new(-27, -32),
         new(-12, -14),
         new(-6, 0),
         new(3, 10),
         new(10, 21),
         new(14, 26),
         new(16, 30),
         new(18, 34),
         new(21, 33),
         new(26, 31),
         new(36, 28),
         new(32, 34),
         new(49, 20),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-33, -19),
         new(-23, 0),
         new(-19, 1),
         new(-13, 4),
         new(-14, 11),
         new(-8, 15),
         new(-4, 21),
         new(1, 23),
         new(7, 26),
         new(11, 31),
         new(16, 33),
         new(17, 39),
         new(20, 43),
         new(22, 41),
         new(15, 45),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-22, -45),
         new(-21, -76),
         new(-26, -16),
         new(-22, 7),
         new(-21, 24),
         new(-17, 28),
         new(-15, 43),
         new(-14, 53),
         new(-12, 59),
         new(-10, 62),
         new(-10, 68),
         new(-7, 73),
         new(-6, 74),
         new(-7, 79),
         new(-4, 80),
         new(-3, 84),
         new(-4, 91),
         new(-2, 90),
         new(5, 87),
         new(19, 78),
         new(22, 79),
         new(68, 53),
         new(60, 57),
         new(80, 37),
         new(138, 26),
         new(97, 24),
         new(53, 52),
         new(36, 44),
      ];

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(11, -6),
         new(19, -5),
         new(23, -6),
         new(14, 13),
      ];

      public static Score[] PawnShield =
      [
         new(-42, -1),
         new(-3, -16),
         new(35, -27),
         new(67, -42),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(36, -31),
         new(18, -9),
         new(3, 28),
         new(9, 66),
         new(-11, 142),
         new(15, 118),
      ];

      public static Score[] DefendedPawn = [
         new(-24, -21),
         new(-8, -13),
         new(6, -2),
         new(19, 13),
         new(31, 32),
         new(42, 40),
         new(36, 16),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-18, -8),
         new(-6, 1),
         new(5, 3),
         new(16, 13),
         new(26, 10),
         new(35, 76),
         new(47, -4),
         new(-13, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(1, 5),
         new(5, 11),
         new(11, 11),
         new(9, 16),
         new(14, 17),
         new(8, 10),
         new(4, 10),
         new(11, 5),
      ];

      public static Score FriendlyKingPawnDistance = new(7, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
   }
}
