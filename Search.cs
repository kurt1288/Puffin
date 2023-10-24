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
         int score = 0;
         Move bestMove = new();
         bool stop = false;

         // Iterative deepening
         for (int i = 1; i <= target && !stop; i++)
         {
            int margin = 10;

            // Use aspiration windows at higher depths
            if (i >= 4)
            {
               alpha = Math.Max(score - margin, -Constants.INFINITY);
               beta = Math.Min(score + margin, Constants.INFINITY);
            }

            while ((stop = Time.LimitReached()) != true)
            {
               score = NegaScout(alpha, beta, i, 0);

               if (score <= alpha)
               {
                  alpha = Math.Max(score - margin, -Constants.INFINITY);
                  beta = (alpha + beta) / 2;
               }
               else if (score >= beta)
               {
                  beta = Math.Min(score + margin, Constants.INFINITY);
               }
               else
               {
                  break;
               }

               margin += margin / 2;
            }

            if (!stop)
            {
               bestMove = Info.GetBestMove();

               Console.WriteLine($@"info depth {i} score {FormatScore(score)} nodes {Info.Nodes} nps {Math.Round((double)((long)Info.Nodes * 1000 / Math.Max(Time.GetElapsedMs(), 1)), 0)} hashfull {TranspositionTable.GetUsed()} time {Time.GetElapsedMs()} pv {Info.GetPv()}");
            }
         }
         
         Time.Stop();
         Console.WriteLine($"bestmove {bestMove}");
      }

      private int NegaScout(int alpha, int beta, int depth, int ply)
      {
         if (Info.Nodes % 2048 == 0 && Time.LimitReached())
         {
            return Constants.INFINITY * 10;
         }

         Info.InitPvLength(ply);

         if (ply >= Constants.MAX_PLY)
         {
            return 0;
         }

         if (ply > 0 && IsRepeated())
         {
            return 0;
         }

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

         MovePicker moves = new(Board, Info.KillerMoves[ply]);

         while (moves.Next())
         {
            if (!Board.MakeMove(moves.Move))
            {
               Board.UndoMove(moves.Move);
               continue;
            }

            Info.Nodes += 1;
            legalMoves += 1;

            if (depth > 2 && legalMoves > 3 && !inCheck && moves.Stage == Stage.Quiet)
            {
               int R = 1 + (depth / 4);

               if (isPVNode)
               {
                  R -= 1;
               }

               R = Math.Max(0, R);

               if (-NegaScout(-alpha - 1, -alpha, depth - 1 - R, ply + 1) <= alpha)
               {
                  Board.UndoMove(moves.Move);
                  continue;
               }
            }

            int score = -NegaScout(-b, -alpha, depth - 1, ply + 1);

            if (score > alpha && score < beta && legalMoves > 1)
            {
               score = -NegaScout(-beta, -alpha, depth - 1, ply + 1);
            }

            Board.UndoMove(moves.Move);

            if (Info.Nodes % 2048 == 0 && Time.LimitReached())
            {
               return Constants.INFINITY * 10;
            }

            if (score > bestScore)
            {
               bestScore = score;
               bestMove = moves.Move;
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

               if (!moves.Move.HasType(MoveType.Capture))
               {
                  if (moves.Move != Info.KillerMoves[ply][0])
                  {
                     Info.KillerMoves[ply][1] = Info.KillerMoves[ply][0];
                     Info.KillerMoves[ply][0] = moves.Move;
                  }
               }

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

         MovePicker moves = new(Board, Info.KillerMoves[ply], true);

         while (moves.Next())
         {
            if (!Board.MakeMove(moves.Move))
            {
               Board.UndoMove(moves.Move);
               continue;
            }

            Info.Nodes += 1;

            int score = -Quiescence(-beta, -alpha, ply + 1);

            Board.UndoMove(moves.Move);

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

      private bool IsRepeated()
      {
         if (Board.Halfmoves < 4 || Board.GameHistory.Count <= 1)
         {
            return false;
         }

         int last = Math.Max(Board.GameHistory.Count - Board.Halfmoves, 0);

         for (int i = Board.GameHistory.Count - 2; i >= last; i -= 2)
         {
            if (Board.GameHistory.Stack[i].Hash == Zobrist.Hash)
            {
               return true;
            }
         }

         return false;
      }
   }
}
