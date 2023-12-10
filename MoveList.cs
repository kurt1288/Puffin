namespace Puffin
{
   internal class MoveList
   {
      private readonly Move[] _moves = new Move[218];
      private readonly int[] _scores = new int[218];
      private int _count;

      public MoveList()
      {
         _count = 0;
      }

      public Move this[int index]
      {
         get => _moves[index];
         set => _moves[index] = value;
      }

      public void Shuffle()
      {
         Random rnd = new();
         rnd.Shuffle(_moves);
      }

      public int GetScore(int index)
      {
         return _scores[index];
      }

      public void SetScore(int index, int score)
      {
         _scores[index] = score;
      }

      public int Count => _count;

      public void Add(Move item)
      {
         _moves[_count] = item;
         _count++;
      }

      public void RemoveAt(int index)
      {
         Array.Copy(_moves, index + 1, _moves, index, _moves.Length - index - 1);
         _count--;
      }

      public void Clear()
      {
         _count = 0;
      }

      public void SwapMoves(int index1, int index2)
      {
         Move temp = _moves[index1];
         _moves[index1] = _moves[index2];
         _moves[index2] = temp;
      }

      public void SwapScores(int index1, int index2)
      {
         int temp = _scores[index1];
         _scores[index1] = _scores[index2];
         _scores[index2] = temp;
      }
   }
}
