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
         ulong[] kingZones = [
            Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)],
            Attacks.KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)]
         ];
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;

         Bitboard whitePawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)Color.White];
         Bitboard blackPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)Color.Black];

         Pawns(Color.White, whitePawns, blackPawns, ref mobilitySquares, ref score);
         Pawns(Color.Black, blackPawns, whitePawns, ref mobilitySquares, ref score);
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

      private static void Pawns(Color color, Bitboard friendlyPawns, Bitboard enemyPawns, ref ulong[] mobilitySquares, ref Score score)
      {
         Bitboard pawns = friendlyPawns;
         int defender = 0;
         int connected = 0;

         while (!pawns.IsEmpty())
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();
            int rank = color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3);
            mobilitySquares[(int)color ^ 1] |= Attacks.PawnAttacks[(int)color][square];

            // Passed pawns
            if ((Constants.PassedPawnMasks[(int)color][square] & enemyPawns.Value) == 0)
            {
               score += PassedPawn[rank - 1] * (1 - 2 * (int)color);
            }

            // Defending pawn
            if ((Attacks.PawnAttacks[(int)color][square] & friendlyPawns.Value) != 0)
            {
               defender++;
            }

            // Connected pawn
            if ((((Constants.SquareBB[square] & ~Constants.FILE_MASKS[(int)File.H]) << 1) & friendlyPawns.Value) != 0)
            {
               connected++;
            }
         }

         score += DefendedPawn[defender] * (1 - 2 * (int)color);
         score += ConnectedPawn[connected] * (1 - 2 * (int)color);

         mobilitySquares[(int)color ^ 1] = ~mobilitySquares[(int)color ^ 1];
      }

      public static readonly Score[] PieceValues = {
         new(64, 102),
         new(292, 339),
         new(323, 357),
         new(412, 619),
         new(837, 1147),
         new(0, 0),
      };

      public static readonly Score[] PST =
      {
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 26, 103), new( 47,  89), new( 25,  89), new( 58,  33), new( 36,  35), new( 22,  49), new(-50,  95), new(-76, 112),
         new(  3,  38), new(  4,  40), new( 33,  -2), new( 40, -40), new( 46, -44), new( 78, -22), new( 51,  17), new( 18,  23),
         new(-18,  21), new( -5,   8), new( -5,  -8), new(  0, -27), new( 20, -26), new( 18, -20), new( 13,  -2), new(  4,  -2),
         new(-22,   2), new(-16,   0), new( -7, -15), new(  1, -23), new(  8, -23), new(  3, -18), new(  3, -10), new( -7, -14),
         new(-27,  -3), new(-19,  -9), new(-17, -15), new(-13, -14), new( -2, -13), new( -9, -14), new(  9, -21), new( -6, -21),
         new(-27,   0), new(-17,  -1), new(-18,  -9), new(-13,  -9), new(-10,  -2), new(-11,  -2), new(  0,  -8), new(-35, -11),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-152, -50), new(-124, -10), new(-72,   5), new(-42,  -2), new(-10,   0), new(-63, -22), new(-92,  -4), new(-101, -72),
         new(-27,  -9), new(-12,   2), new( 10,   2), new( 30,   3), new( 12,   0), new( 63, -15), new( -6,  -1), new( 11, -23),
         new( -7,  -4), new( 20,   2), new( 27,  24), new( 33,  26), new( 57,  16), new( 74,   4), new( 24,   1), new( 15,  -8),
         new( -5,   9), new(  0,  16), new( 21,  32), new( 45,  33), new( 17,  38), new( 40,  33), new( -3,  26), new( 19,   6),
         new(-11,  11), new( -3,  10), new(  9,  31), new( 15,  32), new( 22,  35), new( 18,  24), new( 23,  12), new(  1,   9),
         new(-29,  -5), new(-12,   1), new( -1,   8), new(  3,  26), new( 16,  22), new(  5,   5), new(  5,  -1), new( -7,   1),
         new(-32,  -6), new(-23,   0), new(-13,   1), new(  4,   1), new(  5,   0), new(  3,  -3), new( -8,  -4), new( -7,   7),
         new(-70,  -5), new(-21, -13), new(-30,  -4), new(-10,  -1), new( -7,   1), new( -3,  -8), new(-15,  -9), new(-41,  -6),

         new(-35,  -1), new(-64,   9), new(-61,   4), new(-99,  15), new(-95,  10), new(-81,   0), new(-41,  -1), new(-70,  -2),
         new(-30,  -9), new( -4,  -5), new(-14,   0), new(-25,   1), new(-13,  -4), new(-14,  -4), new(-29,   3), new(-23, -11),
         new(-13,   7), new(  6,  -1), new(  3,   5), new( 10,   0), new( -4,   5), new( 23,  10), new(  5,   2), new( -8,  11),
         new(-21,   3), new( -1,   9), new(  0,   8), new( 14,  21), new( 11,  13), new(  2,  13), new(  0,   5), new(-27,   8),
         new(-12,   0), new(-19,  10), new( -6,  17), new( 15,  15), new( 10,  13), new( -1,  10), new(-11,   8), new(  2,  -8),
         new(-12,   1), new(  1,  10), new(  2,  11), new(  1,  14), new(  7,  18), new(  7,   9), new(  5,   3), new(  6,  -6),
         new(  2,   3), new(  4,  -7), new(  9,  -8), new( -1,   4), new(  7,   3), new( 15,  -4), new( 25,  -5), new(  7,  -9),
         new(-10,  -8), new( 11,   4), new( -1,  -2), new(-10,   1), new( -1,  -1), new( -6,   9), new(  5, -10), new(  7, -22),

         new( -6,  27), new(-21,  35), new(-25,  44), new(-28,  42), new(-15,  36), new(  6,  29), new(  1,  31), new( 21,  22),
         new(-18,  28), new(-23,  40), new( -6,  43), new( 11,  34), new( -5,  36), new( 16,  24), new( 15,  20), new( 40,   9),
         new(-21,  27), new(  3,  27), new(  2,  27), new(  7,  24), new( 33,  12), new( 28,   9), new( 65,   4), new( 33,   2),
         new(-25,  30), new(-13,  27), new( -8,  32), new( -2,  28), new(  2,  16), new(  0,  11), new(  7,  14), new(  2,   8),
         new(-34,  23), new(-37,  27), new(-25,  26), new(-16,  24), new(-15,  21), new(-33,  21), new(-10,  12), new(-20,   8),
         new(-39,  18), new(-32,  16), new(-23,  15), new(-22,  18), new(-14,  13), new(-16,   5), new(  6,  -9), new(-14,  -6),
         new(-39,   9), new(-30,  14), new(-11,  12), new( -8,  12), new( -4,   5), new( -4,   0), new(  9,  -9), new(-23,  -1),
         new(-18,   8), new(-15,  10), new( -3,  14), new(  4,   9), new(  8,   3), new(  0,   4), new(  4,  -2), new(-14,  -3),

         new(-49,  19), new(-57,  33), new(-37,  49), new(-12,  34), new(-20,  34), new(-16,  36), new( 27, -12), new(-24,  20),
         new(-18,   4), new(-38,  31), new(-40,  64), new(-50,  81), new(-58,  96), new(-15,  49), new(-27,  48), new( 29,  25),
         new( -7,  16), new(-11,  26), new(-15,  58), new(-13,  60), new( -6,  64), new( 15,  47), new( 24,  24), new( 14,  21),
         new(-22,  33), new(-12,  41), new(-20,  48), new(-24,  69), new(-17,  69), new(-11,  59), new(  0,  57), new( -5,  41),
         new( -9,  19), new(-22,  44), new(-16,  46), new( -8,  56), new(-10,  58), new(-10,  49), new( -1,  40), new(  2,  30),
         new(-13,   3), new( -4,  20), new( -5,  33), new( -8,  38), new( -2,  46), new(  2,  31), new( 10,  15), new(  3,   7),
         new( -6,  -4), new( -3,   0), new(  5,   3), new( 12,  11), new( 11,  14), new( 18, -15), new( 20, -41), new( 28, -66),
         new( -7,  -8), new( -4,  -7), new(  4,   0), new( 12,  11), new( 11,  -5), new( -1, -14), new(  3, -25), new(  3, -38),

         new(  3, -100), new( 18, -55), new( 31, -38), new(-51,  -1), new(-24, -13), new(  3, -10), new( 33, -15), new( 47, -108),
         new(-74, -15), new(-17,  13), new(-54,  26), new( 46,  10), new( -4,  30), new(  0,  41), new( 32,  28), new( -1,  -1),
         new(-101,   1), new( 18,  20), new(-45,  39), new(-63,  50), new(-23,  50), new( 52,  41), new( 23,  39), new(-21,  10),
         new(-72,  -6), new(-61,  24), new(-84,  45), new(-121,  57), new(-112,  57), new(-80,  53), new(-75,  41), new(-119,  17),
         new(-76, -16), new(-61,  11), new(-85,  35), new(-121,  52), new(-113,  50), new(-76,  36), new(-84,  24), new(-134,  12),
         new(-28, -26), new(  0,  -5), new(-49,  17), new(-63,  29), new(-54,  28), new(-52,  19), new(-20,   1), new(-51,  -7),
         new( 56, -28), new( 22,   0), new(  9,  -6), new(-14,   1), new(-13,   4), new( -6,  -1), new( 32,   0), new( 34, -18),
         new( 17, -57), new( 42, -31), new( 16,  -7), new(-32, -26), new( -1, -16), new(  3, -32), new( 40, -28), new( 36, -61),
      };

      public static readonly Score[] KnightMobility =
      {
         new(-34, -51),
         new(-18, -16),
         new(-8, 4),
         new(-2, 14),
         new(2, 22),
         new(7, 30),
         new(14, 28),
         new(19, 25),
         new(24, 17),
      };

      public static readonly Score[] BishopMobility =
      {
         new(-26, -45),
         new(-16, -24),
         new(-7, -7),
         new(-1, 5),
         new(5, 14),
         new(8, 26),
         new(11, 30),
         new(13, 33),
         new(14, 38),
         new(18, 35),
         new(25, 31),
         new(32, 30),
         new(24, 42),
         new(38, 20),
      };

      public static readonly Score[] RookMobility =
      {
         new(-33, -17),
         new(-23, 2),
         new(-20, 3),
         new(-13, 6),
         new(-15, 14),
         new(-8, 17),
         new(-4, 23),
         new(1, 24),
         new(7, 27),
         new(11, 32),
         new(16, 35),
         new(16, 41),
         new(20, 44),
         new(21, 42),
         new(12, 47),
      };

      public static readonly Score[] QueenMobility =
      {
         new(-23, -46),
         new(-19, -85),
         new(-24, -19),
         new(-21, 4),
         new(-20, 21),
         new(-17, 24),
         new(-15, 40),
         new(-14, 49),
         new(-12, 55),
         new(-11, 59),
         new(-10, 64),
         new(-7, 69),
         new(-6, 70),
         new(-7, 75),
         new(-5, 77),
         new(-3, 81),
         new(-5, 89),
         new(-3, 87),
         new(4, 85),
         new(16, 76),
         new(20, 78),
         new(67, 50),
         new(58, 54),
         new(78, 35),
         new(136, 25),
         new(98, 24),
         new(54, 53),
         new(37, 42),
      };

      public static Score[] KingAttackWeights =
      {
         new(0, 0),
         new(12, -6),
         new(20, -6),
         new(24, -6),
         new(15, 12),
      };

      public static Score[] PawnShield =
      {
         new(-31, -12),
         new(15, -18),
         new(49, -39),
         new(73, -71),
      };

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      {
         new(0, 0),
         new(0, 7),
         new(-6, 15),
         new(-8, 37),
         new(11, 61),
         new(2, 125),
         new(29, 103),
      };

      public static Score[] DefendedPawn = [
         new(-30, -36),
         new(-12, -15),
         new(6, 7),
         new(23, 33),
         new(38, 55),
         new(50, 72),
         new(63, 67),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-27, -26),
         new(-10, -5),
         new(7, 12),
         new(23, 38),
         new(38, 56),
         new(44, 122),
         new(61, -3),
         new(-14, 0),
         new(0, 0),
      ];
   }
}
