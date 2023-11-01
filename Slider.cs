namespace Skookum
{
   internal struct Slider
   {
      public List<ulong> Attacks = new();
      public Bitboard Mask;

      public Slider(Bitboard mask)
      {
         Mask = mask;
      }
   }
}
