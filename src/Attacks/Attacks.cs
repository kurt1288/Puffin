using System.Runtime.CompilerServices;
using static Puffin.Constants;

namespace Puffin.Attacks
{
   internal static partial class Attacks
   {
      public static readonly ulong[] KingAttacks = new ulong[64];
      public static readonly ulong[] KnightAttacks = new ulong[64];
      public static readonly ulong[][] PawnAttacks = new ulong[2][];

      public static void InitAttacks()
      {
         ulong notFileA = ~FILE_MASKS[(int)File.A];
         ulong notFileH = ~FILE_MASKS[(int)File.H];
         ulong notFilesAB = ~(FILE_MASKS[(int)File.A] | FILE_MASKS[(int)File.B]);
         ulong notFilesGH = ~(FILE_MASKS[(int)File.G] | FILE_MASKS[(int)File.H]);

         PawnAttacks[(int)Color.White] = new ulong[64];
         PawnAttacks[(int)Color.Black] = new ulong[64];

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
         }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static ulong PawnAnyAttacks(ulong pawns, Color color)
      {
         return color == Color.White ?
            ((pawns >> 7) & ~FILE_MASKS[(int)File.A]) | ((pawns >> 9) & ~FILE_MASKS[(int)File.H])
            : ((pawns << 7) & ~FILE_MASKS[(int)File.H]) | ((pawns << 9) & ~FILE_MASKS[(int)File.A]);
      }

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
   }
}
