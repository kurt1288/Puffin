using static System.Formats.Asn1.AsnWriter;

namespace Skookum
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

      public static void Reset()
      {
         Array.Clear(Table);
         Used = 0;
      }

      public static TTEntry? GetEntry(ulong hash, int ply)
      {
         TTEntry entry = Table[hash % (ulong)Table.Length];

         if (entry.Hash != hash)
         {
            return null;
         }

         // Mate score adjustments
         if (entry.Score > Constants.MATE - Constants.MAX_PLY)
         {
            entry.Score -= ply;
         }
         else if (entry.Score < -(Constants.MATE - Constants.MAX_PLY))
         {
            entry.Score += ply;
         }

         return entry;
      }

      public static void SaveEntry(ulong hash, byte depth, int ply, ushort move, int score, HashFlag flag)
      {
         if (Table[hash % (ulong)Table.Length].Hash == 0)
         {
            Used++;
         }

         // Mate score adjustments
         if (score > Constants.MATE - Constants.MAX_PLY)
         {
            score += ply;
         }
         else if (score < -(Constants.MATE - Constants.MAX_PLY))
         {
            score -= ply;
         }

         Table[hash % (ulong)Table.Length] = new TTEntry(hash, depth, move, flag, score);
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
