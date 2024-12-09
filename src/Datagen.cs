using System.Diagnostics;
using static Puffin.Constants;

namespace Puffin
{
   internal class Datagen(CancellationToken cancellationToken)
   {
      const int MAX_DEPTH = 5;
      const int MAX_NODES = 8000;
      const double PERCENT_POSTIIONS_FROM_GAME = 0.2;
      const double PERCENT_POSTIIONS_FROM_DRAW_GAME = 0.1;
      static readonly Random random = new();
      static int gamesCompleted = 0;
      static int maxPositions = 0;
      static int totalPositions = 0;
      static string folderPath = string.Empty;
      readonly static Stopwatch sw = new();

      public void Run(int targetFens)
      {
         maxPositions = targetFens;
         string path = @$"./DataGen_Results";

         Thread[] threads = new Thread[Environment.ProcessorCount - 2];
         DirectoryInfo di = Directory.CreateDirectory(path);
         folderPath = di.FullName;
         sw.Start();

         for (int i = 0; i < threads.Length; i++)
         {
            Thread thread = new(Generate)
            {
               Name = $"Thread_{i}", // remove _ to output search to console
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
               // Adding to a HashSet ensures no duplicate positions
               positions.Add(s);
            }
         }

         string[] positionsArray = positions.ToArray();
         int total = positionsArray.Length;

         Console.WriteLine("Writing to datagen.epd...");
         
         using StreamWriter writer = new(Path.Combine(folderPath, "datagen.epd"), false);

         for (int i = 0; i < total; i++)
         {
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

      private void Generate()
      {
         string path = Path.Combine(folderPath, @$"./{Thread.CurrentThread.Name}.epd");
         using StreamWriter writer = new(path, true);
         Board board = new();
         TranspositionTable table = new();
         TimeManager timeManager = new();
         Position position = new();
         SearchInfo info = new();
         Search search = new(board, timeManager, ref table, info, new(1, ref table));

         while (totalPositions < maxPositions && !cancellationToken.IsCancellationRequested)
         {
            CancellationTokenSource cts = new(TimeSpan.FromMinutes(4)); // This should spend, at most, 4 minutes on a single game

            try
            {
               GeneratePositions(board, timeManager, table, info, search, cts, position);

               // To cut down on the number of draw positions, take half as many positions from drawn games
               // This keeps the overall draw positions to around 20-25% of the total
               double percentage = position.WDL == 0.5 ? PERCENT_POSTIIONS_FROM_DRAW_GAME : PERCENT_POSTIIONS_FROM_GAME;

               Interlocked.Increment(ref gamesCompleted);

               string[] fens = position.FENS.ToArray();
               random.Shuffle(fens);
               int length = (int)(fens.Length * percentage);
               Interlocked.Add(ref totalPositions, length);

               for (int i = 0; i < length; i++)
               {
                  writer.WriteLine($"{fens[i]} [{position.WDL:N1}]");
               }

               if (gamesCompleted % 10 == 0)
               {
                  Console.WriteLine($"Games: {gamesCompleted}. FENs: {totalPositions}. F/s: {1000 * (long)totalPositions / sw.ElapsedMilliseconds}. Total time: {sw.Elapsed:hh\\:mm\\:ss}. Estimated time remaining: {TimeSpan.FromMilliseconds((maxPositions - totalPositions) * (sw.ElapsedMilliseconds / totalPositions)):d\\.hh\\:mm\\:ss}");
               }
            }
            catch (TimeoutException)
            {
               Console.WriteLine("Position took to long and timed out. Skipping...");
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Exception during search: {ex}");
            }
            finally
            {
               cts.Dispose();
            }
         }

         writer.Close();
      }

      private static void GeneratePositions(Board board, TimeManager timeManager, TranspositionTable table, SearchInfo info, Search search, CancellationTokenSource cts, Position positions)
      {
         positions.Reset();
         table.Reset();
         timeManager.Reset();
         timeManager.MaxDepth = 8;
         info.ResetAll();
         MoveList moveList = new();

         while (true)
         {
            board.SetPosition(START_POS);

            if (!GetRandomPosition(board, moveList, cts))
            {
               continue;
            }

            // Do a quick search of the position, and if the score is too large (for either side), discard the position entirely
            timeManager.Restart();
            search.Run();
            timeManager.Stop();

            // Get a new position if this one is too lopsided
            if (Math.Abs(info.Score) > 500)
            {
               continue;
            }

            break;
         }

         info.ResetAll();
         timeManager.MaxDepth = MAX_PLY;
         timeManager.SetNodeLimit(MAX_NODES);

         while (true)
         {
            if (cts.IsCancellationRequested)
            {
               throw new TimeoutException($"Too many loops making moves: {Thread.CurrentThread.Name}");
            }

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
            // 5. Less than 2 non-pawn pieces
            if (!bestMove.HasType(MoveType.Capture)
               && !bestMove.HasType(MoveType.Promotion)
               && !board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1)
               && Math.Abs(info.Score) < MATE - MAX_PLY
               && board.NonPawnMaterial.CountBits() >= 2) {
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
            if ((board.History.Count > 40 && board.Halfmoves > 4 && (info.Score is >= -15 and <= 15)) || board.Halfmoves >= 100 || board.IsDrawn())
            {
               positions.WDL = 0.5;
               break;
            }
         }
      }

      private static bool GetRandomPosition(Board board, MoveList moveList, CancellationTokenSource cts)
      {
         int total = 8 + (random.Next(0, 2) % 2);
         bool foundMove = false;

         // Play 8 or 9 random moves
         for (int i = 0; i < total; i++)
         {
            foundMove = false;
            moveList.Reset();
            MoveGen.GenerateQuiet(moveList, board);
            MoveGen.GenerateNoisy(moveList, board);
            moveList.Shuffle();

            for (int j = 0; j < 218; j++) // 218 is the max length for the moves array
            {
               // Make sure the random move to play is legal
               if (cts.IsCancellationRequested)
               {
                  throw new TimeoutException($"Too many loops in GetRandomPosition: {Thread.CurrentThread.Name}");
               }

               Move move = moveList[j];

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
               Piece piece = board.Squares[square];

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

      public void Reset()
      {
         FENS.Clear();
         WDL = -1;
      }
   }
}
