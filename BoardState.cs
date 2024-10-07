namespace Puffin
{
   internal readonly struct BoardState(Color side, Square en_passant, ulong castling, Piece captured, int halfmoves, int fullmoves, ulong hash, int phase)
   {
      public Color SideToMove { get; } = side;
      public Square En_Passant { get; } = en_passant;
      public ulong CastleSquares { get; } = castling;
      public Piece CapturedPiece { get; } = captured;
      public int Halfmoves { get; } = halfmoves;
      public int Fullmoves { get; } = fullmoves;
      public ulong Hash { get; } = hash;
      public int Phase { get; } = phase;
   }

   internal class History : ICloneable
   {
      public int Count { get; private set; } = 0;

      public BoardState[] Stack { get; } = new BoardState[1000]; // arbitrary max length

      public History() { }

      public History(History other)
      {
         Array.Copy(other.Stack, Stack, Stack.Length);
         Count = other.Count;
      }

      public object Clone()
      {
         return new History(this);
      }

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
