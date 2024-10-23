using System.Collections.Immutable;

namespace Puffin
{
   public static class Constants
   {
      public const string START_POS = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

      public const int MAX_PLY = 254;
      public const int INFINITY = 30000;
      public const int MATE = 20000;

      public const double LMR_Reduction_Base = 0.85;
      public const double LMR_Reduction_Multiplier = 0.3;

      public readonly static ulong[] SquareBB = new ulong[64];
      public readonly static ulong[][] BetweenBB = new ulong[64][];
      public readonly static ulong[][] PassedPawnMasks = new ulong[2][];
      public readonly static ulong[] IsolatedPawnMasks = new ulong[8];
      public readonly static ulong[][] ForwardMask = new ulong[2][];
      public readonly static int[][] TaxiDistance = new int[64][];

      public readonly static int[][] LMR_Reductions = new int[MAX_PLY][];

      public readonly static int[] SEE_VALUES = [100, 325, 350, 500, 1000, 0, 0];
      public readonly static ImmutableArray<int> PHASE_VALUES = [0, 1, 1, 2, 4, 0]; // Pawns do not contribute to the phase value

      public readonly static ImmutableArray<ulong> FILE_MASKS =
      [
         0x101010101010101,
         0x202020202020202,
         0x404040404040404,
         0x808080808080808,
         0x1010101010101010,
         0x2020202020202020,
         0x4040404040404040,
         0x8080808080808080
,
      ];

      public readonly static ImmutableArray<ulong> RANK_MASKS = [
         0xFF00000000000000,
         0xFF000000000000,
         0xFF0000000000,
         0xFF00000000,
         0xFF000000,
         0xFF0000,
         0xFF00,
         0xFF
      ];

      static Constants()
      {
         long file, rank, between, line;
         int m1 = -1;
         long a2a7 = 0x0001010101010100;
         long b2g7 = 0x0040201008040200;
         long h1b7 = 0x0002040810204080;
         ulong notFileA = ~FILE_MASKS[(int)File.A];
         ulong notFileH = ~FILE_MASKS[(int)File.H];
         PassedPawnMasks[(int)Color.White] = new ulong[64];
         PassedPawnMasks[(int)Color.Black] = new ulong[64];
         ForwardMask[(int)Color.White] = new ulong[64];
         ForwardMask[(int)Color.Black] = new ulong[64];

         for (int depth = 0; depth < MAX_PLY; depth++)
         {
            LMR_Reductions[depth] = new int[218];

            for (int moves = 0; moves < 218; moves++)
            {
               LMR_Reductions[depth][moves] = (int)(LMR_Reduction_Base + Math.Log(depth) * Math.Log(moves) * LMR_Reduction_Multiplier);
            }
         }

         for (int i = 0; i < 64; i++)
         {
            Bitboard board = new();
            board.SetBit(i);

            SquareBB[i] = board.Value;
            BetweenBB[i] = new ulong[64];
            TaxiDistance[i] = new int[64];

            PassedPawnMasks[(int)Color.White][i] = FILE_MASKS[i & 7] | ((FILE_MASKS[i & 7] & notFileH) << 1) | ((FILE_MASKS[i & 7] & notFileA) >> 1);
            PassedPawnMasks[(int)Color.Black][i] = FILE_MASKS[i & 7] | ((FILE_MASKS[i & 7] & notFileH) << 1) | ((FILE_MASKS[i & 7] & notFileA) >> 1);

            ForwardMask[(int)Color.White][i] = PassedPawnMasks[(int)Color.White][i] & FILE_MASKS[i & 7];
            ForwardMask[(int)Color.Black][i] = PassedPawnMasks[(int)Color.Black][i] & FILE_MASKS[i & 7];

            IsolatedPawnMasks[i & 7] = ((FILE_MASKS[i & 7] & notFileH) << 1) | ((FILE_MASKS[i & 7] & notFileA) >> 1);

            for (int j = 7 - (i >> 3); j >= 0; j--)
            {
               PassedPawnMasks[(int)Color.White][i] &= ~RANK_MASKS[j];
               ForwardMask[(int)Color.White][i] &= ~RANK_MASKS[j];
            }

            for (int j = 7 - (i >> 3); j <= 7; j++)
            {
               PassedPawnMasks[(int)Color.Black][i] &= ~RANK_MASKS[j];
               ForwardMask[(int)Color.Black][i] &= ~RANK_MASKS[j];
            }

            for (int j = 0; j < 64; j++)
            {
               between = (long)m1 << i ^ (long)m1 << j;
               file = ((long)j & 7) - ((long)i & 7);
               rank = ((long)j | 7) - i >> 3;
               line = (file & 7) - 1 & a2a7;
               line += 2 * ((rank & 7) - 1 >> 58);
               line += (rank - file & 15) - 1 & b2g7;
               line += (rank + file & 15) - 1 & h1b7;
               line *= between & -between;

               BetweenBB[i][j] = (ulong)(line & between);
               TaxiDistance[i][j] = Math.Abs((j >> 3) - (i >> 3)) + Math.Abs((j & 7) - (i & 7));
            }
         }
      }
   }
}
