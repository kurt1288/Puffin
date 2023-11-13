using System.Runtime.CompilerServices;

namespace Skookum
{
   internal static class Zobrist
   {
      static ulong RandomSeed = 14674941981828548931;

      static readonly ulong[][] Pieces = new ulong[13][];
      static readonly ulong[] EnPassant = new ulong[64];
      static readonly ulong[] Castle = new ulong[4];
      static readonly ulong SideToMove;

      static Zobrist()
      {
         // foreach piece on each square...
         for (int i = 0; i <= 12; i++)
         {
            Pieces[i] = new ulong[64];

            for (int j = 0; j <= 63; j++)
            {
               Pieces[i][j] = Random();
            }
         }

         for (int i = 0; i <= 63; i++)
         {
            EnPassant[i] = Random();
         }

         for (int i = 0; i < 4; i++)
         {
            Castle[i] = Random();
         }

         SideToMove = Random();
      }

      public static ulong GenerateHash(Board board)
      {
         ulong hash = 0;

         Bitboard pieces = new(board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value);

         while (!pieces.IsEmpty())
         {
            int square = pieces.GetLSB();
            pieces.ClearLSB();
            Piece piece = board.Mailbox[square];

            hash ^= Pieces[(int)piece.Type + (6 * (int)piece.Color)][square];
         }

         if (board.En_Passant != Square.Null)
         {
            hash ^= EnPassant[(int)board.En_Passant];
         }

         UpdateCastle(ref hash, board.CastleSquares);

         if (board.SideToMove == Color.Black)
         {
            hash ^= SideToMove;
         }

         return hash;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void UpdateEnPassant(ref ulong hash, Square epSquare)
      {
         hash ^= EnPassant[(int)epSquare];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void UpdateSideToMove(ref ulong hash)
      {
         hash ^= SideToMove;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static void UpdatePieces(ref ulong hash, Piece piece, int square)
      {
         hash ^= Pieces[(int)piece.Type + (6 * (int)piece.Color)][square];
      }

      public static void UpdateCastle(ref ulong hash, ulong castlingSquares)
      {
         Bitboard castle = new(castlingSquares);
         while (!castle.IsEmpty())
         {
            int square = castle.GetLSB();
            castle.ClearLSB();

            hash ^= GetCastleKey(square);
         }
      }

      private static ulong GetCastleKey(int square)
      {
         if (square == 0)
         {
            return Castle[0];
         }
         else if (square == 7)
         {
            return Castle[1];
         }
         else if (square == 56)
         {
            return Castle[2];
         }
         else if (square == 63)
         {
            return Castle[3];
         }

         return 0;
      }

      public static bool Verify(ulong hash, Board board)
      {
         ulong key = GenerateHash(board);
         return key == hash;
      }

      private static ulong Random()
      {
         ulong s = RandomSeed;

         s ^= s >> 12;
         s ^= s << 25;
         s ^= s >> 27;

         RandomSeed = s;

         return s * 2685821657736338717;
      }
   }
}
