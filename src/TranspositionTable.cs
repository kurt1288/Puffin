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

   public readonly struct TTEntry(ulong hash, byte depth, ushort move, HashFlag flag, int score)
   {
      public readonly ulong Hash { get; } = hash; // 8 bytes
      public readonly int Score { get; } = score; // 4 bytes
      public readonly ushort Move { get; } = move; // 2 bytes
      public readonly byte Depth { get; } = depth; // 1 byte
      public readonly HashFlag Flag { get; } = flag; // 1 byte
   }

   public struct TranspositionTable()
   {
      TTEntry[] Table = new TTEntry[32 * 1024 * 1024 / 16]; // Default to 32MB table size

      // Size in MB
      public void Resize(int size)
      {
         // Note that the Array.Resize method doesn't actually resize. It creates a copy of the original with the new size,
         // and then updates the memory pointer.
         Array.Resize(ref Table, size * 1024 * 1024 / 16);
         Array.Clear(Table);
      }

      public readonly void Reset()
      {
         Array.Clear(Table);
      }

      public readonly bool GetEntry(ulong hash, int ply, out TTEntry entry)
      {
         ref TTEntry current = ref Table[hash % (ulong)Table.Length];

         if (current.Hash != hash)
         {
            entry = default;
            return false;
         }

         int adjustedScore = current.Score;

         // Mate score adjustments
         if (adjustedScore > MATE - MAX_PLY)
         {
            adjustedScore -= ply;
         }
         else if (adjustedScore < -(MATE - MAX_PLY))
         {
            adjustedScore += ply;
         }

         entry = new(current.Hash, current.Depth, current.Move, current.Flag, adjustedScore);

         return true;
      }

      public readonly void SaveEntry(ulong hash, byte depth, int ply, ushort move, int score, HashFlag flag)
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

         Table[hash % (ulong)Table.Length] = new TTEntry(hash, depth, move, flag, score);
      }

      /// <summary>
      /// Returns an estimate of the number of entries in the table
      /// </summary>
      /// <returns></returns>
      public readonly int GetUsed()
      {
         int used = 0;

         for (int i = 0; i < 1000; i++)
         {
            if (Table[i].Hash != 0)
            {
               used++;
            }
         }

         return used;
      }
   }
}
