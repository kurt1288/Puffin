﻿using System.Diagnostics;

namespace Puffin
{
   internal class TimeManager : ICloneable
   {
      public int MaxDepth = Constants.MAX_PLY;

      bool stopped = false;
      bool infititeTime = true;
      double SoftLimit = 0;
      double HardLimit = 0;
      readonly Stopwatch StopWatch = new();

      public TimeManager() { }

      public TimeManager(TimeManager other)
      {
         stopped = other.stopped;
         infititeTime = other.infititeTime;
         SoftLimit = other.SoftLimit;
         HardLimit = other.HardLimit;
         StopWatch = new();
      }

      public object Clone()
      {
         return new TimeManager(this);
      }

      public void SetTimeLimit(int time, int inc, int movestogo, bool movetime)
      {
         infititeTime = false;

         if (movetime)
         {
            SoftLimit = time;
            HardLimit = time;
         }
         else
         {
            time -= 15; // overhead, arbitrary value

            SoftLimit = inc + time / movestogo;
            HardLimit = movetime ? SoftLimit : 6 * SoftLimit;

            SoftLimit = Math.Min(SoftLimit, time / 8);
            HardLimit = Math.Min(HardLimit, time / 2);
         }
      }

      public void Start()
      {
         StopWatch.Start();
         stopped = false;
      }

      public void Stop()
      {
         StopWatch.Stop();
         stopped = true;
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
         else if (stopped)
         {
            return true;
         }
         else if (newIteration && StopWatch.ElapsedMilliseconds >= SoftLimit)
         {
            Stop();
            return true;
         }
         else if (StopWatch.ElapsedMilliseconds >= HardLimit)
         {
            Stop();
            return true;
         }

         return false;
      }
   }
}
