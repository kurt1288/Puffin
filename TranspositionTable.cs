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

   public struct TTEntry
   {
      internal readonly ulong Hash; // 8 bytes
      internal int Score; // 4
      internal readonly ushort Move; // 2
      internal readonly byte Depth; // 1
      internal readonly HashFlag Flag; // 1

      public TTEntry(ulong hash, byte depth, ushort move, HashFlag flag, int score)
      {
         Hash = hash;
         Depth = depth;
         Move = move;
         Flag = flag;
         Score = score;
      }
   }

   public sealed class TranspositionTable
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

      public TTEntry? GetEntry(ulong hash, int ply)
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

      public ushort GetHashMove(ulong hash)
      {
         TTEntry entry = Table[hash % (ulong)Table.Length];
         if (entry.Hash == hash)
         {
            return entry.Move;
         }

         return 0;
      }

      public ulong GetUsed()
      {
         return 1000 * Used / (ulong)Table.Length;
      }
   }
}
