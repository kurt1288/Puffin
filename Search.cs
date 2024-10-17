using static Puffin.Constants;

namespace Puffin
{
   internal class Search(Board board, TimeManager time, ref TranspositionTable tTable, SearchInfo info, ThreadManager manager)
   {
      private readonly Board Board = board;
      private readonly TimeManager TimeManager = time;
      private TranspositionTable TTable = tTable;
      private readonly ThreadManager ThreadManager = manager;

      public SearchInfo ThreadInfo { get; } = info;

      internal static int ASP_Depth = 4;
      internal static int ASP_Margin = 10;
      internal static int NMP_Depth = 3;
      internal static int RFP_Depth = 10;
      internal static int RFP_Margin = 70;
      internal static int LMR_Depth = 2;
      internal static int LMR_MoveLimit = 3;
      internal static int FP_Depth = 7;
      internal static int FP_Margin = 80;
      internal static int LMP_Depth = 8;
      internal static int LMP_Margin = 5;
      internal static int IIR_Depth = 5;

      static string FormatScore(int score)
      {
         if (score < -MATE + MAX_PLY)
         {
            return $"mate {(-MATE - score) / 2}";
         }
         else if (score > MATE - MAX_PLY)
         {
            return $"mate {(MATE - score + 1) / 2}";
         }
         else
         {
            return $"cp {score}";
         }
      }

      public void Run()
      {
         ThreadInfo.ResetForSearch();

         int alpha = -INFINITY;
         int beta = INFINITY;
         int score = 0;
         bool stop;

         // Iterative deepening
         for (int i = 1; i <= TimeManager.MaxDepth && (stop = TimeManager.LimitReached(true)) != true; i++)
         {
            if (TimeManager.NodeLimit > 0 && ThreadInfo.Nodes >= TimeManager.NodeLimit)
            {
               TimeManager.Stop();
               break;
            }

            int margin = ASP_Margin;

            // Use aspiration windows at higher depths
            if (i >= ASP_Depth)
            {
               alpha = Math.Max(score - margin, -INFINITY);
               beta = Math.Min(score + margin, INFINITY);
            }

            try
            {
               while (true)
               {
                  score = ThreadInfo.Score = NegaScout(alpha, beta, i, 0, false);

                  if (score <= alpha)
                  {
                     alpha = Math.Max(score - margin, -INFINITY);
                     beta = (alpha + beta) / 2;
                  }
                  else if (score >= beta)
                  {
                     beta = Math.Min(score + margin, INFINITY);
                  }
                  else
                  {
                     break;
                  }

                  margin += margin / 2;
               }
            }
            catch (TimeoutException)
            {
               stop = true;
            }

            if (!stop)
            {
               if (Thread.CurrentThread.Name == "Thread 0")
               {
                  Console.WriteLine($@"info depth {i} score {FormatScore(score)} nodes {ThreadManager.GetTotalNodes()} nps {Math.Round((double)((long)ThreadManager.GetTotalNodes() * 1000 / Math.Max(TimeManager.GetElapsedMs(), 1)), 0)} hashfull {TTable.GetUsed()} time {TimeManager.GetElapsedMs()} pv {ThreadInfo.GetPv()}");
               }
            }
         }

         if (Thread.CurrentThread.Name == "Thread 0")
         {
            Console.WriteLine($"bestmove {ThreadInfo.GetBestMove()}");
         }
      }

