using System.Collections.Concurrent;
using System.Diagnostics;

namespace Puffin
{
   internal class Datagen
   {
      static readonly string[] pieces = [ "P", "N", "B", "R", "Q", "K", "p", "n", "b", "r", "q", "k" ];
      const int MAX_DEPTH = 5;
      const int MAX_NODES = 5000;
      static readonly Random random = new();

      public static void Run(int targetFens)
      {
         ConcurrentBag<Position> positions = [];
         Stopwatch sw = Stopwatch.StartNew();
         int gamesCompleted = 0;
         int fenCount = 0;

         Parallel.For(0, 10000000, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 },
            (i, state) =>
            {
               Position position = GenerateData();
               positions.Add(position);

               Interlocked.Add(ref fenCount, position.FENS.Count);
               Interlocked.Increment(ref gamesCompleted);

               if (fenCount >= targetFens)
               {
                  Console.WriteLine($"Games: {gamesCompleted}. FENs: {fenCount}. F/s: {1000 * (long)fenCount / sw.ElapsedMilliseconds}. Total time: {sw.Elapsed:hh\\:mm\\:ss}. Estimated time remaining: {TimeSpan.FromMilliseconds((targetFens - fenCount) * (sw.ElapsedMilliseconds / fenCount)):hh\\:mm\\:ss}");
                  state.Stop();
                  return;
               }

               if (state.IsStopped)
               {
                  return;
               }

               if (gamesCompleted % 10 == 0)
               {
                  Console.WriteLine($"Games: {gamesCompleted}. FENs: {fenCount}. F/s: {1000 * (long)fenCount / sw.ElapsedMilliseconds}. Total time: {sw.Elapsed:hh\\:mm\\:ss}. Estimated time remaining: {TimeSpan.FromMilliseconds((targetFens - fenCount) * (sw.ElapsedMilliseconds / fenCount)):hh\\:mm\\:ss}");
               }
            });

         sw.Stop();
         Console.WriteLine($"Writing {fenCount} fens to file...");

         string path = @$"./DataGen_Results_{DateTime.Now:yyyy-MM-dd, HH.mm.sss}.epd";

         using StreamWriter writer = new(path, true);
         foreach (Position position in positions)
         {
            foreach (string fen in position.FENS)
            {
               writer.WriteLine($"{fen} [{position.WDL:N1}]");
            }
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
         Position positions = new();
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
               positions.AddFEN(ToFEN(board));
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

         positions.WDL = result;

         return positions;
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
