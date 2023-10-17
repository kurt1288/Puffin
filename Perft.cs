using System.Diagnostics;

namespace Skookum
{
   internal class Perft
   {
      readonly Board Board;

      public Perft(Board board)
      {
         Board = board;
      }

      public void Run(int depth)
      {
         if (depth == 0)
         {
            return;
         }

         Stopwatch stopWatch = new();
         stopWatch.Start();

         ulong totalNodes = 0;
         MoveList moves = MoveGen.GenerateAll(Board);

         for (int i = 0; i < moves.Count; i++)
         {
            Move move = moves[i];

            if (!Board.MakeMove(move))
            {
               Board.UndoMove(move);
               continue;
            }

            ulong childNodes = 0;
            childNodes += Divide(depth - 1);

            Console.WriteLine($"{move}: {childNodes}");

            totalNodes += childNodes;

            Board.UndoMove(move);
         }

         stopWatch.Stop();

         TimeSpan ts = stopWatch.Elapsed;

         Console.WriteLine($"{Environment.NewLine}Nodes searched: {totalNodes.ToString("N0")}");
         Console.WriteLine($"Elapsed time: {Math.Round(ts.TotalMilliseconds / 1000, 5)} seconds");
         Console.WriteLine($"NPS: {Math.Round(totalNodes / (ts.TotalMilliseconds / 1000)).ToString("N0")}");
      }

      ulong Divide(int depth)
      {
         ulong nodes = 0;

         if (depth == 0)
         {
            return 1;
         }

         MoveList moves = MoveGen.GenerateAll(Board);

         for (int i = 0; i < moves.Count; i++)
         {
            Move move = moves[i];

            if (!Board.MakeMove(move))
            {
               Board.UndoMove(move);
               continue;
            }

            nodes += Divide(depth - 1);

            Board.UndoMove(move);
         }

         return nodes;
      }
   }
}
