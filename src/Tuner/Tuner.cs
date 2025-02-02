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
using System.Text;
using System.Text.RegularExpressions;
using static Puffin.Constants;
using static Puffin.Attacks.Attacks;
using System.Reflection;
using Puffin.Evaluation;

namespace Puffin.Tuner
{
   internal partial class Tuner
   {
      const double Epsilon = 1e-7;
      const string PositionsFile = @"./datagen.epd";
      string ResultsPath = @$"./Tune_{DateTime.Now:yyyy-MM-dd,HHmmss}";

      private readonly Dictionary<int, double> CoefficientPool = [];
      private readonly List<CoefficientEntry> EntryPool = [];

      private class Trace
      {
         public readonly Dictionary<string, double[][]> evaluationTerms = [];
         public double score = 0;

         public Trace()
         {
            var fields = typeof(EvalTerms).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.GetCustomAttribute<EvalAttribute>() != null);

            foreach (var field in fields)
            {
               var attribute = field.GetCustomAttribute<EvalAttribute>();
               var length = attribute.Length;
               var termArray = new double[length][];

               for (int i = 0; i < length; i++)
               {
                  termArray[i] = new double[2];
               }

               evaluationTerms[field.Name] = termArray;
            }
         }

         public void IncrementTrace(string termName, int index, Color color)
         {
            if (!evaluationTerms.TryGetValue(termName, out var term))
            {
               throw new ArgumentException($"Evaluation term '{termName}' not found.");
            }

            if (index < 0 || index >= term.Length)
            {
               throw new ArgumentOutOfRangeException(nameof(index),
                   $"Index {index} is out of range for term '{termName}'.");
            }

            term[index][(int)color]++;
         }

         public void DecrementTrace(string termName, int index, Color color)
         {
            if (!evaluationTerms.TryGetValue(termName, out var term))
            {
               throw new ArgumentException($"Evaluation term '{termName}' not found.");
            }

            if (index < 0 || index >= term.Length)
            {
               throw new ArgumentOutOfRangeException(nameof(index),
                   $"Index {index} is out of range for term '{termName}'.");
            }

            term[index][(int)color]--;
         }

         public void AddTrace(string termName, int index, Color color, int value)
         {
            if (!evaluationTerms.TryGetValue(termName, out var term))
            {
               throw new ArgumentException($"Evaluation term '{termName}' not found.");
            }

            if (index < 0 || index >= term.Length)
            {
               throw new ArgumentOutOfRangeException(nameof(index),
                   $"Index {index} is out of range for term '{termName}'.");
            }

            term[index][(int)color] += value;
         }

