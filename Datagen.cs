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
         ConcurrentDictionary<string, double> positions = new();
         Stopwatch sw = Stopwatch.StartNew();
         int gamesCompleted = 0;

         var result = Parallel.For(0, 10000000, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 },
            (i, state) =>
            {
               Position position = GenerateData();

               foreach (string fen in position.FENS)
               {
                  positions.TryAdd(fen, position.WDL);
               }

               Interlocked.Increment(ref gamesCompleted);

               if (positions.Count >= targetFens)
               {
                  state.Stop();
               }

               if (state.IsStopped)
               {
                  return;
               }

               if (gamesCompleted % 10 == 0)
               {
                  Console.WriteLine($"Games: {gamesCompleted}. FENs: {positions.Count}. F/s: {1000 * (long)positions.Count / sw.ElapsedMilliseconds}. Total time: {sw.Elapsed}. Estimate time remaining: {TimeSpan.FromMilliseconds((targetFens - positions.Count) * (sw.ElapsedMilliseconds / positions.Count))}");
               }
            });

         sw.Stop();
         Console.WriteLine($"Writing {positions.Count} fens to file...");

         string path = @$"./DataGen_Results_{DateTime.Now:yyyy-MM-dd,HHmmss}.epd";

         using StreamWriter writer = new(path, true);
         foreach (var position in positions)
         {
            writer.WriteLine($"{position.Key} [{position.Value:N1}]");
         }

         Console.WriteLine($"Finished");
      }

      private static Position GenerateData()
      {
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
            board.SetPosition(Constants.START_POS);
            GetRandomPosition(ref board);

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
         List<string> positions = new();
         double result = 0.5;

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
               result = info.Score > 0 ? (double)board.SideToMove : (int)board.SideToMove ^ 1;
               break;
            }

            // Don't save the position if the best move is a capture or a checking move
            if (!bestMove.HasType(MoveType.Capture) && !board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1)) {
               positions.Add(ToFEN(board));
            }

            // If the game has ended via checkmate or stalemate
            if (IsMate(board))
            {
               if (board.IsAttacked(board.GetSquareByPiece(PieceType.King, board.SideToMove), (int)board.SideToMove ^ 1))
               {
                  result = (int)board.SideToMove ^ 1;
               }

               break;
            }
            
            if (board.Halfmoves >= 100 || search.IsRepeated() || board.IsDrawn())
            {
               break;
            }
         }

         return new Position(positions, result);
      }

      private static void GetRandomPosition(ref Board board)
      {
         int total = random.Next(0, 2);

         // Play 8 or 9 random moves
         for (int i = 0; i < 8 + (total % 2); i++)
         {
            MoveList moves = MoveGen.GenerateAll(board);

            // Make sure the random move to play is legal
            while (moves.Count > 0)
            {
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
         string[] pieces = { "P", "N", "B", "R", "Q", "K", "p", "n", "b", "r", "q", "k" };
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

   internal class Position
   {
      public List<string> FENS;
      public double WDL;

      public Position(List<string> positions, double result)
      {
         FENS = positions;
         WDL = result;
      }
   }
}
