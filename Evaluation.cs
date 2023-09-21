namespace Skookum
{
   internal class Evaluation
   {
      public static readonly int[] PieceValues = { 100, 250, 275, 350, 500, 10000 };
      readonly Random random = new();
      public int Score = 0;

      public Evaluation(Board board)
      {
         Evaluate(board);
      }

      private void Evaluate(Board board)
      {
         Bitboard all = new (board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value);
         int[] score = { 0, 0 };

         while (!all.IsEmpty())
         {
            int square = all.GetLSB();
            all.ClearLSB();
            Piece piece = board.Mailbox[square];

            score[(int)piece.Color] += PieceValues[(int)piece.Type];
         }

         Score = score[(int)board.SideToMove] - score[(int)board.SideToMove ^ 1] + random.Next(-5, 5);
      }
   }
}