         public double GetTraceValue(string termName, int index, Color color)
         {
            if (!evaluationTerms.TryGetValue(termName, out var term))
            {
               throw new ArgumentException($"Evaluation term '{termName}' not found.");
            }

            return term[index][(int)color];
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

      private struct Entry
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

      private readonly ParameterWeight[] Parameters;
      private readonly List<(string Name, string DisplayName, Score[] Values)> parameterMetadata = [];

      public Tuner()
      {
         //EvalTerms.PieceValues[(int)PieceType.Pawn] = new(100, 100);
         //EvalTerms.PieceValues[(int)PieceType.Knight] = new(300, 300);
         //EvalTerms.PieceValues[(int)PieceType.Bishop] = new(325, 325);
         //EvalTerms.PieceValues[(int)PieceType.Rook] = new(500, 500);
         //EvalTerms.PieceValues[(int)PieceType.Queen] = new(900, 900);

         int length = 0;
         var fields = typeof(EvalTerms).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.GetCustomAttribute<EvalAttribute>() != null);

         foreach (var field in fields)
         {
            var attribute = field.GetCustomAttribute<EvalAttribute>();
            Score[] values;

            if (field.FieldType.IsArray)
            {
               // Handle array fields
               values = (Score[])field.GetValue(null);

               // Resets all values
               for (int i = 0; i < attribute.Length; i++)
               {
                  values[i] = new Score();
               }
            }
            else
            {
               // Handle single Score fields
               values = [(Score)field.GetValue(null)];
               field.SetValue(null, new Score());
            }

            parameterMetadata.Add((field.Name, attribute.DisplayName, values));
            length += attribute.Length;
         }

         Parameters = new ParameterWeight[length];
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
            var boardEval = Evaluation.Evaluation.Evaluate(board);

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
         foreach (var param in parameterMetadata)
         {
            AddParameters(param.Values, ref index);
         }
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
               CoefficientPool.Clear();
               EntryPool.Clear();

               board.SetPosition(line.Split("\"")[0].Trim());

               (Trace trace, double phase) = Evaluate(board);

               GetCoefficients(trace);
               GetCoefficientEntries();

               entries[lines++] = new([.. EntryPool], phase, GetEntryResult(line), line.Split("\"")[0].Trim());

               Console.Write($"\rPositions loaded: {lines}/{totalLines} {100 * (long)lines / totalLines}% | {sw.Elapsed}");

               //Force garbage collection every 1 million lines. This seems to help with memory issues.
               //if (lines % 1000000 == 0)
               //{
               //   GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
               //   GC.Collect();
               //}
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
         Bitboard occ = board.ColorBoard(Color.Both);
         Bitboard blackPawns = board.ColorPieceBB(Color.Black, PieceType.Pawn);
         Bitboard whitePawns = board.ColorPieceBB(Color.White, PieceType.Pawn);

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

         if (board.ColorPieceBB(Color.White, PieceType.Bishop).CountBits() >= 2)
         {
            trace.IncrementTrace(nameof(EvalTerms.BishopPair), 0, Color.White);
            score += EvalTerms.BishopPair;
         }
         if (board.ColorPieceBB(Color.Black, PieceType.Bishop).CountBits() >= 2)
         {
            trace.IncrementTrace(nameof(EvalTerms.BishopPair), 0, Color.Black);
            score -= EvalTerms.BishopPair;
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
         Bitboard pawns = board.ColorPieceBB(color, PieceType.Pawn);

         score += EvalTerms.DefendedPawn[(pawns & PawnAnyAttacks(pawns.Value, color)).CountBits()];
         score += EvalTerms.ConnectedPawn[(pawns & pawns.RightShift()).CountBits()];
         trace.IncrementTrace(nameof(EvalTerms.DefendedPawn), (pawns & PawnAnyAttacks(pawns.Value, color)).CountBits(), color);
         trace.IncrementTrace(nameof(EvalTerms.ConnectedPawn), (pawns & pawns.RightShift()).CountBits(), color);

         // Enemy non-pawn pieces that can be attacked with a pawn push
         ulong pawnShift = (pawns.Shift(color == Color.White ? Direction.Down : Direction.Up) & ~board.ColorBoard(Color.Both).Value).Value;
         ulong enemyPieces = (board.ColorBoard(color ^ (Color)1) ^ board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn)).Value;
         score += EvalTerms.PawnPushThreats * new Bitboard(PawnAnyAttacks(pawnShift, color) & enemyPieces).CountBits();
         trace.AddTrace(nameof(EvalTerms.PawnPushThreats), 0, color, new Bitboard(PawnAnyAttacks(pawnShift, color) & enemyPieces).CountBits());

         // Enemy non-pawn pieces that are attacked
         score += EvalTerms.PawnAttacks * new Bitboard(PawnAnyAttacks(pawns.Value, color) & enemyPieces).CountBits();
         trace.AddTrace(nameof(EvalTerms.PawnAttacks), 0, color, new Bitboard(PawnAnyAttacks(pawns.Value, color) & enemyPieces).CountBits());

         while (pawns)
         {
            int square = pawns.GetLSB();
            pawns.ClearLSB();
            int rank = (color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1;

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value) == 0)
            {
               score += EvalTerms.PassedPawn[rank];
               trace.IncrementTrace(nameof(EvalTerms.PassedPawn), rank, color);

               if (rank < 4)
               {
                  continue;
               }

               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)] * EvalTerms.FriendlyKingPawnDistance;
               score += TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)] * EvalTerms.EnemyKingPawnDistance;

               trace.AddTrace(nameof(EvalTerms.FriendlyKingPawnDistance), 0, color, TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color)]);
               trace.AddTrace(nameof(EvalTerms.EnemyKingPawnDistance), 0, color, TaxiDistance[square][board.GetSquareByPiece(PieceType.King, color ^ (Color)1)]);

               // Free to advance (no enemy non-pawn pieces ahead)
               if ((ForwardMask[(int)color][square] & board.ColorBoard(color ^ (Color)1).Value) == 0)
               {
                  score += EvalTerms.FreeAdvancePawn;
                  trace.IncrementTrace(nameof(EvalTerms.FreeAdvancePawn), 0, color);
               }
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & board.ColorPieceBB(color, PieceType.Pawn).Value) == 0)
            {
               // Penalty is based on file
               score -= EvalTerms.IsolatedPawn[square & 7];
               trace.DecrementTrace(nameof(EvalTerms.IsolatedPawn), square & 7, color);
            }
         }

         return score;
      }

      private static Score EvalKnights(Board board, EvalInfo info, Color color, PotentialKingAttacks[] potentialKingAttacks, Trace trace)
      {
         Score score = new();
         Bitboard knightsBB = board.ColorPieceBB(color, PieceType.Knight);

         while (knightsBB)
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            score += EvalTerms.KnightMobility[new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits()];
            trace.IncrementTrace(nameof(EvalTerms.KnightMobility), new Bitboard(KnightAttacks[square] & info.MobilitySquares[(int)color]).CountBits(), color);

            if ((KnightAttacks[square] & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & info.KingZones[(int)color ^ 1]).CountBits();
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
         Bitboard bishopBB = board.ColorPieceBB(color, PieceType.Bishop);

         while (bishopBB)
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            ulong moves = GetBishopAttacks(square, board.ColorBoard(Color.Both).Value);
            score += EvalTerms.BishopMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];
            trace.IncrementTrace(nameof(EvalTerms.BishopMobility), new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits(), color);

            bool isLSqBishop = ((WHITE_SQUARES >> square) & 1) == 1;
            Bitboard samePawns = board.ColorPieceBB(color, PieceType.Pawn);
            samePawns &= isLSqBishop ? WHITE_SQUARES : DARK_SQUARES;

            score -= EvalTerms.SameColorBishopPawns[samePawns.CountBits()];
            trace.DecrementTrace(nameof(EvalTerms.SameColorBishopPawns), samePawns.CountBits(), color);

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
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
         Bitboard rookBB = board.ColorPieceBB(color, PieceType.Rook);

         while (rookBB)
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            ulong moves = GetRookAttacks(square, board.ColorBoard(Color.Both).Value);
            score += EvalTerms.RookMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];
            trace.IncrementTrace(nameof(EvalTerms.RookMobility), new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits(), color);

            if ((FILE_MASKS[square & 7] & board.ColorPieceBB(color, PieceType.Pawn).Value) == 0)
            {
               if ((FILE_MASKS[square & 7] & board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value) == 0)
               {
                  score += EvalTerms.RookOpenFile;
                  trace.IncrementTrace(nameof(EvalTerms.RookOpenFile), 0, color);
               }
               else
               {
                  score += EvalTerms.RookHalfOpenFile;
                  trace.IncrementTrace(nameof(EvalTerms.RookHalfOpenFile), 0, color);
               }
            }

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
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
         Bitboard queenBB = board.ColorPieceBB(color, PieceType.Queen);

         while (queenBB)
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            ulong moves = GetQueenAttacks(square, board.ColorBoard(Color.Both).Value);
            score += EvalTerms.QueenMobility[new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits()];
            trace.IncrementTrace(nameof(EvalTerms.QueenMobility), new Bitboard(moves & info.MobilitySquares[(int)color]).CountBits(), color);

            if ((moves & info.KingZones[(int)color ^ 1]) != 0)
            {
               info.KingAttacksWeight[(int)color] += EvalTerms.KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & info.KingZones[(int)color ^ 1]).CountBits();
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
         Bitboard kingBB = board.ColorPieceBB(color, PieceType.King);

         while (kingBB)
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & SquareBB[kingSq]) != 0)
            {
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x7070000000000 : 0xe0e00000000000) : (ulong)(kingSq % 8 < 3 ? 0x70700 : 0xe0e000);

               Bitboard pawns = board.ColorPieceBB(color, PieceType.Pawn) & pawnSquares;
               score += EvalTerms.PawnShield[Math.Min(pawns.CountBits(), 3)];
               trace.IncrementTrace(nameof(EvalTerms.PawnShield), Math.Min(pawns.CountBits(), 3), color);

               if ((board.ColorPieceBB(color, PieceType.Pawn).Value & FILE_MASKS[kingSq & 7]) == 0)
               {
                  if ((board.ColorPieceBB(color ^ (Color)1, PieceType.Pawn).Value & FILE_MASKS[kingSq & 7]) == 0)
                  {
                     score -= EvalTerms.KingOpenFile;
                     trace.DecrementTrace(nameof(EvalTerms.KingOpenFile), 0, color);
                  }
                  else
                  {
                     score -= EvalTerms.KingHalfOpenFile;
                     trace.DecrementTrace(nameof(EvalTerms.KingHalfOpenFile), 0, color);
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
                     trace.AddTrace(nameof(EvalTerms.KingAttackWeights), pieceType, color ^ (Color)1, potentialKingAttacks[(int)color ^ 1].Weight[pieceType]);
                  }
               }
            }
         }

         return score;
      }

      private static Score Material(Board board, Color color, Trace trace)
      {
         Bitboard us = board.ColorBoard(color);
         Score score = new();

         while (us)
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Squares[square];

            score += EvalTerms.PieceValues[(int)piece.Type];
            trace.IncrementTrace(nameof(EvalTerms.PieceValues), (int)piece.Type, piece.Color);

            if (piece.Color == Color.Black)
            {
               square ^= 56;
            }

            score += EvalTerms.PST[(int)piece.Type * 64 + square];
            trace.IncrementTrace(nameof(EvalTerms.PST), (int)piece.Type * 64 + square, piece.Color);
         }

         return score;
      }

      private void GetCoefficients(Trace trace)
      {
         foreach (var term in trace.evaluationTerms)
         {
            AddCoefficientsAndEntries(term.Value, term.Value.Length);
         }
      }

      private void AddSingleCoefficientAndEntry(double[] trace)
      {
         CoefficientPool.Add(CoefficientPool.Count, trace[0] - trace[1]);
      }

      private void AddCoefficientsAndEntries( double[][] trace, int size)
      {
         for (int i = 0; i < size; i++)
         {
            AddSingleCoefficientAndEntry(trace[i]);
         }
      }

      private List<CoefficientEntry> GetCoefficientEntries()
      {
         if (CoefficientPool.Count != Parameters.Length)
         {
            throw new Exception("Counts of coefficients and parameters don't match");
         }

         for (int i = 0; i < CoefficientPool.Count; i++)
         {
            if (CoefficientPool[i] == 0)
            {
               continue;
            }

            EntryPool.Add(new CoefficientEntry(CoefficientPool[i], i));
         }

         return EntryPool;
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

         var sb = new StringBuilder();
         int baseIndex = 0;

         foreach (var param in parameterMetadata)
         {
            var field = typeof(EvalTerms).GetField(param.Name);
            var attribute = field?.GetCustomAttribute<EvalAttribute>();

            //sb.AppendLine($"// {param.DisplayName}");

            if (attribute.IsArray)
            {
               sb.AppendLine($"[Eval(\"{param.DisplayName}\", {param.Values.Length})]");
               sb.AppendLine($"public static readonly Score[] {param.Name} =");
               sb.AppendLine("[");
            }

            if (attribute?.IsPst == true)
            {
               for (int piece = 0; piece < 6; ++piece)
               {
                  string pieceName = piece switch
                  {
                     0 => "Pawn",
                     1 => "Knight",
                     2 => "Bishop",
                     3 => "Rook",
                     4 => "Queen",
                     5 => "King",
                     _ => throw new ArgumentException($"Unknown piece type index: {piece}")
                  };

                  sb.AppendLine($"    // {pieceName}");

                  for (int square = 0; square < 64; ++square)
                  {
                     sb.Append("    ");
                     int paramIndex = baseIndex + (piece * 64) + square;
                     sb.Append($"new({(int)Parameters[paramIndex].Mg,3}, {(int)Parameters[paramIndex].Eg,3}), ");

                     if (square % 8 == 7)
                     {
                        sb.AppendLine();
                     }
                  }

                  sb.AppendLine();
               }

               if (attribute.IsArray)
               {
                  sb.AppendLine("];");
               }

               sb.AppendLine();
               baseIndex += 6 * 64;
            }
            else
            {
               for (int i = 0; i < param.Values.Length; i++)
               {
                  var paramWeight = Parameters[baseIndex++];

                  if (attribute.IsArray)
                  {
                     sb.AppendLine($"    new({(int)paramWeight.Mg,3}, {(int)paramWeight.Eg,3}),");
                  }
                  else
                  {
                     sb.AppendLine($"[Eval(\"{param.DisplayName}\")]");
                     sb.AppendLine($"public static Score {param.Name} = new({(int)paramWeight.Mg,3}, {(int)paramWeight.Eg,3});");
                  }
               }

               if (attribute.IsArray)
               {
                  sb.AppendLine("];");
               }

               sb.AppendLine();
            }
         }

         sb.ToString();
         sw.Write(sb);
      }

      [GeneratedRegex("\\[([^]]+)\\]")]
      private static partial Regex FENResultRegex();
   }
}
