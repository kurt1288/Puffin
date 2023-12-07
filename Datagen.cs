using System.Diagnostics;

namespace Puffin
{
   internal class Datagen
   {
      const int MAX_DEPTH = 5;
      const int MAX_NODES = 5000;
      static readonly Random random = new();
      static int gamesCompleted = 0;
      static int maxPositions = 0;
      static int totalPositions = 0;
      static string folderPath = string.Empty;
      static Stopwatch sw;

      public static void Run(int targetFens)
      {
         maxPositions = targetFens;
         string path = @$"./DataGen_Results_{DateTime.Now:yyyy-MM-dd, HH.mm.sss}";

         Thread[] threads = new Thread[Environment.ProcessorCount - 2];
         DirectoryInfo di = Directory.CreateDirectory(path);
         folderPath = di.FullName;

         sw = Stopwatch.StartNew();
         for (int i = 0; i < threads.Length; i++)
         {
            Thread thread = new(Generate)
            {
               Name = $"Thread_{i}",
               IsBackground = true,
            };
            threads[i] = thread;
            thread.Start();
         }

         foreach (Thread thread in threads)
         {
            thread.Join();
         }

         // Combine all the individual thread files into one
         foreach (Thread thread in threads)
         {
            using StreamWriter writer = new(Path.Combine(folderPath, "datagen.epd"), true);
            using StreamReader sr = System.IO.File.OpenText(Path.Combine(folderPath, $"{thread.Name}.epd"));
            string s;
            while ((s = sr.ReadLine()) != null)
            {
               writer.WriteLine(s);
            }
         }

         sw.Stop();
         Console.WriteLine($"Finished. Saved {totalPositions} fens to file");
      }

      private static void Generate()
      {
         string path = Path.Combine(folderPath, @$"./{Thread.CurrentThread.Name}.epd");
         StreamWriter writer = new(path, true);

         while (totalPositions < maxPositions)
         {
            Position position = GeneratePositions();
            Interlocked.Add(ref totalPositions, position.FENS.Count);
            Interlocked.Increment(ref gamesCompleted);

            foreach (string fen in position.FENS)
            {
               writer.WriteLine($"{fen} [{position.WDL:N1}]");
            }

            if (gamesCompleted % 10 == 0)
            {
               Console.WriteLine($"Games: {gamesCompleted}. FENs: {totalPositions}. F/s: {1000 * (long)totalPositions / sw.ElapsedMilliseconds}. Total time: {sw.Elapsed:hh\\:mm\\:ss}. Estimated time remaining: {TimeSpan.FromMilliseconds((maxPositions - totalPositions) * (sw.ElapsedMilliseconds / totalPositions)):hh\\:mm\\:ss}");
            }
         }

         writer.Close();
      }

      private static Position GeneratePositions()
      {
         CancellationTokenSource cts = new(TimeSpan.FromMinutes(4));
         Board board = new();
         TimeManager timeManager = new()
         {
            MaxDepth = 8,
         };
         //timeManager.SetTimeLimit(5000, 0, 1, true);

         SearchInfo info = new();
         Search search = new(board, timeManager, new(), info);

         while (true)
         {
            if (cts.IsCancellationRequested)
            {
               Console.WriteLine($"Too many loops generating a random position: {Thread.CurrentThread.Name}");
               Environment.Exit(100);
            }

            board.Reset();
            board.SetPosition(Constants.START_POS);
            GetRandomPosition(ref board, cts);

            // Do a quick search of the position, and if the score is too large (for either side), discard the position entirely
            timeManager.Restart();
            search.Run();
            timeManager.Stop();

            // Get a new position if this one is too lopsided
            if (Math.Abs(info.Score) > 1000)
            {
               continue;
            }

            break;
         }

         timeManager.MaxDepth = Constants.MAX_PLY;
         timeManager.SetNodeLimit(MAX_NODES);
         Position positions = new();

         while (true)
         {
            timeManager.Restart();
            search.Run();
            timeManager.Stop();
            Move bestMove = info.GetBestMove();

            board.MakeMove(bestMove);

            // Adjudicate large scores
            if (Math.Abs(info.Score) > 1000 || board.IsWon())
            {
               positions.WDL = info.Score > 0 ? (double)board.SideToMove : (int)board.SideToMove ^ 1;
               break;
            }

            // Don't save the position if the best move is a capture or a checking move
            if (!bestMove.HasType(MoveType.Capture) && !bestMove.HasType(MoveType.Promotion)
               && !board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1)) {
               positions.AddFEN(ToFEN(board));
            }

            // If the game has ended via checkmate or stalemate
            if (IsMate(board))
            {
               if (board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1))
               {
                  positions.WDL = (int)board.SideToMove ^ 1;
               }

               break;
            }
            
            if (board.Halfmoves >= 100 || search.IsRepeated() || board.IsDrawn())
            {
               positions.WDL = 0.5;
               break;
            }

            if (cts.IsCancellationRequested)
            {
               Console.WriteLine($"Too many loops making moves: {Thread.CurrentThread.Name}");
               Environment.Exit(100);
            }
         }

         return positions;
      }

      private static void GetRandomPosition(ref Board board, CancellationTokenSource cts)
      {
         int total = random.Next(0, 2);

         // Play 8 or 9 random moves
         for (int i = 0; i < 8 + (total % 2); i++)
         {
            MoveList moves = MoveGen.GenerateAll(board);

            // Make sure the random move to play is legal
            while (moves.Count > 0)
            {
               if (cts.IsCancellationRequested)
               {
                  Console.WriteLine($"Too many loops in GetRandomPosition: {Thread.CurrentThread.Name}");
                  Environment.Exit(100);
               }

               int index = random.Next(moves.Count);
               Move move = moves[index];

               if (!board.MakeMove(move))
               {
                  moves.RemoveAt(index);
                  board.UndoMove(move);
                  continue;
               }

               break;
            }

            // Generated a position with no moves (checkmate or stalemate)
            if (moves.Count == 0)
            {
               // reset and try a new random position
               board.SetPosition(Constants.START_POS);
               i = 0;
            }
         }
      }

      // This is not a complete FEN for the position. It excludes some values (like castling) that aren't needed for datagen
      private static string ToFEN(Board board)
      {
         string[] pieces = ["P", "N", "B", "R", "Q", "K", "p", "n", "b", "r", "q", "k"];
         string fen = string.Empty;

         for (int rank = 0; rank < 8; rank++)
         {
            int empty = 0;

            for (int file = 0; file < 8; file++)
            {
               int square = 8 * rank + file;
               Piece piece = board.Mailbox[square];

               if (piece.Type == PieceType.Null)
               {
                  empty++;
                  continue;
               }

               if (empty > 0)
               {
                  fen += empty.ToString();
                  empty = 0;
               }

               fen += pieces[(int)piece.Type + (6 * (int)piece.Color)];
            }

            if (empty > 0)
            {
               fen += empty.ToString();
            }

            if (rank < 7)
            {
               fen += "/";
            }
         }

         fen += $" {new string[] { "w", "b" }[(int)board.SideToMove]} - - 0 1";

         return fen;
      }
      
      private static bool IsMate(Board board)
      {
         MoveList moves = MoveGen.GenerateAll(board);

         for (int i = 0; i < moves.Count; i++)
         {
            if (board.MakeMove(moves[i]))
            {
               board.UndoMove(moves[i]);
               return false;
            }

            board.UndoMove(moves[i]);
         }

         return true;
      }
   }

   internal sealed class Position
   {
      public List<string> FENS = [];
      public double WDL = 0.5;

      public void AddFEN(string fen)
      {
         FENS.Add(fen);
      }
   }
}
