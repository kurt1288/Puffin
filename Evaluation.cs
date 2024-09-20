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

         Pawns(board, kingSquares, ref mobilitySquares, occupied, ref score);
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
            score += KnightMobility[new Bitboard(KnightAttacks[square] & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

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
            score += BishopMobility[new Bitboard(moves & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

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
            score += RookMobility[new Bitboard(moves & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

            if ((FILE_MASKS[square & 7] & board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value) == 0)
            {
               if ((FILE_MASKS[square & 7] & board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color ^ 1].Value) == 0)
               {
                  score += RookOpenFile * (1 - 2 * (int)color);
               }
               else
               {
                  score += RookHalfOpenFile * (1 - 2 * (int)color);
               }
            }

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
            score += QueenMobility[new Bitboard(moves & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

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

      private static void Pawns(Board board, int[] kingSquares, ref ulong[] mobilitySquares, ulong occupied, ref Score score)
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

            // Remove blocked pawns from mobility squares
            if ((SquareBB[square + (color == Color.White ? -8 : 8)] & occupied) != 0) {
               mobilitySquares[(int)color] ^= SquareBB[square];
            }

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
         new(62, 108),
         new(295, 341),
         new(328, 356),
         new(406, 627),
         new(836, 1164),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 24, 110), new( 35,  99), new( 22, 101), new( 58,  50), new( 30,  51), new( 18,  57), new(-78, 104), new(-95, 126),
         new(  7,  36), new(  1,  42), new( 29,   1), new( 35, -34), new( 47, -40), new( 75, -26), new( 38,  16), new( 17,  23),
         new(-15,  18), new( -8,  10), new( -6,  -6), new( -1, -23), new( 17, -20), new( 20, -21), new(  2,   0), new(  3,   0),
         new(-19,   0), new(-16,   0), new( -7, -14), new(  2, -21), new(  4, -18), new(  8, -20), new( -5,  -9), new( -5, -13),
         new(-26,  -6), new(-18,  -7), new(-15, -13), new(-10, -12), new(  0,  -9), new(-25,  -9), new(-15, -11), new(-28, -11),
         new(-19,  -2), new(-13,  -3), new( -8,  -8), new( -3, -10), new(  0,   1), new(  1,  -5), new(  0, -11), new(-26,  -9),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-117, -34), new(-111,  -2), new(-73,  11), new(-40,   1), new(-16,   5), new(-67, -16), new(-87,   2), new(-80, -50),
         new(-15,   0), new( -5,   6), new( 15,   1), new( 33,   0), new( 18,  -4), new( 58, -16), new(  4,   2), new( 15, -12),
         new( -4,   0), new( 16,   1), new( 24,  15), new( 31,  16), new( 56,   5), new( 72,  -6), new( 29,  -5), new( 16,  -5),
         new(  1,  13), new(  2,  13), new( 19,  24), new( 42,  26), new( 20,  26), new( 41,  22), new(  6,  17), new( 30,   5),
         new( -1,  14), new( -3,   7), new(  2,  25), new(  9,  26), new( 15,  29), new( 14,  17), new( 24,   6), new( 11,  13),
         new(-19,   1), new(-12,   2), new( -9,   5), new( -8,  21), new(  5,  18), new( -4,   1), new( 10,  -1), new( -2,   4),
         new(-22,   3), new(-17,   6), new(-15,   0), new( -1,   1), new( -2,   0), new(  0,  -2), new(  2,   2), new(  0,  17),
         new(-54,  10), new(-14,  -4), new(-30,  -3), new(-13,  -1), new( -9,   2), new( -5,  -5), new(-10,   1), new(-25,  10),

         new(-31,   5), new(-67,  11), new(-60,   7), new(-100,  17), new(-93,  15), new(-81,   1), new(-48,   3), new(-66,   0),
         new(-28,  -5), new( -8,  -2), new(-17,   0), new(-28,   3), new(-11,  -6), new(-17,  -3), new(-29,   1), new(-35,  -3),
         new(-15,  11), new(  0,   0), new(  1,   4), new( 11,  -4), new(  0,   0), new( 32,   3), new( 10,   0), new(  4,   8),
         new(-23,   6), new( -2,   8), new(  0,   3), new( 17,  16), new( 11,   8), new(  4,   8), new(  0,   4), new(-23,   9),
         new( -6,   1), new(-18,   7), new( -6,  13), new( 12,  12), new(  8,  10), new( -5,   7), new(-13,   8), new( 14, -11),
         new( -5,   4), new(  8,   6), new(  0,  10), new(  1,  12), new(  6,  15), new(  5,   8), new(  9,   0), new( 13,  -2),
         new( 14,   9), new(  1,  -4), new( 12,  -7), new( -7,   5), new( -1,   8), new( 10,  -3), new( 23,   0), new( 12,  -2),
         new(  1,   1), new( 17,   7), new(  1,   0), new(-11,   3), new( -1,  -1), new( -6,  14), new(  9,  -4), new( 19, -10),

         new(-14,  29), new(-22,  33), new(-27,  41), new(-31,  37), new(-17,  31), new(  3,  27), new(  5,  29), new( 17,  24),
         new(-25,  27), new(-26,  38), new(-12,  40), new(  1,  30), new(-11,  31), new( 12,  21), new( 15,  16), new( 32,   9),
         new(-30,  25), new( -6,  23), new( -6,  24), new( -5,  21), new( 22,  10), new( 26,   4), new( 62,   0), new( 32,   0),
         new(-29,  27), new(-16,  23), new(-14,  27), new(-10,  22), new( -6,  11), new(  3,   5), new( 11,  10), new(  4,   4),
         new(-34,  19), new(-38,  22), new(-27,  19), new(-23,  16), new(-22,  15), new(-24,  12), new( -1,   6), new(-11,   4),
         new(-36,  13), new(-30,  10), new(-27,   6), new(-25,   7), new(-15,   2), new( -8,  -5), new( 20, -17), new(  0, -13),
         new(-33,   3), new(-30,   7), new(-17,   4), new(-15,   2), new( -9,  -4), new(  0, -11), new( 13, -17), new(-18,  -8),
         new(-16,   7), new(-16,   5), new(-12,  10), new( -3,   2), new(  2,  -3), new(  0,   0), new(  2,  -5), new( -8,  -5),

         new(-36,  32), new(-48,  53), new(-32,  74), new(-10,  67), new(-11,  66), new( -5,  63), new( 43,   8), new(-14,  45),
         new(-13,  13), new(-39,  45), new(-40,  80), new(-52, 104), new(-48, 118), new( -8,  74), new(-16,  63), new( 39,  46),
         new(-12,  24), new(-18,  32), new(-20,  67), new(-15,  75), new( -3,  87), new( 31,  70), new( 39,  40), new( 35,  44),
         new(-23,  33), new(-16,  42), new(-19,  50), new(-24,  76), new(-22,  88), new( -8,  77), new(  1,  75), new(  0,  62),
         new( -8,  18), new(-23,  46), new(-19,  49), new(-16,  64), new(-16,  66), new(-13,  60), new( -3,  53), new(  8,  46),
         new( -6,   2), new( -3,  21), new(-11,  36), new(-12,  36), new( -8,  40), new(  0,  33), new( 13,  18), new( 11,  12),
         new( -5,  -1), new( -7,   0), new(  0,   2), new(  2,   7), new(  1,  11), new(  9, -14), new( 13, -37), new( 24, -54),
         new(-10,  -7), new( -8,  -5), new( -2,  -4), new(  3,   8), new(  3,  -8), new( -9,  -8), new( -5, -14), new(  3, -33),

         new( -4, -76), new( 12, -39), new( 17, -27), new(-75,   8), new(-32,  -7), new( -2,  -2), new( 43,  -5), new( 61, -85),
         new(-75,  -6), new(-25,  16), new(-67,  25), new( 27,  11), new(-20,  26), new(-15,  41), new( 22,  32), new( 10,   8),
         new(-93,   0), new( 12,  15), new(-52,  30), new(-79,  41), new(-34,  41), new( 44,  33), new( 26,  32), new(-14,   5),
         new(-54, -17), new(-55,  10), new(-86,  31), new(-136,  44), new(-127,  45), new(-77,  39), new(-65,  28), new(-101,   7),
         new(-60, -27), new(-52,  -1), new(-73,  19), new(-119,  37), new(-110,  36), new(-66,  23), new(-70,  13), new(-119,   4),
         new(-12, -30), new( 21, -14), new(-34,   5), new(-47,  17), new(-39,  17), new(-39,  11), new( -7,  -2), new(-38,  -8),
         new( 50, -18), new( 21,   0), new( 22,  -8), new( -7,  -1), new( -6,   1), new(  8,  -3), new( 29,   3), new( 25,  -5),
         new(  9, -33), new( 34, -20), new( 13,  -4), new(-30, -19), new(  4, -13), new(  4, -23), new( 34, -18), new( 30, -44),
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-96, -117),
         new(-46, -48),
         new(-24, -13),
         new(-14, 5),
         new(-3, 17),
         new(1, 28),
         new(12, 30),
         new(20, 33),
         new(34, 27),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-54, -97),
         new(-37, -67),
         new(-23, -20),
         new(-13, 0),
         new(-2, 9),
         new(6, 20),
         new(12, 29),
         new(17, 33),
         new(18, 38),
         new(22, 38),
         new(25, 40),
         new(33, 35),
         new(37, 39),
         new(43, 28),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-106, -162),
         new(-30, -85),
         new(-27, -25),
         new(-21, -5),
         new(-15, 2),
         new(-11, 9),
         new(-9, 17),
         new(-6, 21),
         new(-2, 23),
         new(2, 27),
         new(6, 31),
         new(6, 36),
         new(10, 38),
         new(16, 37),
         new(20, 36),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-18, -1),
         new(-80, -25),
         new(-60, -57),
         new(-56, -108),
         new(-36, -52),
         new(-36, 13),
         new(-24, 9),
         new(-21, 29),
         new(-19, 45),
         new(-14, 61),
         new(-10, 60),
         new(-7, 67),
         new(-4, 75),
         new(0, 75),
         new(1, 81),
         new(2, 86),
         new(3, 91),
         new(3, 97),
         new(2, 107),
         new(7, 104),
         new(14, 106),
         new(19, 101),
         new(31, 103),
         new(59, 83),
         new(49, 103),
         new(148, 45),
         new(64, 85),
         new(37, 79),
      ];

      public static Score RookHalfOpenFile = new(10, 10);
      public static Score RookOpenFile = new(29, 7);
      public static Score KingOpenFile = new(69, -7);
      public static Score KingHalfOpenFile = new(26, -26);

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(10, -6),
         new(12, 1),
         new(26, -10),
         new(17, 12),
      ];

      public static Score[] PawnShield =
      [
         new(-28, -5),
         new(5, -19),
         new(39, -28),
         new(72, -43),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(33, -27),
         new(16, -6),
         new(2, 30),
         new(10, 67),
         new(-9, 144),
         new(14, 121),
      ];

      public static Score[] DefendedPawn = [
         new(-24, -20),
         new(-8, -13),
         new(6, -2),
         new(18, 13),
         new(29, 31),
         new(41, 38),
         new(35, 21),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-15, -8),
         new(-4, 2),
         new(4, 4),
         new(13, 13),
         new(21, 11),
         new(24, 55),
         new(41, -4),
         new(-7, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(2, 5),
         new(3, 11),
         new(10, 11),
         new(8, 17),
         new(13, 18),
         new(5, 11),
         new(3, 10),
         new(6, 9),
      ];

      public static Score FriendlyKingPawnDistance = new(6, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
   }
}
