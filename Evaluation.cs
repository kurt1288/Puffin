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
         
         if ((bishopBB & board.ColorBB[(int)Color.White]).CountBits() >= 2)
         {
            score += BishopPair;
         }
         if ((bishopBB & board.ColorBB[(int)Color.Black]).CountBits() >= 2)
         {
            score -= BishopPair;
         }

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
         // Set mobility squares to all squares NOT attacked by pawns
         mobilitySquares[(int)Color.White] = ~PawnAnyAttacks(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)Color.Black].Value, Color.Black);
         mobilitySquares[(int)Color.Black] = ~PawnAnyAttacks(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)Color.White].Value, Color.White);

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
      }

      public static readonly Score[] PieceValues = [
         new(62, 108),
         new(297, 340),
         new(310, 351),
         new(406, 630),
         new(850, 1157),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 24, 110), new( 36,  98), new( 22, 102), new( 59,  51), new( 30,  52), new( 18,  58), new(-78, 104), new(-96, 126),
         new(  7,  36), new(  1,  43), new( 29,   2), new( 35, -33), new( 45, -39), new( 75, -25), new( 39,  17), new( 17,  23),
         new(-15,  18), new( -8,  10), new( -5,  -6), new( -1, -22), new( 17, -20), new( 20, -21), new(  2,   0), new(  3,   0),
         new(-19,  -1), new(-16,   0), new( -7, -14), new(  3, -21), new(  4, -17), new(  9, -21), new( -4,  -9), new( -5, -13),
         new(-26,  -6), new(-19,  -7), new(-15, -13), new(-10, -11), new(  0,  -9), new(-25, -10), new(-15, -11), new(-28, -11),
         new(-19,  -2), new(-13,  -2), new( -9,  -8), new( -3,  -9), new(  0,   1), new(  1,  -6), new( -1, -11), new(-26,  -9),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-116, -31), new(-110,  -2), new(-72,  11), new(-38,   0), new(-13,   4), new(-65, -17), new(-88,   2), new(-77, -49),
         new(-15,   0), new( -6,   6), new( 16,   2), new( 32,   0), new( 18,  -4), new( 58, -17), new(  3,   2), new( 14, -13),
         new( -5,   0), new( 16,   2), new( 24,  15), new( 31,  17), new( 56,   5), new( 71,  -6), new( 30,  -6), new( 15,  -6),
         new(  0,  14), new(  2,  14), new( 19,  24), new( 42,  26), new( 21,  26), new( 40,  22), new(  6,  18), new( 29,   6),
         new( -2,  16), new( -3,   9), new(  2,  26), new(  9,  27), new( 16,  30), new( 14,  18), new( 24,   6), new( 10,  13),
         new(-19,   3), new(-12,   4), new( -9,   7), new( -8,  22), new(  5,  20), new( -4,   4), new(  9,   0), new( -3,   5),
         new(-23,   4), new(-17,   7), new(-15,   1), new( -2,   3), new( -2,   2), new( -1,   0), new(  1,   2), new(  0,  17),
         new(-55,  11), new(-15,  -2), new(-31,  -1), new(-14,   0), new(-10,   4), new( -6,  -4), new(-11,   3), new(-25,   9),

         new(-31,   7), new(-65,  11), new(-61,   7), new(-102,  16), new(-93,  15), new(-82,   1), new(-49,   5), new(-66,  -1),
         new(-29,  -6), new( -9,  -3), new(-19,  -1), new(-28,   3), new(-11,  -7), new(-18,  -2), new(-31,   1), new(-36,  -2),
         new(-16,   9), new( -1,   0), new(  0,   4), new(  9,  -5), new( -1,   0), new( 30,   3), new(  9,   0), new(  2,   7),
         new(-24,   5), new( -4,   8), new( -1,   3), new( 16,  18), new(  9,   7), new(  3,   9), new(  0,   3), new(-24,   8),
         new( -7,   0), new(-19,   7), new( -8,  13), new( 11,  12), new(  7,  11), new( -6,   6), new(-13,   7), new( 13, -12),
         new( -6,   2), new(  7,   6), new( -1,   9), new(  1,  12), new(  5,  14), new(  4,   8), new(  8,   0), new( 12,  -2),
         new( 13,  10), new(  0,  -5), new( 11,  -8), new( -9,   3), new( -2,   6), new(  9,  -3), new( 22,   0), new( 11,  -2),
         new(  0,   0), new( 16,   6), new(  0,  -1), new(-12,   0), new( -3,  -2), new( -7,  12), new(  8,  -5), new( 17, -11),

         new(-13,  29), new(-22,  33), new(-28,  41), new(-31,  37), new(-18,  31), new(  3,  27), new(  3,  29), new( 17,  23),
         new(-25,  26), new(-25,  37), new(-12,  39), new(  0,  30), new(-11,  30), new( 12,  20), new( 16,  16), new( 32,   8),
         new(-30,  25), new( -5,  23), new( -7,  24), new( -5,  21), new( 21,   9), new( 25,   4), new( 62,   0), new( 32,  -1),
         new(-29,  27), new(-15,  22), new(-14,  26), new(-10,  22), new( -6,  10), new(  5,   5), new( 13,   9), new(  5,   4),
         new(-33,  19), new(-37,  21), new(-27,  18), new(-22,  16), new(-21,  14), new(-23,  11), new( -1,   6), new(-10,   3),
         new(-35,  13), new(-30,   9), new(-27,   6), new(-26,   7), new(-15,   1), new( -7,  -5), new( 21, -18), new(  0, -13),
         new(-33,   3), new(-29,   6), new(-17,   4), new(-15,   2), new( -8,  -4), new(  0, -12), new( 15, -18), new(-17,  -9),
         new(-16,   7), new(-16,   5), new(-11,   9), new( -3,   2), new(  2,  -3), new(  0,   0), new(  3,  -5), new( -8,  -6),

         new(-34,  30), new(-48,  53), new(-33,  75), new(-11,  67), new(-13,  66), new( -7,  63), new( 43,   7), new(-12,  43),
         new(-12,  12), new(-38,  44), new(-39,  80), new(-52, 103), new(-47, 118), new( -7,  73), new(-15,  62), new( 40,  44),
         new(-11,  23), new(-17,  32), new(-20,  66), new(-15,  75), new( -3,  87), new( 30,  70), new( 40,  38), new( 37,  42),
         new(-22,  32), new(-15,  41), new(-19,  51), new(-23,  74), new(-21,  87), new( -7,  77), new(  2,  74), new(  0,  60),
         new( -7,  19), new(-22,  46), new(-18,  48), new(-16,  64), new(-15,  64), new(-12,  58), new( -2,  52), new(  9,  45),
         new( -6,   2), new( -3,  21), new(-11,  37), new(-11,  34), new( -6,  39), new(  1,  33), new( 14,  18), new( 13,  11),
         new( -4,  -1), new( -5,   1), new(  0,   2), new(  4,   7), new(  3,  10), new( 10, -14), new( 15, -37), new( 25, -53),
         new(-10,  -5), new( -7,  -5), new( -1,  -3), new(  4,   8), new(  3,  -7), new( -9,  -7), new( -4, -14), new(  4, -32),

         new( -4, -76), new( 12, -39), new( 16, -27), new(-75,   8), new(-33,  -8), new( -3,  -2), new( 43,  -5), new( 61, -85),
         new(-78,  -5), new(-25,  16), new(-66,  25), new( 26,  10), new(-19,  25), new(-16,  41), new( 22,  32), new( 10,   8),
         new(-93,   0), new( 12,  15), new(-52,  29), new(-79,  41), new(-36,  41), new( 43,  33), new( 24,  32), new(-15,   5),
         new(-55, -17), new(-57,  10), new(-86,  31), new(-137,  44), new(-128,  45), new(-79,  39), new(-66,  28), new(-102,   7),
         new(-59, -27), new(-52,  -1), new(-73,  18), new(-119,  37), new(-110,  36), new(-66,  23), new(-70,  13), new(-120,   4),
         new(-12, -30), new( 22, -14), new(-33,   5), new(-46,  17), new(-38,  17), new(-39,  11), new( -7,  -2), new(-38,  -8),
         new( 51, -18), new( 21,   0), new( 22,  -8), new( -6,  -1), new( -5,   1), new(  7,  -3), new( 30,   3), new( 26,  -5),
         new(  9, -33), new( 34, -20), new( 13,  -4), new(-31, -18), new(  3, -13), new(  4, -23), new( 34, -18), new( 30, -44),
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-95, -119),
         new(-46, -45),
         new(-24, -11),
         new(-14, 7),
         new(-3, 18),
         new(1, 30),
         new(12, 31),
         new(19, 35),
         new(32, 29),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-54, -113),
         new(-39, -70),
         new(-25, -21),
         new(-15, 0),
         new(-4, 9),
         new(4, 20),
         new(10, 29),
         new(14, 33),
         new(16, 38),
         new(20, 39),
         new(23, 40),
         new(31, 35),
         new(35, 39),
         new(42, 27),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-105, -156),
         new(-29, -88),
         new(-27, -25),
         new(-21, -7),
         new(-15, 2),
         new(-10, 8),
         new(-9, 16),
         new(-5, 21),
         new(-2, 23),
         new(2, 27),
         new(6, 31),
         new(6, 35),
         new(10, 37),
         new(15, 37),
         new(19, 35),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-19, -1),
         new(-80, -25),
         new(-57, -52),
         new(-54, -113),
         new(-35, -55),
         new(-35, 14),
         new(-23, 6),
         new(-20, 29),
         new(-17, 44),
         new(-13, 60),
         new(-8, 60),
         new(-6, 66),
         new(-3, 74),
         new(0, 74),
         new(2, 80),
         new(3, 85),
         new(4, 90),
         new(4, 96),
         new(3, 105),
         new(9, 102),
         new(14, 105),
         new(19, 100),
         new(32, 102),
         new(58, 83),
         new(48, 103),
         new(145, 46),
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
         new(9, -4),
         new(13, -1),
         new(26, -9),
         new(17, 13),
      ];

      public static Score[] PawnShield =
      [
         new(-27, -5),
         new(5, -19),
         new(40, -28),
         new(72, -42),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(33, -27),
         new(17, -6),
         new(2, 30),
         new(10, 67),
         new(-8, 144),
         new(14, 121),
      ];

      public static Score[] DefendedPawn = [
         new(-24, -20),
         new(-8, -13),
         new(6, -2),
         new(18, 13),
         new(29, 32),
         new(42, 39),
         new(36, 25),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-15, -8),
         new(-4, 2),
         new(4, 4),
         new(13, 14),
         new(22, 12),
         new(25, 63),
         new(41, -5),
         new(-7, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(2, 4),
         new(4, 11),
         new(10, 11),
         new(8, 16),
         new(13, 18),
         new(5, 11),
         new(3, 10),
         new(5, 9),
      ];

      public static Score FriendlyKingPawnDistance = new(6, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
      public static Score BishopPair = new(23, 65);
   }
}
