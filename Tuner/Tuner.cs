// *********************************************************************************
// 
// This tuner is, for the most part, a C# rewrite of 
// the Gedas tuner (with some adaptations for use with Skookum).
// The original source code can be found here:
// https://github.com/GediminasMasaitis/texel-tuner
//
// *********************************************************************************

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Skookum
{
   internal class Tuner
   {
      private class Trace
      {
         public int[][] material = new int[6][];
         public int[][] knightMobility = new int[9][];
         public int[][] bishopMobility = new int[14][];
         public int[][] rookMobility = new int[15][];
         public int[][] queenMobility = new int[28][];
         public int score = 0;

         public Trace()
         {
            for (int i = 0; i < 6; i++)
            {
               material[i] = new int[2];
            }

            for (int i = 0; i < 9; i++)
            {
               knightMobility[i] = new int[2];
            }

            for (int i = 0; i < 14; i++)
            {
               bishopMobility[i] = new int[2];
            }

            for (int i = 0; i < 15; i++)
            {
               rookMobility[i] = new int[2];
            }

            for (int i = 0; i < 28; i++)
            {
               queenMobility[i] = new int[2];
            }
         }
      }

      private class EvalResult
      {
         public List<short> Coefficients = new();
         public double Score;
         public double EndgameScale = 1;
      }

      private class CoefficientEntry
      {
         public short Value;
         public int Index;

         public CoefficientEntry(short value, int index)
         {
            Index = index;
            Value = value;
         }
      }

      private class Entry
      {
         public List<CoefficientEntry> Coefficients = new();
         public int Phase;
         public double EndgameScale;
         public double Result;
      }

      private readonly Engine Engine;
      private readonly List<double[]> Parameters = new();

      public Tuner(Engine engine)
      {
         Engine = engine;
      }

      public void Run(int maxEpochs = 1000)
      {
         Console.WriteLine($"Number of epochs set to: {maxEpochs}");

         LoadParameters();
         List<Entry> entries = LoadPositions();

         double K = FindK(entries);
         Console.WriteLine($"K value: {K}");

         double avgError = GetAverageError(entries, K);
         Console.WriteLine($"Initial average error: {avgError}");

         double learningRate = 1;
         double[][] momentum = new double[entries.Count][];
         double[][] velocity = new double[entries.Count][];

         for (int i = 0; i < entries.Count; i++)
         {
            momentum[i] = new double[2];
            velocity[i] = new double[2];
         }

         using StreamWriter sw = new(@$"./errors_{DateTime.Now.ToString("yyyy-MM-dd,HHmmss")}.txt", true);
         Stopwatch timer = new();
         timer.Start();

         for (int epoch = 1; epoch <= maxEpochs; epoch++)
         {
            double[][] gradients = ComputeGradient(entries, K);
            double beta1 = 0.9;
            double beta2 = 0.999;

            for (int parameterIndex = 0; parameterIndex < Parameters.Count; parameterIndex++)
            {
               for (int phase = 0; phase < 2; phase++)
               {
                  double grad = -K * gradients[parameterIndex][phase] / entries.Count;
                  momentum[parameterIndex][phase] = beta1 * momentum[parameterIndex][phase] + (1 - beta1) * grad;
                  velocity[parameterIndex][phase] = beta2 * velocity[parameterIndex][phase] + (1 - beta2) * grad * grad;
                  Parameters[parameterIndex][phase] -= learningRate * momentum[parameterIndex][phase] / (1e-8 + Math.Sqrt(velocity[parameterIndex][phase]));
               }
            }

            //sw.WriteLine($"epoch: {epoch}, error: {GetAverageError(entries, K)}");

            if (epoch % 100 == 0)
            {
               double error = GetAverageError(entries, K);
               Console.WriteLine($"Epoch: {epoch}, EPS: {epoch * 1000 / timer.ElapsedMilliseconds}, error: {error}, time: {timer.Elapsed}");
            }
         }

         PrintResults();
         Console.WriteLine("Completed");
      }

      private double[][] ComputeGradient(List<Entry> entries, double K)
      {
         double[][] gradients = new double[Parameters.Count][];

         for (int i = 0; i < gradients.Length; i++)
         {
            gradients[i] = new double[2];
         }

         Parallel.For(0, entries.Count, () =>
         {
            double[][] localGradients = new double[Parameters.Count][];
            for (int i = 0; i < localGradients.Length; i++)
            {
               localGradients[i] = new double[2];
            }
            return localGradients;
         }, (j, loop, localGradients) =>
         {
            UpdateSingleGradient(entries[j], K, ref localGradients);
            return localGradients;
         }, (localGradients) =>
         {
            lock (gradients)
            {
               for (int i = 0; i < Parameters.Count; i++)
               {
                  gradients[i][0] += localGradients[i][0];
                  gradients[i][1] += localGradients[i][1];
               }
            }
         });

         return gradients;
      }

      private void UpdateSingleGradient(Entry entry, double K, ref double[][] gradient)
      {
         double eval = Evaluate(entry);
         double sig = Sigmoid(K, eval);
         double res = (entry.Result - sig) * sig * (1 - sig);

         double mg_base = res * (Math.Clamp(entry.Phase, 0, 24) / 24);
         double eg_base = res - mg_base;

         foreach (CoefficientEntry coef in entry.Coefficients)
         {
            gradient[coef.Index][0] += mg_base * coef.Value;
            gradient[coef.Index][1] += eg_base * coef.Value;
         }
      }

      private double FindK(List<Entry> entries)
      {
         double rate = 10;
         double delta = 1e-5;
         double deviation_goal = 1e-6;
         double K = 2.5;
         double deviation = 1;

         while (Math.Abs(deviation) > deviation_goal)
         {
            double up = GetAverageError(entries, K + delta);
            double down = GetAverageError(entries, K - delta);
            deviation = (up - down) / (2 * delta);
            K -= deviation * rate;
         }

         return K;
      }

      private double GetAverageError(List<Entry> entries, double K)
      {
         double sum = 0;

         Parallel.For(0, entries.Count, () => 0.0,
            (j, loop, subtotal) =>
            {
               double score = Evaluate(entries[j]);
               double sigmoid = Sigmoid(K, score);
               double diff = entries[j].Result - sigmoid;
               subtotal += diff * diff;
               return subtotal;
            },
            subtotal => Add(ref sum, subtotal));

         return sum / entries.Count;
      }

      private double Evaluate(Entry entry)
      {
         double midgame = 0;
         double endgame = 0;
         double score = 0;

         foreach (CoefficientEntry coef in entry.Coefficients)
         {
            midgame += coef.Value * Parameters[coef.Index][0];
            endgame += coef.Value * Parameters[coef.Index][1];
         }

         score += endgame + Math.Clamp(entry.Phase, 0, 24) / 24 * (midgame - endgame);

         return score;
      }

      public void LoadParameters()
      {
         AddParameters(Evaluation.PieceValues);
         AddParameters(Evaluation.KnightMobility);
         AddParameters(Evaluation.BishopMobility);
         AddParameters(Evaluation.RookMobility);
         AddParameters(Evaluation.QueenMobility);
      }

      private void AddParameters(Score[] values)
      {
         foreach (Score value in values)
         {
            double[] arr = { value.Mg, value.Eg };
            Parameters.Add(arr);
         }
      }

      private List<Entry> LoadPositions()
      {
         string fileName = @"./lichess-big3-resolved.book";

         List<Entry> entries = new();

         foreach (string line in System.IO.File.ReadLines(fileName))
         {
            if (string.IsNullOrEmpty(line.Trim())) continue;

            string fen = line.Split("\"")[0].Trim();
            Engine.SetPosition(fen);

            (Trace trace, int phase) = GetEval();

            EvalResult result = new()
            {
               Coefficients = GetCoefficients(trace),
               Score = trace.score
            };

            Entry entry = new()
            {
               Phase = phase,
               Coefficients = GetEntryCoefficients(result.Coefficients),
               Result = GetEntryResult(line),
            };

            entries.Add(entry);
         }

         return entries;
      }

      private double GetEntryResult(string fen)
      {
         Match match = Regex.Match(fen, "\\[([^]]+)\\]");
         
         if (match.Success)
         {
            string result = match.Groups[1].Value;
            
            if (result == "1.0")
            {
               return 1;
            }
            else if (result == "0.5")
            {
               return 0.5;
            }
            else if (result == "0.0")
            {
               return 0;
            }
            else
            {
               throw new Exception($"Unknown fen result: {result}");
            }
         }
         else
         {
            throw new Exception("Unable to get fen result");
         }
      }

      private (Trace trace, int phase) GetEval()
      {
         Trace trace = new();

         Score white = Material(Color.White, trace);
         Score black = Material(Color.Black, trace);
         white += Knights(Color.White, trace);
         black += Knights(Color.Black, trace);
         white += Bishops(Color.White, trace);
         black += Bishops(Color.Black, trace);
         white += Rooks(Color.White, trace);
         black += Rooks(Color.Black, trace);
         white += Queens(Color.White, trace);
         black += Queens(Color.Black, trace);
         Score total = white - black;

         if (Engine.Board.SideToMove == Color.Black)
         {
            total *= -1;
         }

         int phase = Math.Clamp(Engine.Board.Phase, 0, 24);
         trace.score = total.Eg + phase / 24 * (total.Mg - total.Eg);

         Debug.Assert(trace.score == Evaluation.Evaluate(Engine.Board));

         return (trace, phase);
      }

      private Score Material(Color color, Trace trace)
      {
         Bitboard us = new(Engine.Board.ColorBB[(int)color].Value);
         Score score = new();

         while (!us.IsEmpty())
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = Engine.Board.Mailbox[square];

            score += Evaluation.PieceValues[(int)piece.Type];
            trace.material[(int)piece.Type][(int)piece.Color]++;
         }

         return score;
      }

      private Score Knights(Color color, Trace trace)
      {
         Bitboard knightsBB = new(Engine.Board.PieceBB[(int)PieceType.Knight].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         Score score = new();
         while (!knightsBB.IsEmpty())
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            int attacks = new Bitboard(Attacks.KnightAttacks[square] & ~us).CountBits();
            score += Evaluation.KnightMobility[attacks];
            trace.knightMobility[attacks][(int)color]++;
         }
         return score;
      }

      private Score Bishops(Color color, Trace trace)
      {
         Bitboard bishopBB = new(Engine.Board.PieceBB[(int)PieceType.Bishop].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         ulong occupied = Engine.Board.ColorBB[(int)Color.White].Value | Engine.Board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!bishopBB.IsEmpty())
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            int attacks = new Bitboard(Attacks.GetBishopAttacks(square, occupied) & ~us).CountBits();
            score += Evaluation.BishopMobility[attacks];
            trace.bishopMobility[attacks][(int)color]++;
         }
         return score;
      }

      private Score Rooks(Color color, Trace trace)
      {
         Bitboard rooksBB = new(Engine.Board.PieceBB[(int)PieceType.Rook].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         ulong occupied = Engine.Board.ColorBB[(int)Color.White].Value | Engine.Board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!rooksBB.IsEmpty())
         {
            int square = rooksBB.GetLSB();
            rooksBB.ClearLSB();
            int attacks = new Bitboard(Attacks.GetRookAttacks(square, occupied) & ~us).CountBits();
            score += Evaluation.RookMobility[attacks];
            trace.rookMobility[attacks][(int)color]++;
         }
         return score;
      }
      private Score Queens(Color color, Trace trace)
      {
         Bitboard queensBB = new(Engine.Board.PieceBB[(int)PieceType.Queen].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         ulong occupied = Engine.Board.ColorBB[(int)Color.White].Value | Engine.Board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!queensBB.IsEmpty())
         {
            int square = queensBB.GetLSB();
            queensBB.ClearLSB();
            int attacks = new Bitboard(Attacks.GetQueenAttacks(square, occupied) & ~us).CountBits();
            score += Evaluation.QueenMobility[attacks];
            trace.queenMobility[attacks][(int)color]++;
         }
         return score;
      }

      private List<short> GetCoefficients(Trace trace)
      {
         List<short> coefficients = new();

         GetCoefficientsFromArray(ref coefficients, trace.material, 6);
         GetCoefficientsFromArray(ref coefficients, trace.knightMobility, 9);
         GetCoefficientsFromArray(ref coefficients, trace.bishopMobility, 14);
         GetCoefficientsFromArray(ref coefficients, trace.rookMobility, 15);
         GetCoefficientsFromArray(ref coefficients, trace.queenMobility, 28);

         return coefficients;
      }

      private List<CoefficientEntry> GetEntryCoefficients(List<short> coefficients)
      {
         List<CoefficientEntry> entryCoefficients = new();

         for (int i = 0; i < coefficients.Count; i++)
         {
            if (coefficients[i] == 0)
            {
               continue;
            }

            var coefEntry = new CoefficientEntry(coefficients[i], i);
            entryCoefficients.Add(coefEntry);
         }

         return entryCoefficients;
      }

      private void GetCoefficientsFromArray(ref List<short> coefficients, int[][] trace, int size)
      {
         for (int i = 0; i < size; i++)
         {
            GetCoefficientSingle(ref coefficients, trace[i]);
         }
      }

      private void GetCoefficientSingle(ref List<short> coefficients, int[] trace)
      {
         coefficients.Add((short)(trace[0] - trace[1]));
      }

      private double Sigmoid(double factor, double score)
      {
         return 1 / (1 + Math.Exp(-(factor * score)));
      }

      // From https://stackoverflow.com/a/16893641
      private static double Add(ref double location1, double value)
      {
         double newCurrentValue = location1;
         while (true)
         {
            double currentValue = newCurrentValue;
            double newValue = currentValue + value;
            newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
            if (newCurrentValue.Equals(currentValue))
               return newValue;
         }
      }

      private void PrintResults()
      {
         string path = @$"./Tuning_Results_{DateTime.Now.ToString("yyyy-MM-dd,HHmmss")}.txt";

         if (!System.IO.File.Exists(path))
         {
            string createText = $"Tuning results generated on {DateTime.Now.ToString("yyyy-MM-dd,HHmmss")}\r\n";
            System.IO.File.WriteAllText(path, createText);
         }

         using StreamWriter sw = new(path, true);

         int index = 0;
         PrintArray("material", ref index, 6, sw);
         PrintArray("knight mobility", ref index, 9, sw);
         PrintArray("bishop mobility", ref index, 14, sw);
         PrintArray("rook mobility", ref index, 15, sw);
         PrintArray("queen mobility", ref index, 28, sw);
      }

      private void PrintArray(string name, ref int index, int count, StreamWriter writer)
      {
         int start = index;
         writer.WriteLine(name);
         for (int i = start; i < start + count; i++)
         {
            index += 1;
            string values = $"new Score({(int)Parameters[i][0]}, {(int)Parameters[i][1]}),";
            writer.WriteLine(values);
         }
         writer.WriteLine("\r\n");
      }
   }
}
