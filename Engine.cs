namespace Skookum
{
   internal class Engine
   {
      public bool IsRunning { get; private set; } = true;
      public readonly Board Board = new();
      readonly TimeManager Timer = new();
      private readonly Search Search;

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

         Search = new Search(Board, Timer);
      }

      public void NewGame()
      {
         Board.Reset();
         TranspositionTable.Reset();
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

      public void UCIParseGo(string[] command)
      {
         Timer.Reset();

         for (int i = 0; i < command.Length; i++)
         {
            string type = command[i];

            switch (type)
            {
               case "wtime":
                  {
                     Timer.wtime = int.Parse(command[i + 1]);
                     break;
                  }
               case "btime":
                  {
                     Timer.btime = int.Parse(command[i + 1]);
                     break;
                  }
               case "winc":
                  {
                     Timer.winc = int.Parse(command[i + 1]);
                     break;
                  }
               case "binc":
                  {
                     Timer.binc = int.Parse(command[i + 1]);
                     break;
                  }
               case "movestogo":
                  {
                     Timer.movestogo = int.Parse(command[i + 1]);
                     break;
                  }
               case "movetime":
                  {
                     Timer.TimeLimit = int.Parse(command[i + 1]);
                     Timer.HardLimit = int.Parse(command[i + 1]);
                     break;
                  }
               case "depth":
                  {
                     int depth = Math.Min(int.Parse(command[1]), Constants.MAX_PLY);
                     Timer.depthLimit = depth;
                     Timer.infititeTime = true;
                     break;
                  }
            }

            if (Timer.TimeLimit == 0)
            {
               Timer.SetTimeLimit(Board);
            }
         }

         Search.Run(Timer.depthLimit);
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
