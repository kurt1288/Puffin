using System.Collections.Concurrent;

namespace Puffin
{
   internal class ThreadManager
   {
      readonly Thread[] _threads;
      readonly SearchInfo[] _infos;
      readonly BlockingCollection<SearchTask> _searchQueue;
      volatile bool _isRunning = true;
      readonly TranspositionTable _tTable;
      readonly ConcurrentBag<Search> _activeSearches;
      readonly int _threadCount;

      public ThreadManager(int threadCount, TranspositionTable tTable)
      {
         _threadCount = threadCount;
         _threads = new Thread[threadCount];
         _infos = new SearchInfo[threadCount];
         _searchQueue = [];
         _tTable = tTable;
         _activeSearches = [];

         for (int i = 0; i < threadCount; i++)
         {
            _infos[i] = new SearchInfo();
            int threadIndex = i;

            _threads[i] = new Thread(() => ThreadWork(threadIndex))
            {
               IsBackground = true,
               Name = $"Thread {i}"
            };

            _threads[i].Start();
         }
      }

      private void ThreadWork(int threadIndex)
      {
         while (_isRunning)
         {
            if (_searchQueue.TryTake(out SearchTask task, Timeout.Infinite))
            {
               Search search = new((Board)task.Board.Clone(), task.Time, _tTable, _infos[threadIndex], this);
               _activeSearches.Add(search);
               search.Run();
               _activeSearches.TryTake(out _);
               task.CompletionSource.SetResult(true);
            }
         }
      }

      public Task[] StartSearches(TimeManager time, Board board)
      {
         time.Start();
         var tasks = new Task[_threadCount];

         for (int i = 0; i < _threadCount; i++)
         {
            TaskCompletionSource<bool> completionSource = new();
            SearchTask searchTask = new((Board)board.Clone(), time, completionSource);
            _searchQueue.Add(searchTask);
            tasks[i] = completionSource.Task;
         }

         return tasks;
      }

      public void Shutdown()
      {
         _isRunning = false;
         _searchQueue.CompleteAdding();

         foreach (Thread thread in _threads)
         {
            thread.Join();
         }
      }

      public void Reset()
      {
         for (int i = 0; i < _threadCount; i++)
         {
            _infos[i].ResetAll();
         }
      }

      public int GetTotalNodes()
      {
         return _activeSearches.Sum(s => s.ThreadInfo.Nodes);
      }

      private class SearchTask(Board board, TimeManager time, TaskCompletionSource<bool> completionSource)
      {
         public Board Board { get; } = board;
         public TimeManager Time { get; } = time;
         public TaskCompletionSource<bool> CompletionSource { get; } = completionSource;
      }
   }
}
