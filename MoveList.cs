using System.Runtime.CompilerServices;

namespace Puffin
{
   internal class MoveList
   {
      private readonly (Move Move, int Score)[] _moves = new (Move, int)[218];
      private int _count;
      private readonly Random rnd = new();

      public MoveList()
      {
         _count = 0;
      }

      public Move this[int index]
      {
         get => _moves[index].Move;
         set => _moves[index] = (value, _moves[index].Score);
      }

      public void Shuffle()
      {
         rnd.Shuffle(_moves);
      }

      public int Count => _count;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetScore(int index) => _moves[index].Score;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void SetScore(int index, int score) => _moves[index].Score = score;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add(Move move) => _moves[_count++] = (move, 0);

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
         var temp = _moves[index1];
         _moves[index1] = _moves[index2];
         _moves[index2] = temp;
      }
   }
}
