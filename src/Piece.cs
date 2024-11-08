namespace Puffin
{
   internal readonly struct Piece
   {
      public PieceType Type { get; }
      public Color Color { get; }

      public Piece(PieceType type, Color color)
      {
         Type = type;
         Color = color;
      }

      public Piece(char piece)
      {
         Type = FromChar(piece);
         Color = char.IsUpper(piece) ? Color.White : Color.Black;
      }

      static PieceType FromChar(char piece)
      {
         return char.ToLower(piece) switch
         {
            'p' => PieceType.Pawn,
            'n' => PieceType.Knight,
            'b' => PieceType.Bishop,
            'r' => PieceType.Rook,
            'q' => PieceType.Queen,
            'k' => PieceType.King,
            _ => throw new Exception($"Invalid piece: {piece}"),
         };
      }

      public static Piece Null => new(PieceType.Null, Color.Null);
   }
}
