namespace Skookum
{
   internal static class Evaluation
   {
      public static readonly Score[] PieceValues = { new Score(100, 100), new Score(250, 250), new Score(275, 275), new Score(350, 350), new Score(500, 500), new Score(10000, 10000) };
      readonly static Random random = new();

      public static int Evaluate(Board board)
      {
         Score white = Material(board, Color.White);
         Score black = Material(board, Color.Black);
         Score total = white - black;

         if (board.SideToMove == Color.Black)
         {
            total *= -1;
         }

         return total.Mg + random.Next(-5, 5);
      }

      private static Score Material(Board board, Color color)
      {
         Bitboard us = new(board.ColorBB[(int)color].Value);
         Score score = new();

         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Mailbox[square];

            score += PieceValues[(int)piece.Type];
         }

         return score;
      }
   }
}
