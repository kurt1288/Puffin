namespace Skookum
{
   internal class Search
   {
      readonly Board Board;
      readonly TimeManager Time;
      readonly SearchInfo Info;

      public Search(Board board, TimeManager time)
      {
         Board = board;
         Time = time;
         Info = new();
      }

      static string FormatScore(int score)
      {
         if (score < -Constants.MATE + Constants.MAX_PLY)
         {
            return $"mate {(-Constants.MATE - score) / 2}";
         }
         else if (score > Constants.MATE - Constants.MAX_PLY)
         {
            return $"mate {(Constants.MATE - score + 1) / 2}";
         }
         else
         {
            return $"cp {score}";
         }
      }

      public void Run(int target)
      {
         Time.Start();

         int alpha = -Constants.INFINITY;
         int beta = Constants.INFINITY;

         try
         {
            // Iterative deepening
            for (int i = 1; i <= target && !Time.LimitReached(); i++)
            {
               int score = NegaScout(alpha, beta, i, 0);
               Console.WriteLine($"info depth {i} score {FormatScore(score)} nodes {Info.Nodes} nps {Math.Round((double)(Info.Nodes * 1000) / Time.GetElapsedMs(), 0)} time {Time.GetElapsedMs()} pv {Info.GetPv()}");
            }
         }
         catch (TimeoutException) { }
         
         Time.Stop();
         Console.WriteLine($"bestmove {Info.GetBestMove()}");
      }

      private int NegaScout(int alpha, int beta, int depth, int ply)
      {
         Info.InitPvLength(ply);

         if (depth <= 0)
         {
            return new Evaluation(Board).Score;
         }

         if (Time.LimitReached())
         {
            throw new TimeoutException();
         }

         int bestScore = -Constants.INFINITY;
         Move bestMove = new();
         int b = beta;
         int legalMoves = 0;
         bool inCheck = Board.IsAttacked(Board.GetSquareByPiece(PieceType.King, Board.SideToMove), (int)Board.SideToMove ^ 1);

         MoveList moves = new(Board);

         for (int i = 0; i < moves.Moves.Count; i++)
         {
            Move move = moves.NextMove(i);

            if (!Board.MakeMove(move))
            {
               Board.UndoMove(move);
               continue;
            }

            Info.Nodes += 1;
            legalMoves += 1;

            int score = -NegaScout(-b, -alpha, depth - 1, ply + 1);

            if (score > alpha && score < beta && legalMoves > 1)
            {
               score = -NegaScout(-beta, -alpha, depth - 1, ply + 1);
            }

            Board.UndoMove(move);

            if (score > bestScore)
            {
               bestScore = score;
               bestMove = move;
            }

            if (score > alpha)
            {
               alpha = score;
               Info.UpdatePV(bestMove, ply);
            }

            if (alpha >= beta)
            {
               break;
            }

            b = alpha + 1;
         }

         if (legalMoves == 0)
         {
            if (inCheck)
            {
               return -Constants.MATE + ply;
            }
            else
            {
               return 0;
            }
         }

         return bestScore;
      }
   }
}
