using System.Diagnostics;

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

   internal sealed class MovePicker
   {
      private readonly MoveList _moveList = new();
      private readonly Board Board;
      private int Index;
      public bool NoisyOnly = false;
      public readonly SearchInfo SearchInfo;
      public readonly int Ply;
      private readonly Move HashMove;
      private readonly Move CounterMove;
      public Stage Stage;
      public Move Move;

      public MovePicker(Board board, SearchInfo info, int ply, Move hashMove, bool noisyOnly, Move counterMove)
      {
         Stage = Stage.HashMove;
         Board = board;
         SearchInfo = info;
         Ply = ply;
         HashMove = hashMove;
         CounterMove = counterMove;
         NoisyOnly = noisyOnly;
      }

      public bool Next()
      {
         switch (Stage)
         {
            case Stage.HashMove:
               {
                  Stage++;

                  if (Board.IsPseudoLegal(HashMove))
                  {
                     Move = HashMove;
                     return true;
                  }

                  goto case Stage.GenNoisy;
               }
            case Stage.GenNoisy:
               {
                  MoveGen.GenerateNoisy(_moveList, Board);
                  ScoreMoves(_moveList);
                  Stage++;
                  Index = 0;
                  goto case Stage.Noisy;
               }
            case Stage.Noisy:
               {
                  if (Index < _moveList.Count)
                  {
                     Move = NextMove(_moveList, Index++);
                     return true;
                  }

                  if (NoisyOnly)
                  {
                     return false;
                  }

                  Stage++;
                  Index = 0;
                  goto case Stage.Killers;
               }
            case Stage.Killers:
               {
                  if (Index < 2)
                  {
                     if (Board.IsPseudoLegal(SearchInfo.KillerMoves[Ply][Index]))
                     {
                        Move = SearchInfo.KillerMoves[Ply][Index++];
                        return true;
                     }

                     Index++;
                     goto case Stage.Killers;
                  }

                  Stage++;
                  goto case Stage.Counter;
               }
            case Stage.Counter:
               {
                  Stage++;

                  if (Board.IsPseudoLegal(CounterMove) && CounterMove != HashMove
                     && CounterMove != SearchInfo.KillerMoves[Ply][0] && CounterMove != SearchInfo.KillerMoves[Ply][1])
                  {
                     Move = CounterMove;
                     return true;
                  }

                  goto case Stage.GenQuiet;
               }
            case Stage.GenQuiet:
               {
                  if (NoisyOnly)
                  {
                     return false;
                  }

                  _moveList.Clear();
                  MoveGen.GenerateQuiet(_moveList, Board);
                  ScoreMoves(_moveList);
                  Stage++;
                  Index = 0;
                  goto case Stage.Quiet;
               }
            case Stage.Quiet:
               {
                  if (NoisyOnly)
                  {
                     return false;
                  }

                  if (Index < _moveList.Count)
                  {
                     Move = NextMove(_moveList, Index++);
                     return true;
                  }

                  return false;
               }
            default:
               {
                  return false;
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

            if (move == HashMove || move == SearchInfo.KillerMoves[Ply][0] || move == SearchInfo.KillerMoves[Ply][1] || move == CounterMove)
            {
               moves.RemoveAt(i);
            }
            else if (move.HasType(MoveType.Capture))
            {
               Piece captured = Board.Mailbox[move.To];

               if (move.Flag == MoveFlag.EPCapture)
               {
                  captured = new Piece(PieceType.Pawn, Color.White); // color doesn't matter here
               }

               Piece moving = Board.Mailbox[move.From];

               moves.SetScore(i, 150000 + (50 * Evaluation.PieceValues[(int)captured.Type].Mg - Evaluation.PieceValues[(int)moving.Type].Mg));
            }
            else
            {
               moves.SetScore(i, SearchInfo.GetHistory(Board.Mailbox[move.From].Color, move));
            }
         }
      }
   }
}
