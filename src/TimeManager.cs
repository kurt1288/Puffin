using System.Diagnostics;
using static Puffin.Constants;

namespace Puffin
{
   internal class TimeManager : ICloneable
   {
      private const int Overhead = 20;
      private readonly Stopwatch StopWatch = new();
      private double SoftTime = -1;
      private double MaxTime = -1;
      private int NodeLimit = -1;

      public bool Stopped { get; private set; } = false;
      public int MaxDepth { get; set; } = MAX_PLY - 1;

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

      public void ConfigureDepth(int depth)
      {
         MaxDepth = depth > 0 ? Math.Min(depth, MAX_PLY - 1) : MAX_PLY - 1;
      }

      public void ConfigureNodes(int nodes)
      {
         NodeLimit = nodes > 0 ? nodes : -1;
      }

      public void ConfigureTime(int time, int inc, int movestogo)
      {
         if (time < 0)
         {
            time = 0;
         }

         movestogo = Math.Min(movestogo, 40);
         double baseTimePerMove = time / (double)movestogo;
         double increment = inc * 0.8;

         SoftTime = (int)Math.Max(0, (0.75 * (baseTimePerMove + increment)) - Overhead);
         MaxTime = (int)Math.Max(0, Math.Min((time * 0.75) + inc, time) - Overhead);
      }

      public void ConfigureMoveTime(int movetime)
      {
         if (movetime < 0)
         {
            movetime = 0;
         }

         SoftTime = movetime;
         MaxTime = movetime;
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
         SoftTime = -1;
         MaxTime = -1;
         NodeLimit = -1;
         Stopped = false;
         StopWatch.Reset();
      }

      public long GetElapsedMs()
      {
         return StopWatch.ElapsedMilliseconds;
      }

      public bool LimitReached(bool newIteration, int nodes)
      {
         if (Stopped)
         {
            return true;
         }

         if (NodeLimit >= 0 && nodes > NodeLimit)
         {
            Stop();
            return true;
         }

         long elapsedTime = StopWatch.ElapsedMilliseconds;

         if (SoftTime >= 0 && newIteration && elapsedTime >= SoftTime)
         {
            Stop();
            return true;
         }

         if (MaxTime >= 0 && elapsedTime >= MaxTime)
         {
            Stop();
            return true;
         }

         return false;
      }
   }
}
