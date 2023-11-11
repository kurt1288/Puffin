using System.Runtime.CompilerServices;

namespace Skookum
{
   internal struct Hash
   {
      ulong _hash;

      public Hash()
      {
         _hash = 0;
      }

      public ulong Value { readonly get => _hash; set { _hash = value; } }
   }

   internal sealed class Zobrist
   {
      ulong RandomSeed = 14674941981828548931;

      readonly ulong[][] Pieces = new ulong[13][];
      readonly ulong[] EnPassant = new ulong[64];
      readonly ulong[] Castle = new ulong[4];
      readonly ulong SideToMove;
      Hash _hash = new();

      public ulong Value { get => _hash.Value; set { _hash.Value = value; } }

      public Zobrist()
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

      public void GenerateHash(Board board)
      {
         _hash = new();

         Bitboard pieces = new(board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value);

         while (!pieces.IsEmpty())
         {
            int square = pieces.GetLSB();
            pieces.ClearLSB();
            Piece piece = board.Mailbox[square];

            _hash.Value ^= Pieces[(int)piece.Type + (6 * (int)piece.Color)][square];
         }

         if (board.En_Passant != Square.Null)
         {
            _hash.Value ^= EnPassant[(int)board.En_Passant];
         }

         UpdateCastle(board.CastleSquares);

         if (board.SideToMove == Color.Black)
         {
            _hash.Value ^= SideToMove;
         }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void UpdateEnPassant(Square epSquare)
      {
         _hash.Value ^= EnPassant[(int)epSquare];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void UpdateSideToMove()
      {
         _hash.Value ^= SideToMove;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void UpdatePieces(Piece piece, int square)
      {
         _hash.Value ^= Pieces[(int)piece.Type + (6 * (int)piece.Color)][square];
      }

      public void UpdateCastle(ulong castlingSquares)
      {
         Bitboard castle = new(castlingSquares);
         while (!castle.IsEmpty())
         {
            int square = castle.GetLSB();
            castle.ClearLSB();

            _hash.Value ^= GetCastleKey(square);
         }
      }

      private ulong GetCastleKey(int square)
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

      public bool Verify(Board board)
      {
         Zobrist key = new();
         key.GenerateHash(board);
         return key.Value == _hash.Value;
      }

      private ulong Random()
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