      private int NegaScout(int alpha, int beta, int depth, int ply, bool doNull)
      {
         if (ThreadInfo.Nodes % 1024 == 0 && TimeManager.LimitReached(false))
         {
            throw new TimeoutException();
         }

         ThreadInfo.InitPvLength(ply);

         if (ply >= MAX_PLY)
         {
            return 0;
         }

         if (ply > 0 && IsDraw())
         {
            return 0;
         }

         bool isPVNode = beta != alpha + 1;
         bool isRoot = ply == 0;

         if (depth <= 0)
         {
            return Quiescence(alpha, beta, ply, isPVNode);
         }

         TTEntry? entry = TTable.GetEntry(Board.Hash, ply);
         ushort ttMove = entry.HasValue ? entry.Value.Move : (ushort)0;

         if (!isPVNode && !isRoot)
         {
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
               return (staticEval + beta) / 2;
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

         int bestScore = -INFINITY;
         Move bestMove = new();
         int b = beta;
         HashFlag flag = HashFlag.Alpha;
         int legalMoves = 0;
         Move[] quietMoves = new Move[100];
         int quietMovesCount = 0;

         // Internal iterative reduction
         if (depth >= IIR_Depth && ttMove == 0)
         {
            depth--;
         }

         MovePicker moves = new(Board, ThreadInfo, ply, new(ttMove), false);

         while (moves.Next())
         {
            bool isQuiet = !moves.Move.HasType(MoveType.Capture) && !moves.Move.HasType(MoveType.Promotion);
            // SEE pruning
            if (!SEE_GE(moves.Move, -75 * depth))
            {
               continue;
            }

            if (isQuiet)
            {
               // Late move pruning
               if (depth <= LMP_Depth && legalMoves > LMP_Margin + depth * depth)
               {
                  moves.NoisyOnly = true;
               }

               // Futility pruning
               if (depth <= FP_Depth && legalMoves > 0 && staticEval + FP_Margin * depth < alpha)
               {
                  moves.NoisyOnly = true;
               }
            }

            if (!Board.MakeMove(moves.Move))
            {
               Board.UndoMove(moves.Move);
               continue;
            }

            Board.MoveStack[ply] = (moves.Move, Board.Squares[moves.Move.To]);
            ThreadInfo.Nodes += 1;
            legalMoves += 1;

            if (isQuiet && quietMovesCount < 100)
            {
               quietMoves[quietMovesCount++] = moves.Move;
            }

            int E = inCheck ? 1 : 0;

            if (depth > LMR_Depth && legalMoves > LMR_MoveLimit && moves.Stage == Stage.Quiet)
            {
               int R = LMR_Reductions[depth][legalMoves];

               if (!isPVNode)
               {
                  R += 1;
               }

               if (Board.IsAttacked(Board.GetSquareByPiece(PieceType.King, Board.SideToMove), (int)Board.SideToMove ^ 1))
               {
                  R -= 1;
               }

               int reduction = Math.Clamp(depth - 1 + E - R, 1, depth - 1 + E + 1);

               // Moves that do not beat the current best value (alpha) are cut-off. Moves that do will be researched below
               if (-NegaScout(-alpha - 1, -alpha, reduction, ply + 1, true) <= alpha)
               {
                  Board.UndoMove(moves.Move);
                  continue;
               }
            }

            // First move of leftmost nodes get searched with a full window (because b = beta)
            // Subsequent moves get searched with a null window (b = alpha + 1)
            int score = -NegaScout(-b, -alpha, depth - 1 + E, ply + 1, true);

            // After the first legal move, if the intial search (above) fails high or low, research with the full window
            if (score > alpha && score < beta && legalMoves > 1)
            {
               score = -NegaScout(-beta, -alpha, depth - 1 + E, ply + 1, true);
            }

            Board.UndoMove(moves.Move);

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

                  int bonus = depth * depth;

                  ThreadInfo.UpdateHistory(Board.SideToMove, moves.Move, bonus);

                  if (!isRoot)
                  {
                     ThreadInfo.UpdateCountermove(Board.MoveStack[ply - 1].Move, moves.Move);
                     ThreadInfo.UpdateContHistory(Board.Squares[moves.Move.From], moves.Move, Board.MoveStack, ply, 1, bonus);
                     ThreadInfo.UpdateContHistory(Board.Squares[moves.Move.From], moves.Move, Board.MoveStack, ply, 2, bonus);
                  }

                  // Reduce history score for other quiet moves
                  for (int i = 0; i < quietMovesCount; i++)
                  {
                     if (quietMoves[i] == moves.Move)
                     {
                        continue;
                     }

                     ThreadInfo.UpdateHistory(Board.SideToMove, quietMoves[i], -bonus);

                     if (!isRoot)
                     {
                        ThreadInfo.UpdateContHistory(Board.Squares[quietMoves[i].From], quietMoves[i], Board.MoveStack, ply, 1, -bonus);
                        ThreadInfo.UpdateContHistory(Board.Squares[quietMoves[i].From], quietMoves[i], Board.MoveStack, ply, 2, -bonus);
                     }
                  }
               }

               break;
            }

            // Adjust null window
            b = alpha + 1;
         }

