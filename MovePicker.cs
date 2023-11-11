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
      private int Index;
      private readonly bool NoisyOnly = false;
      private readonly Move[] KillerMoves;
      private ushort HashMove = 0;
      public Stage Stage;
      public Move Move;

      public MovePicker(Board board, Move[] killerMoves, bool noisyOnly = false)
      {
         Stage = Stage.HashMove;
         Board = board;
         KillerMoves = killerMoves;

         if (noisyOnly)
         {
            NoisyOnly = true;
            Stage = Stage.GenNoisy;
         }
      }

      public bool Next()
      {
         switch (Stage)
         {
            case Stage.HashMove:
               {
                  HashMove = TranspositionTable.GetHashMove(Board.Hash.Value);
                  Stage++;

                  if (HashMove != 0)
                  {
                     Move = new Move(HashMove);
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
                     if (Board.MoveIsValid(KillerMoves[Index]))
                     {
                        Move = KillerMoves[Index++];
                        return true;
                     }

                     Index++;
                     goto case Stage.Killers;
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
                     Move = NextMove(_moveList, Index++);
                     return true;
                  }

                  return false;
               }
         }

         return false;
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

            if (move == HashMove || move == KillerMoves[0] || move == KillerMoves[1])
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
               moves.SetScore(i, 0);
            }
         }
      }
   }
}
