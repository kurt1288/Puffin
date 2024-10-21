namespace Puffin
{
   enum Stage
   {
      HashMove,
      GenNoisy,
      Noisy,
      Killers,
      Counter,
      GenQuiet,
      Quiet,
   }

   internal sealed class MovePicker(Board board, SearchInfo info, int ply, Move hashMove, bool noisyOnly)
   {
      private readonly MoveList MoveList = new();
      private readonly Board Board = board;
      private readonly Move HashMove = hashMove;
      private readonly Move CounterMove = ply > 0 ? info.GetCountermove(board.MoveStack[ply - 1].Move) : default;
      private int Index = 0;
      private readonly SearchInfo SearchInfo = info;

      public bool NoisyOnly { get; set; } = noisyOnly;
      public Stage Stage { get; private set; } = Stage.HashMove;

      public Move? Next()
      {
         switch (Stage)
         {
            case Stage.HashMove:
               {
                  Stage++;

                  if (Board.IsPseudoLegal(HashMove))
                  {
                     return HashMove;
                  }

                  goto case Stage.GenNoisy;
               }
            case Stage.GenNoisy:
               {
                  MoveGen.GenerateNoisy(MoveList, Board);
                  ScoreMoves(MoveList);
                  Stage++;
                  Index = 0;
                  goto case Stage.Noisy;
               }
            case Stage.Noisy:
               {
                  if (Index < MoveList.Count)
                  {
                     return NextMove(MoveList, Index++);
                  }

                  if (NoisyOnly)
                  {
                     return null;
                  }

                  Stage++;
                  Index = 0;
                  goto case Stage.Killers;
               }
            case Stage.Killers:
               {
                  while (Index < 2)
                  {
                     if (Board.IsPseudoLegal(SearchInfo.KillerMoves[ply][Index]))
                     {
                        return SearchInfo.KillerMoves[ply][Index++];
                     }

                     Index++;
                  }

                  Stage++;
                  goto case Stage.Counter;
               }
            case Stage.Counter:
               {
                  Stage++;

                  if (Board.IsPseudoLegal(CounterMove) && CounterMove != HashMove
                     && CounterMove != SearchInfo.KillerMoves[ply][0] && CounterMove != SearchInfo.KillerMoves[ply][1])
                  {
                     return CounterMove;
                  }

                  goto case Stage.GenQuiet;
               }
            case Stage.GenQuiet:
               {
                  if (NoisyOnly)
                  {
                     return null;
                  }

                  MoveList.Clear();
                  MoveGen.GenerateQuiet(MoveList, Board);
                  ScoreMoves(MoveList);
                  Stage++;
                  Index = 0;
                  goto case Stage.Quiet;
               }
            case Stage.Quiet:
               {
                  if (!NoisyOnly && Index < MoveList.Count)
                  {
                     return NextMove(MoveList, Index++);
                  }

                  return null;
               }
            default:
               {
                  return null;
               }
         }
      }

      public static Move NextMove(MoveList list, int index)
      {
         // Selection sort
         int best = index;

         for (int i = index; i < list.Count; i++)
         {
            if (list.GetScore(i) > list.GetScore(best))
            {
               best = i;
            }
         }

         list.SwapMoves(index, best);

         return list[index];
      }

      private void ScoreMoves(MoveList moves)
      {
         for (int i = moves.Count - 1; i >= 0; i--)
         {
            Move move = moves[i];

            if (move == HashMove || move == SearchInfo.KillerMoves[ply][0] || move == SearchInfo.KillerMoves[ply][1] || move == CounterMove)
            {
               moves.RemoveAt(i);
            }
            else if (move.HasType(MoveType.Capture))
            {
               Piece captured = Board.Squares[move.To];

               if (move.Flag == MoveFlag.EPCapture)
               {
                  captured = new Piece(PieceType.Pawn, Color.White); // color doesn't matter here
               }

               Piece moving = Board.Squares[move.From];

               moves.SetScore(i, 150000 + (50 * Evaluation.PieceValues[(int)captured.Type].Mg - Evaluation.PieceValues[(int)moving.Type].Mg));
            }
            else
            {
               moves.SetScore(i,
                  SearchInfo.GetHistory(Board.Squares[move.From].Color, move)
                  + SearchInfo.GetContHistory(Board.Squares[move.From], move, Board.MoveStack, ply, 1)
                  + SearchInfo.GetContHistory(Board.Squares[move.From], move, Board.MoveStack, ply, 2)
               );
            }
         }
      }
   }
}
