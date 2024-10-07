namespace Puffin
{
   internal readonly struct Slider(Bitboard mask)
   {
      public List<ulong> Attacks { get; } = [];
      public Bitboard Mask { get; } = mask;
   }
}
