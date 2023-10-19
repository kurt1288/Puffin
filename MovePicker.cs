namespace Skookum
{
   enum Stage
   {
      HashMove,
      GenNoisy,
      Noisy,
      Killers,
      GenQuiet,
      Quiet,
   }

   internal sealed class MovePicker
   {
      private readonly MoveList _moveList = new();
      private readonly Board Board;
      private Stage Stage;
      private int Index;
      private readonly bool NoisyOnly = false;
      private readonly SearchInfo SearchInfo;
      private readonly int Ply;
      private ushort HashMove = 0;

      public MovePicker(Board board, SearchInfo info, int ply, bool noisyOnly = false)
      {
         Stage = Stage.HashMove;
         Board = board;
         SearchInfo = info;
         Ply = ply;

         if (noisyOnly)
         {
            NoisyOnly = true;
            Stage = Stage.GenNoisy;
         }
      }

      public Move Next()
      {
         switch (Stage)
         {
            case Stage.HashMove:
               {
                  HashMove = TranspositionTable.GetHashMove();
                  Stage++;

                  if (HashMove != 0)
                  {
                     return new Move(HashMove);
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
                     NextMove(_moveList, Index);

                     if (_moveList[Index] != 0)
                     {
                        return _moveList[Index++];
                     }
                  }

                  if (NoisyOnly)
                  {
                     break;
                  }

                  Stage++;
                  Index = 0;
                  goto case Stage.Killers;
               }
            case Stage.Killers:
               {
                  while (Index <= 1)
                  {
                     if (Board.MoveIsValid(SearchInfo.KillerMoves[Ply][Index]))
                     {
                        return SearchInfo.KillerMoves[Ply][Index++];
                     }

                     Index++;
                  }

                  Stage++;
                  goto case Stage.GenQuiet;
               }
            case Stage.GenQuiet:
               {
                  _moveList.Clear();
                  MoveGen.GenerateQuiet(_moveList, Board);
                  ScoreMoves(_moveList);
                  Stage++;
                  Index = 0;
                  goto case Stage.Quiet;
               }
            case Stage.Quiet:
               {
                  if (Index < _moveList.Count)
                  {
                     NextMove(_moveList, Index);

                     if (_moveList[Index] != 0)
                     {
                        return _moveList[Index++];
                     }
                  }                  

                  break;
               }
         }

         return new Move();
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
         list.SwapScores(index, best);

         return list[index];
      }

      private void ScoreMoves(MoveList moves)
      {
         for (int i = moves.Count - 1; i >= 0; i--)
         {
            Move move = moves[i];

            if (move == HashMove || move == SearchInfo.KillerMoves[Ply][0] || move == SearchInfo.KillerMoves[Ply][1])
            {
               moves.RemoveAt(i);
            }
            else if (move.HasType(MoveType.Capture))
            {
               Piece captured = Board.Mailbox[move.GetTo()];

               if (move.GetFlag() == MoveFlag.EPCapture)
               {
                  captured = new Piece(PieceType.Pawn, Color.White); // color doesn't matter here
               }
               
               Piece moving = Board.Mailbox[move.GetFrom()];

               moves.SetScore(i, 150000 + ((50 * Evaluation.PieceValues[(int)captured.Type].Mg) - Evaluation.PieceValues[(int)moving.Type].Mg));
            }
            else
            {
               moves.SetScore(i, SearchInfo.GetHistory(Board.SideToMove, move));
            }
         }
      }
   }
}
