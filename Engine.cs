namespace Skookum
{
   internal class Engine
   {
      public bool IsRunning { get; private set; } = true;
      public readonly Board Board = new();
      readonly TimeManager Timer = new();
      private readonly Search Search;
      readonly TranspositionTable TTable = new();

      public Engine()
      {
         // Initializes the Attacks static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Attacks).TypeHandle);

         // Initializes the Constants static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Constants).TypeHandle);

         // Initializes the Zobirst table static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Zobrist).TypeHandle);

         Search = new Search(Board, Timer, TTable);
      }

      public void NewGame()
      {
         Board.Reset();
         TTable.Reset();
         Timer.Reset();
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
               if (move[4] == 'n')
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
               if (move[4] == 'b')
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
               if (move[4] == 'r')
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
               if (move[4] == 'q')
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
         int movestogo = 40;
         bool movetime = false;

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
                     Timer.SetTimeLimit(int.Parse(command[i + 1]), 0, 1, movetime = true);
                     break;
                  }
               case "depth":
                  {
                     Timer.MaxDepth = Math.Min(int.Parse(command[1]), Constants.MAX_PLY);
                     break;
                  }
            }
         }

         if (!movetime && Timer.MaxDepth == Constants.MAX_PLY)
         {
            Timer.SetTimeLimit(Board.SideToMove == Color.White ? wtime : btime, Board.SideToMove == Color.White ? winc : binc, movestogo, false);
         }

         Search.Run();
      }

      public void SetOption(string[] option)
      {
         switch (option[0].ToLower())
         {
            case "hash":
               {
                  _ = int.TryParse(option[2], out int value);
                  value = Math.Clamp(value, 1, 512);
                  TTable.Resize(value);
                  break;
               }
            default:
               {
                  Console.WriteLine($"Unknown or unsupported option: {option[0]}");
                  break;
               }
         }
      }
   }
}
