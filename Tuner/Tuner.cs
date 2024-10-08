// *********************************************************************************
// 
// This tuner is, for the most part, a C# rewrite of 
// the Gedas tuner (with some adaptations for use with Puffin).
// The original source code can be found here:
// https://github.com/GediminasMasaitis/texel-tuner
//
// *********************************************************************************

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using static Puffin.Constants;
using static Puffin.Attacks;

namespace Puffin.Tuner
{
   internal partial class Tuner
   {
      const double Epsilon = 1e-7;
      const string PositionsFile = @"./datagen.epd";
      string ResultsPath = @$"./Tune_{DateTime.Now:yyyy-MM-dd,HHmmss}";

      private class Trace
      {
         public double[][] material = new double[6][];
         public double[][] pst = new double[384][];
         public double[][] knightMobility = new double[9][];
         public double[][] bishopMobility = new double[14][];
         public double[][] rookMobility = new double[15][];
         public double[][] queenMobility = new double[28][];
         public double[] rookHalfOpenFile = new double[2];
         public double[] rookOpenFile = new double[2];
         public double[] kingOpenFile = new double[2];
         public double[] kingHalfOpenFile = new double[2];
         public double[][] kingAttackWeights = new double[5][];
         public double[][] pawnShield = new double[4][];
         public double[][] passedPawn = new double[7][];
         public double[][] defendedPawn = new double[8][];
         public double[][] connectedPawn = new double[9][];
         public double[][] isolatedPawn = new double[8][];
         public double[] friendlyKingPawnDistance = new double[2];
         public double[] enemyKingPawnDistance = new double[2];
         public double[] bishopPair = new double[2];
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
               connectedPawn[i] = new double[2];
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

            for (int i = 0; i < 7; i++)
            {
               passedPawn[i] = new double[2];
            }

            for (int i = 0; i < 8; i++)
            {
               defendedPawn[i] = new double[2];
               isolatedPawn[i] = new double[2];
            }
         }
      }

      private struct PotentialKingAttacks
      {
         public int[] Count;
         public int[] Weight;

         public PotentialKingAttacks(int pieceTypeCount)
         {
            Count = new int[pieceTypeCount];
            Weight = new int[pieceTypeCount];
         }
      }

      private struct ParameterWeight
      {
         public double Mg;
         public double Eg;

         public ParameterWeight() { }

         public ParameterWeight(double mg, double eg)
         {
            Mg = mg;
            Eg = eg;
         }

         public static ParameterWeight operator +(ParameterWeight a, ParameterWeight b) => new(a.Mg + b.Mg, a.Eg + b.Eg);
      }

      private readonly struct CoefficientEntry
      {
         public readonly double Value;
         public readonly int Index;

         public CoefficientEntry(double value, int index)
         {
            Index = index;
            Value = value;
         }
      }

      private class Entry
      {
         public readonly List<CoefficientEntry> Coefficients;
         public readonly double Phase;
         public readonly double Result;
         public readonly string Fen;

         public Entry(List<CoefficientEntry> coefficients, double phase, double result, string fen)
         {
            Coefficients = coefficients;
            Phase = phase;
            Result = result;
            Fen = fen;
         }
      }

      // readonly Engine Engine;
      private readonly ParameterWeight[] Parameters = new ParameterWeight[504];

