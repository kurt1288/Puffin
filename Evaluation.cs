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
         int[] defender = [0, 0];
         int[] connected = [0, 0];

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

            // Defending pawn
            if ((PawnAttacks[(int)color][square] & colorPawns[(int)color].Value) != 0)
            {
               defender[(int)color]++;
            }

            // Connected pawn
            if ((((SquareBB[square] & ~FILE_MASKS[(int)File.H]) << 1) & colorPawns[(int)color].Value) != 0)
            {
               connected[(int)color]++;
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & colorPawns[(int)color].Value) == 0)
            {
               // Penalty is based on file
               score -= IsolatedPawn[square & 7] * (1 - 2 * (int)color);
            }
         }

         score += DefendedPawn[defender[(int)Color.White]] - DefendedPawn[defender[(int)Color.Black]];
         score += ConnectedPawn[connected[(int)Color.White]] - ConnectedPawn[connected[(int)Color.Black]];
         mobilitySquares[(int)Color.White] = ~mobilitySquares[(int)Color.White];
         mobilitySquares[(int)Color.Black] = ~mobilitySquares[(int)Color.Black];
      }

      public static readonly Score[] PieceValues = [
         new(64, 105),
         new(292, 344),
         new(322, 364),
         new(412, 630),
         new(837, 1159),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 18, 108), new( 39,  97), new( 20, 101), new( 52,  51), new( 32,  51), new( 14,  59), new(-67, 102), new(-91, 120), 
         new(  0,  38), new(  4,  43), new( 31,   2), new( 37, -33), new( 45, -39), new( 76, -23), new( 46,  16), new( 19,  21), 
         new(-20,  20), new( -5,   9), new( -5,  -5), new( -1, -22), new( 19, -20), new( 17, -19), new(  7,   0), new(  6,  -2), 
         new(-25,   0), new(-16,   0), new( -7, -12), new(  2, -19), new(  8, -18), new(  3, -17), new( -4,  -9), new( -5, -15), 
         new(-28,  -4), new(-17,  -7), new(-15, -11), new(-12, -10), new(  0,  -7), new( -7, -13), new(  5, -18), new( -3, -20), 
         new(-28,  -1), new(-15,  -2), new(-17,  -6), new( -8,  -7), new( -8,   3), new(-10,  -2), new( -5,  -8), new(-31, -11), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-153, -52), new(-123,  -9), new(-72,   6), new(-42,  -1), new( -9,   0), new(-64, -21), new(-93,  -4), new(-101, -73), 
         new(-27,  -9), new(-12,   2), new(  9,   2), new( 31,   2), new( 13,   0), new( 61, -15), new( -8,   0), new( 11, -24), 
         new( -8,  -4), new( 20,   2), new( 28,  23), new( 34,  26), new( 58,  16), new( 77,   3), new( 25,   0), new( 14,  -9), 
         new( -4,   9), new(  0,  16), new( 22,  31), new( 44,  33), new( 17,  38), new( 40,  33), new( -3,  27), new( 20,   5), 
         new(-11,  11), new( -3,  10), new(  8,  31), new( 14,  32), new( 21,  35), new( 18,  24), new( 23,  12), new(  2,   9), 
         new(-30,  -5), new(-11,   1), new( -1,   7), new(  3,  25), new( 16,  22), new(  4,   5), new(  6,  -1), new( -8,   2), 
         new(-32,  -6), new(-23,   0), new(-12,   1), new(  3,   1), new(  5,   0), new(  4,  -3), new( -7,  -4), new( -6,   6), 
         new(-69,  -6), new(-21, -13), new(-31,  -4), new(-12,   0), new( -7,   1), new( -5,  -7), new(-16,  -8), new(-41,  -6), 

         new(-36,   2), new(-63,  10), new(-59,   5), new(-96,  15), new(-95,  13), new(-78,   0), new(-43,   1), new(-69,  -2), 
         new(-27,  -9), new( -6,  -3), new(-15,   0), new(-25,   3), new(-12,  -4), new(-16,  -2), new(-31,   3), new(-24, -10), 
         new(-15,   8), new(  5,   0), new(  2,   6), new( 11,  -1), new( -4,   5), new( 24,   8), new(  4,   3), new( -7,  10), 
         new(-21,   3), new( -1,   8), new(  0,   7), new( 14,  21), new( 11,  13), new(  1,  12), new(  1,   4), new(-28,   8), 
         new( -9,   0), new(-20,   9), new( -7,  16), new( 14,  15), new(  9,  13), new( -2,   9), new(-13,   9), new( 10, -10), 
         new(-10,   1), new(  5,  10), new(  0,  12), new(  0,  14), new(  6,  17), new(  5,   9), new(  9,   2), new(  7,  -5), 
         new(  7,   5), new(  1,  -6), new(  7,  -6), new( -5,   4), new(  0,   6), new( 10,  -4), new( 21,  -4), new(  6, -11), 
         new( -4,  -7), new( 16,   4), new(  3,   0), new(-12,   3), new(  0,  -1), new( -2,  10), new(  9,  -9), new( 13, -20), 

         new( -4,  26), new(-20,  35), new(-23,  44), new(-27,  40), new(-14,  34), new(  4,  28), new( -2,  33), new( 20,  23), 
         new(-18,  27), new(-23,  40), new( -6,  42), new( 11,  33), new( -4,  34), new( 17,  22), new( 13,  19), new( 40,   7), 
         new(-20,  25), new(  4,  25), new(  3,  26), new(  8,  23), new( 34,  11), new( 31,   7), new( 66,   2), new( 33,   1), 
         new(-24,  28), new(-13,  27), new( -6,  30), new( -1,  27), new(  3,  14), new(  0,  10), new(  7,  13), new(  1,   8), 
         new(-34,  22), new(-37,  26), new(-24,  25), new(-15,  23), new(-15,  20), new(-32,  19), new( -9,  11), new(-20,   9), 
         new(-39,  18), new(-31,  15), new(-22,  14), new(-21,  17), new(-13,  12), new(-16,   3), new(  7,  -9), new(-15,  -6), 
         new(-39,   9), new(-30,  14), new(-10,  11), new( -8,  11), new( -4,   4), new( -2,  -2), new( 10,  -9), new(-24,  -1), 
         new(-18,   7), new(-15,  10), new( -4,  13), new(  4,   8), new(  9,   2), new( -2,   4), new(  4,  -1), new(-15,  -2), 

         new(-47,  20), new(-56,  35), new(-37,  51), new(-11,  37), new(-20,  37), new(-14,  38), new( 30, -12), new(-21,  20), 
         new(-16,   3), new(-39,  34), new(-39,  66), new(-50,  85), new(-58, 100), new(-13,  50), new(-27,  50), new( 33,  23), 
         new( -8,  17), new(-12,  27), new(-15,  60), new(-13,  62), new( -5,  66), new( 17,  48), new( 26,  23), new( 15,  22), 
         new(-21,  33), new(-13,  43), new(-18,  49), new(-23,  72), new(-16,  72), new(-11,  61), new(  0,  59), new( -4,  42), 
         new( -8,  20), new(-22,  45), new(-17,  49), new( -9,  59), new(-10,  62), new( -9,  52), new( -1,  41), new(  3,  33), 
         new(-12,   4), new( -3,  22), new( -5,  35), new( -8,  41), new( -2,  49), new(  2,  34), new( 10,  17), new(  3,  10), 
         new( -5,  -3), new( -3,   1), new(  5,   5), new( 11,  13), new( 11,  16), new( 18, -13), new( 19, -38), new( 27, -61), 
         new( -8,  -4), new( -4,  -5), new(  3,   3), new( 11,  14), new( 11,  -3), new( -3, -12), new(  2, -20), new(  1, -33), 

         new( -1, -75), new( 18, -39), new( 22, -25), new(-57,   7), new(-24,  -6), new( -5,   0), new( 30,  -2), new( 49, -81), 
         new(-71,  -4), new(-17,  16), new(-52,  24), new( 46,   8), new( -1,  23), new( -1,  40), new( 27,  33), new( -1,  11), 
         new(-89,   1), new( 23,  14), new(-39,  27), new(-59,  38), new(-19,  38), new( 54,  32), new( 28,  33), new(-17,   8), 
         new(-56, -15), new(-45,   9), new(-77,  29), new(-120,  41), new(-112,  42), new(-71,  38), new(-63,  29), new(-105,  10), 
         new(-63, -25), new(-44,  -2), new(-71,  19), new(-111,  36), new(-103,  35), new(-62,  22), new(-66,  12), new(-122,   6), 
         new(-22, -26), new( 12, -11), new(-36,   7), new(-51,  18), new(-42,  18), new(-40,  12), new(-10,  -1), new(-45,  -5), 
         new( 54, -21), new( 25,  -1), new( 13,  -5), new(-10,   0), new(-10,   3), new( -2,   0), new( 33,   0), new( 32, -12), 
         new(  8, -36), new( 38, -21), new( 13,  -1), new(-34, -16), new( -1, -14), new( -2, -20), new( 37, -19), new( 30, -47), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-34, -52),
         new(-17, -16),
         new(-8, 4),
         new(-2, 14),
         new(2, 22),
         new(6, 30),
         new(13, 29),
         new(18, 26),
         new(23, 17),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-49, -52),
         new(-27, -32),
         new(-13, -14),
         new(-5, 1),
         new(2, 11),
         new(10, 22),
         new(14, 26),
         new(16, 30),
         new(18, 34),
         new(21, 33),
         new(26, 31),
         new(36, 28),
         new(31, 34),
         new(48, 20),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-32, -20),
         new(-23, 0),
         new(-19, 1),
         new(-13, 4),
         new(-14, 12),
         new(-8, 15),
         new(-4, 21),
         new(0, 23),
         new(6, 26),
         new(11, 31),
         new(16, 33),
         new(17, 39),
         new(20, 43),
         new(22, 41),
         new(15, 45),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-22, -44),
         new(-20, -81),
         new(-25, -17),
         new(-22, 7),
         new(-21, 25),
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
         new(-4, 92),
         new(-2, 90),
         new(5, 87),
         new(19, 78),
         new(22, 79),
         new(68, 53),
         new(60, 57),
         new(81, 37),
         new(138, 27),
         new(98, 24),
         new(52, 53),
         new(36, 45),
      ];

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(12, -6),
         new(19, -5),
         new(24, -6),
         new(15, 12),
      ];

      public static Score[] PawnShield =
      [
         new(-31, -6),
         new(13, -12),
         new(46, -31),
         new(69, -62),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(39, -32),
         new(20, -9),
         new(5, 28),
         new(10, 65),
         new(-10, 141),
         new(14, 119),
      ];

      public static Score[] DefendedPawn = [
         new(-25, -21),
         new(-11, -13),
         new(4, -1),
         new(19, 16),
         new(32, 32),
         new(42, 45),
         new(54, 37),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-24, -12),
         new(-9, -1),
         new(6, 6),
         new(22, 22),
         new(36, 28),
         new(47, 98),
         new(56, -4),
         new(-21, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(-1, 5),
         new(5, 10),
         new(9, 11),
         new(8, 16),
         new(14, 17),
         new(7, 10),
         new(5, 10),
         new(10, 5),
      ];

      public static Score FriendlyKingPawnDistance = new(7, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
   }
}
