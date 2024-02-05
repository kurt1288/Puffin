using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Puffin
{
   internal sealed class Board : ICloneable
   {
      public Piece[] Mailbox { get; private set; } = new Piece[64];
      public Bitboard[] ColorBB { get; private set; } = new Bitboard[2];
      public Bitboard[] PieceBB { get; private set; } = new Bitboard[6];
      public Color SideToMove { get; private set; }
      public Square En_Passant { get; private set; } = Square.Null;
      public ulong CastleSquares { get; private set; } = 0;
      public int Halfmoves { get; private set; } = 0;
      public int Fullmoves { get; private set; } = 0;
      public int Phase { get; private set; } = 0;

      public History GameHistory = new();

      public int[] PhaseValues = { 0, 1, 1, 2, 4, 0 }; // Pawns do not contribute to the phase value
      public Score[] MaterialValue = { new Score(0, 0), new Score(0, 0) };
      public ulong Hash = 0;

      public Board()
      {
         for (int i = 0; i < Mailbox.Length; i++)
         {
            Mailbox[i].Reset();
         }

         for (int i = 0; i < ColorBB.Length; i++)
         {
            ColorBB[i].Reset();
         }

         for (int i = 0; i < PieceBB.Length; i++)
         {
            PieceBB[i].Reset();
         }

         GameHistory.Reset();
         SideToMove = Color.Null;
         En_Passant = Square.Null;
         CastleSquares = 0;
         Halfmoves = 0;
         Fullmoves = 0;
         Phase = 0;
         MaterialValue[(int)Color.White] = new Score(0, 0);
         MaterialValue[(int)Color.Black] = new Score(0, 0);
         Hash = 0;
      }

      public Board(Board other)
      {
         SideToMove = other.SideToMove;
         En_Passant = other.En_Passant;
         CastleSquares = other.CastleSquares;
         Halfmoves = other.Halfmoves;
         Fullmoves = other.Fullmoves;
         Phase = other.Phase;
         Hash = other.Hash;
         GameHistory = (History)GameHistory.Clone();
         Array.Copy(other.Mailbox, Mailbox, Mailbox.Length);
         Array.Copy(other.ColorBB, ColorBB, ColorBB.Length);
         Array.Copy(other.PieceBB, PieceBB, PieceBB.Length);
         Array.Copy(other.MaterialValue, MaterialValue, MaterialValue.Length);
      }

      public object Clone()
      {
         return new Board(this);
      }

      public void Reset()
      {
         for (int i = 0; i < Mailbox.Length; i++)
         {
            Mailbox[i].Reset();
         }

         for (int i = 0; i < ColorBB.Length; i++)
         {
            ColorBB[i].Reset();
         }

         for (int i = 0; i < PieceBB.Length; i++)
         {
            PieceBB[i].Reset();
         }

         GameHistory.Reset();
         SideToMove = Color.Null;
         En_Passant = Square.Null;
         CastleSquares = 0;
         Halfmoves = 0;
         Fullmoves = 0;
         Phase = 0;
         MaterialValue[(int)Color.White] = new Score(0, 0);
         MaterialValue[(int)Color.Black] = new Score(0, 0);
         Hash = 0;
      }

      public void SetPosition(string fen)
      {
         Reset();

         try
         {
            string[] fenParts = fen.Split();
            string pieces = fenParts[0];

            int square = 0;

            foreach (char piece in pieces)
            {
               if (char.IsNumber(piece))
               {
                  square += (int)char.GetNumericValue(piece);
               }
               else
               {
                  if (piece == '/')
                  {
                     continue;
                  }

                  SetPiece(new Piece(piece), square);
                  square++;
               }
            }

            SideToMove = fenParts[1] == "w" ? Color.White : Color.Black;

            // castling
            foreach (char letter in fenParts[2])
            {
               switch (letter)
               {
                  case 'K':
                     {
                        CastleSquares |= PieceBB[(int)PieceType.Rook].Value & ColorBB[(int)Color.White].Value & Constants.RANK_MASKS[(int)Rank.Rank_1] & Constants.FILE_MASKS[(int)File.H];
                        break;
                     }
                  case 'Q':
                     {
                        CastleSquares |= PieceBB[(int)PieceType.Rook].Value & ColorBB[(int)Color.White].Value & Constants.RANK_MASKS[(int)Rank.Rank_1] & Constants.FILE_MASKS[(int)File.A];
                        break;
                     }
                  case 'k':
                     {
                        CastleSquares |= PieceBB[(int)PieceType.Rook].Value & ColorBB[(int)Color.Black].Value & Constants.RANK_MASKS[(int)Rank.Rank_8] & Constants.FILE_MASKS[(int)File.H];
                        break;
                     }
                  case 'q':
                     {
                        CastleSquares |= PieceBB[(int)PieceType.Rook].Value & ColorBB[(int)Color.Black].Value & Constants.RANK_MASKS[(int)Rank.Rank_8] & Constants.FILE_MASKS[(int)File.A];
                        break;
                     }
                  default:
                     {
                        break;
                     }
               }
            }

            // set en passant
            if (fenParts[3] != "-")
            {
               var f = fenParts[3][0];
               int file = "abcdefgh".IndexOf(fenParts[3][0]);
               int rank = 8 - int.Parse(fenParts[3][1].ToString());
               En_Passant = (Square)(rank * 8 + file);
            }

            if (fenParts.Length > 4 && int.TryParse(fenParts[4], out int halfMoves))
            {
               Halfmoves = halfMoves;
            }

            if (fenParts.Length > 5 && int.TryParse(fenParts[5], out int fullMoves))
            {
               Fullmoves = fullMoves;
            }

            Hash = Zobrist.GenerateHash(this);
         }
         catch
         {
            Console.WriteLine($"Unable to parse fen");
         }
      }

      // Returns false if the move is illegal (leaves the king in check)
      public bool MakeMove(Move move)
      {
         MoveFlag flag = move.Flag;
         int from = move.From;
         int to = move.To;
         Piece piece = Mailbox[from];

         GameHistory.Add(
            new BoardState(
               SideToMove, En_Passant, CastleSquares, Mailbox[flag == MoveFlag.EPCapture ? piece.Color == Color.White ? to + 8 : to - 8 : to],
               Halfmoves, Fullmoves, Hash, Phase)
         );

         if (En_Passant != Square.Null)
         {
            Zobrist.UpdateEnPassant(ref Hash, En_Passant);
         }

         En_Passant = Square.Null;
         Halfmoves += 1;

         if (piece.Type == PieceType.Pawn || move.HasType(MoveType.Capture))
         {
            Halfmoves = 0;
         }

         switch (flag)
         {
            case MoveFlag.Quiet:
               {
                  RemovePiece(piece, from);
                  SetPiece(piece, to);
                  break;
               }
            case MoveFlag.DoublePawnPush:
               {
                  RemovePiece(piece, from);
                  SetPiece(piece, to);
                  En_Passant = (Square)((to + from) / 2);
                  Zobrist.UpdateEnPassant(ref Hash, En_Passant);
                  break;
               }
            case MoveFlag.Capture:
               {
                  RemovePiece(Mailbox[to], to);
                  RemovePiece(piece, from);
                  SetPiece(piece, to);
                  break;
               }
            case MoveFlag.EPCapture:
               {
                  RemovePiece(piece, from);
                  SetPiece(piece, to);
                  RemovePiece(new Piece(PieceType.Pawn, (Color)((int)piece.Color ^ 1)), piece.Color == Color.White ? to + 8 : to - 8);
                  break;
               }
            case MoveFlag.KingCastle:
               {
                  // Move king
                  RemovePiece(piece, from);
                  SetPiece(piece, to);

                  // Move rook
                  int rFrom = new Bitboard(CastleSquares & Constants.RANK_MASKS[SideToMove == Color.White ? (int)Rank.Rank_1 : (int)Rank.Rank_8]).GetMSB();
                  int rTo = SideToMove == Color.White ? (int)Square.F1 : (int)Square.F8;
                  SetPiece(Mailbox[rFrom], rTo);
                  RemovePiece(Mailbox[rFrom], rFrom);

                  // Check the path of the king to make sure it isn't moving from check or moving through check
                  Bitboard kingPath = new(Constants.BetweenBB[from][to] | Constants.SquareBB[to] | Constants.SquareBB[from]);
                  while (!kingPath.IsEmpty())
                  {
                     int square = kingPath.GetLSB();
                     kingPath.ClearLSB();
                     if (IsAttacked(square, (int)piece.Color ^ 1))
                     {
                        return false;
                     }
                  }

                  break;
               }
            case MoveFlag.QueenCastle:
               {
                  // Move king
                  RemovePiece(piece, from);
                  SetPiece(piece, to);

                  // Move rook
                  int rFrom = new Bitboard(CastleSquares & Constants.RANK_MASKS[SideToMove == Color.White ? (int)Rank.Rank_1 : (int)Rank.Rank_8]).GetLSB();
                  int rTo = SideToMove == Color.White ? (int)Square.D1 : (int)Square.D8;
                  SetPiece(Mailbox[rFrom], rTo);
                  RemovePiece(Mailbox[rFrom], rFrom);

                  // Check the path of the king to make sure it isn't moving from check or doesn't moving through check
                  Bitboard kingPath = new(Constants.BetweenBB[from][to] | Constants.SquareBB[to] | Constants.SquareBB[from]);
                  while (!kingPath.IsEmpty())
                  {
                     int square = kingPath.GetLSB();
                     kingPath.ClearLSB();
                     if (IsAttacked(square, (int)piece.Color ^ 1))
                     {
                        return false;
                     }
                  }

                  break;
               }
            case MoveFlag.KnightPromotion:
               {
                  RemovePiece(piece, from);
                  SetPiece(new Piece(PieceType.Knight, piece.Color), to);
                  break;
               }
            case MoveFlag.BishopPromotion:
               {
                  RemovePiece(piece, from);
                  SetPiece(new Piece(PieceType.Bishop, piece.Color), to);
                  break;
               }
            case MoveFlag.RookPromotion:
               {
                  RemovePiece(piece, from);
                  SetPiece(new Piece(PieceType.Rook, piece.Color), to);
                  break;
               }
            case MoveFlag.QueenPromotion:
               {
                  RemovePiece(piece, from);
                  SetPiece(new Piece(PieceType.Queen, piece.Color), to);
                  break;
               }
            case MoveFlag.KnightPromotionCapture:
               {
                  RemovePiece(piece, from);
                  RemovePiece(Mailbox[to], to);
                  SetPiece(new Piece(PieceType.Knight, piece.Color), to);
                  break;
               }
            case MoveFlag.BishopPromotionCapture:
               {
                  RemovePiece(piece, from);
                  RemovePiece(Mailbox[to], to);
                  SetPiece(new Piece(PieceType.Bishop, piece.Color), to);
                  break;
               }
            case MoveFlag.RookPromotionCapture:
               {
                  RemovePiece(piece, from);
                  RemovePiece(Mailbox[to], to);
                  SetPiece(new Piece(PieceType.Rook, piece.Color), to);
                  break;
               }
            case MoveFlag.QueenPromotionCapture:
               {
                  RemovePiece(piece, from);
                  RemovePiece(Mailbox[to], to);
                  SetPiece(new Piece(PieceType.Queen, piece.Color), to);
                  break;
               }
            default:
               {
                  throw new Exception($"Unable to make move: {move}. Unknown flag: {flag}");
               }
         }

         // update castling
         if (piece.Type == PieceType.King)
         {
            // If the king moves, remove the castle squares from the home rank
            Zobrist.UpdateCastle(ref Hash, CastleSquares & Constants.RANK_MASKS[SideToMove == Color.White ? (int)Rank.Rank_1 : (int)Rank.Rank_8]);
            CastleSquares &= ~Constants.RANK_MASKS[SideToMove == Color.White ? (int)Rank.Rank_1 : (int)Rank.Rank_8];
         }

         // if a piece is moving either to or from a rook square, either the rook is moving, being captured, or just not on that square. either way, castling rights
         // on that side are gone.
         if ((Constants.SquareBB[move.From] & CastleSquares) != 0)
         {
            Zobrist.UpdateCastle(ref Hash, CastleSquares & Constants.SquareBB[move.From]);
            CastleSquares &= ~Constants.SquareBB[move.From];
         }
         if ((Constants.SquareBB[move.To] & CastleSquares) != 0)
         {
            Zobrist.UpdateCastle(ref Hash, CastleSquares & Constants.SquareBB[move.To]);
            CastleSquares &= ~Constants.SquareBB[move.To];
         }

         SideToMove = (Color)((int)SideToMove ^ 1);
         Fullmoves += 1;

         Zobrist.UpdateSideToMove(ref Hash);

         Debug.Assert(Zobrist.Verify(Hash, this));
         Debug.Assert(Phase == VerifyPhase());
         Debug.Assert(MaterialValue[0] == Evaluation.Material(this, Color.White));
         Debug.Assert(MaterialValue[1] == Evaluation.Material(this, Color.Black));

         if (IsAttacked(GetSquareByPiece(PieceType.King, SideToMove ^ (Color)1), (int)SideToMove))
         {
            return false;
         }

         return true;
      }

      public void MakeNullMove()
      {
         GameHistory.Add(
            new BoardState(
               SideToMove, En_Passant, CastleSquares, new Piece(), Halfmoves, Fullmoves, Hash, Phase
            )
         );

         if (En_Passant != Square.Null)
         {
            Zobrist.UpdateEnPassant(ref Hash, En_Passant);
         }

         En_Passant = Square.Null;
         Halfmoves = 0;
         Fullmoves += 1;
         SideToMove = (Color)((int)SideToMove ^ 1);

         Zobrist.UpdateSideToMove(ref Hash);

         Debug.Assert(Zobrist.Verify(Hash, this));
         Debug.Assert(Phase == VerifyPhase());
      }

      public void UndoMove(Move move)
      {
         BoardState previousState = GameHistory.Pop();

         SideToMove = previousState.SideToMove;
         En_Passant = previousState.En_Passant;
         CastleSquares = previousState.CastleSquares;
         Halfmoves = previousState.Halfmoves;
         Fullmoves = previousState.Fullmoves;

         int from = move.From;
         int to = move.To;
         Piece piece = Mailbox[to];

         if (move.HasType(MoveType.Promotion))
         {
            RemovePiece(piece, to);
            SetPiece(new Piece(PieceType.Pawn, piece.Color), from);
         }
         else if (move.IsCastle())
         {
            RemovePiece(piece, to);
            SetPiece(piece, from);

            Piece rook = new(PieceType.Rook, piece.Color);

            if (move.Flag == MoveFlag.KingCastle)
            {
               RemovePiece(rook, piece.Color == Color.White ? (int)Square.F1 : (int)Square.F8);
               SetPiece(rook, piece.Color == Color.White ? (int)Square.H1 : (int)Square.H8);
            }
            else
            {
               RemovePiece(rook, piece.Color == Color.White ? (int)Square.D1 : (int)Square.D8);
               SetPiece(rook, piece.Color == Color.White ? (int)Square.A1 : (int)Square.A8);
            }
         }
         else
         {
            RemovePiece(piece, to);
            SetPiece(piece, from);
         }

         if (move.HasType(MoveType.Capture))
         {
            if (move.Flag == MoveFlag.EPCapture)
            {
               SetPiece(previousState.CapturedPiece, piece.Color == Color.White ? to + 8 : to - 8);
            }
            else
            {
               SetPiece(previousState.CapturedPiece, to);
            }
         }

         Phase = previousState.Phase;
         Hash = previousState.Hash;
         Debug.Assert(Zobrist.Verify(Hash, this));
         Debug.Assert(Phase == VerifyPhase());
         Debug.Assert(MaterialValue[0] == Evaluation.Material(this, Color.White));
         Debug.Assert(MaterialValue[1] == Evaluation.Material(this, Color.Black));
      }

      public void UnmakeNullMove()
      {
         BoardState previousState = GameHistory.Pop();

         SideToMove = previousState.SideToMove;
         En_Passant = previousState.En_Passant;
         CastleSquares = previousState.CastleSquares;
         Halfmoves = previousState.Halfmoves;
         Fullmoves = previousState.Fullmoves;
         Phase = previousState.Phase;
         Hash = previousState.Hash;

         Debug.Assert(Zobrist.Verify(Hash, this));
         Debug.Assert(Phase == VerifyPhase());
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private void SetPiece(Piece piece, int square)
      {
         ColorBB[(int)piece.Color].SetBit(square);
         PieceBB[(int)piece.Type].SetBit(square);
         Mailbox[square] = piece;
         Phase += PhaseValues[(int)piece.Type];
         Zobrist.UpdatePieces(ref Hash, piece, square);
         MaterialValue[(int)piece.Color] += Evaluation.PieceValues[(int)piece.Type];
         MaterialValue[(int)piece.Color] += Evaluation.GetPSTScore(piece, square);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private void RemovePiece(Piece piece, int square)
      {
         ColorBB[(int)piece.Color].ResetBit(square);
         PieceBB[(int)piece.Type].ResetBit(square);
         Mailbox[square] = new Piece();
         Phase -= PhaseValues[(int)piece.Type];
         Zobrist.UpdatePieces(ref Hash, piece, square);
         MaterialValue[(int)piece.Color] -= Evaluation.PieceValues[(int)piece.Type];
         MaterialValue[(int)piece.Color] -= Evaluation.GetPSTScore(piece, square);
      }

      public int GetSquareByPiece(PieceType piece, Color color)
      {
         return new Bitboard(PieceBB[(int)piece].Value & ColorBB[(int)color].Value).GetLSB();
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Bitboard MinorPieces(Color color)
      {
         return (PieceBB[(int)PieceType.Knight] | PieceBB[(int)PieceType.Bishop]) & ColorBB[(int)color].Value;
      }

      public bool IsAttacked(int square, int color)
      {
         if ((Attacks.PawnAttacks[color ^ 1][square] & PieceBB[(int)PieceType.Pawn].Value & ColorBB[color].Value) != 0)
         {
            return true;
         }

         if ((Attacks.KnightAttacks[square] & PieceBB[(int)PieceType.Knight].Value & ColorBB[color].Value) != 0)
         {
            return true;
         }

         if ((Attacks.KingAttacks[square] & PieceBB[(int)PieceType.King].Value & ColorBB[color].Value) != 0)
         {
            return true;
         }

         ulong occupied = ColorBB[(int)Color.White].Value | ColorBB[(int)Color.Black].Value;
         ulong bishopQueens = (PieceBB[(int)PieceType.Bishop].Value | PieceBB[(int)PieceType.Queen].Value) & ColorBB[color].Value;

         if ((Attacks.GetBishopAttacks(square, occupied) & bishopQueens) != 0)
         {
            return true;
         }

         ulong rookQueens = (PieceBB[(int)PieceType.Rook].Value | PieceBB[(int)PieceType.Queen].Value) & ColorBB[color].Value;

         if ((Attacks.GetRookAttacks(square, occupied) & rookQueens) != 0)
         {
            return true;
         }

         return false;
      }

      public bool IsDrawn()
      {
         // No draw if there are pawns or major pieces left
         if (PieceBB[(int)PieceType.Pawn].CountBits() != 0 || PieceBB[(int)PieceType.Rook].CountBits() != 0 || PieceBB[(int)PieceType.Queen].CountBits() != 0)
         {
            return false;
         }

         // Only kings
         if ((ColorBB[(int)Color.White] | ColorBB[(int)Color.Black]).CountBits() == 2)
         {
            return true;
         }

         // White only has 1 king left, check what black pieces are left
         if (ColorBB[(int)Color.White].CountBits() == 1)
         {
            // Only 1 minor piece
            if (MinorPieces(Color.Black).CountBits() == 1)
            {
               return true;
            }
            // Only two knights
            else if (MinorPieces(Color.Black).CountBits() == 2 && (PieceBB[(int)PieceType.Bishop] & ColorBB[(int)Color.Black]).CountBits() == 0)
            {
               return true;
            }
         }

         // Black only has 1 king left, check what white pieces are left
         else if (ColorBB[(int)Color.Black].CountBits() == 1)
         {
            // Only 1 minor piece
            if (MinorPieces(Color.White).CountBits() == 1)
            {
               return true;
            }
            // Only two knights
            else if (MinorPieces(Color.White).CountBits() == 2 && (PieceBB[(int)PieceType.Bishop] & ColorBB[(int)Color.White]).CountBits() == 0)
            {
               return true;
            }
         }

         // Both sides only have a king and 1 minor piece
         else if (ColorBB[(int)Color.White].CountBits() == 2 && ColorBB[(int)Color.Black].CountBits() == 2)
         {
            // We've already returned false if there are pawns, rooks, or queens on the board
            // so the 2nd piece has to be a minor piece
            return true;
         }

         return false;
      }

      public bool IsWon()
      {
         // One side has only a king left
         if (ColorBB[(int)Color.White].Value == (PieceBB[(int)PieceType.King].Value & ColorBB[(int)Color.White].Value))
         {
            // Other side only has a king and either a rook or queen
            if (ColorBB[(int)Color.Black].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Queen].Value) & ColorBB[(int)Color.Black].Value))
            {
               return true;
            }
            else if (ColorBB[(int)Color.Black].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Rook].Value) & ColorBB[(int)Color.Black].Value))
            {
               return true;
            }
            // 2 bishops, on different color squares?
            else if (ColorBB[(int)Color.Black].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Bishop].Value) & ColorBB[(int)Color.Black].Value)
               && PieceBB[(int)PieceType.Bishop].CountBits() == 2)
            {
               int sq1 = PieceBB[(int)PieceType.Bishop].GetLSB();
               int sq2 = PieceBB[(int)PieceType.Bishop].GetMSB();

               return ((9 * (sq1 ^ sq2)) & 8) != 0;
            }
            // bishop + knight
            else if (ColorBB[(int)Color.Black].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Bishop].Value | PieceBB[(int)PieceType.Knight].Value) & ColorBB[(int)Color.Black].Value))
            {               
               if (PieceBB[(int)PieceType.Knight].CountBits() == 1 && PieceBB[(int)PieceType.Bishop].CountBits() == 1)
               {
                  return true;
               }

               return false;
            }
         }
         else if (ColorBB[(int)Color.Black].Value == (PieceBB[(int)PieceType.King].Value & ColorBB[(int)Color.Black].Value))
         {
            if (ColorBB[(int)Color.White].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Queen].Value) & ColorBB[(int)Color.White].Value))
            {
               return true;
            }
            else if (ColorBB[(int)Color.White].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Rook].Value) & ColorBB[(int)Color.White].Value))
            {
               return true;
            }
            else if (ColorBB[(int)Color.White].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Bishop].Value) & ColorBB[(int)Color.White].Value)
               && PieceBB[(int)PieceType.Bishop].CountBits() == 2)
            {
               int sq1 = PieceBB[(int)PieceType.Bishop].GetLSB();
               int sq2 = PieceBB[(int)PieceType.Bishop].GetMSB();

               return ((9 * (sq1 ^ sq2)) & 8) != 0;
            }
            else if (ColorBB[(int)Color.White].Value == ((PieceBB[(int)PieceType.King].Value | PieceBB[(int)PieceType.Bishop].Value | PieceBB[(int)PieceType.Knight].Value) & ColorBB[(int)Color.White].Value))
            {
               if (PieceBB[(int)PieceType.Knight].CountBits() == 1 && PieceBB[(int)PieceType.Bishop].CountBits() == 1)
               {
                  return true;
               }

               return false;
            }
         }

         return false;
      }

      public bool MoveIsValid(Move move)
      {
         if (move == 0)
         {
            return false;
         }

         Piece piece = Mailbox[move.From];

         // there's no piece to move?
         if (piece.Type == PieceType.Null)
         {
            return false;
         }

         // moving an opponent's piece?
         if (piece.Color != SideToMove)
         {
            return false;
         }

         // capturing our own piece?
         if (Mailbox[move.To].Color == SideToMove)
         {
            return false;
         }

         // sliding piece trying to slide through pieces?
         if (piece.Type == PieceType.Bishop || piece.Type == PieceType.Rook || piece.Type == PieceType.Queen)
         {
            if ((Constants.BetweenBB[move.From][move.To] & (ColorBB[(int)Color.White].Value | ColorBB[(int)Color.Black].Value)) != 0)
            {
               return false;
            }
         }

         MoveList moves = MoveGen.GenerateAll(this);
         for (int i = 0; i < moves.Count; i++)
         {
            if (moves[i] == move)
            {
               return true;
            }
         }

         return false;
      }

      private int VerifyPhase()
      {
         int phase = 0;
         Bitboard pieces = new(ColorBB[(int)Color.White].Value | ColorBB[(int)Color.Black].Value);

         while (!pieces.IsEmpty())
         {
            int square = pieces.GetLSB();
            pieces.ClearLSB();
            Piece piece = Mailbox[square];

            phase += PhaseValues[(int)piece.Type];
         }

         return phase;
      }
   }
}
