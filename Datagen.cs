using System.Collections.Concurrent;
using System.Diagnostics;

namespace Puffin
{
   internal class Datagen
   {
      const int MAX_DEPTH = 5;
      const int MAX_NODES = 5000;
      static readonly Random random = new();

      public static void Run(int targetFens)
      {
         string path = @$"./DataGen_Results_{DateTime.Now:yyyy-MM-dd, HH.mm.sss}.epd";
         ConcurrentBag<Position> positions = [];
         Stopwatch sw = Stopwatch.StartNew();
         int gamesCompleted = 0;
         int fenCount = 0;

         while (fenCount < targetFens)
         {
            Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 },
            (i, state) =>
            {
               Position position = GenerateData();
               positions.Add(position);

               Interlocked.Add(ref fenCount, position.FENS.Count);
               Interlocked.Increment(ref gamesCompleted);

               if (gamesCompleted % 10 == 0)
               {
                  Console.WriteLine($"Games: {gamesCompleted}. FENs: {fenCount}. F/s: {1000 * (long)fenCount / sw.ElapsedMilliseconds}. Total time: {sw.Elapsed:hh\\:mm\\:ss}. Estimated time remaining: {TimeSpan.FromMilliseconds((targetFens - fenCount) * (sw.ElapsedMilliseconds / fenCount)):hh\\:mm\\:ss}");
               }
            });

            Console.WriteLine("Writing to file...");
            using (StreamWriter writer = new(path, true))
            {
               foreach (Position position in positions)
               {
                  foreach (string fen in position.FENS)
                  {
                     writer.WriteLine($"{fen} [{position.WDL:N1}]");
                  }
               }
            }

            positions.Clear();
            GC.Collect();
         }

         sw.Stop();
         Console.WriteLine($"Finished. Saved {fenCount} fens to file");
      }

      private static Position GenerateData()
      {
         CancellationTokenSource cts = new(TimeSpan.FromMinutes(2));
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
               Console.WriteLine("Too many loops generating a random position");
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
            if (Math.Abs(info.Score) > 400)
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
            if (Math.Abs(info.Score) > 1000)
            {
               positions.WDL = info.Score > 0 ? (double)board.SideToMove : (int)board.SideToMove ^ 1;
               break;
            }

            // Don't save the position if the best move is a capture or a checking move
            if (!bestMove.HasType(MoveType.Capture) && !board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1)) {
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
               break;
            }

            if (cts.IsCancellationRequested)
            {
               Console.WriteLine("Too many loops making moves");
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
                  Console.WriteLine("Too many loops in GetRandomPosition");
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
      public double WDL;

      public void AddFEN(string fen)
      {
         FENS.Add(fen);
      }
   }
}
