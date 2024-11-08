using System.Runtime.CompilerServices;

namespace Puffin
{
   internal readonly struct Move
   {
      private readonly ushort Encoded;

      public readonly int To => Encoded & 0x3F;
      public readonly int From => (Encoded >> 6) & 0x3F;
      public readonly MoveFlag Flag => (MoveFlag)(Encoded >> 12 & 0xF);

      public Move(int from, int to, MoveFlag flag)
      {
         Encoded = (ushort)(((int)flag & 0xf) << 12 | (from & 0x3f) << 6 | to & 0x3f);
      }

      public Move(ushort move)
      {
         Encoded = move;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public readonly bool HasType(MoveType type) => ((byte)Flag & (int)type) != 0;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public readonly bool IsCastle() => Flag == MoveFlag.KingCastle || Flag == MoveFlag.QueenCastle;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public readonly ushort GetEncoded() => Encoded;

      public static bool operator ==(Move a, int b) => a.Encoded == b;
      public static bool operator !=(Move a, int b) => a.Encoded != b;
      public static bool operator ==(Move a, Move b) => a.Encoded == b.Encoded;
      public static bool operator !=(Move a, Move b) => a.Encoded != b.Encoded;

      public override string ToString()
      {
         string str = $"{Enum.GetName(typeof(Square), From).ToLower()}{Enum.GetName(typeof(Square), To).ToLower()}";

         if (HasType(MoveType.Promotion))
         {
            if (Flag == MoveFlag.KnightPromotion || Flag == MoveFlag.KnightPromotionCapture)
            {
               str += "n";
            }
            else if (Flag == MoveFlag.BishopPromotion || Flag == MoveFlag.BishopPromotionCapture)
            {
               str += "b";
            }
            else if (Flag == MoveFlag.RookPromotion || Flag == MoveFlag.RookPromotionCapture)
            {
               str += "r";
            }
            else if (Flag == MoveFlag.QueenPromotion || Flag == MoveFlag.QueenPromotionCapture)
            {
               str += "q";
            }
         }

         return str;
      }
   }
}
