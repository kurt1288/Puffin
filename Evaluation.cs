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
         new(294, 345),
         new(325, 361),
         new(403, 623),
         new(839, 1151),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 22, 110), new( 35,  99), new( 20, 102), new( 60,  50), new( 34,  52), new( 21,  57), new(-76, 103), new(-94, 126), 
         new(  7,  36), new(  2,  42), new( 30,   1), new( 37, -34), new( 49, -40), new( 79, -26), new( 39,  16), new( 19,  22), 
         new(-15,  19), new( -8,  10), new( -5,  -6), new(  0, -23), new( 18, -20), new( 21, -21), new(  2,   0), new(  4,   0), 
         new(-20,   0), new(-16,   0), new( -7, -14), new(  3, -21), new(  4, -18), new(  8, -20), new( -4,  -9), new( -5, -14), 
         new(-27,  -5), new(-19,  -7), new(-16, -13), new(-11, -11), new( -1,  -9), new(-25,  -9), new(-15, -10), new(-27, -11), 
         new(-20,  -1), new(-13,  -3), new( -9,  -8), new( -4, -10), new(  0,   1), new(  1,  -5), new( -1, -11), new(-25,  -9), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-114, -35), new(-108,  -3), new(-70,  11), new(-36,   1), new( -5,   4), new(-59, -17), new(-76,   1), new(-76, -52), 
         new(-13,   0), new( -3,   6), new( 17,   1), new( 35,   1), new( 22,  -2), new( 64, -16), new(  7,   2), new( 23, -14), 
         new( -3,   0), new( 18,   1), new( 23,  16), new( 32,  19), new( 53,   9), new( 74,  -4), new( 25,  -2), new( 20,  -5), 
         new(  2,  12), new(  3,  13), new( 19,  24), new( 43,  27), new( 17,  31), new( 39,  26), new(  1,  23), new( 31,   7), 
         new( -1,  13), new( -2,   7), new(  3,  25), new( 10,  26), new( 16,  29), new( 14,  18), new( 25,   7), new( 11,  12), 
         new(-18,   0), new(-11,   0), new( -9,   3), new( -8,  20), new(  4,  18), new( -5,   0), new(  9,  -2), new( -2,   3), 
         new(-22,   1), new(-16,   5), new(-15,  -1), new( -1,   0), new( -3,  -1), new(  0,  -4), new(  0,   0), new(  0,  15), 
         new(-53,   8), new(-14,  -6), new(-30,  -5), new(-13,  -3), new( -9,   0), new( -5,  -8), new(-10,   0), new(-24,   7), 

         new(-29,   3), new(-64,  10), new(-58,   7), new(-96,  17), new(-92,  15), new(-81,   2), new(-45,   3), new(-63,   0), 
         new(-28,  -6), new( -4,  -4), new(-15,  -1), new(-25,   2), new(-14,  -5), new(-16,  -3), new(-31,   2), new(-30,  -4), 
         new(-15,   9), new(  0,   0), new(  2,   3), new(  7,  -3), new( -4,   2), new( 23,   6), new(  1,   3), new( -7,  12), 
         new(-22,   5), new( -1,   7), new( -2,   4), new( 13,  18), new(  8,   9), new(  0,   9), new(  1,   4), new(-27,  11), 
         new( -5,   0), new(-20,   7), new( -8,  13), new( 10,  12), new(  7,  11), new( -3,   6), new(-12,   7), new( 14, -11), 
         new( -6,   4), new(  7,   6), new(  0,   9), new(  0,  12), new(  7,  14), new(  6,   6), new(  9,  -1), new( 13,  -3), 
         new( 12,  10), new(  2,  -6), new( 12,  -8), new( -6,   3), new(  0,   5), new( 11,  -5), new( 23,  -1), new( 13,  -3), 
         new(  1,   0), new( 16,   6), new(  1,  -1), new(-10,   1), new( -1,  -3), new( -5,  11), new(  9,  -6), new( 21, -13), 

         new(-15,  31), new(-24,  36), new(-30,  44), new(-32,  39), new(-17,  34), new(  4,  29), new(  7,  31), new( 18,  26), 
         new(-26,  29), new(-28,  41), new(-14,  43), new(  1,  32), new(-13,  33), new( 14,  22), new( 16,  19), new( 35,  10), 
         new(-28,  28), new( -5,  26), new( -5,  27), new( -3,  23), new( 24,  12), new( 28,   6), new( 62,   2), new( 30,   2), 
         new(-28,  29), new(-15,  25), new(-13,  29), new( -9,  25), new( -5,  14), new(  2,   8), new(  9,  13), new(  2,   8), 
         new(-33,  21), new(-37,  24), new(-27,  22), new(-22,  19), new(-21,  17), new(-27,  15), new( -2,   9), new(-14,   7), 
         new(-36,  16), new(-30,  12), new(-27,   9), new(-26,  10), new(-15,   5), new(-10,  -2), new( 18, -14), new( -2, -10), 
         new(-33,   6), new(-29,  10), new(-18,   7), new(-15,   4), new(-10,  -1), new(  0,  -9), new( 11, -14), new(-18,  -5), 
         new(-16,  10), new(-16,   8), new(-12,  13), new( -3,   4), new(  2,   0), new(  0,   2), new(  1,  -2), new( -9,  -2), 

         new(-34,  17), new(-47,  34), new(-31,  52), new( -7,  37), new(-14,  37), new( -9,  38), new( 45, -14), new( -6,  19), 
         new(-12,   5), new(-35,  34), new(-39,  69), new(-50,  87), new(-57, 101), new(-13,  50), new(-18,  47), new( 41,  25), 
         new( -8,  21), new(-14,  30), new(-15,  63), new(-16,  66), new( -3,  66), new( 17,  48), new( 28,  24), new( 18,  27), 
         new(-20,  36), new(-13,  46), new(-19,  51), new(-26,  76), new(-19,  74), new( -9,  59), new(  2,  62), new( -1,  45), 
         new( -4,  23), new(-21,  47), new(-18,  51), new(-14,  63), new(-14,  64), new(-10,  52), new(  1,  43), new( 10,  34), 
         new( -4,   2), new( -1,  22), new( -8,  37), new(-10,  40), new( -4,  46), new(  3,  31), new( 15,  13), new( 14,   4), 
         new( -2,  -2), new( -3,   1), new(  2,   5), new(  6,  14), new(  5,  17), new( 13, -15), new( 17, -39), new( 28, -60), 
         new( -7,  -6), new( -5,  -4), new(  0,   1), new(  7,  15), new(  6,  -3), new( -6, -12), new( -2, -18), new(  7, -36), 

         new( -3, -76), new( 13, -39), new( 12, -26), new(-69,   8), new(-31,  -7), new(-13,   0), new( 25,  -2), new( 48, -83), 
         new(-76,  -5), new(-21,  16), new(-62,  25), new( 34,  10), new(-12,  25), new(-11,  41), new( 21,  33), new( -7,  11), 
         new(-98,   1), new( 16,  14), new(-47,  29), new(-72,  41), new(-28,  40), new( 45,  33), new( 25,  32), new(-19,   7), 
         new(-57, -16), new(-50,  10), new(-83,  30), new(-129,  43), new(-119,  44), new(-75,  39), new(-65,  28), new(-107,   9), 
         new(-61, -27), new(-46,  -2), new(-71,  19), new(-115,  36), new(-108,  36), new(-62,  22), new(-68,  13), new(-121,   5), 
         new(-14, -29), new( 21, -13), new(-33,   6), new(-46,  17), new(-37,  17), new(-36,  11), new( -5,  -1), new(-38,  -8), 
         new( 50, -19), new( 24,   0), new( 24,  -8), new( -4,  -1), new( -3,   1), new( 11,  -3), new( 32,   3), new( 27,  -7), 
         new(  9, -35), new( 35, -20), new( 12,  -3), new(-30, -18), new(  2, -11), new(  5, -23), new( 33, -18), new( 28, -45), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-97, -117),
         new(-46, -49),
         new(-24, -14),
         new(-14, 4),
         new(-3, 16),
         new(1, 28),
         new(12, 29),
         new(20, 33),
         new(34, 27),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-54, -98),
         new(-36, -70),
         new(-22, -23),
         new(-12, -2),
         new(-2, 7),
         new(6, 18),
         new(11, 27),
         new(15, 32),
         new(16, 37),
         new(20, 38),
         new(23, 40),
         new(30, 34),
         new(33, 39),
         new(41, 27),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-106, -154),
         new(-30, -81),
         new(-27, -21),
         new(-21, -1),
         new(-15, 7),
         new(-11, 13),
         new(-10, 21),
         new(-6, 25),
         new(-3, 27),
         new(1, 31),
         new(5, 35),
         new(6, 39),
         new(9, 41),
         new(14, 41),
         new(16, 40),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-16, -1),
         new(-69, -21),
         new(-56, -51),
         new(-55, -100),
         new(-33, -44),
         new(-33, 16),
         new(-22, 12),
         new(-19, 31),
         new(-17, 47),
         new(-13, 61),
         new(-9, 61),
         new(-7, 67),
         new(-4, 74),
         new(-1, 73),
         new(0, 77),
         new(2, 81),
         new(2, 85),
         new(3, 89),
         new(2, 96),
         new(8, 91),
         new(16, 90),
         new(22, 81),
         new(36, 78),
         new(64, 54),
         new(59, 68),
         new(149, 12),
         new(53, 59),
         new(19, 44),
      ];

      public static Score RookHalfOpenFile = new(9, 10);
      public static Score RookOpenFile = new(28, 7);
      public static Score KingOpenFile = new(68, -6);
      public static Score KingHalfOpenFile = new(26, -25);

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(10, -6),
         new(18, -4),
         new(20, -5),
         new(13, 14),
      ];

      public static Score[] PawnShield =
      [
         new(-27, -4),
         new(5, -18),
         new(39, -28),
         new(70, -43),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(30, -27),
         new(13, -6),
         new(0, 30),
         new(8, 67),
         new(-11, 144),
         new(15, 120),
      ];

      public static Score[] DefendedPawn = [
         new(-24, -20),
         new(-8, -13),
         new(6, -2),
         new(18, 13),
         new(29, 31),
         new(42, 38),
         new(36, 21),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-15, -8),
         new(-4, 2),
         new(4, 4),
         new(13, 13),
         new(22, 11),
         new(24, 56),
         new(41, -4),
         new(-5, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(2, 5),
         new(4, 11),
         new(10, 11),
         new(8, 17),
         new(13, 18),
         new(5, 11),
         new(3, 10),
         new(6, 9),
      ];

      public static Score FriendlyKingPawnDistance = new(6, -9);
      public static Score EnemyKingPawnDistance = new(-6, 9);
   }
}
