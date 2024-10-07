using System.Runtime.CompilerServices;

namespace Puffin
{
   internal class MoveList()
   {
      private readonly (Move Move, int Score)[] Moves = new (Move, int)[218];

      public int Count { get; private set; } = 0;

      public Move this[int index]
      {
         get => Moves[index].Move;
         set => Moves[index] = (value, Moves[index].Score);
      }

      public void Shuffle()
      {
         Random rnd = new();
         rnd.Shuffle(Moves);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetScore(int index) => Moves[index].Score;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void SetScore(int index, int score) => Moves[index].Score = score;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add(Move move) => Moves[Count++] = (move, 0);

      public void RemoveAt(int index)
      {
         Array.Copy(Moves, index + 1, Moves, index, Moves.Length - index - 1);
         Count--;
      }

      public void Clear()
      {
         Count = 0;
      }

      public void SwapMoves(int index1, int index2)
      {
         var temp = Moves[index1];
         Moves[index1] = Moves[index2];
         Moves[index2] = temp;
      }
   }
}