      public Tuner()
      {
         Evaluation.PieceValues[(int)PieceType.Pawn] = new(100, 100);
         Evaluation.PieceValues[(int)PieceType.Knight] = new(300, 300);
         Evaluation.PieceValues[(int)PieceType.Bishop] = new(325, 325);
         Evaluation.PieceValues[(int)PieceType.Rook] = new(500, 500);
         Evaluation.PieceValues[(int)PieceType.Queen] = new(900, 900);

         Evaluation.FriendlyKingPawnDistance = new();
         Evaluation.EnemyKingPawnDistance = new();
         Evaluation.KingOpenFile = new();
         Evaluation.KingHalfOpenFile = new();
         Evaluation.RookHalfOpenFile = new();
         Evaluation.RookOpenFile = new();
         Evaluation.BishopPair = new();

         for (int i = 0; i < 384; i++)
         {
            Evaluation.PST[i] = new Score();
         }

         for (int i = 0; i < 9; i++)
         {
            Evaluation.KnightMobility[i] = new Score();
            Evaluation.ConnectedPawn[i] = new();
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

         for (int i = 0; i < 7; i++)
         {
            Evaluation.PassedPawn[i] = new Score();
         }

         for (int i = 0; i < 8; i++)
         {
            Evaluation.DefendedPawn[i] = new();
            Evaluation.IsolatedPawn[i] = new();
         }
      }

      public void Run(int maxEpochs = 10000)
      {
         Console.WriteLine($"Number of epochs set to: {maxEpochs}");

         LoadParameters();
         Console.WriteLine($"Loading positions...");
         Entry[] entries = LoadPositions();

         Console.WriteLine($"\r\nCalculating K value...");
         double K = FindK(entries);
         // double K = 2.5;
         Console.WriteLine($"K value: {K}");

         double avgError = GetAverageError(entries, K);
         double bestError = avgError + Epsilon * 2;
         Console.WriteLine($"Initial average error: {avgError}");

         int learningRateStepRate = 250;
         double learningRateDrop = 1.0; // 1.0 wont ever drop the learning rate
         double learningRate = 1;
         double beta1 = 0.9;
         double beta2 = 0.999;

         ParameterWeight[] momentum = new ParameterWeight[Parameters.Length];
         ParameterWeight[] velocity = new ParameterWeight[Parameters.Length];

         int epoch = 1;

         Console.WriteLine("Tuning...");
         Stopwatch timer = new();
         timer.Start();

         while (Math.Abs(bestError - avgError) >= Epsilon && epoch <= maxEpochs)
         {
            ParameterWeight[] gradients = new ParameterWeight[Parameters.Length];
            ComputeGradient(ref gradients, entries, K);

            for (int parameterIndex = 0; parameterIndex < Parameters.Length; parameterIndex++)
            {
               double grad = -K / 400 * gradients[parameterIndex].Mg / entries.Length;
               momentum[parameterIndex].Mg = beta1 * momentum[parameterIndex].Mg + (1.0 - beta1) * grad;
               velocity[parameterIndex].Mg = beta2 * velocity[parameterIndex].Mg + (1.0 - beta2) * grad * grad;
               Parameters[parameterIndex].Mg -= learningRate * momentum[parameterIndex].Mg / (1e-8 + Math.Sqrt(velocity[parameterIndex].Mg));

               grad = -K / 400 * gradients[parameterIndex].Eg / entries.Length;
               momentum[parameterIndex].Eg = beta1 * momentum[parameterIndex].Eg + (1.0 - beta1) * grad;
               velocity[parameterIndex].Eg = beta2 * velocity[parameterIndex].Eg + (1.0 - beta2) * grad * grad;
               Parameters[parameterIndex].Eg -= learningRate * momentum[parameterIndex].Eg / (1e-8 + Math.Sqrt(velocity[parameterIndex].Eg));
            }

            if (epoch % 100 == 0)
            {
               PrintResults(epoch);
               bestError = avgError;
               avgError = GetAverageError(entries, K);
               Console.WriteLine($"Epoch: {epoch}, EPS: {1000 * (long)epoch / timer.ElapsedMilliseconds}, error: {bestError}, E: {bestError - avgError}, Time: {timer.Elapsed:hh\\:mm\\:ss}. Remaining: {TimeSpan.FromMilliseconds((maxEpochs - epoch) * (timer.ElapsedMilliseconds / epoch)):hh\\:mm\\:ss}");
            }

            if (epoch % learningRateStepRate == 0)
            {
               learningRate /= learningRateDrop;
            }

            epoch++;
         }

         timer.Stop();
         Console.WriteLine("Completed");
         Environment.Exit(100);
      }

      public void Test()
      {
         LoadParameters();
         Console.WriteLine($"Loading positions...");
         Entry[] entries = LoadPositions();
         Board board = new();
         Console.WriteLine($"\r\nRunning evaluation test...");

         foreach (Entry entry in entries) {
            board.SetPosition(entry.Fen);
            var tunerEval = Evaluate(entry);
            var boardEval = Evaluation.Evaluate(board);

            if (board.SideToMove == Color.Black)
            {
               tunerEval *= -1;
            }

            if (Math.Abs(boardEval - tunerEval) > 1.5)
            {
               Console.WriteLine($"Position {entry.Fen} got {boardEval} from the engine evaluation but {tunerEval} from the tuner evaluation");
            }
         }

         Console.WriteLine($"Test complete");
      }

      private void ComputeGradient(ref ParameterWeight[] gradients, Entry[] entries, double K)
      {
         ConcurrentBag<ParameterWeight[]> newGradients = new();

         Parallel.ForEach(
            entries,
            () => new ParameterWeight[Parameters.Length],
            (j, loop, localGradients) =>
            {
               UpdateSingleGradient(j, K, ref localGradients);
               return localGradients;
            },
            newGradients.Add
         );

         foreach (var grad in newGradients)
         {
            for (int n = 0; n < grad.Length; n++)
            {
               gradients[n] += grad[n];
            }
         }
      }

      private void UpdateSingleGradient(Entry entry, double K, ref ParameterWeight[] gradient)
      {
         double sig = Sigmoid(K, Evaluate(entry));
         double res = (entry.Result - sig) * sig * (1.0 - sig);

         double mg_base = res * (entry.Phase / 24);
         double eg_base = res - mg_base;

         foreach (CoefficientEntry coef in entry.Coefficients)
         {
            gradient[coef.Index].Mg += mg_base * coef.Value;
            gradient[coef.Index].Eg += eg_base * coef.Value;
         }
      }

      private double FindK(Entry[] entries)
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

      private double GetAverageError(Entry[] entries, double K)
      {
         double sum = 0;

         Parallel.ForEach(
            entries,
            () => 0.0,
            (j, loop, subtotal) =>
            {
               subtotal += Math.Pow(j.Result - Sigmoid(K, Evaluate(j)), 2);
               return subtotal;
            },
            subtotal => Add(ref sum, subtotal)
         );

         return sum / entries.Length;
      }

      private double Evaluate(Entry entry)
      {
         double midgame = 0;
         double endgame = 0;

         foreach (CoefficientEntry coef in entry.Coefficients)
         {
            midgame += coef.Value * Parameters[coef.Index].Mg;
            endgame += coef.Value * Parameters[coef.Index].Eg;
         }

         return (midgame * entry.Phase + endgame * (24 - entry.Phase)) / 24;
      }

      public void LoadParameters()
      {
         int index = 0;
         AddParameters(Evaluation.PieceValues, ref index);
         AddParameters(Evaluation.PST, ref index);
         AddParameters(Evaluation.KnightMobility, ref index);
         AddParameters(Evaluation.BishopMobility, ref index);
         AddParameters(Evaluation.RookMobility, ref index);
         AddParameters(Evaluation.QueenMobility, ref index);
         AddSingleParameter(Evaluation.RookHalfOpenFile, ref index);
         AddSingleParameter(Evaluation.RookOpenFile, ref index);
         AddSingleParameter(Evaluation.KingOpenFile, ref index);
         AddSingleParameter(Evaluation.KingHalfOpenFile, ref index);
         AddParameters(Evaluation.KingAttackWeights, ref index);
         AddParameters(Evaluation.PawnShield, ref index);
         AddParameters(Evaluation.PassedPawn, ref index);
         AddParameters(Evaluation.DefendedPawn, ref index);
         AddParameters(Evaluation.ConnectedPawn, ref index);
         AddParameters(Evaluation.IsolatedPawn, ref index);
         AddSingleParameter(Evaluation.FriendlyKingPawnDistance, ref index);
         AddSingleParameter(Evaluation.EnemyKingPawnDistance, ref index);
         AddSingleParameter(Evaluation.BishopPair, ref index);
      }

      private void AddSingleParameter(Score value, ref int index)
      {
         Parameters[index++] = new(value.Mg, value.Eg);
      }

      private void AddParameters(Score[] values, ref int index)
      {
         foreach (Score value in values)
         {
            Parameters[index++] = new(value.Mg, value.Eg);
         }
      }

      private Entry[] LoadPositions()
      {
         int lines = 0;
         int totalLines = System.IO.File.ReadLines(PositionsFile).Count();
         Entry[] entries = new Entry[totalLines];
         Board board = new();
         Stopwatch sw = Stopwatch.StartNew();

         using (StreamReader sr = System.IO.File.OpenText(PositionsFile))
         {
            string line = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
               board.SetPosition(line.Split("\"")[0].Trim());

               (Trace trace, double phase) = Evaluate(board);

               entries[lines++] = new(GetCoefficientEntries(GetCoefficients(trace)), phase, GetEntryResult(line), line.Split("\"")[0].Trim());

               Console.Write($"\rPositions loaded: {lines}/{totalLines} {100 * (long)lines / totalLines}% | {sw.Elapsed}");

               //Force garbage collection every 1 million lines. This seems to help with memory issues.
               if (lines % 1000000 == 0)
               {
                  GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                  GC.Collect();
               }
            }
         }

         sw.Stop();

         return entries;
      }

