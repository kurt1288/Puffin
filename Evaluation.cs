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

               if ((board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & FILE_MASKS[kingSq & 7]) == 0)
               {
                  score -= (board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color ^ 1].Value & FILE_MASKS[kingSq & 7]) == 0
                     ? KingOpenFile * (1 - 2 * (int)color)
                     : KingHalfOpenFile * (1 - 2 * (int)color);
               }
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
         new(294, 344),
         new(324, 364),
         new(415, 629),
         new(839, 1157),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 19, 109), new( 37,  99), new( 21, 101), new( 55,  50), new( 34,  52), new( 21,  58), new(-72, 103), new(-99, 126), 
         new(  3,  36), new(  5,  42), new( 32,   1), new( 41, -36), new( 50, -40), new( 78, -25), new( 42,  16), new( 15,  22), 
         new(-18,  18), new( -5,  10), new( -2,  -6), new(  3, -24), new( 20, -20), new( 20, -21), new(  5,   0), new(  2,   0), 
         new(-23,   0), new(-15,   0), new( -5, -14), new(  6, -21), new(  7, -18), new(  7, -20), new( -5,  -8), new( -7, -14), 
         new(-28,  -6), new(-19,  -7), new(-14, -14), new( -9, -12), new( -1,  -8), new(-26,  -9), new(-17, -10), new(-27, -11), 
         new(-23,  -1), new(-14,  -2), new(-12,  -6), new(  0,  -9), new( -2,   4), new( -3,  -4), new( -3, -10), new(-27, -10), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-150, -52), new(-120,  -9), new(-73,   6), new(-42,  -2), new( -8,   0), new(-66, -21), new(-93,  -4), new(-105, -71), 
         new(-27,  -8), new(-11,   2), new( 12,   1), new( 32,   1), new( 13,  -1), new( 63, -16), new( -4,  -1), new( 10, -22), 
         new( -8,  -3), new( 21,   1), new( 28,  22), new( 35,  24), new( 59,  15), new( 80,   1), new( 28,  -2), new( 11,  -8), 
         new( -4,   9), new(  1,  16), new( 22,  30), new( 45,  32), new( 18,  36), new( 42,  32), new( -1,  26), new( 21,   5), 
         new(-11,  12), new( -3,   9), new(  8,  30), new( 14,  31), new( 22,  34), new( 17,  24), new( 23,  11), new(  2,  10), 
         new(-31,  -4), new(-11,   1), new( -1,   7), new(  3,  24), new( 17,  21), new(  4,   3), new(  7,  -2), new( -9,   2), 
         new(-33,  -5), new(-24,   1), new(-12,   0), new(  3,   0), new(  4,   0), new(  2,  -3), new( -7,  -4), new( -6,   7), 
         new(-70,  -6), new(-22, -12), new(-32,  -4), new(-12,   0), new( -7,   0), new( -6,  -7), new(-17,  -7), new(-41,  -5), 

         new(-35,   2), new(-63,   9), new(-59,   5), new(-97,  15), new(-94,  13), new(-80,   0), new(-43,   1), new(-70,  -2), 
         new(-29,  -9), new( -5,  -3), new(-13,   0), new(-25,   3), new(-13,  -4), new(-13,  -3), new(-32,   3), new(-35,  -7), 
         new(-15,   8), new(  4,   0), new(  3,   6), new( 11,  -2), new( -2,   4), new( 25,   8), new(  2,   3), new( -8,  10), 
         new(-21,   3), new( -1,   8), new(  0,   7), new( 15,  20), new( 12,  12), new(  2,  12), new(  0,   4), new(-28,   8), 
         new( -8,   0), new(-19,   8), new( -7,  15), new( 15,  14), new( 10,  13), new( -2,   8), new(-12,   8), new( 10, -12), 
         new(-10,   0), new(  6,   9), new(  0,  11), new(  0,  13), new(  7,  16), new(  5,   8), new( 10,   1), new(  8,  -6), 
         new(  6,   4), new(  2,  -6), new(  9,  -7), new( -4,   4), new(  0,   5), new( 10,  -5), new( 21,  -1), new(  8, -10), 
         new( -3,  -7), new( 17,   3), new(  3,   0), new(-11,   2), new(  0,  -1), new( -2,  10), new(  9, -10), new( 14, -20), 

         new( -6,  27), new(-20,  35), new(-25,  44), new(-27,  40), new(-14,  34), new(  4,  28), new(  3,  31), new( 21,  23), 
         new(-18,  26), new(-23,  39), new( -5,  42), new( 11,  33), new( -3,  33), new( 21,  20), new( 19,  17), new( 41,   7), 
         new(-20,  25), new(  3,  25), new(  3,  26), new(  8,  23), new( 35,  11), new( 33,   6), new( 65,   1), new( 32,   2), 
         new(-25,  29), new(-13,  27), new( -6,  30), new( -2,  27), new(  1,  15), new(  1,  10), new(  7,  13), new(  0,   8), 
         new(-35,  22), new(-37,  27), new(-23,  25), new(-15,  22), new(-15,  20), new(-30,  19), new( -9,  11), new(-20,   9), 
         new(-39,  18), new(-30,  16), new(-22,  14), new(-21,  17), new(-13,  12), new(-14,   3), new(  9,  -9), new(-12,  -6), 
         new(-38,   9), new(-29,  14), new(-10,  10), new( -8,  10), new( -4,   4), new( -2,  -3), new( 11, -10), new(-23,  -2), 
         new(-18,   8), new(-15,  10), new( -4,  13), new(  4,   7), new(  8,   2), new( -1,   4), new(  2,   0), new(-13,  -2), 

         new(-47,  20), new(-56,  35), new(-36,  51), new(-14,  37), new(-20,  36), new(-17,  39), new( 34, -12), new(-20,  22), 
         new(-15,   3), new(-37,  33), new(-38,  65), new(-50,  85), new(-57,  98), new(-11,  47), new(-19,  46), new( 34,  23), 
         new( -7,  17), new(-12,  27), new(-13,  60), new(-12,  60), new( -3,  65), new( 18,  47), new( 25,  25), new( 14,  24), 
         new(-21,  32), new(-13,  43), new(-17,  48), new(-24,  72), new(-15,  71), new( -9,  60), new(  0,  60), new( -5,  43), 
         new( -9,  21), new(-21,  46), new(-17,  50), new( -8,  59), new(-10,  61), new( -9,  52), new( -1,  43), new(  4,  32), 
         new(-12,   4), new( -4,  23), new( -5,  35), new( -8,  41), new( -2,  48), new(  2,  34), new( 12,  17), new(  5,   8), 
         new( -5,  -3), new( -3,   2), new(  5,   5), new( 11,  13), new( 10,  16), new( 18, -14), new( 19, -37), new( 27, -61), 
         new( -8,  -5), new( -4,  -5), new(  2,   4), new( 10,  15), new( 11,  -4), new( -3, -11), new(  0, -21), new(  1, -34), 

         new( -2, -76), new( 13, -39), new( 13, -25), new(-67,   8), new(-30,  -6), new(-12,   0), new( 24,  -3), new( 48, -83), 
         new(-75,  -5), new(-22,  16), new(-61,  25), new( 36,  10), new(-11,  25), new(-11,  41), new( 23,  32), new( -7,  11), 
         new(-96,   1), new( 18,  14), new(-47,  29), new(-69,  40), new(-28,  40), new( 49,  33), new( 26,  32), new(-17,   6), 
         new(-60, -16), new(-50,  10), new(-83,  30), new(-127,  43), new(-119,  43), new(-75,  38), new(-65,  28), new(-107,   8), 
         new(-65, -26), new(-48,  -2), new(-73,  19), new(-114,  36), new(-108,  36), new(-64,  22), new(-70,  12), new(-123,   5), 
         new(-20, -28), new( 14, -13), new(-36,   6), new(-48,  17), new(-40,  17), new(-39,  11), new( -8,  -2), new(-41,  -8), 
         new( 45, -19), new( 20,   0), new( 19,  -7), new( -5,  -1), new( -4,   1), new(  8,  -3), new( 29,   2), new( 25,  -8), 
         new(  8, -35), new( 38, -21), new( 14,  -2), new(-29, -18), new(  4, -13), new(  5, -23), new( 35, -18), new( 28, -45), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-34, -54),
         new(-17, -18),
         new(-7, 2),
         new(-2, 12),
         new(2, 21),
         new(6, 30),
         new(13, 29),
         new(19, 26),
         new(25, 17),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-50, -50),
         new(-26, -32),
         new(-12, -14),
         new(-5, 0),
         new(3, 10),
         new(10, 21),
         new(14, 26),
         new(17, 30),
         new(18, 34),
         new(21, 33),
         new(26, 31),
         new(36, 28),
         new(32, 34),
         new(47, 20),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-32, -19),
         new(-22, 0),
         new(-19, 0),
         new(-13, 4),
         new(-14, 11),
         new(-7, 15),
         new(-4, 21),
         new(1, 23),
         new(7, 26),
         new(11, 31),
         new(16, 33),
         new(17, 39),
         new(21, 43),
         new(22, 41),
         new(15, 45),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-20, -47),
         new(-20, -76),
         new(-25, -16),
         new(-22, 6),
         new(-21, 23),
         new(-17, 27),
         new(-15, 43),
         new(-14, 53),
         new(-11, 59),
         new(-10, 62),
         new(-9, 68),
         new(-7, 73),
         new(-6, 74),
         new(-6, 79),
         new(-4, 79),
         new(-3, 84),
         new(-3, 91),
         new(-1, 89),
         new(5, 87),
         new(20, 77),
         new(22, 79),
         new(69, 53),
         new(60, 57),
         new(80, 37),
         new(138, 26),
         new(97, 25),
         new(52, 51),
         new(34, 43),
      ];

      public static Score KingOpenFile = new(72, -8);
      public static Score KingHalfOpenFile = new(27, -25);

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(10, -6),
         new(19, -4),
         new(23, -5),
         new(14, 13),
      ];

      public static Score[] PawnShield =
      [
         new(-26, -5),
         new(6, -18),
         new(40, -28),
         new(69, -42),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(30, -27),
         new(12, -6),
         new(0, 31),
         new(7, 68),
         new(-13, 144),
         new(14, 120),
      ];

      public static Score[] DefendedPawn = [
         new(-24, -21),
         new(-8, -13),
         new(6, -2),
         new(19, 13),
         new(30, 32),
         new(42, 40),
         new(35, 18),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-18, -8),
         new(-6, 2),
         new(4, 3),
         new(15, 14),
         new(26, 11),
         new(36, 75),
         new(49, -4),
         new(-11, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(2, 4),
         new(5, 11),
         new(10, 11),
         new(9, 16),
         new(13, 18),
         new(5, 11),
         new(3, 10),
         new(6, 8),
      ];

      public static Score FriendlyKingPawnDistance = new(6, -9);
      public static Score EnemyKingPawnDistance = new(-6, 9);
   }
}
