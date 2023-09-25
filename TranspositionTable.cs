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
      static int Used = 0;

      static TranspositionTable()
      {
         Table = new TTEntry[(32 * 1024 * 1024) / 16]; // Default to 32MB table size
      }

      // Size in MB
      public static void Resize(int size)
      {
         // Note that the Array.Resize method doesn't actually resize. It creates a copy of the original with the new size,
         // and then updates the memory pointer.
         Array.Resize(ref Table, (size * 1024 * 1024) / 16);
         Array.Clear(Table);
         Used = 0;
      }

      public static TTEntry? GetEntry(ulong hash)
      {
         TTEntry entry = Table[hash % (ulong)Table.Length];

         if (entry.Hash != hash)
         {
            return null;
         }

         return entry;
      }

      public static void SaveEntry(ulong hash, byte depth, ushort move, int score, HashFlag flag)
      {
         if (Table[hash % (ulong)Table.Length].Hash == 0)
         {
            Used++;
         }

         Table[hash % (ulong)Table.Length] = new TTEntry
         {
            Hash = hash,
            Depth = depth,
            Move = move,
            Score = score,
            Flag = flag,
         };
      }

      public static ushort GetHashMove()
      {
         TTEntry entry = Table[Zobrist.Hash % (ulong)Table.Length];
         if (entry.Hash == Zobrist.Hash)
         {
            return entry.Move;
         }

         return 0;
      }

      public static int GetUsed()
      {
         return 1000 * Used / Table.Length;
      }
   }
}
