using static Puffin.Constants;

namespace Puffin
{
   internal class Engine
   {
      public readonly Board Board = new();
      readonly TimeManager Timer = new();
      TranspositionTable TTable = new();
      int Threads = 1;
      ThreadManager SearchManager;

      public Engine()
      {
         // Initializes the Attacks static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Attacks).TypeHandle);

         // Initializes the Constants static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Constants).TypeHandle);

         // Initializes the Zobirst table static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Zobrist).TypeHandle);

         SearchManager = new(Threads, ref TTable);
      }

      public void NewGame()
      {
         Board.Reset();
         TTable.Reset();
         Timer.Reset();
         SearchManager.Reset();
      }

      public void SetPosition(string fen)
      {
         Board.SetPosition(fen);
      }

      public void MakeMoves(string[] moves)
      {
         foreach (string move in moves)
         {
            MoveFlag flag = MoveFlag.Quiet;
            int from = (int)Enum.Parse(typeof(Square), move.Substring(0, 2).ToUpper());
            int to = (int)Enum.Parse(typeof(Square), move.Substring(2, 2).ToUpper());
            Piece piece = Board.Mailbox[from];

            if (piece.Type == PieceType.Pawn && Math.Abs((from >> 3) - (to >> 3)) == 2)
            {
               flag = MoveFlag.DoublePawnPush;
            }

            if (Board.Mailbox[to].Type != PieceType.Null)
            {
               flag = MoveFlag.Capture;
            }

            if (to == (int)Board.En_Passant && piece.Type == PieceType.Pawn)
            {
               flag = MoveFlag.EPCapture;
            }

            if (piece.Type == PieceType.King)
            {
               if (move == "e1g1" || move == "e8g8")
               {
                  flag = MoveFlag.KingCastle;
               }
               else if (move == "e1c1" || move == "e8c8")
               {
                  flag = MoveFlag.QueenCastle;
               }
            }

            if (move.Length == 5)
            {
               if (char.ToLower(move[4]) == 'n')
               {
                  if (Board.Mailbox[to].Type != PieceType.Null)
                  {
                     flag = MoveFlag.KnightPromotionCapture;
                  }
                  else
                  {
                     flag = MoveFlag.KnightPromotion;
                  }
               }
               else if (char.ToLower(move[4]) == 'b')
               {
                  if (Board.Mailbox[to].Type != PieceType.Null)
                  {
                     flag = MoveFlag.BishopPromotionCapture;
                  }
                  else
                  {
                     flag = MoveFlag.BishopPromotion;
                  }
               }
               else if (char.ToLower(move[4]) == 'r')
               {
                  if (Board.Mailbox[to].Type != PieceType.Null)
                  {
                     flag = MoveFlag.RookPromotionCapture;
                  }
                  else
                  {
                     flag = MoveFlag.RookPromotion;
                  }
               }
               else if (char.ToLower(move[4]) == 'q')
               {
                  if (Board.Mailbox[to].Type != PieceType.Null)
                  {
                     flag = MoveFlag.QueenPromotionCapture;
                  }
                  else
                  {
                     flag = MoveFlag.QueenPromotion;
                  }
               }
            }

            Board.MakeMove(new Move(from, to, flag));
         }
      }

      public void Perft(int depth)
      {
         Perft perft = new(Board);
         perft.Run(depth);
      }

      public int Evaluate()
      {
         return Evaluation.Evaluate(Board);
      }

      public void UCIParseGo(string[] command)
      {
         Timer.Reset();
         int wtime = 0;
         int btime = 0;
         int winc = 0;
         int binc = 0;
         int movestogo = 0;
         int movetime = 0;
         int depth = 0;
         int nodes = 0;

         for (int i = 0; i < command.Length; i += 2)
         {
            string type = command[i];

            switch (type)
            {
               case "wtime":
                  {
                     wtime = int.Parse(command[i + 1]);
                     break;
                  }
               case "btime":
                  {
                     btime = int.Parse(command[i + 1]);
                     break;
                  }
               case "winc":
                  {
                     winc = int.Parse(command[i + 1]);
                     break;
                  }
               case "binc":
                  {
                     binc = int.Parse(command[i + 1]);
                     break;
                  }
               case "movestogo":
                  {
                     movestogo = int.Parse(command[i + 1]);
                     break;
                  }
               case "movetime":
                  {
                     movetime = int.Parse(command[i + 1]);
                     break;
                  }
               case "depth":
                  {
                     depth = Math.Clamp(int.Parse(command[1]), 1, MAX_PLY - 1);
                     break;
                  }
               case "nodes":
                  {
                     nodes = int.Parse(command[1]);
                     break;
                  }
            }
         }

         Timer.SetLimits(Board.SideToMove == Color.White ? wtime : btime, Board.SideToMove == Color.White ? winc : binc, movestogo, movetime, depth, nodes);

         SearchManager.StartSearches(Timer, Board);
      }

      public void StopSearch()
      {
         Timer.Stop();
      }

      public void SetOption(string[] option)
      {
         _ = int.TryParse(option[3], out int value);

         switch (option[1].ToLower())
         {
            case "hash":
               {
                  value = Math.Clamp(value, 1, 512);
                  TTable.Resize(value);
                  break;
               }
            case "threads":
               {
                  Threads = value;
                  SearchManager.Shutdown();
                  SearchManager = new(value, ref TTable);
                  break;
               }
            case "ASP_Depth":
               {
                  Search.ASP_Depth = value;
                  break;
               }
            case "ASP_Margin":
               {
                  Search.ASP_Margin = value;
                  break;
               }
            case "NMP_Depth":
               {
                  Search.NMP_Depth = value;
                  break;
               }
            case "RFP_Depth":
               {
                  Search.RFP_Depth = value;
                  break;
               }
            case "RFP_Margin":
               {
                  Search.RFP_Margin = value;
                  break;
               }
            case "LMR_Depth":
               {
                  Search.LMR_Depth = value;
                  break;
               }
            case "LMR_MoveLimit":
               {
                  Search.LMR_MoveLimit = value;
                  break;
               }
            case "FP_Depth":
               {
                  Search.FP_Depth = value;
                  break;
               }
            case "FP_Margin":
               {
                  Search.FP_Margin = value;
                  break;
               }
            case "LMP_Depth":
               {
                  Search.LMP_Depth = value;
                  break;
               }
            case "LMP_Margin":
               {
                  Search.LMP_Margin = value;
                  break;
               }
            case "IIR_Depth":
               {
                  Search.IIR_Depth = value;
                  break;
               }
            default:
               {
                  Console.WriteLine($"Unknown or unsupported option: {option[1]}");
                  break;
               }
         }
      }
   }
}
