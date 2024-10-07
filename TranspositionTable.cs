using static Puffin.Constants;

namespace Puffin
{
   public enum HashFlag : byte
   {
      None,
      Exact,
      Alpha,
      Beta
   }

   public struct TTEntry(ulong hash, byte depth, ushort move, HashFlag flag, int score)
   {
      public readonly ulong Hash { get; } = hash; // 8 bytes
      public int Score { get; set; } = score; // 4 bytes
      public readonly ushort Move { get; } = move; // 2 bytes
      public readonly byte Depth { get; } = depth; // 1 byte
      public readonly HashFlag Flag { get; } = flag; // 1 byte
   }

   public struct TranspositionTable
   {
      TTEntry[] Table = new TTEntry[32 * 1024 * 1024 / 16]; // Default to 32MB table size
      ulong Used = 0;

      public TranspositionTable() { }

      // Size in MB
      public void Resize(int size)
      {
         // Note that the Array.Resize method doesn't actually resize. It creates a copy of the original with the new size,
         // and then updates the memory pointer.
         Array.Resize(ref Table, size * 1024 * 1024 / 16);
         Array.Clear(Table);
         Used = 0;
      }

      public void Reset()
      {
         Array.Clear(Table);
         Used = 0;
      }

      public readonly TTEntry? GetEntry(ulong hash, int ply)
      {
         TTEntry entry = Table[hash % (ulong)Table.Length];

         if (entry.Hash != hash)
         {
            return null;
         }

         // Mate score adjustments
         if (entry.Score > MATE - MAX_PLY)
         {
            entry.Score -= ply;
         }
         else if (entry.Score < -(MATE - MAX_PLY))
         {
            entry.Score += ply;
         }

         return entry;
      }

      public void SaveEntry(ulong hash, byte depth, int ply, ushort move, int score, HashFlag flag)
      {
         // Mate score adjustments
         if (score > MATE - MAX_PLY)
         {
            score += ply;
         }
         else if (score < -(MATE - MAX_PLY))
         {
            score -= ply;
         }

         ref TTEntry entry = ref Table[hash % (ulong)Table.Length];
         if (entry.Hash == 0)
         {
            Used++;
         }

         entry = new TTEntry(hash, depth, move, flag, score);
      }

      public readonly ulong GetUsed()
      {
         return 1000 * Used / (ulong)Table.Length;
      }
   }
}
