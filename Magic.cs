namespace Skookum
{
   internal struct Magic
   {
      public List<ulong> Attacks = new();
      public Bitboard Mask;

      public Magic(Bitboard mask)
      {
         Mask = mask;
      }
   }
}
