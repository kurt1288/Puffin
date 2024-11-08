using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

#if Pext
namespace Puffin.Attacks
{
   internal static partial class Attacks
   {
      private static readonly ulong[][] RookAttacks = new ulong[64][];
      private static readonly Bitboard[] RookMasks = new Bitboard[64];

      private static readonly ulong[][] BishopAttacks = new ulong[64][];
      private static readonly Bitboard[] BishopMasks = new Bitboard[64];

      static Attacks()
      {
         InitAttacks();

         for (int i = 0; i < 64; i++)
         {
            BishopMasks[i] = GenerateBishopMasks(i);
            RookMasks[i] = GenerateRookMasks(i);

            List<ulong> tempAttacks = [];
            int count = RookMasks[i].CountBits();

            for (ulong j = 0; j < (1ul << count); j++)
            {
               ulong occupied = Bmi2.X64.ParallelBitDeposit(j, RookMasks[i].Value);
               tempAttacks.Add(GenerateRookAttacks(i, occupied));
            }

            // Convert List to array for faster access during gameplay
            RookAttacks[i] = [.. tempAttacks];

            tempAttacks.Clear();
            count = BishopMasks[i].CountBits();

            for (ulong j = 0; j < (1ul << count); j++)
            {
               ulong occupied = Bmi2.X64.ParallelBitDeposit(j, BishopMasks[i].Value);
               tempAttacks.Add(GenerateBishopAttacks(i, occupied));
            }

            BishopAttacks[i] = [.. tempAttacks];
         }
      }

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

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetBishopAttacks(int square, ulong occupied)
      {
         return BishopAttacks[square][(int)Bmi2.X64.ParallelBitExtract(occupied, BishopMasks[square].Value)];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetRookAttacks(int square, ulong occupied)
      {
         return RookAttacks[square][(int)Bmi2.X64.ParallelBitExtract(occupied, RookMasks[square].Value)];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetQueenAttacks(int square, ulong occupied)
      {
         return GetBishopAttacks(square, occupied) | GetRookAttacks(square, occupied);
      }
   }
}
#endif
