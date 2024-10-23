using System.Runtime.CompilerServices;
using static Puffin.Constants;
using static Puffin.Attacks.Attacks;

namespace Puffin
{
   internal struct EvalInfo(ulong[] mobilitySquares, ulong[] kingZones)
   {
      internal readonly ulong[] MobilitySquares = mobilitySquares;
      internal readonly ulong[] KingZones = kingZones;
      internal int[] KingAttacksCount = [0, 0];
      internal Score[] KingAttacksWeight = [new(), new()];
   }

   internal static class Evaluation
   {
      public static int Evaluate(Board board)
      {
         EvalInfo info = InitEval(board);
         
         // Material and PST score
         Score score = board.MaterialScore[(int)Color.White] - board.MaterialScore[(int)Color.Black];
         
         // Piece evaluation
         score += EvaluatePieces(board, info);

         if (board.SideToMove == Color.Black)
         {
            score *= -1;
         }

         return (score.Mg * board.Phase + score.Eg * (24 - board.Phase)) / 24;
      }

      private static EvalInfo InitEval(Board board)
      {
         // Mobility squares: All squares not attacked by enemy pawns minus own blocked pawns.
         Bitboard occ = board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black];

         Bitboard blackPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)Color.Black];
         Bitboard whitePawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)Color.White];

         EvalInfo info = new(
            [
               ~PawnAnyAttacks(blackPawns.Value, Color.Black) ^ (occ.Shift(Direction.Up) & whitePawns).Value,
               ~PawnAnyAttacks(whitePawns.Value, Color.White) ^ (occ.Shift(Direction.Down) & blackPawns).Value,
            ],
            [
               KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)],
               KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)],
            ]
         );

         return info;
      }

      private static Score EvaluatePieces(Board board, EvalInfo info)
      {
         Score score = new();

         if ((board.PieceBB[(int)PieceType.Bishop] & board.ColorBB[(int)Color.White]).CountBits() >= 2)
         {
            score += BishopPair;
         }
         if ((board.PieceBB[(int)PieceType.Bishop] & board.ColorBB[(int)Color.Black]).CountBits() >= 2)
         {
            score -= BishopPair;
         }

         score += EvalPawns(board, info, Color.White) - EvalPawns(board, info, Color.Black);
         score += EvalKnights(board, info, Color.White) - EvalKnights(board, info, Color.Black);
         score += EvalBishops(board, info, Color.White) - EvalBishops(board, info, Color.Black);
         score += EvalRooks(board, info, Color.White) - EvalRooks(board, info, Color.Black);
         score += EvalQueens(board, info, Color.White) - EvalQueens(board, info, Color.Black);
         score += EvalKings(board, info, Color.White) - EvalKings(board, info, Color.Black);

         return score;
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
            Piece piece = board.Squares[square];
            score += PieceValues[(int)piece.Type];
            score += GetPSTScore(piece, square);
         }
         return score;
      }

      private static Score EvalPawns(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard pawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color];

         score += DefendedPawn[(pawns & PawnAnyAttacks(pawns.Value, color)).CountBits()];
         score += ConnectedPawn[(pawns & pawns.RightShift()).CountBits()];

         // Enemy non-pawn pieces that can be attacked with a pawn push
         ulong pawnShift = (pawns.Shift(color == Color.White ? Direction.Down : Direction.Up) & ~(board.ColorBB[(int)color] | board.ColorBB[(int)color ^ 1]).Value).Value;
         ulong enemyPieces = (board.ColorBB[(int)color ^ 1] ^ (board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color ^ 1])).Value;
         score += PawnPushThreats * new Bitboard(PawnAnyAttacks(pawnShift, color) & enemyPieces).CountBits();

         // Enemy non-pawn pieces that are attacked
         score += PawnAttacks * new Bitboard(PawnAnyAttacks(pawns.Value, color) & enemyPieces).CountBits();

         while (pawns)
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & (board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color ^ 1]).Value) == 0)
            {
               score += PassedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1];
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)] * FriendlyKingPawnDistance;
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)] * EnemyKingPawnDistance;
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & (board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color]).Value) == 0)
            {
               // Penalty is based on file
               score -= IsolatedPawn[square & 7];
            }
         }

         return score;
      }

      private static Score EvalKnights(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard knightsBB = board.PieceBB[(int)PieceType.Knight] & board.ColorBB[(int)color];

         while (knightsBB)
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += KnightMobility[new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits()];

            if ((KnightAttacks[square] & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalBishops(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard bishopBB = board.PieceBB[(int)PieceType.Bishop] & board.ColorBB[(int)color];

         while (bishopBB)
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            ulong moves = GetBishopAttacks(square, (board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black]).Value);
            score += BishopMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalRooks(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard rookBB = board.PieceBB[(int)PieceType.Rook] & board.ColorBB[(int)color];

         while (rookBB)
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            ulong moves = GetRookAttacks(square, (board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black]).Value);
            score += RookMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((FILE_MASKS[square & 7] & board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value) == 0)
            {
               if ((FILE_MASKS[square & 7] & board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color ^ 1].Value) == 0)
               {
                  score += RookOpenFile;
               }
               else
               {
                  score += RookHalfOpenFile;
               }
            }

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalQueens(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard queenBB = board.PieceBB[(int)PieceType.Queen] & board.ColorBB[(int)color];

         while (queenBB)
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            ulong moves = GetQueenAttacks(square, (board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black]).Value);
            score += QueenMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;
            }
         }

         return score;
      }

      private static Score EvalKings(Board board, EvalInfo info, Color color)
      {
         Score score = new();
         Bitboard kingBB = board.PieceBB[(int)PieceType.King] & board.ColorBB[(int)color];

         while (kingBB)
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & SquareBB[kingSq]) != 0)
            {
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x7070000000000 : 0xe0e00000000000) : (ulong)(kingSq % 8 < 3 ? 0x70700 : 0xe0e000);

               Bitboard pawns = new(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & pawnSquares);
               score += PawnShield[Math.Min(pawns.CountBits(), 3)];

               if ((board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & FILE_MASKS[kingSq & 7]) == 0)
               {
                  score -= (board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color ^ 1].Value & FILE_MASKS[kingSq & 7]) == 0
                     ? KingOpenFile
                     : KingHalfOpenFile;
               }
            }

            if (info.KingAttacksCount[(int)color ^ 1] >= 2)
            {
               score -= info.KingAttacksWeight[(int)color ^ 1];
            }
         }

         return score;
      }

      public static readonly Score[] PieceValues = [
         new(60, 109),
         new(301, 340),
         new(314, 351),
         new(409, 629),
         new(855, 1156),
         new(0, 0),
      ];

      public static readonly Score[] PST =
      [
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),
         new( 27, 108), new( 28, 100), new( 21, 102), new( 59,  50), new( 26,  53), new( 21,  56), new(-84, 105), new(-91, 125),
         new( 10,  35), new( -1,  43), new( 25,   3), new( 32, -33), new( 40, -38), new( 71, -24), new( 36,  17), new( 16,  23),
         new(-12,  17), new(-11,  11), new( -6,  -6), new( -4, -22), new( 12, -18), new( 17, -21), new(  0,   0), new(  3,   0),
         new(-17,  -1), new(-19,   1), new( -7, -14), new(  0, -19), new(  0, -15), new(  9, -21), new( -9,  -7), new( -4, -14),
         new(-24,  -6), new(-20,  -7), new(-14, -14), new(-10, -12), new(  0,  -9), new(-25, -10), new(-16, -10), new(-27, -11),
         new(-17,  -2), new(-11,  -3), new( -6,  -9), new( -1, -10), new(  3,   0), new(  3,  -6), new(  0, -11), new(-25,  -9),
         new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0), new(  0,   0),

         new(-122, -31), new(-114,  -2), new(-79,  11), new(-45,   1), new(-19,   4), new(-72, -16), new(-93,   2), new(-85, -49),
         new(-19,   0), new( -9,   6), new( 12,   2), new( 28,   0), new( 15,  -4), new( 55, -16), new(  0,   2), new( 11, -13),
         new( -6,   0), new( 14,   2), new( 25,  15), new( 31,  17), new( 58,   4), new( 72,  -7), new( 31,  -6), new( 15,  -7),
         new(  1,  13), new( 12,  12), new( 25,  22), new( 46,  25), new( 26,  26), new( 53,  19), new( 21,  15), new( 35,   4),
         new( -3,  15), new(  1,   8), new(  7,  25), new( 13,  26), new( 17,  30), new( 19,  17), new( 29,   6), new( 11,  12),
         new(-21,   2), new(-12,   4), new( -7,   7), new( -8,  22), new(  5,  20), new(  0,   3), new(  9,   1), new( -5,   5),
         new(-27,   4), new(-22,   7), new(-19,   2), new( -6,   4), new( -6,   3), new( -4,   0), new( -2,   2), new( -4,  16),
         new(-60,  10), new(-20,  -1), new(-35,  -1), new(-18,   0), new(-14,   4), new(-10,  -3), new(-16,   4), new(-31,   9),

         new(-35,   7), new(-69,  11), new(-65,   8), new(-106,  17), new(-97,  16), new(-87,   2), new(-55,   6), new(-70,  -1),
         new(-31,  -6), new(-12,  -2), new(-21,  -1), new(-32,   3), new(-14,  -7), new(-20,  -2), new(-33,   1), new(-37,  -2),
         new(-15,   9), new( -1,   0), new(  1,   4), new(  6,  -5), new(  0,   0), new( 36,   1), new( 13,   0), new(  6,   6),
         new(-20,   4), new(  9,   5), new(  4,   2), new( 18,  17), new( 15,   7), new( 16,   6), new( 16,   0), new(-16,   7),
         new( -6,   0), new(-14,   6), new( -2,  11), new( 14,  11), new(  9,  11), new(  1,   4), new( -8,   7), new( 16, -12),
         new( -8,   3), new(  9,   5), new(  0,   8), new(  2,  12), new(  5,  15), new(  6,   8), new(  9,   0), new( 11,  -1),
         new( 10,  10), new( -1,  -5), new(  9,  -7), new(-11,   4), new( -4,   6), new(  7,  -3), new( 20,   0), new(  9,  -2),
         new( -2,   0), new( 13,   7), new( -3,   0), new(-13,   0), new( -4,  -2), new(-10,  13), new(  6,  -4), new( 15, -11),

         new(-15,  29), new(-23,  33), new(-30,  41), new(-32,  37), new(-18,  31), new(  1,  28), new(  1,  30), new( 17,  23),
         new(-26,  27), new(-26,  37), new(-13,  40), new( -1,  30), new(-12,  30), new( 11,  21), new( 16,  16), new( 31,   9),
         new(-31,  26), new( -6,  23), new( -8,  24), new( -6,  21), new( 21,  10), new( 26,   4), new( 64,   0), new( 33,  -1),
         new(-27,  27), new( -8,  21), new( -7,  25), new( -8,  22), new(  0,  10), new( 15,   3), new( 25,   7), new( 11,   3),
         new(-33,  19), new(-33,  20), new(-22,  17), new(-17,  15), new(-19,  15), new(-17,  11), new(  6,   5), new( -6,   3),
         new(-35,  13), new(-27,   9), new(-23,   5), new(-23,   7), new(-12,   1), new( -4,  -5), new( 26, -18), new(  2, -13),
         new(-34,   3), new(-30,   7), new(-17,   4), new(-15,   2), new( -9,  -4), new(  0, -11), new( 16, -18), new(-18,  -8),
         new(-17,   7), new(-17,   5), new(-12,  10), new( -4,   2), new(  1,  -3), new(  0,   0), new(  3,  -5), new( -9,  -5),

         new(-38,  32), new(-51,  54), new(-34,  74), new(-14,  68), new(-16,  67), new( -7,  62), new( 40,   8), new(-14,  43),
         new(-14,  12), new(-39,  44), new(-40,  79), new(-54, 103), new(-49, 118), new( -9,  74), new(-16,  61), new( 39,  44),
         new(-13,  23), new(-18,  31), new(-21,  66), new(-17,  75), new( -4,  87), new( 28,  70), new( 40,  37), new( 35,  42),
         new(-17,  30), new( -3,  34), new(-11,  46), new(-21,  72), new(-14,  87), new(  3,  74), new( 16,  70), new( 10,  56),
         new( -5,  18), new(-16,  43), new(-15,  47), new(-13,  64), new(-14,  65), new( -6,  58), new(  3,  52), new( 13,  46),
         new( -7,   3), new( -2,  22), new( -9,  37), new(-11,  35), new( -6,  40), new(  3,  32), new( 15,  18), new( 12,  12),
         new( -6,   0), new( -7,   1), new( -1,   3), new(  2,   8), new(  1,  11), new(  8, -14), new( 14, -36), new( 23, -53),
         new(-14,  -3), new(-10,  -4), new( -3,  -3), new(  1,   9), new(  0,  -6), new(-11,  -6), new( -7, -12), new(  2, -32),

         new( -5, -75), new( 11, -39), new( 17, -27), new(-74,   8), new(-33,  -8), new( -2,  -2), new( 43,  -5), new( 62, -85),
         new(-79,  -5), new(-27,  16), new(-67,  25), new( 27,  10), new(-19,  25), new(-15,  41), new( 22,  32), new( 10,   8),
         new(-93,   0), new( 11,  15), new(-53,  29), new(-80,  41), new(-35,  41), new( 43,  33), new( 24,  31), new(-13,   5),
         new(-53, -17), new(-55,  10), new(-85,  30), new(-136,  44), new(-125,  44), new(-75,  38), new(-60,  27), new(-99,   7),
         new(-59, -28), new(-51,  -1), new(-71,  18), new(-116,  36), new(-108,  36), new(-62,  22), new(-67,  13), new(-117,   4),
         new(-12, -30), new( 22, -14), new(-31,   5), new(-45,  17), new(-37,  17), new(-37,  11), new( -6,  -1), new(-38,  -8),
         new( 51, -18), new( 21,   0), new( 22,  -8), new( -7,  -1), new( -6,   1), new(  7,  -3), new( 30,   3), new( 25,  -5),
         new(  8, -33), new( 34, -20), new( 12,  -4), new(-32, -18), new(  2, -12), new(  3, -23), new( 34, -18), new( 30, -44),
      ];

      public static readonly Score[] KnightMobility =
      [
         new(-97, -116),
         new(-44, -45),
         new(-21, -11),
         new(-13, 7),
         new(-2, 18),
         new(2, 29),
         new(11, 31),
         new(18, 34),
         new(29, 28),
      ];

      public static readonly Score[] BishopMobility =
      [
         new(-55, -112),
         new(-38, -72),
         new(-24, -22),
         new(-14, -1),
         new(-4, 8),
         new(4, 20),
         new(10, 28),
         new(14, 33),
         new(17, 38),
         new(21, 38),
         new(24, 40),
         new(32, 35),
         new(35, 38),
         new(42, 27),
      ];

      public static readonly Score[] RookMobility =
      [
         new(-100, -152),
         new(-28, -87),
         new(-27, -25),
         new(-20, -7),
         new(-15, 2),
         new(-10, 9),
         new(-9, 17),
         new(-5, 21),
         new(-2, 23),
         new(2, 27),
         new(6, 31),
         new(6, 36),
         new(10, 37),
         new(16, 37),
         new(20, 35),
      ];

      public static readonly Score[] QueenMobility =
      [
         new(-20, -2),
         new(-70, -22),
         new(-54, -50),
         new(-53, -111),
         new(-32, -60),
         new(-33, 11),
         new(-22, 3),
         new(-19, 26),
         new(-17, 42),
         new(-13, 59),
         new(-9, 59),
         new(-6, 66),
         new(-3, 75),
         new(0, 75),
         new(1, 81),
         new(3, 86),
         new(4, 91),
         new(5, 96),
         new(4, 106),
         new(10, 102),
         new(15, 106),
         new(20, 101),
         new(33, 102),
         new(59, 83),
         new(50, 102),
         new(145, 46),
         new(65, 84),
         new(37, 78),
      ];

      public static Score RookHalfOpenFile = new(10, 10);
      public static Score RookOpenFile = new(29, 7);
      public static Score KingOpenFile = new(69, -7);
      public static Score KingHalfOpenFile = new(26, -25);

      public static Score[] KingAttackWeights =
      [
         new(0, 0),
         new(10, -5),
         new(13, -1),
         new(26, -9),
         new(18, 13),
      ];

      public static Score[] PawnShield =
      [
         new(-28, -5),
         new(4, -18),
         new(38, -28),
         new(72, -43),
      ];

      // bonus/penalty depending on the rank of the pawn
      public static Score[] PassedPawn =
      [
         new(0, 0),
         new(32, -27),
         new(16, -7),
         new(2, 29),
         new(7, 67),
         new(-14, 144),
         new(12, 120),
      ];

      public static Score[] DefendedPawn = [
         new(-26, -19),
         new(-8, -13),
         new(7, -3),
         new(20, 12),
         new(31, 30),
         new(43, 38),
         new(37, 26),
         new(0, 0),
      ];

      public static Score[] ConnectedPawn = [
         new(-15, -8),
         new(-4, 2),
         new(4, 3),
         new(13, 13),
         new(22, 12),
         new(25, 63),
         new(46, -4),
         new(-5, 0),
         new(0, 0),
      ];

      public static Score[] IsolatedPawn =
      [
         new(3, 4),
         new(2, 12),
         new(10, 11),
         new(8, 17),
         new(12, 19),
         new(6, 11),
         new(2, 11),
         new(5, 9),
      ];

      public static Score FriendlyKingPawnDistance = new(6, -9);
      public static Score EnemyKingPawnDistance = new(-7, 9);
      public static Score BishopPair = new(23, 65);
      public static Score PawnPushThreats = new(19, 0);
      public static Score PawnAttacks = new(46, 8);
   }
}