         if (legalMoves == 0)
         {
            if (inCheck)
            {
               return -MATE + ply;
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
         if (ThreadInfo.Nodes % 1024 == 0 && TimeManager.LimitReached(false))
         {
            throw new TimeoutException();
         }

         if (ply >= MAX_PLY)
         {
            return 0;
         }

         if (IsDraw())
         {
            return 0;
         }

         TTEntry? entry = TTable.GetEntry(Board.Hash, ply);
         ushort ttMove = entry.HasValue ? entry.Value.Move : (ushort)0;

         if (!isPVNode)
         {
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
         MovePicker moves = new(Board, ThreadInfo, ply, new(ttMove), true);

         while (moves.Next())
         {
            // Delta pruning
            if (((moves.Move.HasType(MoveType.Promotion) ? 1 : 0) * Evaluation.GetPieceValue(PieceType.Queen, Board)) + bestScore + Evaluation.GetPieceValue(Board.Squares[moves.Move.To].Type, Board) + 200 < alpha)
            {
               continue;
            }

            if (!SEE_GE(moves.Move, -50))
            {
               continue;
            }

            if (!Board.MakeMove(moves.Move))
            {
               Board.UndoMove(moves.Move);
               continue;
            }

            Board.MoveStack[ply] = (moves.Move, Board.Squares[moves.Move.To]);
            ThreadInfo.Nodes += 1;

            int score = -Quiescence(-beta, -alpha, ply + 1, isPVNode);

            Board.UndoMove(moves.Move);

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

      public bool IsDraw()
      {
         return IsRepeated() || Board.Halfmoves >= 100 || Board.IsDrawn();
      }

      private bool IsRepeated()
      {
         if (Board.Halfmoves < 4 || Board.History.Count <= 1)
         {
            return false;
         }

         int last = Math.Max(Board.History.Count - Board.Halfmoves, 0);

         for (int i = Board.History.Count - 4; i >= last; i -= 2)
         {
            if (Board.History.Stack[i].Hash == Board.Hash)
            {
               return true;
            }
         }

         return false;
      }

      /// <summary>
      /// Static Exchange Evaluation Greater or Equal. Is <paramref name="move"/> better than <paramref name="threshold"/>?
      /// </summary>
      public bool SEE_GE(Move move, int threshold)
      {
         if (move.IsCastle() || move.HasType(MoveType.Promotion) || move.Flag == MoveFlag.EPCapture)
         {
            return threshold <= 0;
         }

         int from = move.From;
         int to = move.To;

         int swap = SEE_VALUES[(int)Board.Squares[to].Type] - threshold;
         if (swap < 0)
         {
            return false;
         }

         swap = SEE_VALUES[(int)Board.Squares[from].Type] - swap;
         if (swap <= 0)
         {
            return true;
         }

         ulong occupied = ((Board.ColorBB[(int)Color.White] | Board.ColorBB[(int)Color.Black]).Value ^ SquareBB[from]) | SquareBB[to];

         ulong attackers = Board.AttackersTo(to, occupied);
         int stm = (int)Board.SideToMove;
         int res = 1;
         ulong stmAttackers, bb;

         while (true)
         {
            stm ^= 1;
            attackers &= occupied;

            stmAttackers = attackers & Board.ColorBB[stm].Value;
            if (stmAttackers == 0)
            {
               break;
            }

            res ^= 1;

            if ((bb = stmAttackers & Board.PieceBB[(int)PieceType.Pawn].Value) != 0)
            {
               occupied ^= SquareBB[Bitboard.LSB(bb)];

               if ((swap = SEE_VALUES[(int)PieceType.Pawn] - swap) < res)
               {
                  break;
               }

               attackers |= Attacks.GetBishopAttacks(to, occupied) & (Board.PieceBB[(int)PieceType.Bishop] | Board.PieceBB[(int)PieceType.Queen]).Value;
            }
            else if ((bb = stmAttackers & Board.PieceBB[(int)PieceType.Knight].Value) != 0)
            {
               occupied ^= SquareBB[Bitboard.LSB(bb)];

               if ((swap = SEE_VALUES[(int)PieceType.Knight] - swap) < res)
               {
                  break;
               }
            }
            else if ((bb = stmAttackers & Board.PieceBB[(int)PieceType.Bishop].Value) != 0)
            {
               occupied ^= SquareBB[Bitboard.LSB(bb)];

               if ((swap = SEE_VALUES[(int)PieceType.Bishop] - swap) < res)
               {
                  break;
               }

               attackers |= Attacks.GetBishopAttacks(to, occupied) & (Board.PieceBB[(int)PieceType.Bishop] | Board.PieceBB[(int)PieceType.Queen]).Value;
            }
            else if ((bb = stmAttackers & Board.PieceBB[(int)PieceType.Rook].Value) != 0)
            {
               occupied ^= SquareBB[Bitboard.LSB(bb)];

               if ((swap = SEE_VALUES[(int)PieceType.Rook] - swap) < res)
               {
                  break;
               }

               attackers |= Attacks.GetRookAttacks(to, occupied) & (Board.PieceBB[(int)PieceType.Rook] | Board.PieceBB[(int)PieceType.Queen]).Value;
            }
            else if ((bb = stmAttackers & Board.PieceBB[(int)PieceType.Queen].Value) != 0)
            {
               occupied ^= SquareBB[Bitboard.LSB(bb)];

               if ((swap = SEE_VALUES[(int)PieceType.Queen] - swap) < res)
               {
                  break;
               }

               attackers |= (Attacks.GetBishopAttacks(to, occupied) & (Board.PieceBB[(int)PieceType.Bishop] | Board.PieceBB[(int)PieceType.Queen]).Value)
                  | (Attacks.GetRookAttacks(to, occupied) & (Board.PieceBB[(int)PieceType.Rook] | Board.PieceBB[(int)PieceType.Queen]).Value);
            }
            else
            {
               return ((attackers & ~Board.ColorBB[stm].Value) != 0) ? (res ^ 1) != 0 : res != 0;
            }
         }

         return res != 0;
      }
   }
}
