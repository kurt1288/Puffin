// Legacy build uses Kindergarten Super SISSY Bitboards (KiSS)
// https://www.talkchess.com/forum3/viewtopic.php?f=7&t=81234&start=30

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using static Puffin.Constants;

namespace Puffin
{
   internal static class Attacks
   {
#if Pext
      readonly static Slider[] Bishops = new Slider[64];
      readonly static Slider[] Rooks = new Slider[64];
#elif Legacy
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
#endif

      public static readonly ulong[] KingAttacks = new ulong[64];
      public static readonly ulong[] KnightAttacks = new ulong[64];
      public static readonly ulong[][] PawnAttacks = new ulong[2][];

      static Attacks()
      {
         ulong notFileA = ~FILE_MASKS[(int)File.A];
         ulong notFileH = ~FILE_MASKS[(int)File.H];
         ulong notFilesAB = ~(FILE_MASKS[(int)File.A] | FILE_MASKS[(int)File.B]);
         ulong notFilesGH = ~(FILE_MASKS[(int)File.G] | FILE_MASKS[(int)File.H]);

         PawnAttacks[(int)Color.White] = new ulong[64];
         PawnAttacks[(int)Color.Black] = new ulong[64];

         // Generate blocking masks for sliding pieces
         for (int i = 0; i < 64; i++)
         {
#if Pext
            Bishops[i] = new(GenerateBishopMasks(i));
            Rooks[i] = new(GenerateRookMasks(i));
#elif Legacy
            vSubset[i] = new ulong[64];
            hSubset[i] = new ulong[64];
            dSubset[i] = new ulong[64];
            aSubset[i] = new ulong[64];
            HorizontalShiftTable[i] = (i & 56) + 1;
#endif
         }

         for (int square = 0; square < 64; square++)
         {
            ulong bitboard = 1ul << square;

            PawnAttacks[(int)Color.White][square] = bitboard >> 7 & notFileA | bitboard >> 9 & notFileH;
            PawnAttacks[(int)Color.Black][square] = bitboard << 7 & notFileH | bitboard << 9 & notFileA;

            bitboard = 1ul << square;

            KingAttacks[square] = bitboard >> 8 |
                                    bitboard >> 9 & notFileH |
                                    bitboard >> 7 & notFileA |
                                    bitboard >> 1 & notFileH |
                                    bitboard << 8 |
                                    bitboard << 9 & notFileA |
                                    bitboard << 7 & notFileH |
                                    bitboard << 1 & notFileA;

            bitboard = 1ul << square;

            KnightAttacks[square] = bitboard >> 17 & notFileH |
                                     bitboard >> 15 & notFileA |
                                     bitboard >> 10 & notFilesGH |
                                     bitboard >> 6 & notFilesAB |
                                     bitboard << 17 & notFileA |
                                     bitboard << 15 & notFileH |
                                     bitboard << 10 & notFilesAB |
                                     bitboard << 6 & notFilesGH;

#if Pext
            int count = Rooks[square].Mask.CountBits();

            for (ulong i = 0; i < (1ul << count); i++)
            {
               ulong occupied = Bmi2.X64.ParallelBitDeposit(i, Rooks[square].Mask.Value);
               Rooks[square].Attacks.Add(GenerateRookAttacks(square, occupied));
            }

            count = Bishops[square].Mask.CountBits();

            for (ulong i = 0; i < (1ul << count); i++)
            {
               ulong occupied = Bmi2.X64.ParallelBitDeposit(i, Bishops[square].Mask.Value);
               Bishops[square].Attacks.Add(GenerateBishopAttacks(square, occupied));
            }
#elif Legacy
            int x = square % 8;
            int y = square / 8;

            // diagonals
            for (int ts = square + 9, dx = x + 1, dy = y + 1; dx < (int)File.H && dy < (int)Rank.Rank_8; dMask[square] |= 1UL << ts, ts += 9, dx++, dy++);
            for (int ts = square - 9, dx = x - 1, dy = y - 1; dx > (int)File.A && dy > (int)Rank.Rank_1; dMask[square] |= 1UL << ts, ts -= 9, dx--, dy--);

            // anti-diagonals
            for (int ts = square + 7, dx = x - 1, dy = y + 1; dx > (int)File.A && dy < (int)Rank.Rank_8; aMask[square] |= 1UL << ts, ts += 7, dx--, dy++);
            for (int ts = square - 7, dx = x + 1, dy = y - 1; dx < (int)File.H && dy > (int)Rank.Rank_1; aMask[square] |= 1UL << ts, ts -= 7, dx++, dy--);

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
            for (int ts = square + 1, dx = x + 1; dx < (int)File.H; hMask[square] |= 1UL << ts, ts += 1, dx++);
            for (int ts = square - 1, dx = x - 1; dx > (int)File.A; hMask[square] |= 1UL << ts, ts -= 1, dx--);

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
#endif
         }
      }

#if Pext
      static Bitboard GenerateBishopMasks(int square)
      {
         int rank = square / 8;
         int file = square % 8;

         ulong attacks = 0ul;

         for (int r = rank + 1, f = file + 1; r <= 6 && f <= 6; r++, f++) attacks |= 1ul << (r * 8 + f);
         for (int r = rank - 1, f = file + 1; r >= 1 && f <= 6; r--, f++) attacks |= 1ul << (r * 8 + f);
         for (int r = rank + 1, f = file - 1; r <= 6 && f >= 1; r++, f--) attacks |= 1ul << (r * 8 + f);
         for (int r = rank - 1, f = file - 1; r >= 1 && f >= 1; r--, f--) attacks |= 1ul << (r * 8 + f);

         return new Bitboard(attacks);
      }

