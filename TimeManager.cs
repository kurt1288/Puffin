using System.Diagnostics;

namespace Skookum
{
   internal class TimeManager
   {
      public int MaxDepth = Constants.MAX_PLY;

      bool infititeTime = true;
      double SoftLimit = 0;
      double HardLimit = 0;
      readonly Stopwatch StopWatch = new();

      public void SetTimeLimit(int time, int inc, int movestogo, bool movetime)
      {
         time -= 15; // overhead, arbitrary value

         SoftLimit = inc + time / movestogo;
         HardLimit = movetime ? SoftLimit : 6 * SoftLimit;
         infititeTime = false;

         SoftLimit = Math.Min(SoftLimit, time / 8);
         HardLimit = Math.Min(HardLimit, time / 2);
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
         MaxDepth = Constants.MAX_PLY;
         infititeTime = true;
         SoftLimit = 0;
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
         else if (newIteration && StopWatch.ElapsedMilliseconds >= SoftLimit)
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
