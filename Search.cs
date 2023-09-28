using static Skookum.TranspositionTable;

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
         Info.Reset();
         Time.Start();

         int alpha = -Constants.INFINITY;
         int beta = Constants.INFINITY;
         Move bestMove = new Move();

         // Iterative deepening
         for (int i = 1; i <= target; i++)
         {
            int score = NegaScout(alpha, beta, i, 0);

            if (Time.LimitReached())
            {
               break;
            }

            bestMove = Info.GetBestMove();

            Console.WriteLine($@"info depth {i} score {FormatScore(score)} nodes {Info.Nodes} nps {Math.Round((double)((long)Info.Nodes * 1000 / Math.Max(Time.GetElapsedMs(), 1)), 0)} hashfull {TranspositionTable.GetUsed()} time {Time.GetElapsedMs()} pv {Info.GetPv()}");
         }
         
         Time.Stop();
         Console.WriteLine($"bestmove {bestMove}");
      }

      private int NegaScout(int alpha, int beta, int depth, int ply)
      {
         if (Info.Nodes % 1000 == 0 && Time.LimitReached())
         {
            return Constants.INFINITY * 10;
         }

         if (ply >= Constants.MAX_PLY)
         {
            return 0;
         }

         Info.InitPvLength(ply);

         if (depth <= 0)
         {
            return Quiescence(alpha, beta, ply);
         }

         bool isPVNode = beta != alpha + 1;

         if (!isPVNode)
         {
            TTEntry? entry = GetEntry(Zobrist.Hash);

            if (entry.HasValue && entry.Value.Depth >= depth
               && (entry.Value.Flag == HashFlag.Exact
               || (entry.Value.Flag == HashFlag.Beta && entry.Value.Score >= beta)
               || (entry.Value.Flag == HashFlag.Alpha && entry.Value.Score <= alpha)
               ))
            {
               return entry.Value.Score;
            }
         }

         int bestScore = -Constants.INFINITY;
         Move bestMove = new();
         int b = beta;
         HashFlag flag = HashFlag.Alpha;
         int legalMoves = 0;
         bool inCheck = Board.IsAttacked(Board.GetSquareByPiece(PieceType.King, Board.SideToMove), (int)Board.SideToMove ^ 1);

         MoveList moves = new(Board);
         Move move;

         while ((move = moves.Next()) != 0)
         {
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

            if (Time.LimitReached())
            {
               return Constants.INFINITY * 10;
            }

            if (score > bestScore)
            {
               bestScore = score;
               bestMove = move;
            }

            if (score > alpha)
            {
               alpha = score;
               flag = HashFlag.Exact;
               Info.UpdatePV(bestMove, ply);
            }

            if (alpha >= beta)
            {
               flag = HashFlag.Beta;
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

         SaveEntry(Zobrist.Hash, (byte)depth, bestMove.GetEncoded(), bestScore, flag);

         return bestScore;
      }

      private int Quiescence(int alpha, int beta, int ply)
      {
         if (Info.Nodes % 1000 == 0 && Time.LimitReached())
         {
            return Constants.INFINITY * 10;
         }

         if (ply >= Constants.MAX_PLY)
         {
            return 0;
         }

         int bestScore = Evaluation.Evaluate(Board);

         if (bestScore >= beta)
         {
            return bestScore;
         }
         else if (bestScore > alpha)
         {
            alpha = bestScore;
         }

         MoveList moves = new(Board, true);
         Move move;

         while ((move = moves.Next()) != 0)
         {
            if (!Board.MakeMove(move))
            {
               Board.UndoMove(move);
               continue;
            }

            Info.Nodes += 1;

            int score = -Quiescence(-beta, -alpha, ply + 1);

            Board.UndoMove(move);

            if (score > bestScore)
            {
               bestScore = score;
            }

            if (score > alpha)
            {
               alpha = score;
            }

            if (score >= beta)
            {
               break;
            }
         }

         return bestScore;
      }
   }
}
