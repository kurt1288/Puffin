using Skookum;

const string Name = "Skookum";
const string Version = "0.1";
const string Author = "Kurt Peters";

Console.WriteLine($"{Name} {Version}");

Engine engine = new();

while (engine.IsRunning)
{
   string input = await Task.Run(Console.ReadLine);

   if (string.IsNullOrWhiteSpace(input))
   {
      continue;
   }

   string[] tokens = input.Trim().Split();

   switch (tokens[0])
   {
      case "uci":
         {
            Console.WriteLine($"id name {Name} {Version}");
            Console.WriteLine($"id author {Author}");
            Console.WriteLine("uciok");
            break;
         }
      case "isready":
         {
            Console.WriteLine("readyok");
            break;
         }
      case "quit":
         {
            Environment.Exit(100);
            break;
         }
      case "ucinewgame":
         {
            engine.NewGame();
            break;
         }
      case "position":
         {
            if (tokens[1] == "startpos")
            {
               engine.SetPosition(Constants.START_POS);

               if (tokens.Length > 2 && tokens[2] == "moves")
               {
                  engine.MakeMoves(tokens[3..]);
               }
            }
            else if (tokens[1] == "fen")
            {
               try
               {
                  string fen = string.Join(" ", tokens[2..8]);
                  engine.SetPosition(fen);
               }
               catch
               {
                  Console.WriteLine("Unable to parse fen");
                  break;
               }

               if (tokens.Length > 9 && tokens[8] == "moves")
               {
                  engine.MakeMoves(tokens[9..]);
               }
            }

            break;
         }
      case "go":
         {
            engine.UCIParseGo(tokens[1..]);
            break;
         }
      case "perft":
         {
            Console.WriteLine();
            engine.Perft(int.Parse(tokens[1]));
            break;
         }
      default:
         {
            Console.WriteLine($"Unrecognized UCI command: {input}");
            break;
         }
   }
}

// This should never be hit
Console.WriteLine("The engine stopped running for an unknown reason.");
Environment.Exit(100);