using System.Diagnostics;
using static Puffin.Constants;

namespace Puffin
{
   internal class TimeManager : ICloneable
   {
      private readonly Stopwatch StopWatch = new();
      private readonly int Overhead = 20;
      private bool Stopped = false;
      private double SoftTime = 0;
      private double MaxTime = 0;

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
            SoftTime = (0.75 * ((time / movesToGo) + (inc * 0.8))) - Overhead;
            MaxTime = Math.Min((time * 0.75) + inc, time) - Overhead;
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
         Stopped = false;
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

         long elapsedTime = StopWatch.ElapsedMilliseconds;

         if (SoftTime != 0 && newIteration && elapsedTime >= SoftTime)
         {
            Stop();
            return true;
         }

         if (MaxTime != 0 && elapsedTime >= MaxTime)
         {
            Stop();
            return true;
         }

         return false;
      }
   }
}
