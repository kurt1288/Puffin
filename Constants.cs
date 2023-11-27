using System.Collections.Immutable;

namespace Puffin
{
   public static class Constants
   {
      public const string START_POS = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

      public const int MAX_PLY = 254;
      public const int INFINITY = 30000;
      public const int MATE = 20000;

      public readonly static ulong[] SquareBB = new ulong[64];
      public readonly static ulong[][] BetweenBB = new ulong[64][];

      public readonly static ImmutableArray<ulong> FILE_MASKS = ImmutableArray.Create<ulong>(
         0x101010101010101,
         0x202020202020202,
         0x404040404040404,
         0x808080808080808,
         0x1010101010101010,
         0x2020202020202020,
         0x4040404040404040,
         0x8080808080808080
      );

      public readonly static ImmutableArray<ulong> RANK_MASKS = ImmutableArray.Create<ulong>(
         0xFF00000000000000,
         0xFF000000000000,
         0xFF0000000000,
         0xFF00000000,
         0xFF000000,
         0xFF0000,
         0xFF00,
         0xFF
      );

      static Constants()
      {
         long file, rank, between, line;
         int m1 = -1;
         long a2a7 = 0x0001010101010100;
         long b2g7 = 0x0040201008040200;
         long h1b7 = 0x0002040810204080;

         for (int i = 0; i < 64; i++)
         {
            Bitboard board = new();
            board.SetBit(i);

            SquareBB[i] = board.Value;
            BetweenBB[i] = new ulong[64];

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
            }
         }
      }
   }
}
