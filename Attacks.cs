using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Skookum
{
   internal static class Attacks
   {
      readonly static Magic[] BishopMagics = new Magic[64];
      readonly static Magic[] RookMagics = new Magic[64];

      public static readonly ulong[] KingAttacks = new ulong[64];
      public static readonly ulong[] KnightAttacks = new ulong[64];
      public static readonly ulong[][] PawnAttacks = new ulong[2][];

      static Attacks()
      {
         ulong notFileA = ~Constants.FILE_MASKS[(int)File.A];
         ulong notFileH = ~Constants.FILE_MASKS[(int)File.H];
         ulong notFilesAB = ~(Constants.FILE_MASKS[(int)File.A] | Constants.FILE_MASKS[(int)File.B]);
         ulong notFilesGH = ~(Constants.FILE_MASKS[(int)File.G] | Constants.FILE_MASKS[(int)File.H]);

         PawnAttacks[(int)Color.White] = new ulong[64];
         PawnAttacks[(int)Color.Black] = new ulong[64];

         for (int i = 0; i < 64; i++)
         {
            BishopMagics[i] = new(GenerateBishopMasks(i));
            RookMagics[i] = new(GenerateRookMasks(i));
         }

         for (int square = 0; square < 64; square++)
         {
            ulong bitboard = 1ul << square;
            
            PawnAttacks[(int)Color.White][square] = ((bitboard >> 7) & notFileA) | (bitboard >> 9 & notFileH);
            PawnAttacks[(int)Color.Black][square] = ((bitboard << 7) & notFileH) | (bitboard << 9 & notFileA);

            bitboard = 1ul << square;

            KingAttacks[square] = (bitboard >> 8) |
                                    ((bitboard >> 9) & notFileH) |
                                    ((bitboard >> 7) & notFileA) |
                                    ((bitboard >> 1) & notFileH) |
                                    (bitboard << 8) |
                                    ((bitboard << 9) & notFileA) |
                                    ((bitboard << 7) & notFileH) |
                                    ((bitboard << 1) & notFileA);

            bitboard = 1ul << square;

            KnightAttacks[square] = (bitboard >> 17) & notFileH |
                                     (bitboard >> 15) & notFileA |
                                     (bitboard >> 10) & notFilesGH |
                                     (bitboard >> 6) & notFilesAB |
                                     (bitboard << 17) & notFileA |
                                     (bitboard << 15) & notFileH |
                                     (bitboard << 10) & notFilesAB |
                                     (bitboard << 6) & notFilesGH;

            int count = RookMagics[square].Mask.CountBits();
            List<ulong> attacks = new();

            for (ulong i = 0; i < (1ul << count); i++)
            {
               ulong occupied = Bmi2.X64.ParallelBitDeposit(i, RookMagics[square].Mask.Value);
               attacks.Add(GenerateRookAttacks(square, occupied));
               RookMagics[square].Attacks.Add(attacks.Last());
            }

            count = BishopMagics[square].Mask.CountBits();
            attacks = new();

            for (ulong i = 0; i < (1ul << count); i++)
            {
               ulong occupied = Bmi2.X64.ParallelBitDeposit(i, BishopMagics[square].Mask.Value);
               attacks.Add(GenerateBishopAttacks(square, occupied));
               BishopMagics[square].Attacks.Add(attacks.Last());
            }
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
         ulong index = Bmi2.X64.ParallelBitExtract(occupied, BishopMagics[square].Mask.Value);
         return BishopMagics[square].Attacks[(int)index];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetRookAttacks(int square, ulong occupied)
      {
         ulong index = Bmi2.X64.ParallelBitExtract(occupied, RookMagics[square].Mask.Value);
         return RookMagics[square].Attacks[(int)index];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong GetQueenAttacks(int square, ulong occupied)
      {
         return GetBishopAttacks(square, occupied) | GetRookAttacks(square, occupied);
      }
   }
}
