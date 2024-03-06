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

         while (bishopBB)
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

         while (rookBB)
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

         while (queenBB)
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

         while (kingBB)
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

         while (pawns)
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

            // Isolated pawn
            if ((Constants.IsolatedPawnMasks[square & 7] & colorPawns[(int)color].Value) == 0)
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
         new(293, 344),
         new(324, 362),
         new(413, 630),
         new(836, 1160),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 
         new( 18, 108), new( 39,  97), new( 20, 101), new( 53,  51), new( 32,  51), new( 15,  59), new(-67, 101), new(-91, 120), 
         new(  1,  38), new(  5,  42), new( 33,   2), new( 38, -33), new( 47, -39), new( 78, -23), new( 48,  15), new( 20,  21), 
         new(-20,  20), new( -5,   9), new( -4,  -5), new(  0, -23), new( 19, -20), new( 18, -20), new(  8,  -1), new(  7,  -2), 
         new(-24,   0), new(-15,   0), new( -7, -13), new(  2, -19), new(  8, -18), new(  3, -17), new( -3,  -9), new( -5, -15), 
         new(-29,  -4), new(-17,  -7), new(-15, -11), new(-12, -10), new( -1,  -7), new( -7, -13), new(  4, -18), new( -3, -20), 
         new(-28,  -1), new(-15,  -1), new(-17,  -5), new(-13,  -6), new( -9,   3), new(-11,  -2), new( -5,  -8), new(-31, -11), 
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), 

         new(-153, -52), new(-124,  -9), new(-73,   6), new(-43,  -1), new(-10,   0), new(-65, -21), new(-94,  -4), new(-102, -73), 
         new(-27,  -9), new(-12,   2), new(  9,   2), new( 30,   3), new( 12,   0), new( 62, -15), new( -7,   0), new( 11, -24), 
         new( -8,  -4), new( 20,   2), new( 27,  24), new( 33,  26), new( 57,  16), new( 75,   3), new( 25,   0), new( 15, -10), 
         new( -5,   9), new(  0,  17), new( 21,  32), new( 45,  33), new( 17,  38), new( 39,  33), new( -3,  27), new( 20,   5), 
         new(-11,  11), new( -3,  10), new(  8,  31), new( 14,  32), new( 22,  36), new( 18,  24), new( 23,  12), new(  1,   9), 
         new(-29,  -5), new(-12,   1), new( -2,   8), new(  3,  26), new( 16,  22), new(  5,   5), new(  5,   0), new( -7,   2), 
         new(-32,  -6), new(-23,   0), new(-13,   1), new(  3,   1), new(  4,   0), new(  2,  -2), new( -7,  -4), new( -7,   6), 
         new(-69,  -7), new(-21, -13), new(-30,  -4), new(-11,   0), new( -7,   1), new( -4,  -7), new(-16,  -9), new(-41,  -6), 

         new(-37,   2), new(-63,   9), new(-60,   4), new(-98,  14), new(-97,  12), new(-81,   0), new(-43,   0), new(-70,  -2), 
         new(-29, -10), new( -4,  -4), new(-14,   0), new(-25,   3), new(-13,  -4), new(-14,  -3), new(-30,   3), new(-23, -11), 
         new(-13,   7), new(  6,   0), new(  3,   7), new( 10,   0), new( -3,   5), new( 23,  10), new(  5,   3), new( -7,  10), 
         new(-20,   3), new(  0,   9), new(  0,   8), new( 14,  22), new( 11,  14), new(  3,  13), new(  0,   4), new(-26,   8), 
         new(-12,   0), new(-18,   9), new( -6,  17), new( 15,  16), new( 11,  14), new( -1,  10), new(-10,   8), new(  2,  -9), 
         new(-12,   1), new(  2,  11), new(  2,  12), new(  2,  15), new(  7,  19), new(  7,  10), new(  5,   4), new(  6,  -5), 
         new(  2,   4), new(  4,  -7), new( 10,  -7), new( -1,   4), new(  7,   4), new( 15,  -3), new( 24,  -4), new(  7, -10), 
         new(-10,  -8), new( 12,   3), new( -1,  -1), new(-10,   2), new( -1,  -1), new( -6,  10), new(  4,  -9), new(  7, -23), 

         new( -6,  27), new(-22,  35), new(-25,  44), new(-28,  40), new(-15,  35), new(  3,  29), new( -4,  33), new( 18,  23), 
         new(-18,  27), new(-23,  40), new( -6,  42), new( 11,  33), new( -4,  34), new( 18,  22), new( 14,  19), new( 40,   7), 
         new(-21,  26), new(  4,  25), new(  3,  26), new(  8,  23), new( 35,  11), new( 30,   7), new( 66,   2), new( 33,   1), 
         new(-25,  29), new(-13,  27), new( -7,  31), new( -1,  27), new(  3,  14), new(  0,  10), new(  7,  13), new(  2,   7), 
         new(-35,  22), new(-37,  27), new(-24,  25), new(-15,  23), new(-14,  20), new(-32,  20), new( -9,  11), new(-20,   9), 
         new(-39,  18), new(-31,  15), new(-22,  14), new(-21,  17), new(-13,  12), new(-16,   3), new(  6,  -9), new(-14,  -6), 
         new(-39,   9), new(-30,  14), new(-10,  11), new( -8,  11), new( -4,   4), new( -3,  -1), new( 10,  -9), new(-24,  -1), 
         new(-18,   8), new(-15,  10), new( -3,  13), new(  4,   8), new(  8,   3), new(  0,   3), new(  4,  -1), new(-15,  -2), 

         new(-49,  20), new(-58,  36), new(-39,  52), new(-13,  37), new(-21,  37), new(-16,  38), new( 28, -11), new(-23,  21), 
         new(-16,   3), new(-38,  34), new(-40,  66), new(-51,  85), new(-58, 100), new(-13,  49), new(-27,  50), new( 33,  23), 
         new( -8,  17), new(-12,  28), new(-15,  61), new(-13,  62), new( -5,  66), new( 16,  49), new( 26,  23), new( 15,  22), 
         new(-21,  33), new(-12,  42), new(-19,  50), new(-23,  72), new(-16,  72), new(-11,  61), new(  0,  59), new( -4,  42), 
         new( -9,  20), new(-22,  46), new(-17,  49), new( -9,  59), new(-10,  62), new(-10,  51), new( -1,  41), new(  2,  33), 
         new(-13,   4), new( -4,  22), new( -5,  35), new( -8,  40), new( -2,  49), new(  2,  34), new( 10,  17), new(  3,   9), 
         new( -6,  -2), new( -4,   2), new(  5,   6), new( 12,  13), new( 11,  16), new( 18, -13), new( 19, -38), new( 26, -61), 
         new( -8,  -6), new( -5,  -4), new(  4,   2), new( 12,  13), new( 11,  -3), new( -1, -13), new(  2, -21), new(  1, -32), 

         new( -2, -75), new( 18, -39), new( 21, -25), new(-57,   7), new(-25,  -6), new( -5,   0), new( 30,  -2), new( 49, -81), 
         new(-71,  -5), new(-18,  16), new(-52,  24), new( 45,   8), new( -2,  24), new( -2,  40), new( 26,  33), new( -2,  11), 
         new(-90,   1), new( 23,  14), new(-40,  28), new(-59,  38), new(-20,  38), new( 53,  32), new( 27,  33), new(-18,   8), 
         new(-56, -15), new(-46,   9), new(-77,  30), new(-120,  41), new(-112,  43), new(-70,  38), new(-63,  29), new(-106,  10), 
         new(-64, -25), new(-45,  -2), new(-71,  19), new(-111,  37), new(-103,  35), new(-62,  23), new(-67,  12), new(-123,   6), 
         new(-22, -27), new( 11, -11), new(-36,   7), new(-51,  18), new(-42,  18), new(-40,  12), new(-11,  -1), new(-46,  -6), 
         new( 54, -21), new( 25,  -1), new( 12,  -5), new(-10,   0), new( -9,   2), new( -3,   0), new( 33,   0), new( 31, -12), 
         new(  9, -36), new( 38, -22), new( 15,  -1), new(-34, -16), new( -1, -13), new(  0, -21), new( 37, -19), new( 30, -48), 
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-35, -50),
         new(-18, -15),
         new(-8, 4),
         new(-2, 14),
         new(2, 22),
         new(7, 30),
         new(13, 29),
         new(19, 26),
         new(24, 17),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-26, -43),
         new(-16, -23),
         new(-7, -6),
         new(-1, 6),
         new(5, 15),
         new(8, 26),
         new(11, 30),
         new(13, 33),
         new(14, 37),
         new(18, 34),
         new(25, 30),
         new(31, 30),
         new(23, 43),
         new(38, 20),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-32, -20),
         new(-22, 0),
         new(-19, 1),
         new(-13, 4),
         new(-15, 12),
         new(-8, 15),
         new(-4, 21),
         new(1, 23),
         new(7, 27),
         new(11, 31),
         new(16, 34),
         new(17, 39),
         new(20, 43),
         new(23, 41),
         new(15, 45),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-23, -45),
         new(-18, -80),
         new(-24, -16),
         new(-21, 8),
         new(-20, 25),
         new(-17, 28),
         new(-14, 43),
         new(-14, 53),
         new(-11, 59),
         new(-10, 62),
         new(-10, 68),
         new(-7, 72),
         new(-6, 74),
         new(-7, 79),
         new(-4, 79),
         new(-3, 84),
         new(-4, 91),
         new(-2, 90),
         new(5, 87),
         new(18, 78),
         new(22, 79),
         new(68, 53),
         new(60, 57),
         new(80, 36),
         new(138, 26),
         new(99, 24),
         new(52, 53),
         new(36, 44),
      ];

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(12, -6),
         new(20, -5),
         new(24, -6),
         new(14, 12),
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
         new(39, -33),
         new(20, -10),
         new(4, 28),
         new(10, 65),
         new(-10, 141),
         new(15, 119),
      ];

      public static Score[] DefendedPawn = [
         new(-26, -21),
         new(-12, -13),
         new(4, -1),
         new(19, 16),
         new(33, 32),
         new(43, 45),
         new(56, 38),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-23, -12),
         new(-8, -1),
         new(7, 7),
         new(21, 22),
         new(34, 29),
         new(39, 87),
         new(55, -4),
         new(-18, 0),
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
