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

      public double TimeLimit = 0;
      double HardLimit = 0;
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
         {
            TimeLimit = (time / (board.Fullmoves <= 20 ? (45 - board.Fullmoves) : 25)) + inc;
            HardLimit = Math.Max(inc + 0.001, time / 2);
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

      public void Reset()
      {
         wtime = 0;
         btime = 0;
         winc = 0;
         binc = 0;
         movestogo = 0;
         depthLimit = Constants.MAX_PLY;
         infititeTime = false;
         TimeLimit = 0;
         HardLimit = 0;
         StopWatch.Reset();
      }

      public long GetElapsedMs()
      {
         return StopWatch.ElapsedMilliseconds;
      }

      public bool LimitReached(bool newIteration)
      {
         if (infititeTime)
         {
            return false;
         }
         else if (newIteration && StopWatch.ElapsedMilliseconds >= TimeLimit)
         {
            return true;
         }
         else if (StopWatch.ElapsedMilliseconds >= HardLimit)
         {
            return true;
         }

         return false;
      }
   }
}
