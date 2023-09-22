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

      public BoardState(Color side, Square en_passant, ulong castling, Piece captured, int halfmoves, int fullmoves, ulong hash)
      {
         SideToMove = side;
         En_Passant = en_passant;
         CastleSquares = castling;
         CapturedPiece = captured;
         Halfmoves = halfmoves;
         Fullmoves = fullmoves;
         Hash = hash;
      }
   }
}
