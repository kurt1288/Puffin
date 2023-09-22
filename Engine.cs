namespace Skookum
{
   internal class Engine
   {
      public bool IsRunning { get; private set; } = true;
      public readonly Board Board = new();

      public Engine()
      {
         // Initializes the Attacks static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Attacks).TypeHandle);

         // Initializes the Constants static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Constants).TypeHandle);

         // Initializes the Transposition table static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(TranspositionTable).TypeHandle);

         // Initializes the Zobirst table static class
         System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Zobrist).TypeHandle);
      }

      public void NewGame()
      {
         Board.Reset();
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
               if (move[5] == 'n')
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
               if (move[5] == 'b')
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
               if (move[5] == 'r')
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
               if (move[5] == 'q')
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

      public void UCIParseGo(string[] command)
      {
         TimeManager timer = new();

         for (int i = 0; i < command.Length; i++)
         {
            string type = command[i];

            switch (type)
            {
               case "wtime":
                  {
                     timer.wtime = int.Parse(command[i + 1]);
                     break;
                  }
               case "btime":
                  {
                     timer.btime = int.Parse(command[i + 1]);
                     break;
                  }
               case "winc":
                  {
                     timer.winc = int.Parse(command[i + 1]);
                     break;
                  }
               case "binc":
                  {
                     timer.binc = int.Parse(command[i + 1]);
                     break;
                  }
               case "movestogo":
                  {
                     timer.movestogo = int.Parse(command[i + 1]);
                     break;
                  }
               case "movetime":
                  {
                     timer.TimeLimit = int.Parse(command[i + 1]);
                     break;
                  }
               case "depth":
                  {
                     int depth = Math.Min(int.Parse(command[1]), Constants.MAX_PLY);
                     timer.depthLimit = depth;
                     timer.infititeTime = true;
                     break;
                  }
            }

            if (timer.TimeLimit == 0)
            {
               timer.SetTimeLimit(Board);
            }
         }

         Search search = new(Board, timer);
         search.Run(timer.depthLimit);
      }

      public void SetOption(string[] option)
      {
         switch (option[0].ToLower())
         {
            case "hash":
               {
                  _ = int.TryParse(option[2], out int value);
                  value = Math.Clamp(value, 1, 512);
                  TranspositionTable.Resize(value);
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
