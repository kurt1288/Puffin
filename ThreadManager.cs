using System.Collections.Concurrent;

namespace Puffin
{
   internal class ThreadManager
   {
      private readonly Thread[] Threads;
      private readonly SearchInfo[] Infos;
      private readonly BlockingCollection<SearchTask> SearchQueue;
      private volatile bool IsRunning = true;
      private TranspositionTable tTable;
      private readonly ConcurrentBag<Search> ActiveSearches;
      private readonly int ThreadCount;

      public ThreadManager(int threadCount, ref TranspositionTable tTable)
      {
         ThreadCount = threadCount;
         Threads = new Thread[threadCount];
         Infos = new SearchInfo[threadCount];
         SearchQueue = [];
         this.tTable = tTable;
         ActiveSearches = [];

         for (int i = 0; i < threadCount; i++)
         {
            Infos[i] = new SearchInfo();
            int threadIndex = i;

            Threads[i] = new Thread(() => ThreadWork(threadIndex))
            {
               IsBackground = true,
               Name = $"Thread {i}"
            };

            Threads[i].Start();
         }
      }

      private void ThreadWork(int threadIndex)
      {
         while (IsRunning)
         {
            if (SearchQueue.TryTake(out SearchTask task, Timeout.Infinite))
            {
               Search search = new((Board)task.Board.Clone(), task.Time, ref tTable, Infos[threadIndex], this);
               ActiveSearches.Add(search);
               search.Run();
               ActiveSearches.TryTake(out _);
               task.CompletionSource.SetResult(true);
            }
         }
      }

      public Task[] StartSearches(TimeManager time, Board board)
      {
         time.Start();
         var tasks = new Task[ThreadCount];

         for (int i = 0; i < ThreadCount; i++)
         {
            TaskCompletionSource<bool> completionSource = new();
            SearchTask searchTask = new((Board)board.Clone(), time, completionSource);
            SearchQueue.Add(searchTask);
            tasks[i] = completionSource.Task;
         }

         return tasks;
      }

      public void Shutdown()
      {
         IsRunning = false;
         SearchQueue.CompleteAdding();

         foreach (Thread thread in Threads)
         {
            thread.Join();
         }
      }

      public void Reset()
      {
         for (int i = 0; i < ThreadCount; i++)
         {
            Infos[i].ResetAll();
         }
      }

      public int GetTotalNodes()
      {
         return ActiveSearches.Sum(s => s.ThreadInfo.Nodes);
      }

      private class SearchTask(Board board, TimeManager time, TaskCompletionSource<bool> completionSource)
      {
         public Board Board { get; } = board;
         public TimeManager Time { get; } = time;
         public TaskCompletionSource<bool> CompletionSource { get; } = completionSource;
      }
   }
}
