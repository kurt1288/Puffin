// Legacy build uses Kindergarten Super SISSY Bitboards (KiSS)
// https://www.talkchess.com/forum3/viewtopic.php?f=7&t=81234&start=30

using System.Runtime.CompilerServices;

#if Legacy
namespace Puffin.Attacks
{
   internal static partial class Attacks
   {
      readonly static ulong[] dMask = new ulong[64];
      readonly static ulong[] aMask = new ulong[64];
      readonly static ulong[] hMask = new ulong[64];
      readonly static ulong[][] vSubset = new ulong[64][];
      readonly static ulong[][] hSubset = new ulong[64][];
      readonly static ulong[][] dSubset = new ulong[64][];
      readonly static ulong[][] aSubset = new ulong[64][];
      readonly static ulong FileA2A7 = 0x0001010101010100;
      readonly static ulong FileB2B7 = 0x0002020202020200;
      readonly static ulong DiagC2H7 = 0x0080402010080400;
      readonly static int[] HorizontalShiftTable = new int[64];

      static Attacks()
      {
         InitAttacks();

         for (int i = 0; i < 64; i++)
         {
            vSubset[i] = new ulong[64];
            hSubset[i] = new ulong[64];
            dSubset[i] = new ulong[64];
            aSubset[i] = new ulong[64];
            HorizontalShiftTable[i] = (i & 56) + 1;
         }

         for (int square = 0; square < 64; square++)
         {
            int x = square % 8;
            int y = square / 8;

            // diagonals
            for (int ts = square + 9, dx = x + 1, dy = y + 1; dx < (int)File.H && dy < (int)Rank.Rank_8; dMask[square] |= 1UL << ts, ts += 9, dx++, dy++) ;
            for (int ts = square - 9, dx = x - 1, dy = y - 1; dx > (int)File.A && dy > (int)Rank.Rank_1; dMask[square] |= 1UL << ts, ts -= 9, dx--, dy--) ;

            // anti-diagonals
            for (int ts = square + 7, dx = x - 1, dy = y + 1; dx > (int)File.A && dy < (int)Rank.Rank_8; aMask[square] |= 1UL << ts, ts += 7, dx--, dy++) ;
            for (int ts = square - 7, dx = x + 1, dy = y - 1; dx < (int)File.H && dy > (int)Rank.Rank_1; aMask[square] |= 1UL << ts, ts -= 7, dx++, dy--) ;

            // diagonal indexes
            for (int index = 0; index < 64; index++)
            {
               dSubset[square][index] = 0;
               ulong occ = (ulong)index << 1;

               if ((square & 7) != (int)File.H && (square >> 3) != (int)Rank.Rank_8)
               {
                  for (int ts = square + 9; ; ts += 9)
                  {
                     dSubset[square][index] |= (1UL << ts);
                     if ((occ & (1UL << (ts & 7))) != 0) break;
                     if ((ts & 7) == (int)File.H || (ts >> 3) == (int)Rank.Rank_8) break;
                  }
               }

               if ((square & 7) != (int)File.A && (square >> 3) != (int)Rank.Rank_1)
               {
                  for (int ts = square - 9; ; ts -= 9)
                  {
                     dSubset[square][index] |= (1UL << ts);
                     if ((occ & (1UL << (ts & 7))) != 0) break;
                     if ((ts & 7) == (int)File.A || (ts >> 3) == (int)Rank.Rank_1) break;
                  }
               }
            }

            // ant-diagonal indexes
            for (int index = 0; index < 64; index++)
            {
               aSubset[square][index] = 0;
               ulong occ = (ulong)index << 1;

               if ((square & 7) != (int)File.A && (square >> 3) != (int)Rank.Rank_8)
               {
                  for (int ts = square + 7; ; ts += 7)
                  {
                     aSubset[square][index] |= (1UL << ts);
                     if ((occ & (1UL << (ts & 7))) != 0) break;
                     if ((ts & 7) == (int)File.A || (ts >> 3) == (int)Rank.Rank_8) break;
                  }
               }

               if ((square & 7) != (int)File.H && (square >> 3) != (int)Rank.Rank_1)
               {
                  for (int ts = square - 7; ; ts -= 7)
                  {
                     aSubset[square][index] |= (1UL << ts);
                     if ((occ & (1UL << (ts & 7))) != 0) break;
                     if ((ts & 7) == (int)File.H || (ts >> 3) == (int)Rank.Rank_1) break;
                  }
               }
            }

            // horizontals
            for (int ts = square + 1, dx = x + 1; dx < (int)File.H; hMask[square] |= 1UL << ts, ts += 1, dx++) ;
            for (int ts = square - 1, dx = x - 1; dx > (int)File.A; hMask[square] |= 1UL << ts, ts -= 1, dx--) ;

            // vertical indexes
            for (ulong index = 0; index < 64; index++)
            {
               vSubset[square][index] = 0;
               ulong blockers = 0;

               for (int i = 0; i <= 5; i++)
               {
                  if ((index & (1UL << i)) != 0)
                  {
                     blockers |= (1UL << ((5 - i) << 3) + 8);
                  }
               }

               if ((square >> 3) != (int)Rank.Rank_8)
               {
                  for (int ts = square + 8; ; ts += 8)
                  {
                     vSubset[square][index] |= (1UL << ts);
                     if ((blockers & (1UL << (ts - (ts & 7)))) != 0) break;
                     if ((ts >> 3) == (int)Rank.Rank_8) break;
                  }
               }

               if ((square >> 3) != (int)Rank.Rank_1)
               {
                  for (int ts = square - 8; ; ts -= 8)
                  {
                     vSubset[square][index] |= (1UL << ts);
                     if ((blockers & (1UL << (ts - (ts & 7)))) != 0) break;
                     if ((ts >> 3) == (int)Rank.Rank_1) break;
                  }
               }
            }

            // horizontal indexes
            for (int index = 0; index < 64; index++)
            {
               hSubset[square][index] = 0;
               ulong occ = (ulong)index << 1;

               if ((square & 7) != (int)File.H)
               {
                  for (int ts = square + 1; ; ts += 1)
                  {
                     hSubset[square][index] |= (1UL << ts);
                     if ((occ & (1UL << (ts & 7))) != 0) break;
                     if ((ts & 7) == (int)File.H) break;
                  }
               }

               if ((square & 7) != (int)File.A)
               {
                  for (int ts = square - 1; ; ts -= 1)
                  {
                     hSubset[square][index] |= (1UL << ts);
                     if ((occ & (1UL << (ts & 7))) != 0) break;
                     if ((ts & 7) == (int)File.A) break;
                  }
               }
            }
         }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetBishopAttacks(int square, ulong occupied)
      {
         return dSubset[square][((occupied & dMask[square]) * FileB2B7) >> 58] + aSubset[square][((occupied & aMask[square]) * FileB2B7) >> 58];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetRookAttacks(int square, ulong occupied)
      {
         return hSubset[square][(occupied >> HorizontalShiftTable[square]) & 63] + vSubset[square][(((occupied >> (square & 7)) & FileA2A7) * DiagC2H7) >> 58];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetQueenAttacks(int square, ulong occupied)
      {
         return GetBishopAttacks(square, occupied) | GetRookAttacks(square, occupied);
      }
   }
}
#endif
