using System.Runtime.CompilerServices;

namespace Puffin
{
   internal readonly struct Move
   {
      readonly ushort Encoded;

      public Move(int from, int to, MoveFlag flag)
      {
         Encoded = (ushort)(((int)flag & 0xf) << 12 | (from & 0x3f) << 6 | to & 0x3f);
      }

      public Move(ushort move)
      {
         Encoded = move;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetTo()
      {
         return Encoded & 0x3f;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetFrom()
      {
         return Encoded >> 6 & 0x3f;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public MoveFlag GetFlag()
      {
         return (MoveFlag)(Encoded >> 12 & 0xf);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool HasType(MoveType type)
      {
         return (Encoded >> 12 & (int)type) != 0;
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

      public static bool operator ==(Move a, int b) => a.Encoded == b;
      public static bool operator !=(Move a, int b) => a.Encoded != b;
      public static bool operator ==(Move a, Move b) => a.Encoded == b.Encoded;
      public static bool operator !=(Move a, Move b) => a.Encoded != b.Encoded;

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
