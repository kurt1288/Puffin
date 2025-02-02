using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Puffin
{
   [DebuggerDisplay("{DebuggerDisplay,nq}")]
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

      // Only do Up/Down shifts for now
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public readonly Bitboard Shift(Direction direction)
      {
         return direction == Direction.Up ? new Bitboard(Value << (int)direction) : new Bitboard(Value >> -(int)direction);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static int LSB(ulong value)
      {
         return BitOperations.TrailingZeroCount(value);
      }

      public static Bitboard operator &(Bitboard a, ulong b) => new(a.Value & b);
      public static Bitboard operator &(Bitboard a, Bitboard b) => new(a.Value & b.Value);
      public static Bitboard operator |(Bitboard a, Bitboard b) => new(a.Value | b.Value);
      public static Bitboard operator ^(Bitboard a, Bitboard b) => new(a.Value ^ b.Value);
      public static Bitboard operator >>(Bitboard a, int b) => new(a.Value >> b);
      public static Bitboard operator <<(Bitboard a, int b) => new(a.Value << b);
      public static Bitboard operator ~(Bitboard a) => new(~a.Value);
      public static bool operator ==(Bitboard a, Bitboard b) => a.Value == b.Value;
      public static bool operator !=(Bitboard a, Bitboard b) => a.Value != b.Value;
      public static implicit operator bool(Bitboard a) => a.Value != 0;

      private readonly string DebuggerDisplay
      {
         get
         {
            string result = "\n";

            for (int rank = 0; rank < 8; rank++)
            {
               for (int file = 0; file < 8; file++)
               {
                  int square = rank * 8 + file;
                  bool isSet = ((Value >> square) & 1UL) == 1;
                  result += isSet ? "1 " : "0 ";
               }

               result += "\n";
            }

            return result;
         }
      }
   }
}
