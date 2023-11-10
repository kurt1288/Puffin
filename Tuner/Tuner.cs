// *********************************************************************************
// 
// This tuner is, for the most part, a C# rewrite of 
// the Gedas tuner (with some adaptations for use with Skookum).
// The original source code can be found here:
// https://github.com/GediminasMasaitis/texel-tuner
//
// *********************************************************************************

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Skookum
{
   internal class Tuner
   {
      const double Epsilon = 1e-7;
      const string PositionsFile = @"./lichess-big3-resolved.book";

      private class Trace
      {
         public double[][] material = new double[6][];
         public double[][] pst = new double[384][];
         public double[][] knightMobility = new double[9][];
         public double[][] bishopMobility = new double[14][];
         public double[][] rookMobility = new double[15][];
         public double[][] queenMobility = new double[28][];
         public double[][] kingAttackWeights = new double[5][];
         public double[][] pawnShield = new double[4][];
         public double score = 0;

         public Trace()
         {
            for (int i = 0; i < 6; i++)
            {
               material[i] = new double[2];
            }

            for (int i = 0; i < 384; i++)
            {
               pst[i] = new double[2];
            }

            for (int i = 0; i < 9; i++)
            {
               knightMobility[i] = new double[2];
            }

            for (int i = 0; i < 14; i++)
            {
               bishopMobility[i] = new double[2];
            }

            for (int i = 0; i < 15; i++)
            {
               rookMobility[i] = new double[2];
            }

            for (int i = 0; i < 28; i++)
            {
               queenMobility[i] = new double[2];
            }

            for (int i = 0; i < 5; i++)
            {
               kingAttackWeights[i] = new double[2];
            }

            for (int i = 0; i < 4; i++)
            {
               pawnShield[i] = new double[2];
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
         public double Phase;
         public double EndgameScale;
         public double Result;
      }

      private readonly Engine Engine;
      private readonly List<double[]> Parameters = new();

      public Tuner(Engine engine)
      {
         Engine = engine;

         for (int i = 0; i < 384; i++)
         {
            Evaluation.PST[i] = new Score();
         }

         for (int i = 0; i < 9; i++)
         {
            Evaluation.KnightMobility[i] = new Score();
         }

         for (int i = 0; i < 14; i++)
         {
            Evaluation.BishopMobility[i] = new Score();
         }

         for (int i = 0; i < 15; i++)
         {
            Evaluation.RookMobility[i] = new Score();
         }

         for (int i = 0; i < 28; i++)
         {
            Evaluation.QueenMobility[i] = new Score();
         }

         for (int i = 0; i < 5; i++)
         {
            Evaluation.KingAttackWeights[i] = new Score();
         }

         for (int i = 0; i < 4; i++)
         {
            Evaluation.PawnShield[i] = new Score();
         }
      }

      public void Run(int maxEpochs = 10000)
      {
         Console.WriteLine($"Number of epochs set to: {maxEpochs}");

         LoadParameters();
         List<Entry> entries = LoadPositions();

         //double K = FindK(entries);
         double K = 2.5;
         Console.WriteLine($"K value: {K}");

         double avgError = GetAverageError(entries, K);
         double bestError = avgError + Epsilon * 2;
         Console.WriteLine($"Initial average error: {avgError}");

         double learningRate = 1;
         double[][] momentum = new double[Parameters.Count][];
         double[][] velocity = new double[Parameters.Count][];

         for (int i = 0; i < Parameters.Count; i++)
         {
            momentum[i] = new double[2];
            velocity[i] = new double[2];
         }

         int epoch = 0;

         //using StreamWriter sw = new(@$"./errors_{DateTime.Now.ToString("yyyy-MM-dd,HHmmss")}.txt", true);
         Stopwatch timer = new();
         timer.Start();

         //for (int epoch = 1; epoch <= maxEpochs; epoch++)
         //{
         while (Math.Abs(bestError - avgError) >= Epsilon && epoch < maxEpochs)
         {
            double[][] gradients = ComputeGradient(entries, K);
            double beta1 = 0.9;
            double beta2 = 0.999;

            for (int parameterIndex = 0; parameterIndex < Parameters.Count; parameterIndex++)
            {
               for (int phase = 0; phase < 2; phase++)
               {
                  double grad = -K / 400 * gradients[parameterIndex][phase] / entries.Count;
                  momentum[parameterIndex][phase] = beta1 * momentum[parameterIndex][phase] + (1 - beta1) * grad;
                  velocity[parameterIndex][phase] = beta2 * velocity[parameterIndex][phase] + (1 - beta2) * grad * grad;
                  Parameters[parameterIndex][phase] -= learningRate * momentum[parameterIndex][phase] / (1e-8 + Math.Sqrt(velocity[parameterIndex][phase]));
               }
            }

            //sw.WriteLine($"epoch: {epoch}, error: {GetAverageError(entries, K)}");

            if (epoch % 100 == 0)
            {
               bestError = avgError;
               avgError = GetAverageError(entries, K);
               Console.WriteLine($"Epoch: {epoch}, EPS: {epoch * 1000 / timer.ElapsedMilliseconds}, error: {bestError}, E: {bestError - avgError}, time: {timer.Elapsed}");
            }

            epoch += 1;
         }

         PrintResults();
         Console.WriteLine("Completed");
         Environment.Exit(100);
      }

      private double[][] ComputeGradient(List<Entry> entries, double K)
      {
         double[][] gradients = new double[Parameters.Count][];

         for (int i = 0; i < Parameters.Count; i++)
         {
            gradients[i] = new double[2];
         }

         Parallel.For(0, entries.Count, () =>
         {
            double[][] localGradients = new double[Parameters.Count][];
            for (int i = 0; i < Parameters.Count; i++)
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
         double res = (entry.Result - sig) * sig * (1.0 - sig);

         double mg_base = res * (entry.Phase / 24);
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

         score += ((midgame * entry.Phase) + (endgame * (24 - entry.Phase))) / 24;

         return score;
      }

      public void LoadParameters()
      {
         AddParameters(Evaluation.PieceValues);
         AddParameters(Evaluation.PST);
         AddParameters(Evaluation.KnightMobility);
         AddParameters(Evaluation.BishopMobility);
         AddParameters(Evaluation.RookMobility);
         AddParameters(Evaluation.QueenMobility);
         AddParameters(Evaluation.KingAttackWeights);
         AddParameters(Evaluation.PawnShield);
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
         List<Entry> entries = new();

         foreach (string line in System.IO.File.ReadLines(PositionsFile))
         {
            if (string.IsNullOrEmpty(line.Trim())) continue;

            string fen = line.Split("\"")[0].Trim();
            Engine.SetPosition(fen);

            (Trace trace, double phase) = GetEval();

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
               return 1.0;
            }
            else if (result == "0.5")
            {
               return 0.5;
            }
            else if (result == "0.0")
            {
               return 0.0;
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

      private (Trace trace, double phase) GetEval()
      {
         Trace trace = new();

         ulong whiteKingZone = Attacks.KingAttacks[Engine.Board.GetSquareByPiece(PieceType.King, Color.White)];
         ulong blackKingZone = Attacks.KingAttacks[Engine.Board.GetSquareByPiece(PieceType.King, Color.Black)];

         Score[] kingAttacks = { new(), new() };
         int[] kingAttacksCount = { 0, 0 };
         Score[] kingAttacksWeights = { new(), new() };

         Score white = Material(Color.White, ref trace);
         Score black = Material(Color.Black, ref trace);
         white += Knights(Color.White, trace, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Knights(Color.Black, trace, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Bishops(Color.White, trace, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Bishops(Color.Black, trace, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Rooks(Color.White, trace, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Rooks(Color.Black, trace, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Queens(Color.White, trace, blackKingZone, ref kingAttacks, ref kingAttacksCount);
         black += Queens(Color.Black, trace, whiteKingZone, ref kingAttacks, ref kingAttacksCount);
         white += Kings(trace, Color.White, ref kingAttacks, ref kingAttacksCount);
         black += Kings(trace, Color.Black, ref kingAttacks, ref kingAttacksCount);

         Score total = white - black;

         trace.score = ((total.Mg * Engine.Board.Phase) + (total.Eg * (24 - Engine.Board.Phase))) / 24;

         return (trace, Engine.Board.Phase);
      }

      private Score Material(Color color, ref Trace trace)
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

            if (piece.Color == Color.Black)
            {
               square ^= 56;
            }

            score += Evaluation.PST[((int)piece.Type * 64) + square];
            trace.pst[((int)piece.Type * 64) + square][(int)piece.Color]++;
         }

         return score;
      }

      private Score Knights(Color color, Trace trace, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
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

            if ((Attacks.KnightAttacks[square] & kingZone) != 0)
            {
               kingAttacksCount[(int)color]++;
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Knight] * new Bitboard(Attacks.KnightAttacks[square] & kingZone).CountBits();
               trace.kingAttackWeights[(int)PieceType.Knight][(int)color] += new Bitboard(Attacks.KnightAttacks[square] & kingZone).CountBits();
            }
         }
         return score;
      }

      private Score Bishops(Color color, Trace trace, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard bishopBB = new(Engine.Board.PieceBB[(int)PieceType.Bishop].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         ulong occupied = Engine.Board.ColorBB[(int)Color.White].Value | Engine.Board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!bishopBB.IsEmpty())
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            ulong moves = Attacks.GetBishopAttacks(square, occupied);
            int attacks = new Bitboard(moves & ~us).CountBits();
            score += Evaluation.BishopMobility[attacks];
            trace.bishopMobility[attacks][(int)color]++;

            if ((moves & kingZone) != 0)
            {
               kingAttacksCount[(int)color]++;
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & kingZone).CountBits();
               trace.kingAttackWeights[(int)PieceType.Bishop][(int)color] += new Bitboard(moves & kingZone).CountBits();
            }
         }
         return score;
      }

      private Score Rooks(Color color, Trace trace, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard rooksBB = new(Engine.Board.PieceBB[(int)PieceType.Rook].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         ulong occupied = Engine.Board.ColorBB[(int)Color.White].Value | Engine.Board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!rooksBB.IsEmpty())
         {
            int square = rooksBB.GetLSB();
            rooksBB.ClearLSB();
            ulong moves = Attacks.GetRookAttacks(square, occupied);
            int attacks = new Bitboard(moves & ~us).CountBits();
            score += Evaluation.RookMobility[attacks];
            trace.rookMobility[attacks][(int)color]++;

            if ((moves & kingZone) != 0)
            {
               kingAttacksCount[(int)color]++;
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & kingZone).CountBits();
               trace.kingAttackWeights[(int)PieceType.Rook][(int)color] += new Bitboard(moves & kingZone).CountBits();
            }
         }
         return score;
      }
      private Score Queens(Color color, Trace trace, ulong kingZone, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Bitboard queensBB = new(Engine.Board.PieceBB[(int)PieceType.Queen].Value & Engine.Board.ColorBB[(int)color].Value);
         ulong us = Engine.Board.ColorBB[(int)color].Value;
         ulong occupied = Engine.Board.ColorBB[(int)Color.White].Value | Engine.Board.ColorBB[(int)Color.Black].Value;
         Score score = new();
         while (!queensBB.IsEmpty())
         {
            int square = queensBB.GetLSB();
            queensBB.ClearLSB();
            ulong moves = Attacks.GetQueenAttacks(square, occupied);
            int attacks = new Bitboard(moves & ~us).CountBits();
            score += Evaluation.QueenMobility[attacks];
            trace.queenMobility[attacks][(int)color]++;

            if ((moves & kingZone) != 0)
            {
               kingAttacksCount[(int)color]++;
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & kingZone).CountBits();
               trace.kingAttackWeights[(int)PieceType.Queen][(int)color] += new Bitboard(moves & kingZone).CountBits();
            }
         }
         return score;
      }

      private Score Kings(Trace trace, Color color, ref Score[] kingAttacks, ref int[] kingAttacksCount)
      {
         Score score = new();
         Bitboard kingBB = new(Engine.Board.PieceBB[(int)PieceType.King].Value & Engine.Board.ColorBB[(int)color].Value);
         int kingSq = kingBB.GetLSB();
         ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

         if ((kingSquares & Constants.SquareBB[kingSq]) != 0)
         {
            ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x007000000000000 : 0x000E0000000000000) : (ulong)(kingSq % 8 < 3 ? 0x700 : 0xE000);

            Bitboard pawns = new(Engine.Board.PieceBB[(int)PieceType.Pawn].Value & Engine.Board.ColorBB[(int)color].Value & pawnSquares);
            score += Evaluation.PawnShield[Math.Min(pawns.CountBits(), 3)];
            trace.pawnShield[Math.Min(pawns.CountBits(), 3)][(int)color]++;
         }

         if (kingAttacksCount[(int)color ^ 1] >= 2)
         {
            score -= kingAttacks[(int)color ^ 1];
         }

         return score;
      }

      private List<short> GetCoefficients(Trace trace)
      {
         List<short> coefficients = new();

         GetCoefficientsFromArray(ref coefficients, trace.material, 6);
         GetCoefficientsFromArray(ref coefficients, trace.pst, 384);
         GetCoefficientsFromArray(ref coefficients, trace.knightMobility, 9);
         GetCoefficientsFromArray(ref coefficients, trace.bishopMobility, 14);
         GetCoefficientsFromArray(ref coefficients, trace.rookMobility, 15);
         GetCoefficientsFromArray(ref coefficients, trace.queenMobility, 28);
         GetCoefficientsFromArray(ref coefficients, trace.kingAttackWeights, 5);
         GetCoefficientsFromArray(ref coefficients, trace.pawnShield, 4);

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

      private void GetCoefficientsFromArray(ref List<short> coefficients, double[][] trace, int size)
      {
         for (int i = 0; i < size; i++)
         {
            GetCoefficientSingle(ref coefficients, trace[i]);
         }
      }

      private void GetCoefficientSingle(ref List<short> coefficients, double[] trace)
      {
         coefficients.Add((short)(trace[0] - trace[1]));
      }

      private double Sigmoid(double factor, double score)
      {
         return 1.0 / (1.0 + Math.Exp(-(factor * score)));
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
         PrintPSTArray("pst", ref index, sw);
         PrintArray("knight mobility", ref index, 9, sw);
         PrintArray("bishop mobility", ref index, 14, sw);
         PrintArray("rook mobility", ref index, 15, sw);
         PrintArray("queen mobility", ref index, 28, sw);
         PrintArray("king attack weights", ref index, 5, sw);
         PrintArray("pawn shield", ref index, 4, sw);
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

      private void PrintPSTArray(string name, ref int index, StreamWriter writer)
      {
         int offset = index;
         StringBuilder stringBuilder = new();
         writer.WriteLine(name);

         for (int piece = 0; piece < 6; ++piece)
         {
            for (int square = 0; square < 64; ++square)
            {
               int i = piece * 64 + square + offset;
               stringBuilder.Append($"new Score({(int)Parameters[i][0], 3}, {(int)Parameters[i][1], 3}), ");
               index += 1;

               if (square % 8 == 7)
               {
                  writer.WriteLine(stringBuilder);
                  stringBuilder.Clear();
               }
            }

            writer.WriteLine();
         }
      }
   }
}
