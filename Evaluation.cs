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
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x007000000000000 : 0x000E0000000000000) : (ulong)(kingSq % 8 < 3 ? 0x700 : 0xE000);

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
         new(64, 105),
         new(292, 344),
         new(323, 364),
         new(412, 630),
         new(837, 1159),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 18, 109), new( 38,  98), new( 20, 101), new( 52,  51), new( 32,  51), new( 15,  60), new(-67, 101), new(-89, 121), 
         new(  0,  38), new(  5,  43), new( 31,   3), new( 37, -32), new( 47, -39), new( 77, -22), new( 43,  15), new( 22,  21), 
         new(-19,  19), new( -6,  10), new( -4,  -5), new(  0, -22), new( 20, -20), new( 17, -19), new(  5,   0), new(  8,  -3), 
         new(-24,   0), new(-16,   1), new( -8, -13), new(  2, -19), new(  7, -18), new(  3, -17), new( -6,  -8), new( -4, -15), 
         new(-28,  -5), new(-18,  -6), new(-15, -12), new(-13, -10), new( -1,  -8), new( -8, -14), new(  4, -18), new( -1, -20), 
         new(-26,   0), new(-16,  -2), new(-15,  -5), new( -2,  -6), new( -4,   4), new( -8,  -1), new( -9,  -9), new(-27,  -9), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-152, -52), new(-121, -10), new(-72,   5), new(-42,  -1), new(-10,   0), new(-65, -21), new(-93,  -4), new(-101, -73), 
         new(-26,  -9), new(-11,   2), new( 10,   2), new( 31,   2), new( 12,   0), new( 61, -15), new( -7,   0), new( 11, -23), 
         new( -8,  -3), new( 21,   1), new( 28,  23), new( 34,  26), new( 58,  16), new( 77,   3), new( 24,   0), new( 13,  -9), 
         new( -4,   9), new(  0,  16), new( 22,  31), new( 44,  33), new( 17,  37), new( 40,  33), new( -3,  27), new( 20,   6), 
         new(-11,  11), new( -2,   9), new(  8,  31), new( 14,  32), new( 22,  35), new( 17,  24), new( 23,  11), new(  2,  10), 
         new(-30,  -5), new(-11,   1), new( -1,   7), new(  3,  25), new( 16,  22), new(  4,   5), new(  6,  -1), new( -9,   2), 
         new(-32,  -6), new(-23,   0), new(-13,   1), new(  3,   1), new(  4,   0), new(  3,  -3), new( -7,  -4), new( -6,   5), 
         new(-70,  -6), new(-21, -13), new(-31,  -4), new(-11,   0), new( -7,   1), new( -5,  -7), new(-16,  -8), new(-41,  -6), 

         new(-35,   2), new(-63,  10), new(-59,   5), new(-97,  15), new(-94,  12), new(-78,   0), new(-43,   1), new(-68,  -2), 
         new(-28,  -9), new( -5,  -3), new(-14,   0), new(-24,   3), new(-13,  -4), new(-16,  -2), new(-30,   4), new(-27,  -9), 
         new(-15,   8), new(  4,   0), new(  3,   6), new( 11,  -1), new( -3,   5), new( 26,   8), new(  3,   3), new( -7,  10), 
         new(-21,   3), new( -1,   8), new(  0,   7), new( 14,  21), new( 12,  12), new(  1,  12), new(  0,   4), new(-29,   8), 
         new( -9,   0), new(-20,   9), new( -7,  16), new( 14,  15), new( 10,  13), new( -2,   9), new(-13,   9), new( 10, -11), 
         new(-11,   1), new(  5,  10), new(  0,  12), new(  0,  14), new(  6,  17), new(  4,   9), new(  9,   2), new(  7,  -5), 
         new(  6,   4), new(  1,  -6), new(  8,  -7), new( -5,   5), new(  0,   6), new( 10,  -5), new( 21,  -3), new(  6, -11), 
         new( -4,  -7), new( 16,   4), new(  3,   0), new(-11,   2), new(  0,   0), new( -2,  10), new(  9,  -9), new( 13, -21), 

         new( -5,  27), new(-20,  35), new(-24,  44), new(-27,  40), new(-14,  34), new(  3,  29), new( -3,  33), new( 18,  23), 
         new(-18,  27), new(-23,  40), new( -6,  42), new( 11,  33), new( -5,  35), new( 17,  22), new( 13,  19), new( 41,   7), 
         new(-20,  26), new(  3,  25), new(  3,  27), new(  8,  23), new( 34,  11), new( 31,   7), new( 65,   1), new( 33,   2), 
         new(-24,  28), new(-13,  27), new( -6,  30), new( -2,  27), new(  3,  14), new(  0,  10), new(  7,  13), new(  2,   7), 
         new(-35,  22), new(-37,  27), new(-23,  25), new(-16,  23), new(-14,  20), new(-32,  20), new( -8,  11), new(-20,   9), 
         new(-39,  18), new(-31,  16), new(-22,  14), new(-21,  17), new(-13,  11), new(-16,   3), new(  7,  -9), new(-14,  -6), 
         new(-38,   9), new(-30,  14), new(-10,  11), new( -8,  11), new( -4,   4), new( -2,  -2), new( 10,  -9), new(-23,  -2), 
         new(-18,   8), new(-15,  10), new( -4,  13), new(  3,   8), new(  9,   2), new( -2,   4), new(  5,  -1), new(-14,  -2), 

         new(-47,  21), new(-56,  36), new(-37,  52), new(-11,  38), new(-21,  38), new(-14,  38), new( 31, -12), new(-21,  21), 
         new(-16,   4), new(-38,  34), new(-39,  66), new(-50,  86), new(-58, 100), new(-13,  50), new(-26,  50), new( 32,  24), 
         new( -8,  17), new(-12,  28), new(-14,  61), new(-13,  62), new( -5,  66), new( 18,  48), new( 24,  23), new( 15,  22), 
         new(-21,  33), new(-12,  43), new(-17,  49), new(-23,  72), new(-16,  72), new(-10,  60), new(  1,  59), new( -5,  42), 
         new( -9,  20), new(-22,  46), new(-17,  50), new( -9,  59), new(-10,  62), new( -9,  52), new( -2,  42), new(  3,  32), 
         new(-12,   4), new( -3,  22), new( -5,  35), new( -8,  41), new( -2,  49), new(  1,  34), new( 10,  17), new(  3,   9), 
         new( -5,  -3), new( -3,   1), new(  6,   4), new( 11,  14), new( 10,  16), new( 19, -14), new( 20, -38), new( 29, -62), 
         new( -8,  -4), new( -4,  -6), new(  2,   3), new( 10,  14), new( 11,  -4), new( -3, -11), new(  3, -21), new(  2, -34), 

         new( -1, -75), new( 18, -39), new( 21, -25), new(-58,   8), new(-25,  -6), new( -5,   0), new( 29,  -2), new( 49, -81), 
         new(-72,  -5), new(-18,  16), new(-52,  24), new( 46,   8), new( -1,  23), new( -1,  40), new( 29,  32), new( -3,  12), 
         new(-90,   1), new( 24,  13), new(-40,  28), new(-58,  38), new(-19,  38), new( 55,  32), new( 28,  33), new(-17,   8), 
         new(-57, -15), new(-44,   9), new(-76,  29), new(-120,  41), new(-112,  43), new(-70,  38), new(-62,  29), new(-105,   9), 
         new(-64, -26), new(-44,  -2), new(-71,  19), new(-110,  36), new(-103,  35), new(-61,  22), new(-66,  12), new(-122,   5), 
         new(-22, -27), new( 13, -12), new(-36,   6), new(-50,  18), new(-41,  18), new(-40,  12), new( -9,  -1), new(-47,  -6), 
         new( 54, -21), new( 27,   0), new( 13,  -5), new(-10,   0), new(-10,   2), new( -2,   0), new( 33,   1), new( 31, -11), 
         new(  8, -36), new( 38, -21), new( 13,  -1), new(-35, -16), new( -1, -13), new( -2, -20), new( 37, -19), new( 29, -47), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-34, -52),
         new(-17, -17),
         new(-8, 3),
         new(-2, 13),
         new(2, 22),
         new(6, 30),
         new(13, 29),
         new(18, 26),
         new(23, 17),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-49, -52),
         new(-26, -32),
         new(-12, -13),
         new(-5, 1),
         new(2, 10),
         new(10, 21),
         new(14, 26),
         new(16, 30),
         new(18, 34),
         new(21, 33),
         new(26, 31),
         new(36, 28),
         new(31, 34),
         new(47, 20),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-33, -19),
         new(-23, 0),
         new(-19, 1),
         new(-13, 4),
         new(-14, 12),
         new(-8, 15),
         new(-4, 21),
         new(0, 23),
         new(6, 27),
         new(11, 31),
         new(16, 33),
         new(17, 39),
         new(20, 43),
         new(22, 41),
         new(15, 45),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-23, -46),
         new(-20, -79),
         new(-25, -16),
         new(-22, 7),
         new(-21, 25),
         new(-17, 28),
         new(-15, 43),
         new(-14, 53),
         new(-12, 59),
         new(-11, 63),
         new(-10, 68),
         new(-7, 73),
         new(-6, 74),
         new(-7, 80),
         new(-5, 80),
         new(-3, 84),
         new(-4, 92),
         new(-2, 90),
         new(5, 87),
         new(19, 78),
         new(22, 80),
         new(68, 53),
         new(60, 57),
         new(81, 36),
         new(138, 27),
         new(98, 24),
         new(52, 53),
         new(36, 44),
      ];

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(12, -6),
         new(19, -5),
         new(24, -6),
         new(14, 12),
      ];

      public static Score[] PawnShield =
      [
         new(-30, -7),
         new(11, -14),
         new(48, -29),
         new(69, -63),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(40, -32),
         new(21, -9),
         new(5, 28),
         new(10, 65),
         new(-10, 141),
         new(14, 119),
      ];

      public static Score[] DefendedPawn = [
         new(-26, -20),
         new(-10, -12),
         new(5, -2),
         new(19, 13),
         new(33, 31),
         new(46, 39),
         new(42, 11),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-22, -11),
         new(-8, 0),
         new(6, 6),
         new(20, 21),
         new(33, 26),
         new(44, 92),
         new(54, -4),
         new(-21, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(0, 5),
         new(4, 11),
         new(10, 11),
         new(8, 17),
         new(14, 17),
         new(8, 10),
         new(2, 10),
         new(14, 5),
      ];

      public static Score FriendlyKingPawnDistance = new(7, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
   }
}
