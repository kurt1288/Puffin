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

   internal class MoveList
   {
      public readonly Move[] Moves = new Move[218];
      private readonly int[] Scores = new int[218];
      public int MovesIndex { get; private set; } = 0;
      private int ScoresIndex = 0;
      private readonly Board Board;
      private Stage Stage;
      private int Index;
      private readonly bool NoisyOnly = false;
      private readonly Move[] KillerMoves;
      private ushort HashMove = 0;

      public MoveList(Board board, Move[] killerMoves, bool noisyOnly = false)
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

      public MoveList(Board board)
      {
         Board = board;
         KillerMoves = new Move[2];
      }

      public Move Next(int ply)
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
                  MoveGen.GenerateNoisy(this, Board);
                  Stage++;
                  Index = 0;
                  goto case Stage.Noisy;
               }
            case Stage.Noisy:
               {
                  NextMove(Index);

                  if (Moves[Index] != 0)
                  {
                     return Moves[Index++];
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
                     if (Board.MoveIsValid(KillerMoves[Index]))
                     {
                        return KillerMoves[Index++];
                     }

                     Index++;
                  }

                  Stage++;
                  goto case Stage.GenQuiet;
               }
            case Stage.GenQuiet:
               {
                  MoveGen.GenerateQuiet(this, Board);
                  Stage++;
                  Index = 0;
                  goto case Stage.Quiet;
               }
            case Stage.Quiet:
               {
                  NextMove(Index);

                  if (Moves[Index] != 0)
                  {
                     return Moves[Index++];
                  }

                  break;
               }
         }

         return new Move();
      }

      public void Add(Move move, Piece? piece, Piece? captured)
      {
         if (ScoreMove(move, piece, captured))
         {
            Moves[MovesIndex++] = move;
         }
      }

      private bool ScoreMove(Move move, Piece? piece, Piece? captured)
      {
         if (move == HashMove)
         {
            return false;
         }
         else if (move == KillerMoves[0] || move == KillerMoves[1])
         {
            return false;
         }
         else if (move.HasType(MoveType.Capture))
         {
            Scores[ScoresIndex++] = 10000 + (Evaluation.PieceValues[(int)captured!.Value.Type] - Evaluation.PieceValues[(int)piece!.Value.Type]).Mg;
         }
         else
         {
            Scores[ScoresIndex++] = 0;
         }

         return true;
      }

      public Move NextMove(int index)
      {
         // Selection sort
         int best = index;

         for (int i = index; i < MovesIndex; i++)
         {
            if (Scores[i] > Scores[best])
            {
               best = i;
            }
         }

         // Swap moves
         Move temp = Moves[index];
         Moves[index] = Moves[best];
         Moves[best] = temp;

         // Swap scores
         int t = Scores[index];
         Scores[index] = Scores[best];
         Scores[best] = t;

         return Moves[index];
      }
   }
}
