using System.Diagnostics;

namespace Skookum
{
   internal class TimeManager
   {
      public int wtime = 0;
      public int btime = 0;
      public int winc = 0;
      public int binc = 0;
      public int movestogo = 0;
      public int depthLimit = Constants.MAX_PLY;
      public bool infititeTime = false;
      public bool ForcedStop = false;

      public int TimeLimit = 0;

      private readonly Stopwatch StopWatch = new();

      public void SetTimeLimit(Board board)
      {
         int time = wtime;
         int inc = winc;

         if (board.SideToMove == Color.Black)
         {
            time = btime;
            inc = binc;
         }

         // repeating time control
         if (movestogo != 0)
         {
            TimeLimit = (time + inc) / movestogo;
         }
         // increment time control only
         else
         {
            TimeLimit = (time / (board.Halfmoves <= 20 ? (45 - board.Halfmoves) : 25)) + inc;
         }
      }

      public void Start()
      {
         StopWatch.Start();
      }

      public void Stop()
      {
         StopWatch.Stop();
      }

      public long GetElapsedMs()
      {
         return StopWatch.ElapsedMilliseconds;
      }

      public bool LimitReached()
      {
         return (StopWatch.ElapsedMilliseconds >= TimeLimit && infititeTime == false) || ForcedStop;
      }
   }
}
