﻿namespace Skookum
{
   internal class MoveList
   {
      public readonly List<Move> Moves = new();
      private readonly List<int> Scores = new();
      private readonly Random random = new();

      public MoveList(Board board, bool noisyOnly = false)
      {
         MoveGen gen = new(board);

         if (noisyOnly)
         {
            gen.GenerateNoisy(this);
         }
         else
         {
            gen.GenerateAll(this);
         }
      }

      public void Add(Move move, Piece? piece, Piece? captured)
      {
         Moves.Add(move);
         ScoreMove(move, piece, captured);
      }

      public bool IsEmpty()
      {
         return Moves.Count == 0;
      }

      private void ScoreMove(Move move, Piece? piece, Piece? captured)
      {
         if (move.HasType(MoveType.Capture))
         {
            Scores.Add(10000 + (int)(Evaluation.PieceValues[(int)captured!.Value.Type] - piece!.Value.Type));
         }
         else
         {
            Scores.Add(random.Next(1000));
         }
      }

      public Move NextMove(int index)
      {
         // Selection sort
         int best = index;

         for (int i = index; i < Moves.Count; i++)
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

         return Moves[index++];
      }
   }
}
