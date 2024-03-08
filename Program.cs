using Puffin;
using Puffin.Tuner;
using static Puffin.Constants;
using System.Runtime.Intrinsics.X86;

const string Name = "Puffin";
const string Version = "2.0";
const string Author = "Kurt Peters";

// These intrinsics aren't required. If they're not supported BitOperations will fall back to whatever is.
// Fallbacks will likely be slower (depending on what it falls back to) and engine performance wont be maximized.
if (!Lzcnt.X64.IsSupported || !Popcnt.X64.IsSupported || !Bmi1.X64.IsSupported)
{
   Console.WriteLine("Optimal CPU-instruction support not found. Engine will run, but performance may be degraded.");
   Console.WriteLine($"Lzcnt: {Lzcnt.X64.IsSupported}");
   Console.WriteLine($"Popcnt: {Popcnt.X64.IsSupported}");
   Console.WriteLine($"Bmi1: {Bmi1.X64.IsSupported}");
}

#if Pext
if (!Bmi2.X64.IsSupported)
{
   Console.WriteLine("Your device hardware is not supported. Press any key to exit.");
   Console.ReadLine();
   Environment.Exit(100);
}
#endif

Console.WriteLine($"{Name} {Version}");

Engine engine = new();

if (args.Length != 0)
{
   for (int i = 0; i < args.Length; i++)
   {
      var arg = args[i];
      if (arg == "tune")
      {
         int epochs = int.Parse(args[i + 1]);
         Tuner tuner = new();
         tuner.Run(epochs);
         break;
      }
      else if (arg == "datagen")
      {
         Datagen datagen = new();
         Datagen.Run(int.Parse(args[i + 1]));
         Environment.Exit(100);
         break;
      }
   }
}

while (true)
{
   string? input = await Task.Run(Console.ReadLine);

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
            Console.WriteLine($"option name Hash type spin default 32 min 1 max 512");
            Console.WriteLine($"option name Threads type spin default 1 min 1 max 256");
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
               engine.SetPosition(START_POS);

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
      case "stop":
         {
            engine.StopSearch();
            break;
         }
      case "setoption":
         {
            engine.SetOption(tokens[1..]);
            break;
         }
      case "perft":
         {
            Console.WriteLine();
            engine.Perft(int.Parse(tokens[1]));
            break;
         }
      case "evaluate":
         {
            Console.WriteLine(engine.Evaluate());
            break;
         }
      default:
         {
            Console.WriteLine($"Unrecognized UCI command: {input}");
            break;
         }
   }
}
