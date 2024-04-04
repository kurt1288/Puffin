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
         new(64, 108),
         new(294, 345),
         new(324, 364),
         new(404, 625),
         new(842, 1153),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 24, 110), new( 35,  99), new( 23, 102), new( 63,  50), new( 39,  51), new( 23,  57), new(-77, 103), new(-96, 126), 
         new(  8,  36), new(  6,  42), new( 35,   1), new( 44, -35), new( 55, -40), new( 83, -26), new( 41,  16), new( 19,  22), 
         new(-14,  18), new( -5,  10), new( -2,  -6), new(  4, -24), new( 22, -20), new( 24, -21), new(  5,   0), new(  5,   0), 
         new(-20,   0), new(-15,   0), new( -6, -14), new(  6, -21), new(  8, -18), new( 10, -20), new( -5,  -8), new( -5, -14), 
         new(-27,  -5), new(-21,  -7), new(-16, -13), new(-11, -12), new( -2,  -9), new(-24, -10), new(-18, -10), new(-26, -11), 
         new(-22,  -1), new(-16,  -2), new(-14,  -6), new(  0, -10), new( -4,   3), new( -2,  -4), new( -4, -10), new(-26, -10), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-147, -54), new(-120, -10), new(-72,   5), new(-41,  -3), new( -9,  -1), new(-66, -22), new(-92,  -4), new(-105, -72), 
         new(-28,  -9), new(-11,   1), new( 11,   0), new( 31,   1), new( 14,  -2), new( 61, -17), new( -4,  -2), new(  9, -23), 
         new( -9,  -4), new( 20,   0), new( 28,  21), new( 35,  23), new( 59,  14), new( 80,   0), new( 27,  -2), new( 13,  -9), 
         new( -4,   8), new(  2,  15), new( 22,  29), new( 45,  31), new( 19,  35), new( 43,  31), new(  0,  25), new( 22,   4), 
         new(-11,  11), new( -3,   9), new(  9,  29), new( 15,  30), new( 23,  33), new( 18,  23), new( 24,  11), new(  2,  10), 
         new(-31,  -4), new(-10,   0), new( -1,   7), new(  3,  24), new( 17,  21), new(  4,   3), new(  9,  -2), new( -8,   2), 
         new(-33,  -5), new(-23,   1), new(-12,   0), new(  3,   0), new(  4,   0), new(  2,  -3), new( -5,  -4), new( -5,   7), 
         new(-68,  -6), new(-24, -12), new(-34,  -4), new(-15,   1), new( -8,   1), new( -5,  -7), new(-18,  -7), new(-40,  -5), 

         new(-33,   2), new(-61,   9), new(-57,   5), new(-96,  14), new(-93,  13), new(-80,   0), new(-43,   1), new(-69,  -2), 
         new(-29, -10), new( -4,  -4), new(-13,  -1), new(-24,   2), new(-13,  -5), new(-14,  -3), new(-32,   3), new(-33,  -7), 
         new(-15,   7), new(  4,   0), new(  3,   5), new( 10,  -2), new( -2,   4), new( 24,   7), new(  2,   3), new( -8,  10), 
         new(-22,   2), new(  0,   8), new(  0,   6), new( 15,  20), new( 12,  12), new(  2,  11), new(  1,   4), new(-28,   8), 
         new( -8,   0), new(-19,   8), new( -7,  16), new( 15,  14), new( 10,  13), new( -1,   8), new(-11,   8), new( 11, -12), 
         new(-11,   1), new(  7,  10), new(  0,  11), new(  0,  13), new(  7,  17), new(  5,   8), new( 12,   1), new(  9,  -6), 
         new(  6,   4), new(  2,  -6), new( 10,  -7), new( -4,   4), new(  0,   5), new( 10,  -5), new( 22,  -1), new(  8, -11), 
         new( -3,  -7), new( 17,   3), new(  1,   0), new(-14,   2), new( -1,  -1), new( -2,  10), new(  9,  -9), new( 15, -21), 

         new(-17,  31), new(-26,  36), new(-30,  44), new(-31,  39), new(-16,  34), new(  4,  29), new(  4,  32), new( 15,  26), 
         new(-27,  29), new(-30,  41), new(-13,  42), new(  1,  31), new(-13,  33), new( 14,  22), new( 13,  19), new( 34,  10), 
         new(-29,  27), new( -4,  25), new( -7,  26), new( -6,  22), new( 22,  11), new( 28,   6), new( 63,   2), new( 28,   3), 
         new(-29,  28), new(-17,  25), new(-14,  28), new(-10,  24), new( -6,  12), new(  0,   8), new(  7,  12), new(  0,   7), 
         new(-35,  20), new(-39,  24), new(-28,  22), new(-21,  19), new(-20,  17), new(-29,  16), new( -5,   9), new(-16,   6), 
         new(-37,  16), new(-32,  13), new(-27,  11), new(-25,  13), new(-15,   7), new(-11,   0), new( 14, -12), new( -6,  -9), 
         new(-35,   7), new(-30,  12), new(-17,  10), new(-14,   7), new( -9,   1), new(  0,  -5), new( 11, -12), new(-20,  -5), 
         new(-15,  12), new(-15,  10), new(-11,  15), new( -2,   6), new(  4,   0), new(  0,   5), new(  3,  -1), new( -8,   0), 

         new(-47,  22), new(-53,  35), new(-35,  52), new(-10,  37), new(-16,  36), new(-13,  38), new( 39, -13), new(-20,  24), 
         new(-16,   3), new(-36,  33), new(-39,  66), new(-51,  87), new(-58, 100), new(-13,  48), new(-20,  47), new( 33,  25), 
         new( -7,  17), new(-12,  28), new(-13,  60), new(-13,  62), new( -2,  65), new( 19,  46), new( 27,  24), new( 15,  24), 
         new(-20,  32), new(-12,  43), new(-16,  48), new(-23,  72), new(-15,  71), new( -8,  58), new(  3,  60), new( -3,  43), 
         new( -8,  22), new(-20,  45), new(-16,  49), new( -7,  58), new( -9,  61), new( -7,  51), new(  1,  43), new(  6,  33), 
         new(-11,   5), new( -3,  24), new( -4,  35), new( -7,  40), new(  0,  48), new(  3,  34), new( 13,  18), new(  6,   9), 
         new( -5,  -1), new( -1,   1), new(  6,   4), new( 12,  14), new( 11,  18), new( 18, -13), new( 20, -35), new( 25, -57), 
         new(-10,  -2), new( -6,  -3), new(  0,   7), new(  7,  17), new(  8,   0), new( -4,  -8), new( -1, -17), new(  0, -32), 

         new( -1, -76), new( 14, -40), new( 13, -26), new(-68,   7), new(-30,  -7), new(-11,  -1), new( 26,  -2), new( 49, -83), 
         new(-75,  -6), new(-21,  16), new(-60,  24), new( 35,  10), new(-11,  25), new(-12,  41), new( 23,  32), new( -4,  11), 
         new(-96,   1), new( 18,  14), new(-47,  29), new(-72,  40), new(-29,  40), new( 46,  33), new( 25,  32), new(-18,   7), 
         new(-57, -16), new(-50,   9), new(-82,  30), new(-129,  43), new(-120,  44), new(-75,  38), new(-65,  28), new(-106,   8), 
         new(-62, -26), new(-46,  -2), new(-72,  19), new(-114,  36), new(-108,  36), new(-63,  22), new(-69,  13), new(-121,   5), 
         new(-16, -29), new( 19, -13), new(-33,   6), new(-48,  17), new(-38,  17), new(-37,  11), new( -7,  -1), new(-39,  -8), 
         new( 51, -19), new( 24,   0), new( 23,  -7), new( -3,  -1), new( -2,   1), new( 10,  -3), new( 31,   3), new( 27,  -7), 
         new( 11, -35), new( 39, -21), new( 13,  -3), new(-31, -18), new(  0, -12), new(  3, -22), new( 34, -18), new( 29, -45), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-34, -54),
         new(-17, -19),
         new(-7, 1),
         new(-2, 11),
         new(2, 20),
         new(6, 29),
         new(13, 28),
         new(19, 25),
         new(25, 16),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-50, -51),
         new(-26, -33),
         new(-12, -14),
         new(-6, 0),
         new(3, 9),
         new(10, 20),
         new(14, 26),
         new(16, 29),
         new(18, 34),
         new(21, 33),
         new(26, 31),
         new(36, 28),
         new(32, 34),
         new(46, 20),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-27, -20),
         new(-17, 1),
         new(-14, 2),
         new(-9, 5),
         new(-11, 13),
         new(-7, 17),
         new(-6, 23),
         new(-3, 25),
         new(-1, 28),
         new(0, 32),
         new(2, 34),
         new(1, 38),
         new(5, 40),
         new(7, 37),
         new(2, 40),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-15, -50),
         new(-16, -79),
         new(-23, -18),
         new(-20, 7),
         new(-20, 24),
         new(-16, 27),
         new(-15, 44),
         new(-14, 54),
         new(-12, 61),
         new(-11, 63),
         new(-10, 69),
         new(-8, 74),
         new(-6, 75),
         new(-7, 80),
         new(-5, 81),
         new(-3, 85),
         new(-4, 92),
         new(-2, 91),
         new(4, 89),
         new(19, 79),
         new(21, 81),
         new(67, 55),
         new(59, 60),
         new(78, 40),
         new(137, 29),
         new(97, 27),
         new(53, 54),
         new(35, 45),
      ];

      public static Score RookHalfOpenFile = new(10, 9);
      public static Score RookOpenFile = new(30, 7);
      public static Score KingOpenFile = new(68, -6);
      public static Score KingHalfOpenFile = new(26, -25);

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(10, -6),
         new(19, -4),
         new(20, -5),
         new(14, 13),
      ];

      public static Score[] PawnShield =
      [
         new(-27, -5),
         new(5, -18),
         new(39, -28),
         new(69, -42),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(30, -27),
         new(13, -6),
         new(0, 30),
         new(7, 67),
         new(-12, 144),
         new(19, 120),
      ];

      public static Score[] DefendedPawn = [
         new(-23, -21),
         new(-8, -13),
         new(6, -2),
         new(18, 13),
         new(30, 32),
         new(41, 41),
         new(35, 24),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-18, -8),
         new(-6, 2),
         new(4, 3),
         new(16, 13),
         new(26, 10),
         new(37, 74),
         new(48, -4),
         new(-10, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(3, 4),
         new(4, 11),
         new(10, 11),
         new(9, 16),
         new(14, 18),
         new(6, 11),
         new(3, 11),
         new(7, 9),
      ];

      public static Score FriendlyKingPawnDistance = new(6, -9);
      public static Score EnemyKingPawnDistance = new(-6, 9);
   }
}
