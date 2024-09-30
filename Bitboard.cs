using System.Numerics;
using System.Runtime.CompilerServices;

namespace Puffin
{
   internal struct Bitboard
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
         Value &= Value - 1;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetLSB()
      {
         return BitOperations.TrailingZeroCount(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetMSB()
      {
         return 63 - BitOperations.LeadingZeroCount(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int CountBits()
      {
         return BitOperations.PopCount(Value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public ulong RightShift()
      {
         return (Value & ~Constants.FILE_MASKS[(int)File.H]) << 1;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static int LSB(ulong value)
      {
         return BitOperations.TrailingZeroCount(value);
      }

      public static Bitboard operator &(Bitboard a, ulong b) => new(a.Value & b);
      public static Bitboard operator &(Bitboard a, Bitboard b) => new(a.Value & b.Value);
      public static Bitboard operator |(Bitboard a, Bitboard b) => new(a.Value | b.Value);
      public static bool operator ==(Bitboard a, Bitboard b) => a.Value == b.Value;
      public static bool operator !=(Bitboard a, Bitboard b) => a.Value != b.Value;
      public static implicit operator bool(Bitboard a) => a.Value != 0;
   }
}
