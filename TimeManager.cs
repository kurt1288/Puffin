using System.Diagnostics;
using static Puffin.Constants;

namespace Puffin
{
   internal class TimeManager : ICloneable
   {
      readonly Stopwatch StopWatch = new();
      readonly int Overhead = 15;
      bool Stopped = false;
      double SoftTime = 0;
      double MaxTime = 0;
      public int MaxDepth { get; set; } = MAX_PLY - 1;
      public int NodeLimit { get; private set; } = -1; // -1 is unlimited

      public TimeManager() { }

      public TimeManager(TimeManager other)
      {
         Stopped = other.Stopped;
         SoftTime = other.SoftTime;
         MaxTime = other.MaxTime;
         NodeLimit = other.NodeLimit;
         StopWatch = new();
      }

      public object Clone()
      {
         return new TimeManager(this);
      }

      public void SetLimits(int time, int inc, int movestogo, int movetime, int depth, int nodes)
      {
         MaxDepth = depth != 0 ? Math.Min(depth, MAX_PLY - 1) : MAX_PLY - 1;
         NodeLimit = nodes != 0 ? nodes : -1;
         int movesToGo = movestogo != 0 ? Math.Min(movestogo, 40) : 40;

         if (movetime != 0)
         {
            SoftTime = movetime;
            MaxTime = movetime;
         }
         else if (time != 0)
         {
            int optimal = (time / movesToGo) + inc - Overhead;
            SoftTime = Math.Min(optimal, (time - Overhead) * 0.2);
            MaxTime = (time * 0.75) - Overhead;
         }
      }

      public void SetNodeLimit(int nodeLimit)
      {
         NodeLimit = nodeLimit;
      }

      public void Start()
      {
         StopWatch.Start();
         Stopped = false;
      }

      public void Stop()
      {
         StopWatch.Stop();
         Stopped = true;
      }

      public void Restart()
      {
         StopWatch.Reset();
         Start();
      }

      public void Reset()
      {
         MaxDepth = MAX_PLY - 1;
         SoftTime = 0;
         MaxTime = 0;
         NodeLimit = -1;
         StopWatch.Reset();
      }

      public long GetElapsedMs()
      {
         return StopWatch.ElapsedMilliseconds;
      }

      public bool LimitReached(bool newIteration)
      {
         if (Stopped)
         {
            return true;
         }

         if (SoftTime != 0 && MaxTime != 0 && ((StopWatch.ElapsedMilliseconds >= SoftTime && newIteration) || StopWatch.ElapsedMilliseconds >= MaxTime))
         {
            Stop();
            return true;
         }

         return false;
      }
   }
}
