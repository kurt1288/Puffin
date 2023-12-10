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

         int whitePositions = 0;
         int blackPositions = 0;
         int whiteWins = 0;
         int whiteDraws = 0;
         int whiteLosses = 0;
         int blackWins = 0;
         int blackDraws = 0;
         int blackLosses = 0;

         HashSet<string> positions = [];

         Console.WriteLine("Combining separate files...");
         foreach (Thread thread in threads)
         {
            using StreamReader sr = System.IO.File.OpenText(Path.Combine(folderPath, $"{thread.Name}.epd"));
            string s;
            while ((s = sr.ReadLine()) != null)
            {
               positions.Add(s);
            }
         }

         Console.WriteLine("Shuffling...");
         string[] positionsArray = positions.ToArray();
         // random.Shuffle(positionsArray); 
         int total = positionsArray.Length; // (int)(positionsArray.Length * 0.2);

         Console.WriteLine("Writing to datagen.epd...");
         for (int i = 0; i < total; i++)
         {
            using StreamWriter writer = new(Path.Combine(folderPath, "datagen.epd"), true);
            writer.WriteLine(positionsArray[i]);

            string[] parts = positionsArray[i].Split(" ");

            if (parts[1] == "w")
            {
               whitePositions++;

               if (parts.Last() == "[0.0]")
               {
                  whiteLosses++;
               }
               else if (parts.Last() == "[1.0]")
               {
                  whiteWins++;
               }
               else
               {
                  whiteDraws++;
               }
            }
            else
            {
               blackPositions++;

               if (parts.Last() == "[0.0]")
               {
                  blackLosses++;
               }
               else if (parts.Last() == "[1.0]")
               {
                  blackWins++;
               }
               else
               {
                  blackDraws++;
               }
            }
         }

         Console.WriteLine("Stats:");
         Console.WriteLine($"Total Positions: {total}");
         Console.WriteLine($"White Positions: {whitePositions} ({100 * whitePositions / total}%)");
         Console.WriteLine($"White 1.0: {whiteWins} ({100 * whiteWins / total}%)");
         Console.WriteLine($"White 0.5: {whiteDraws} ({100 * whiteDraws / total}%)");
         Console.WriteLine($"White 0.0: {whiteLosses} ({100 * whiteLosses / total}%)");
         Console.WriteLine($"Black Positions: {blackPositions} ({100 * blackPositions / total}%)");
         Console.WriteLine($"Black 1.0: {blackWins} ({100 * blackWins / total}%)");
         Console.WriteLine($"Black 0.5: {blackDraws} ({100 * blackDraws / total}%)");
         Console.WriteLine($"Black 0.0: {blackLosses} ({100 * blackLosses / total}%)");

         sw.Stop();
         Console.WriteLine($"Finished.");
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
         SearchInfo info = new();
         Search search = new(board, timeManager, new(), info);

         while (true)
         {
            if (cts.IsCancellationRequested)
            {
               Console.WriteLine($"Too many loops generating a random position: {Thread.CurrentThread.Name}");
               Environment.Exit(100);
            }

            while (true)
            {
               board.SetPosition(Constants.START_POS);
               if (GetRandomPosition(ref board, cts))
               {
                  break;
               }
            }

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

            // Adjudicate large scores or easy winnable positions
            if (Math.Abs(info.Score) > 1000 || board.IsWon())
            {
               positions.WDL = info.Score > 0 ? (int)board.SideToMove ^ 1 : (int)board.SideToMove;
               break;
            }

            // Conditions under which the positions should NOT be saved:
            // 1. Best move is a capture
            // 2. Best move is a promotion
            // 3. Side to move is in check
            // 4. Evaluation score value is a mate value
            // 5. There are 4 or less non-pawn pieces on the board
            if (!bestMove.HasType(MoveType.Capture) && !bestMove.HasType(MoveType.Promotion)
               && !board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1)
               && Math.Abs(info.Score) < Constants.MATE - Constants.MAX_PLY
               && (board.PieceBB[(int)PieceType.Knight] | board.PieceBB[(int)PieceType.Bishop] | board.PieceBB[(int)PieceType.Rook] | board.PieceBB[(int)PieceType.Queen]).CountBits() > 4) {
               positions.AddFEN(ToFEN(board));
            }

            if (bestMove == 0 || !board.MakeMove(bestMove))
            {
               Console.WriteLine("No valid bestmove");
               break;
            }

            // If the game has ended via checkmate or stalemate
            if (IsMate(board))
            {
               if (board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1))
               {
                  positions.WDL = (int)board.SideToMove ^ 1;
               }
               else
               {
                  positions.WDL = 0.5;
               }

               break;
            }
            
            // Adjudicate draws
            if ((board.Fullmoves > 40 && board.Halfmoves > 5 && (info.Score is >= -20 and <= 20)) || board.Halfmoves >= 100 || search.IsRepeated() || board.IsDrawn())
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

      private static bool GetRandomPosition(ref Board board, CancellationTokenSource cts)
      {
         int total = 8 + (random.Next(0, 2) % 2);
         bool foundMove = false;

         // Play 8 or 9 random moves
         for (int i = 0; i < total; i++)
         {
            foundMove = false;
            MoveList moves = MoveGen.GenerateAll(board);
            moves.Shuffle();

            // Make sure the random move to play is legal
            if (cts.IsCancellationRequested)
            {
               Console.WriteLine($"Too many loops in GetRandomPosition: {Thread.CurrentThread.Name}");
               Environment.Exit(100);
            }

            for (int j = 0; j < 218; j++) // 218 is the max length for the moves array
            {
               Move move = moves[j];

               // because the moves array is intiailized with default move values (0), after shuffling some of those null moves might be at the beginning
               if (move == 0)
               {
                  continue;
               }

               if (!board.MakeMove(move))
               {
                  board.UndoMove(move);
                  continue;
               }

               foundMove = true;
               break;
            }

            if (!foundMove)
            {
               break;
            }
         }

         return foundMove;
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
      public double WDL = -1;

      public void AddFEN(string fen)
      {
         FENS.Add(fen);
      }
   }
}