      static Bitboard GenerateRookMasks(int square)
      {
         int rank = square / 8;
         int file = square % 8;

         ulong attacks = 0ul;

         // Rook masks
         for (int j = rank + 1; j <= 6; j++) attacks |= 1ul << (j * 8 + file);
         for (int j = rank - 1; j >= 1; j--) attacks |= 1ul << (j * 8 + file);
         for (int j = file + 1; j <= 6; j++) attacks |= 1ul << (rank * 8 + j);
         for (int j = file - 1; j >= 1; j--) attacks |= 1ul << (rank * 8 + j);

         return new Bitboard(attacks);
      }

      static ulong GenerateBishopAttacks(int index, ulong occupied)
      {
         ulong attacks = 0ul;
         int targetRank = index / 8;
         int targetFile = index % 8;

         for (int dr = -1; dr <= 1; dr += 2)
         {
            for (int df = -1; df <= 1; df += 2)
            {
               for (int i = 1; i <= 7; i++)
               {
                  int r = targetRank + i * dr;
                  int f = targetFile + i * df;

                  if (r < 0 || r > 7 || f < 0 || f > 7) break;

                  ulong squareMask = 1ul << (r * 8 + f);
                  attacks |= squareMask;

                  if ((squareMask & occupied) != 0ul) break;
               }
            }
         }

         return attacks;
      }

      static ulong GenerateRookAttacks(int index, ulong occupied)
      {
         ulong attacks = 0ul;
         int targetRank = index / 8;
         int targetFile = index % 8;

         int[] deltas = { 1, -1, 8, -8 }; // Right, Left, Down, Up

         foreach (int delta in deltas)
         {
            int r = targetRank;
            int f = targetFile;

            while (true)
            {
               r += delta / 8;
               f += delta % 8;

               if (r < 0 || r > 7 || f < 0 || f > 7) break;

               ulong squareMask = 1ul << (r * 8 + f);
               attacks |= squareMask;

               if ((squareMask & occupied) != 0ul) break;
            }
         }

         return attacks;
      }
#endif

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong WhitePawnAttacks(ulong pawns)
      {
         return ((pawns >> 7) & ~FILE_MASKS[(int)File.A]) | ((pawns >> 9) & ~FILE_MASKS[(int)File.H]);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong BlackPawnAttacks(ulong pawns)
      {
         return ((pawns << 7) & ~FILE_MASKS[(int)File.H]) | ((pawns << 9) & ~FILE_MASKS[(int)File.A]);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetBishopAttacks(int square, ulong occupied)
      {
#if Pext
         return Bishops[square].Attacks[(int)Bmi2.X64.ParallelBitExtract(occupied, Bishops[square].Mask.Value)];
#elif Legacy
         return dSubset[square][(((occupied & dMask[square]) * FileB2B7) >> 58)] + aSubset[square][(((occupied & aMask[square]) * FileB2B7) >> 58)];
#endif
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetRookAttacks(int square, ulong occupied)
      {
#if Pext
         return Rooks[square].Attacks[(int)Bmi2.X64.ParallelBitExtract(occupied, Rooks[square].Mask.Value)];
#elif Legacy
         return hSubset[square][(occupied >> HorizontalShiftTable[square]) & 63] + vSubset[square][((((occupied >> (square & 7)) & FileA2A7) * DiagC2H7) >> 58)];
#endif
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetQueenAttacks(int square, ulong occupied)
      {
         return GetBishopAttacks(square, occupied) | GetRookAttacks(square, occupied);
      }
   }
}
