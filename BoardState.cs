using System.Runtime.InteropServices;

namespace Puffin
{
   internal readonly struct BoardState(Square en_passant, ulong castling, Piece captured, int halfmoves, ulong hash, int phase)
   {
      public ulong CastleSquares { get; } = castling;
      public ulong Hash { get; } = hash;
      public int Halfmoves { get; } = halfmoves;
      public int Phase { get; } = phase;
      public Piece CapturedPiece { get; } = captured;
      public Square En_Passant { get; } = en_passant;
   }

   internal class History()
   {
      private readonly BoardState[] Stack = new BoardState[1000]; // arbitrary max length

      public int Count { get; private set; } = 0;
      public BoardState this[int index] => Stack[index];

      public void Reset()
      {
         Count = 0;
      }

      public void Add(BoardState state)
      {
         Stack[Count++] = state;
      }

      public BoardState Pop()
      {
         return Stack[--Count];
      }
   }
}
