namespace Skookum
{
   internal class MoveList
   {
      public readonly Move[] Moves = new Move[218];
      private readonly int[] Scores = new int[218];
      public int MovesIndex { get; private set; } = 0;
      private int ScoresIndex = 0;

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
         Moves[MovesIndex++] = move;
         ScoreMove(move, piece, captured);
      }

      private void ScoreMove(Move move, Piece? piece, Piece? captured)
      {
         if (move.GetEncoded() == TranspositionTable.GetHashMove())
         {
            Scores[ScoresIndex++] = 1000000;
         }
         else if (move.HasType(MoveType.Capture))
         {
            Scores[ScoresIndex++] = 10000 + (Evaluation.PieceValues[(int)captured!.Value.Type] - Evaluation.PieceValues[(int)piece!.Value.Type]).Mg;
         }
         else
         {
            Scores[ScoresIndex++] = 0;
         }
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

         return Moves[index++];
      }
   }
}
