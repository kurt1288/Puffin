using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Skookum
{
   internal class Bitboard
   {
      public ulong Value { get; private set; }

      public Bitboard()
      {
         Value = 0;
      }

      public Bitboard(ulong board)
      {
         Value = board;
      }

      public void Reset()
      {
         Value = 0;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void SetBit(int square)
      {
         Value |= 1UL << square;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void ResetBit(int square)
      {
         Value &= ~(1ul << square);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void ClearLSB()
      {
         Value = Bmi1.X64.ResetLowestSetBit(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetLSB()
      {
         return (int)Bmi1.X64.TrailingZeroCount(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetMSB()
      {
         return (int)Lzcnt.X64.LeadingZeroCount(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int CountBits()
      {
         return (int)Popcnt.X64.PopCount(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void And(ulong value)
      {
         Value &= value;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Or(ulong value)
      {
         Value |= value;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool IsEmpty()
      {
         return Value == 0UL;
      }
   }
}
