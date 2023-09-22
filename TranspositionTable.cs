namespace Skookum
{
   public static class TranspositionTable
   {
      public enum HashFlag : byte
      {
         None,
         Exact,
         Alpha,
         Beta
      }

      public struct TTEntry
      {
         internal ulong Hash;
         internal byte Depth;
         internal ushort Move;
         internal HashFlag Flag;
         internal int Score;
      }

      static TTEntry[] Table;

      static TranspositionTable()
      {
         Table = new TTEntry[(32 * 1024 * 1024) / 16]; // Default to 32MB table size
      }

      // Size in MB
      public static void Resize(int size)
      {
         Array.Resize(ref Table, (size * 1024 * 1024) / 16);
         Array.Clear(Table);
      }

      public static TTEntry? GetEntry(ulong hash)
      {
         TTEntry entry = Table[hash & (ulong)Table.Length];

         if (entry.Hash != hash)
         {
            return null;
         }

         return entry;
      }

      public static void SaveEntry(uint hash, byte depth, ushort move, int score, HashFlag flag)
      {
         Table[hash & (ulong)Table.Length] = new TTEntry
         {
            Hash = hash,
            Depth = depth,
            Move = move,
            Score = score,
            Flag = flag,
         };
      }
   }
}
