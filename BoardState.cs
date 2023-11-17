namespace Skookum
{
   internal readonly struct BoardState
   {
      public readonly Color SideToMove;
      public readonly Square En_Passant;
      public readonly ulong CastleSquares;
      public readonly Piece CapturedPiece;
      public readonly int Halfmoves;
      public readonly int Fullmoves;
      public readonly ulong Hash;
      public readonly int Phase;

      public BoardState(Color side, Square en_passant, ulong castling, Piece captured, int halfmoves, int fullmoves, ulong hash, int phase)
      {
         SideToMove = side;
         En_Passant = en_passant;
         CastleSquares = castling;
         CapturedPiece = captured;
         Halfmoves = halfmoves;
         Fullmoves = fullmoves;
         Hash = hash;
         Phase = phase;
      }
   }

   internal class History : ICloneable
   {
      public BoardState[] Stack = new BoardState[1000]; // arbitrary max length
      int _count = 0;

      public History() { }

      public History(History other)
      {
         Array.Copy(other.Stack, Stack, Stack.Length);
         _count = other._count;
      }

      public object Clone()
      {
         return new History(this);
      }

      public int Count
      {
         get
         {
            return _count;
         }
      }

      public void Add(BoardState state)
      {
         Stack[_count++] = state;
      }

      public BoardState Pop()
      {
         return Stack[--_count];
      }
   }
}