      private static double GetEntryResult(string fen)
      {
         Match match = FENResultRegex().Match(fen);

         if (match.Success)
         {
            return Convert.ToDouble(match.Groups[1].Value);
         }
         else
         {
            throw new Exception("Unable to get fen result");
         }
      }

      private static (Trace trace, double phase) Evaluate(Board board)
      {
         EvalInfo info = InitEval(board);
         Trace trace = new();
         PotentialKingAttacks[] potentialKingAttacks =
         [
            new PotentialKingAttacks(6),
            new PotentialKingAttacks(6),
         ];

         // Material and PST score
         Score score = Material(board, Color.White, trace) - Material(board, Color.Black, trace);

         // Piece evaluation
         score += EvaluatePieces(board, info, potentialKingAttacks, trace);

         if (board.SideToMove == Color.Black)
         {
            score *= -1;
         }

         trace.score += (score.Mg * board.Phase + score.Eg * (24 - board.Phase)) / 24;
         return (trace, board.Phase);
      }

      private static EvalInfo InitEval(Board board)
      {
         // Mobility squares: All squares not attacked by enemy pawns minus own blocked pawns.
         Bitboard occ = board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black];

         Bitboard blackPawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)Color.Black];
         Bitboard whitePawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)Color.White];

         EvalInfo info = new(
            [
               ~PawnAnyAttacks(blackPawns.Value, Color.Black) ^ (occ.Shift(Direction.Up) & whitePawns).Value,
               ~PawnAnyAttacks(whitePawns.Value, Color.White) ^ (occ.Shift(Direction.Down) & blackPawns).Value,
            ],
            [
               KingAttacks[board.GetSquareByPiece(PieceType.King, Color.White)],
               KingAttacks[board.GetSquareByPiece(PieceType.King, Color.Black)],
            ]
         );

         return info;
      }

      private static Score EvaluatePieces(Board board, EvalInfo info, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();

         if ((board.PieceBB[(int)PieceType.Bishop] & board.ColorBB[(int)Color.White]).CountBits() >= 2)
         {
            trace.bishopPair[(int)Color.White]++;
            score += Evaluation.BishopPair;
         }
         if ((board.PieceBB[(int)PieceType.Bishop] & board.ColorBB[(int)Color.Black]).CountBits() >= 2)
         {
            trace.bishopPair[(int)Color.Black]++;
            score -= Evaluation.BishopPair;
         }

         score += EvalPawns(board, info, Color.White, trace) - EvalPawns(board, info, Color.Black, trace);
         score += EvalKnights(board, info, Color.White, potentialKingAttacks, trace) - EvalKnights(board, info, Color.Black, potentialKingAttacks, trace);
         score += EvalBishops(board, info, Color.White, potentialKingAttacks, trace) - EvalBishops(board, info, Color.Black, potentialKingAttacks, trace);
         score += EvalRooks(board, info, Color.White, potentialKingAttacks, trace) - EvalRooks(board, info, Color.Black, potentialKingAttacks, trace);
         score += EvalQueens(board, info, Color.White, potentialKingAttacks, trace) - EvalQueens(board, info, Color.Black, potentialKingAttacks, trace);
         score += EvalKings(board, info, Color.White, potentialKingAttacks, trace) - EvalKings(board, info, Color.Black, potentialKingAttacks, trace);

         return score;
      }

      private static Score EvalPawns(Board board, EvalInfo info, Color color, Trace trace)
      {
         Score score = new();
         Bitboard pawns = board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color];

         score += Evaluation.DefendedPawn[(pawns & PawnAnyAttacks(pawns.Value, color)).CountBits()];
         score += Evaluation.ConnectedPawn[(pawns & pawns.RightShift()).CountBits()];
         trace.defendedPawn[(pawns & PawnAnyAttacks(pawns.Value, color)).CountBits()][(int)color]++;
         trace.connectedPawn[(pawns & pawns.RightShift()).CountBits()][(int)color]++;

         while (pawns)
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & (board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color ^ 1]).Value) == 0)
            {
               score += Evaluation.PassedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1];
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)] * Evaluation.FriendlyKingPawnDistance;
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)] * Evaluation.EnemyKingPawnDistance;

               trace.passedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1][(int)color]++;
               trace.friendlyKingPawnDistance[(int)color] += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)];
               trace.enemyKingPawnDistance[(int)color] += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)];
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & (board.PieceBB[(int)PieceType.Pawn] & board.ColorBB[(int)color]).Value) == 0)
            {
               // Penalty is based on file
               score -= Evaluation.IsolatedPawn[square & 7];
               trace.isolatedPawn[square & 7][(int)color]--;
            }
         }

         return score;
      }

      private static Score EvalKnights(Board board, EvalInfo info, Color color, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();
         Bitboard knightsBB = board.PieceBB[(int)PieceType.Knight] & board.ColorBB[(int)color];

         while (knightsBB)
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += Evaluation.KnightMobility[new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits()];
            trace.knightMobility[new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((KnightAttacks[square] & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;

               potentialKingAttacks[(int)color].Count[(int)PieceType.Knight]++;
               potentialKingAttacks[(int)color].Weight[(int)PieceType.Knight] += new Bitboard(KnightAttacks[square] & info.KingZones[(int)color ^ 1]).CountBits();
            }
         }

         return score;
      }

      private static Score EvalBishops(Board board, EvalInfo info, Color color, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();
         Bitboard bishopBB = board.PieceBB[(int)PieceType.Bishop] & board.ColorBB[(int)color];

         while (bishopBB)
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            ulong moves = GetBishopAttacks(square, (board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black]).Value);
            score += Evaluation.BishopMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];
            trace.bishopMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;

               potentialKingAttacks[(int)color].Count[(int)PieceType.Bishop]++;
               potentialKingAttacks[(int)color].Weight[(int)PieceType.Bishop] += new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
            }
         }

         return score;
      }

      private static Score EvalRooks(Board board, EvalInfo info, Color color, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();
         Bitboard rookBB = board.PieceBB[(int)PieceType.Rook] & board.ColorBB[(int)color];

         while (rookBB)
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            ulong moves = GetRookAttacks(square, (board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black]).Value);
            score += Evaluation.RookMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];
            trace.rookMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((FILE_MASKS[square & 7] & board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value) == 0)
            {
               if ((FILE_MASKS[square & 7] & board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color ^ 1].Value) == 0)
               {
                  score += Evaluation.RookOpenFile;
                  trace.rookOpenFile[(int)color]++;
               }
               else
               {
                  score += Evaluation.RookHalfOpenFile;
                  trace.rookHalfOpenFile[(int)color]++;
               }
            }

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;

               potentialKingAttacks[(int)color].Count[(int)PieceType.Rook]++;
               potentialKingAttacks[(int)color].Weight[(int)PieceType.Rook] += new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
            }
         }

         return score;
      }

      private static Score EvalQueens(Board board, EvalInfo info, Color color, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();
         Bitboard queenBB = board.PieceBB[(int)PieceType.Queen] & board.ColorBB[(int)color];

         while (queenBB)
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            ulong moves = GetQueenAttacks(square, (board.ColorBB[(int)Color.White] | board.ColorBB[(int)Color.Black]).Value);
            score += Evaluation.QueenMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];
            trace.queenMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
               info.KingAttacksCount[(int)color]++;

               potentialKingAttacks[(int)color].Count[(int)PieceType.Queen]++;
               potentialKingAttacks[(int)color].Weight[(int)PieceType.Queen] += new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
            }
         }

         return score;
      }

      private static Score EvalKings(Board board, EvalInfo info, Color color, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();
         Bitboard kingBB = board.PieceBB[(int)PieceType.King] & board.ColorBB[(int)color];

         while (kingBB)
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & SquareBB[kingSq]) != 0)
            {
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x7070000000000 : 0xe0e00000000000) : (ulong)(kingSq % 8 < 3 ? 0x70700 : 0xe0e000);

               Bitboard pawns = new(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & pawnSquares);
               score += Evaluation.PawnShield[Math.Min(pawns.CountBits(), 3)];
               trace.pawnShield[Math.Min(pawns.CountBits(), 3)][(int)color]++;

               if ((board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & FILE_MASKS[kingSq & 7]) == 0)
               {
                  if ((board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color ^ 1].Value & FILE_MASKS[kingSq & 7]) == 0)
                  {
                     score -= Evaluation.KingOpenFile;
                     trace.kingOpenFile[(int)color]--;
                  }
                  else
                  {
                     score -=Evaluation.KingHalfOpenFile;
                     trace.kingHalfOpenFile[(int)color]--;
                  }
               }
            }

            if (info.KingAttacksCount[(int)color ^ 1] >= 2)
            {
               score -= info.KingAttacksWeight[(int)color ^ 1];

               // Update trace with each piece attacks
               for (int pieceType = 0; pieceType < potentialKingAttacks[(int)color ^ 1].Count.Length; pieceType++)
               {
                  if (potentialKingAttacks[(int)color ^ 1].Count[pieceType] > 0)
                  {
                     trace.kingAttackWeights[pieceType][(int)color ^ 1] += potentialKingAttacks[(int)color ^ 1].Weight[pieceType];
                  }
               }
            }
         }

         return score;
      }

      private static Score Material(Board board, Color color, Trace trace)
      {
         Bitboard us = new(board.ColorBB[(int)color].Value);
         Score score = new();

         while (us)
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Squares[square];

            score += Evaluation.PieceValues[(int)piece.Type];
            trace.material[(int)piece.Type][(int)piece.Color]++;

            if (piece.Color == Color.Black)
            {
               square ^= 56;
            }

            score += Evaluation.PST[(int)piece.Type * 64 + square];
            trace.pst[(int)piece.Type * 64 + square][(int)piece.Color]++;
         }

         return score;
      }

      private Dictionary<int, double> GetCoefficients(Trace trace)
      {
         Dictionary<int, double> entryCoefficients = [];

         AddCoefficientsAndEntries(ref entryCoefficients, trace.material, 6);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.pst, 384);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.knightMobility, 9);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.bishopMobility, 14);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.rookMobility, 15);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.queenMobility, 28);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.rookHalfOpenFile);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.rookOpenFile);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.kingOpenFile);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.kingHalfOpenFile);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.kingAttackWeights, 5);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.pawnShield, 4);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.passedPawn, 7);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.defendedPawn, 8);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.connectedPawn, 9);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.isolatedPawn, 8);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.friendlyKingPawnDistance);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.enemyKingPawnDistance);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.bishopPair);

         return entryCoefficients;
      }

      private void AddSingleCoefficientAndEntry(ref Dictionary<int, double> entryCoefficients, double[] trace)
      {
         entryCoefficients.Add(entryCoefficients.Count, trace[0] - trace[1]);
      }

      private void AddCoefficientsAndEntries(ref Dictionary<int, double> entryCoefficients, double[][] trace, int size)
      {
         for (int i = 0; i < size; i++)
         {
            AddSingleCoefficientAndEntry(ref entryCoefficients, trace[i]);
         }
      }

      private List<CoefficientEntry> GetCoefficientEntries(Dictionary<int, double> coefficients)
      {
         List<CoefficientEntry> coefficientEntries = [];

         if (coefficients.Count != Parameters.Length)
         {
            throw new Exception("Counts of coefficients and parameters don't match");
         }

         for (int i = 0; i < coefficients.Count(); i++)
         {
            if (coefficients[i] == 0)
            {
               continue;
            }

            coefficientEntries.Add(new CoefficientEntry(coefficients[i], i));
         }

         return coefficientEntries;
      }

      private double Sigmoid(double factor, double score)
      {
         return 1.0 / (1.0 + Math.Exp(-(factor * score / 400)));
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

      private void PrintResults(int epoch)
      {
         if (!Directory.Exists(ResultsPath))
         {
            Directory.CreateDirectory(ResultsPath);
         }

         using StreamWriter sw = new($"{ResultsPath}/Epoch_{epoch}.txt", true);

         sw.WriteLine($"Tuning results generated on {DateTime.Now.ToString("yyyy-MM-dd,HHmmss")}\r\n");

         int index = 0;
         PrintArray("material", ref index, 6, sw);
         PrintPSTArray("pst", ref index, sw);
         PrintArray("knight mobility", ref index, 9, sw);
         PrintArray("bishop mobility", ref index, 14, sw);
         PrintArray("rook mobility", ref index, 15, sw);
         PrintArray("queen mobility", ref index, 28, sw);
         PrintSingle("rook half open file", ref index, sw);
         PrintSingle("rook open file", ref index, sw);
         PrintSingle("king open file", ref index, sw);
         PrintSingle("king half open file", ref index, sw);
         PrintArray("king attack weights", ref index, 5, sw);
         PrintArray("pawn shield", ref index, 4, sw);
         PrintArray("passed pawn", ref index, 7, sw);
         PrintArray("defended pawn", ref index, 8, sw);
         PrintArray("connected pawn", ref index, 9, sw);
         PrintArray("isolated pawn", ref index, 8, sw);
         PrintSingle("friendly king pawn distance", ref index, sw);
         PrintSingle("enemy king pawn distance", ref index, sw);
         PrintSingle("bishop pair", ref index, sw);
      }

      private void PrintSingle(string name, ref int index, StreamWriter writer)
      {
         writer.WriteLine(name);
         writer.WriteLine($"new({(int)Parameters[index].Mg}, {(int)Parameters[index].Eg});");
         writer.WriteLine("\r\n");
         index++;
      }

      private void PrintArray(string name, ref int index, int count, StreamWriter writer)
      {
         int start = index;
         writer.WriteLine(name);
         for (int i = start; i < start + count; i++)
         {
            index += 1;
            string values = $"new({(int)Parameters[i].Mg}, {(int)Parameters[i].Eg}),";
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
               stringBuilder.Append($"new({(int)Parameters[i].Mg,3}, {(int)Parameters[i].Eg,3}), ");
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

      [GeneratedRegex("\\[([^]]+)\\]")]
      private static partial Regex FENResultRegex();
   }
}
