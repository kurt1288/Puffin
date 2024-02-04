using System.Runtime.CompilerServices;

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
            Attacks.KingAttacks[kingSquares[(int)Color.White]],
            Attacks.KingAttacks[kingSquares[(int)Color.Black]]
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

      private static void Knights(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard knightsBB = board.PieceBB[(int)PieceType.Knight];

         while (!knightsBB.IsEmpty())
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            // * (1 - 2 * (int)color) evaluates to 1 when color is white and to -1 when color is black (so that black score is subtracted)
            score += KnightMobility[new Bitboard(Attacks.KnightAttacks[square] & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

            if ((Attacks.KnightAttacks[square] & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += KingAttackWeights[(int)PieceType.Knight] * new Bitboard(Attacks.KnightAttacks[square] & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
            }
         }
      }

      private static void Bishops(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied)
      {
         Bitboard bishopBB = board.PieceBB[(int)PieceType.Bishop];

         while (!bishopBB.IsEmpty())
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = Attacks.GetBishopAttacks(square, occupied);
            score += BishopMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);

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

         while (!rookBB.IsEmpty())
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = Attacks.GetRookAttacks(square, occupied);
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

         while (!queenBB.IsEmpty())
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = Attacks.GetQueenAttacks(square, occupied);
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

         while (!kingBB.IsEmpty())
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            Color color = board.Mailbox[kingSq].Color;
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & Constants.SquareBB[kingSq]) != 0)
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

         while (!pawns.IsEmpty())
         {
            int square = pawns.GetLSB();
            Color color = board.Mailbox[square].Color;
            pawns.ClearLSB();
            mobilitySquares[(int)color ^ 1] |= Attacks.PawnAttacks[(int)color][square];

            // Passed pawns
            if ((Constants.PassedPawnMasks[(int)color][square] & colorPawns[(int)color ^ 1].Value) == 0)
            {
               score += PassedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1] * (1 - 2 * (int)color);
               score += Constants.TaxiDistance[square][kingSquares[(int)color]] * FriendlyKingPawnDistance * (1 - 2 * (int)color);
               score += Constants.TaxiDistance[square][kingSquares[(int)color ^ 1]] * EnemyKingPawnDistance * (1 - 2 * (int)color);
            }

            // Defending pawn
            if ((Attacks.PawnAttacks[(int)color][square] & colorPawns[(int)color].Value) != 0)
            {
               defender[(int)color]++;
            }

            // Connected pawn
            if ((((Constants.SquareBB[square] & ~Constants.FILE_MASKS[(int)File.H]) << 1) & colorPawns[(int)color].Value) != 0)
            {
               connected[(int)color]++;
            }            
         }

         score += DefendedPawn[defender[(int)Color.White]] - DefendedPawn[defender[(int)Color.Black]];
         score += ConnectedPawn[connected[(int)Color.White]] - ConnectedPawn[connected[(int)Color.Black]];
         mobilitySquares[(int)Color.White] = ~mobilitySquares[(int)Color.White];
         mobilitySquares[(int)Color.Black] = ~mobilitySquares[(int)Color.Black];
      }

      public static readonly Score[] PieceValues = [
         new(64, 103),
         new(292, 342),
         new(323, 360),
         new(411, 628),
         new(832, 1157),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 18, 109), new( 38,  95), new( 19,  98), new( 52,  46), new( 31,  46), new( 16,  59), new(-62, 100), new(-97, 121), 
         new(  3,  39), new(  4,  41), new( 32,   0), new( 39, -35), new( 46, -42), new( 79, -22), new( 52,  14), new( 16,  23), 
         new(-18,  21), new( -5,   9), new( -5,  -8), new(  0, -25), new( 19, -24), new( 18, -19), new( 14,  -1), new(  4,   0), 
         new(-23,   3), new(-17,   0), new( -8, -14), new(  1, -22), new(  8, -22), new(  3, -16), new(  3,  -9), new( -7, -13), 
         new(-28,  -3), new(-19,  -8), new(-17, -14), new(-13, -13), new( -2, -11), new( -9, -13), new(  9, -20), new( -6, -20), 
         new(-27,   0), new(-17,   0), new(-18,  -8), new(-13,  -7), new(-10,   0), new(-11,  -1), new(  0,  -7), new(-34, -10), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-152, -50), new(-123,  -9), new(-74,   6), new(-42,  -1), new(-10,   0), new(-64, -21), new(-91,  -4), new(-100, -73), 
         new(-27,  -9), new(-11,   2), new( 10,   2), new( 30,   3), new( 12,   0), new( 62, -15), new( -6,   0), new( 11, -24), 
         new( -8,  -4), new( 20,   2), new( 27,  24), new( 33,  26), new( 57,  16), new( 75,   4), new( 24,   0), new( 15,  -9), 
         new( -5,   9), new(  0,  16), new( 21,  32), new( 45,  33), new( 17,  38), new( 40,  33), new( -3,  27), new( 19,   5), 
         new(-11,  11), new( -3,  10), new(  8,  32), new( 14,  32), new( 22,  36), new( 18,  24), new( 23,  12), new(  1,  10), 
         new(-29,  -5), new(-12,   1), new( -2,   8), new(  3,  26), new( 16,  22), new(  5,   6), new(  5,   0), new( -7,   2), 
         new(-32,  -6), new(-23,   0), new(-13,   1), new(  3,   1), new(  4,   0), new(  2,  -2), new( -8,  -4), new( -7,   6), 
         new(-69,  -7), new(-21, -13), new(-30,  -4), new(-11,   0), new( -8,   2), new( -4,  -7), new(-16,  -8), new(-41,  -7), 

         new(-36,   1), new(-63,   8), new(-61,   4), new(-98,  14), new(-96,  12), new(-80,   0), new(-42,   0), new(-69,  -3), 
         new(-29, -10), new( -4,  -5), new(-13,  -1), new(-25,   2), new(-12,  -4), new(-14,  -4), new(-29,   3), new(-23, -11), 
         new(-13,   7), new(  5,   0), new(  3,   6), new( 10,  -1), new( -3,   5), new( 23,   9), new(  5,   3), new( -8,  11), 
         new(-20,   2), new( -1,   9), new(  0,   8), new( 13,  21), new( 11,  13), new(  3,  12), new(  0,   5), new(-26,   7), 
         new(-12,   0), new(-18,   9), new( -6,  17), new( 15,  15), new( 10,  13), new( -1,  10), new(-11,   9), new(  2,  -8), 
         new(-13,   1), new(  1,  11), new(  2,  11), new(  1,  15), new(  7,  19), new(  7,  10), new(  5,   4), new(  6,  -6), 
         new(  1,   4), new(  4,  -6), new(  9,  -7), new( -1,   4), new(  7,   4), new( 15,  -3), new( 25,  -4), new(  8, -10), 
         new(-10,  -8), new( 11,   4), new( -1,  -1), new(-10,   2), new( -1,  -1), new( -6,  10), new(  4,  -8), new(  7, -22), 

         new( -6,  26), new(-21,  34), new(-25,  44), new(-27,  40), new(-14,  34), new(  4,  29), new( -1,  32), new( 20,  22), 
         new(-17,  26), new(-22,  39), new( -5,  42), new( 12,  33), new( -3,  34), new( 18,  22), new( 16,  18), new( 40,   7), 
         new(-21,  26), new(  4,  25), new(  2,  26), new(  8,  23), new( 34,  11), new( 29,   7), new( 65,   2), new( 32,   1), 
         new(-25,  29), new(-13,  27), new( -7,  31), new( -1,  27), new(  2,  14), new(  0,  10), new(  7,  13), new(  1,   7), 
         new(-34,  22), new(-37,  26), new(-24,  25), new(-15,  23), new(-15,  19), new(-32,  19), new(-10,  11), new(-20,   9), 
         new(-39,  18), new(-32,  15), new(-22,  14), new(-21,  17), new(-13,  12), new(-16,   3), new(  6,  -9), new(-14,  -6), 
         new(-39,   9), new(-30,  14), new(-10,  11), new( -8,  11), new( -4,   4), new( -3,  -1), new(  9,  -9), new(-24,  -1), 
         new(-18,   7), new(-15,  10), new( -3,  13), new(  4,   8), new(  8,   2), new(  0,   3), new(  3,  -1), new(-15,  -3), 

         new(-49,  21), new(-58,  35), new(-37,  51), new(-13,  37), new(-21,  37), new(-17,  37), new( 28, -12), new(-22,  20), 
         new(-16,   3), new(-38,  33), new(-39,  65), new(-49,  83), new(-57,  98), new(-12,  49), new(-26,  48), new( 32,  23), 
         new( -7,  17), new(-11,  26), new(-15,  60), new(-13,  61), new( -5,  65), new( 15,  48), new( 25,  24), new( 14,  21), 
         new(-21,  33), new(-12,  42), new(-19,  50), new(-24,  71), new(-17,  71), new(-11,  60), new(  0,  58), new( -4,  41), 
         new( -9,  20), new(-22,  46), new(-17,  49), new( -8,  58), new(-10,  61), new(-10,  51), new( -1,  41), new(  2,  31), 
         new(-13,   5), new( -4,  22), new( -5,  35), new( -8,  40), new( -2,  48), new(  2,  34), new(  9,  17), new(  3,   9), 
         new( -6,  -2), new( -4,   2), new(  4,   5), new( 11,  13), new( 11,  16), new( 18, -13), new( 19, -38), new( 26, -60), 
         new( -8,  -6), new( -5,  -5), new(  4,   1), new( 11,  13), new( 10,  -3), new( -2, -13), new(  1, -21), new(  1, -32), 

         new( -2, -76), new( 18, -39), new( 22, -26), new(-56,   6), new(-26,  -7), new( -7,  -1), new( 31,  -2), new( 48, -82), 
         new(-70,  -6), new(-17,  16), new(-51,  23), new( 44,   7), new( -3,  23), new( -4,  39), new( 26,  32), new(  0,  11), 
         new(-91,   1), new( 23,  14), new(-39,  27), new(-63,  38), new(-22,  38), new( 51,  32), new( 24,  33), new(-18,   9), 
         new(-56, -15), new(-46,   9), new(-78,  29), new(-121,  41), new(-113,  42), new(-73,  38), new(-65,  30), new(-108,  11), 
         new(-63, -25), new(-45,  -2), new(-72,  19), new(-113,  37), new(-104,  35), new(-63,  23), new(-67,  13), new(-123,   6), 
         new(-21, -26), new( 11, -11), new(-36,   7), new(-51,  18), new(-42,  19), new(-41,  13), new(-12,   0), new(-46,  -5), 
         new( 54, -21), new( 25,  -1), new( 12,  -4), new(-10,   0), new( -9,   3), new( -3,   1), new( 33,   0), new( 31, -12), 
         new(  7, -36), new( 38, -22), new( 14,  -1), new(-35, -16), new( -1, -13), new(  0, -20), new( 37, -19), new( 30, -47), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-34, -51),
         new(-18, -16),
         new(-8, 4),
         new(-2, 15),
         new(2, 23),
         new(7, 31),
         new(13, 29),
         new(19, 26),
         new(23, 18),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-27, -45),
         new(-16, -24),
         new(-7, -7),
         new(-1, 5),
         new(4, 15),
         new(8, 26),
         new(11, 30),
         new(12, 33),
         new(14, 38),
         new(18, 35),
         new(25, 31),
         new(31, 30),
         new(22, 43),
         new(37, 21),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-32, -21),
         new(-22, 0),
         new(-19, 1),
         new(-13, 4),
         new(-15, 12),
         new(-8, 15),
         new(-4, 21),
         new(0, 23),
         new(6, 26),
         new(11, 31),
         new(15, 34),
         new(16, 39),
         new(19, 43),
         new(22, 41),
         new(13, 44),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-23, -45),
         new(-19, -82),
         new(-24, -16),
         new(-21, 7),
         new(-20, 24),
         new(-17, 27),
         new(-15, 43),
         new(-14, 52),
         new(-12, 58),
         new(-11, 61),
         new(-10, 67),
         new(-7, 71),
         new(-6, 72),
         new(-7, 78),
         new(-5, 79),
         new(-3, 83),
         new(-4, 91),
         new(-2, 89),
         new(5, 86),
         new(17, 78),
         new(21, 79),
         new(67, 52),
         new(58, 57),
         new(78, 37),
         new(134, 28),
         new(98, 23),
         new(52, 53),
         new(35, 43),
      ];

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(12, -6),
         new(20, -5),
         new(23, -6),
         new(14, 12),
      ];

      public static Score[] PawnShield =
      [
         new(-31, -5),
         new(13, -11),
         new(46, -32),
         new(69, -63),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(34, -36),
         new(15, -13),
         new(0, 23),
         new(6, 62),
         new(-13, 138),
         new(13, 117),
      ];

      public static Score[] DefendedPawn = [
         new(-29, -36),
         new(-12, -15),
         new(7, 6),
         new(24, 32),
         new(39, 55),
         new(51, 71),
         new(63, 66),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-27, -24),
         new(-10, -3),
         new(7, 12),
         new(23, 37),
         new(39, 54),
         new(45, 119),
         new(61, -3),
         new(-15, 0),
         new(0, 0),
      ];

      public static Score FriendlyKingPawnDistance = new(7, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
   }
}
