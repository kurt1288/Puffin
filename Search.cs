namespace Puffin
{
   internal class Search
   {
      readonly Board Board;
      readonly TimeManager Time;
      readonly SearchInfo ThreadInfo;
      readonly TranspositionTable TTable;
      static SearchInfo[] infos;

      const int ASP_Depth = 4;
      const int ASP_Margin = 10;
      const int NMP_Depth = 3;
      const int RFP_Depth = 10;
      const int RFP_Margin = 70;
      const int LMR_Depth = 2;
      const int FP_Depth = 7;
      const int FP_Margin = 80;

      public Search(Board board, TimeManager time, TranspositionTable tTable, SearchInfo info)
      {
         Board = board;
         Time = time;
         TTable = tTable;
         ThreadInfo = info;
      }

      public static void StartSearch(TimeManager time, int threadCount, Board board, TranspositionTable tTable)
      {
         time.Start();

         Thread[] threads = new Thread[threadCount];
         infos = new SearchInfo[threadCount];

         // Initialize search info for each thread
         for (int i = 0; i < threadCount; i++)
         {
            infos[i] = new SearchInfo();
         }

         // Start each thread
         for (int i = 0; i < threadCount; i++)
         {
            (threads[i] = new Thread(new Search((Board)board.Clone(), time, tTable, infos[i]).Run)
            {
               IsBackground = true,
               Name = $"Thread {i}",
            }).Start();
         }
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

      static int GetNodesInfo()
      {
         int nodes = 0;
         for (int i = 0; i < infos.Length; i++)
         {
            nodes += infos[i].Nodes;
         }

         return nodes;
      }

      public void Run()
      {
         ThreadInfo.Reset();

         int alpha = -Constants.INFINITY;
         int beta = Constants.INFINITY;
         int score = 0;
         bool stop;

         // Iterative deepening
         for (int i = 1; i <= Time.MaxDepth && (stop = Time.LimitReached(true)) != true; i++)
         {
            if (Time.NodeLimit > 0 && ThreadInfo.Nodes >= Time.NodeLimit)
            {
               stop = true;
               Time.Stop();
               break;
            }

            int margin = ASP_Margin;

            // Use aspiration windows at higher depths
            if (i >= ASP_Depth)
            {
               alpha = Math.Max(score - margin, -Constants.INFINITY);
               beta = Math.Min(score + margin, Constants.INFINITY);
            }

            while (true)
            {
               if (ThreadInfo.Nodes % 1024 == 0 && Time.LimitReached(false))
               {
                  stop = true;
                  break;
               }

               try
               {
                  score = ThreadInfo.Score = NegaScout(alpha, beta, i, 0, false);
               }
               catch (TimeoutException)
               {
                  stop = true;
                  break;
               }

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
               if (Thread.CurrentThread.Name == "Thread 0")
               {
                  Console.WriteLine($@"info depth {i} score {FormatScore(score)} nodes {GetNodesInfo()} nps {Math.Round((double)((long)GetNodesInfo() * 1000 / Math.Max(Time.GetElapsedMs(), 1)), 0)} hashfull {TTable.GetUsed()} time {Time.GetElapsedMs()} pv {ThreadInfo.GetPv()}");
               }
            }
         }

         if (Thread.CurrentThread.Name == "Thread 0")
         {
            Console.WriteLine($"bestmove {ThreadInfo.GetBestMove()}");
         }
      }

      private int NegaScout(int alpha, int beta, int depth, int ply, bool doNull = true)
      {
         if (ThreadInfo.Nodes % 1024 == 0 && Time.LimitReached(false))
         {
            throw new TimeoutException();
         }

         ThreadInfo.InitPvLength(ply);

         if (ply >= Constants.MAX_PLY)
         {
            return 0;
         }

         if (ply > 0 && IsRepeated())
         {
            return 0;
         }

         bool isPVNode = beta != alpha + 1;

         if (depth <= 0)
         {
            return Quiescence(alpha, beta, ply, isPVNode);
         }

         if (!isPVNode && ply > 0)
         {
            TTEntry? entry = TTable.GetEntry(Board.Hash, ply);

            if (entry.HasValue && entry.Value.Depth >= depth
               && (entry.Value.Flag == HashFlag.Exact
               || entry.Value.Flag == HashFlag.Beta && entry.Value.Score >= beta
               || entry.Value.Flag == HashFlag.Alpha && entry.Value.Score <= alpha
               ))
            {
               return entry.Value.Score;
            }
         }

         bool inCheck = Board.IsAttacked(Board.GetSquareByPiece(PieceType.King, Board.SideToMove), (int)Board.SideToMove ^ 1);
         int staticEval = Evaluation.Evaluate(Board);

         if (!isPVNode && !inCheck)
         {
            // Reverse futility pruning
            if (depth <= RFP_Depth && staticEval - RFP_Margin * depth >= beta)
            {
               return staticEval;
            }

            // Null move pruning
            // The last condition prevents NMP if the STM only has a king and pawns left
            if (doNull && depth >= NMP_Depth && staticEval >= beta
               && (Board.ColorBB[(int)Board.SideToMove].Value
               & (Board.PieceBB[(int)PieceType.Knight].Value | Board.PieceBB[(int)PieceType.Bishop].Value
               | Board.PieceBB[(int)PieceType.Rook].Value | Board.PieceBB[(int)PieceType.Queen].Value)) != 0)
            {
               Board.MakeNullMove();
               int score = -NegaScout(-beta, -beta + 1, depth - 1 - (3 + depth / 6), ply + 1, false);
               Board.UnmakeNullMove();

               if (score >= beta)
               {
                  return score;
               }
            }
         }

         int bestScore = -Constants.INFINITY;
         Move bestMove = new();
         int b = beta;
         HashFlag flag = HashFlag.Alpha;
         int legalMoves = 0;
         Move[] quietMoves = new Move[100];
         int quietMovesCount = 0;

         MovePicker moves = new(Board, ThreadInfo, ply, TTable);

         while (moves.Next())
         {
            bool isQuiet = !moves.Move.HasType(MoveType.Capture) && !moves.Move.HasType(MoveType.Promotion);
            if (!isPVNode && !inCheck && isQuiet)
            {
               // Futility pruning
               if (depth <= FP_Depth && legalMoves > 0 && staticEval + FP_Margin * depth < alpha)
               {
                  continue;
               }
            }

            if (!Board.MakeMove(moves.Move))
            {
               Board.UndoMove(moves.Move);
               continue;
            }

            ThreadInfo.Nodes += 1;
            legalMoves += 1;
            int E = inCheck ? 1 : 0;

            if (depth > LMR_Depth && legalMoves > 3 && !inCheck && moves.Stage == Stage.Quiet)
            {
               int R = Constants.LMR_Reductions[depth][legalMoves];

               if (!isPVNode)
               {
                  R += 1;
               }

               if (Board.IsAttacked(Board.GetSquareByPiece(PieceType.King, Board.SideToMove), (int)Board.SideToMove ^ 1))
               {
                  R -= 1;
               }

               // Moves that do not beat the current best value (alpha) are cut-off. Moves that do will be researched below
               if (-NegaScout(-alpha - 1, -alpha, depth - 1 - Math.Max(0, R) + E, ply + 1) <= alpha)
               {
                  Board.UndoMove(moves.Move);
                  continue;
               }
            }

            // First move of leftmost nodes get searched with a full window (because b = beta)
            // Subsequent moves get searched with a null window (b = alpha + 1)
            int score = -NegaScout(-b, -alpha, depth - 1 + E, ply + 1);

            // After the first legal move, if the intial search (above) fails high or low, research with the full window
            if (score > alpha && score < beta && legalMoves > 1)
            {
               score = -NegaScout(-beta, -alpha, depth - 1 + E, ply + 1);
            }

            Board.UndoMove(moves.Move);

            if (ThreadInfo.Nodes % 1024 == 0 && Time.LimitReached(false))
            {
               throw new TimeoutException();
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
               ThreadInfo.UpdatePV(bestMove, ply);
            }

            if (alpha >= beta)
            {
               flag = HashFlag.Beta;

               if (isQuiet)
               {
                  if (moves.Move != ThreadInfo.KillerMoves[ply][0])
                  {
                     ThreadInfo.KillerMoves[ply][1] = ThreadInfo.KillerMoves[ply][0];
                     ThreadInfo.KillerMoves[ply][0] = moves.Move;
                  }

                  ThreadInfo.UpdateHistory(Board.SideToMove, moves.Move, depth * depth);

                  // Reduce history score for other quiet moves
                  for (int i = 0; i < quietMovesCount; i++)
                  {
                     if (quietMoves[i] == moves.Move)
                     {
                        continue;
                     }

                     ThreadInfo.UpdateHistory(Board.SideToMove, quietMoves[i], -depth * depth);
                  }
               }

               break;
            }

            if (isQuiet)
            {
               quietMoves[quietMovesCount++] = moves.Move;
            }

            // Adjust null window
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

         TTable.SaveEntry(Board.Hash, (byte)depth, ply, bestMove.GetEncoded(), bestScore, flag);

         return bestScore;
      }

      private int Quiescence(int alpha, int beta, int ply, bool isPVNode)
      {
         if (ThreadInfo.Nodes % 1024 == 0 && Time.LimitReached(false))
         {
            throw new TimeoutException();
         }

         if (ply >= Constants.MAX_PLY)
         {
            return 0;
         }

         if (!isPVNode)
         {
            TTEntry? entry = TTable.GetEntry(Board.Hash, ply);

            if (entry.HasValue
               && (entry.Value.Flag == HashFlag.Exact
               || entry.Value.Flag == HashFlag.Beta && entry.Value.Score >= beta
               || entry.Value.Flag == HashFlag.Alpha && entry.Value.Score <= alpha
               ))
            {
               return entry.Value.Score;
            }
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

         Move bestMove = new();
         HashFlag flag = HashFlag.Alpha;
         MovePicker moves = new(Board, ThreadInfo, ply, TTable, true);

         while (moves.Next())
         {
            // Delta pruning
            if (((moves.Move.HasType(MoveType.Promotion) ? 1 : 0) * Evaluation.GetPieceValue(PieceType.Queen, Board)) + bestScore + Evaluation.GetPieceValue(Board.Mailbox[moves.Move.To].Type, Board) + 200 < alpha)
            {
               continue;
            }

            if (!Board.MakeMove(moves.Move))
            {
               Board.UndoMove(moves.Move);
               continue;
            }

            ThreadInfo.Nodes += 1;

            int score = -Quiescence(-beta, -alpha, ply + 1, isPVNode);

            Board.UndoMove(moves.Move);

            if (ThreadInfo.Nodes % 1024 == 0 && Time.LimitReached(false))
            {
               throw new TimeoutException();
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
            }

            if (score >= beta)
            {
               flag = HashFlag.Beta;
               break;
            }
         }

         TTable.SaveEntry(Board.Hash, 0, ply, bestMove.GetEncoded(), bestScore, flag);

         return bestScore;
      }

      public bool IsRepeated()
      {
         if (Board.Halfmoves < 4 || Board.GameHistory.Count <= 1)
         {
            return false;
         }

         int last = Math.Max(Board.GameHistory.Count - Board.Halfmoves, 0);

         for (int i = Board.GameHistory.Count - 4; i >= last; i -= 2)
         {
            if (Board.GameHistory.Stack[i].Hash == Board.Hash)
            {
               return true;
            }
         }

         return false;
      }
   }
}
