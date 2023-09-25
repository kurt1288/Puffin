using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Skookum
{
   internal readonly struct Move
   {
      readonly ushort Encoded;

      public Move(int from, int to, MoveFlag flag)
      {
         Encoded = (ushort)((((int)flag & 0xf) << 12) | ((from & 0x3f) << 6) | (to & 0x3f));
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetTo()
      {
         return (int)Bmi1.X64.BitFieldExtract(Encoded, 0, 6);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetFrom()
      {
         return (int)Bmi1.X64.BitFieldExtract(Encoded, 6, 6);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public MoveFlag GetFlag()
      {
         return (MoveFlag)Bmi1.X64.BitFieldExtract(Encoded, 12, 4);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool HasType(MoveType type)
      {
         return ((Encoded >> 12) & (int)type) != 0;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool IsCastle()
      {
         return GetFlag() == MoveFlag.KingCastle || GetFlag() == MoveFlag.QueenCastle;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public ushort GetEncoded()
      {
         return Encoded;
      }

      public override string ToString()
      {
         string str = $"{Enum.GetName(typeof(Square), GetFrom()).ToLower()}{Enum.GetName(typeof(Square), GetTo()).ToLower()}";

         if (HasType(MoveType.Promotion))
         {
            if (GetFlag() == MoveFlag.KnightPromotion || GetFlag() == MoveFlag.KnightPromotionCapture)
            {
               str += "n";
            }
            else if (GetFlag() == MoveFlag.BishopPromotion || GetFlag() == MoveFlag.BishopPromotionCapture)
            {
               str += "b";
            }
            else if (GetFlag() == MoveFlag.RookPromotion || GetFlag() == MoveFlag.RookPromotionCapture)
            {
               str += "r";
            }
            else if (GetFlag() == MoveFlag.QueenPromotion || GetFlag() == MoveFlag.QueenPromotionCapture)
            {
               str += "q";
            }
         }

         return str;
      }
   }
}
