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
         public double[][] kingAttackWeights = new double[5][];
         public double[][] pawnShield = new double[4][];
         public double[][] passedPawn = new double[7][];
         public double[][] defendedPawn = new double[8][];
         public double[][] connectedPawn = new double[9][];
         public double[][] isolatedPawn = new double[8][];
         public double[] friendlyKingPawnDistance = new double[2];
         public double[] enemyKingPawnDistance = new double[2];
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
         public readonly short Value;
         public readonly int Index;

         public CoefficientEntry(short value, int index)
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

         public Entry(List<CoefficientEntry> coefficients, double phase, double result)
         {
            Coefficients = coefficients;
            Phase = phase;
            Result = result;
         }
      }

      // readonly Engine Engine;
      private ParameterWeight[] Parameters = new ParameterWeight[499];

      public Tuner()
      {
         Evaluation.PieceValues[(int)PieceType.Pawn] = new(100, 100);
         Evaluation.PieceValues[(int)PieceType.Knight] = new(300, 300);
         Evaluation.PieceValues[(int)PieceType.Bishop] = new(325, 325);
         Evaluation.PieceValues[(int)PieceType.Rook] = new(500, 500);
         Evaluation.PieceValues[(int)PieceType.Queen] = new(900, 900);

         Evaluation.FriendlyKingPawnDistance = new();
         Evaluation.EnemyKingPawnDistance = new();

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
         AddParameters(Evaluation.KingAttackWeights, ref index);
         AddParameters(Evaluation.PawnShield, ref index);
         AddParameters(Evaluation.PassedPawn, ref index);
         AddParameters(Evaluation.DefendedPawn, ref index);
         AddParameters(Evaluation.ConnectedPawn, ref index);
         AddParameters(Evaluation.IsolatedPawn, ref index);
         AddSingleParameter(Evaluation.FriendlyKingPawnDistance, ref index);
         AddSingleParameter(Evaluation.EnemyKingPawnDistance, ref index);
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

               (Trace trace, double phase) = GetEval(board);

               entries[lines++] = new(GetCoefficients(trace), phase, GetEntryResult(line));

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

      private static (Trace trace, double phase) GetEval(Board board)
      {
         Trace trace = new();

         Score score = Material(board, Color.White, ref trace) - Material(board, Color.Black, ref trace);
         Score[] kingAttacks = [new(), new()];
         int[] kingAttacksCount = [0, 0];
         ulong[] mobilitySquares = [0, 0];
         int[] kingSquares = [
            board.GetSquareByPiece(PieceType.King, Color.White),
            board.GetSquareByPiece(PieceType.King, Color.Black)
         ];
         ulong[] kingZones = [
            KingAttacks[kingSquares[(int)Color.White]],
            KingAttacks[kingSquares[(int)Color.Black]]
         ];
         ulong occupied = board.ColorBB[(int)Color.White].Value | board.ColorBB[(int)Color.Black].Value;

         Pawns(board, kingSquares, ref mobilitySquares, ref score, ref trace);
         Knights(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, ref trace);
         Bishops(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, occupied, ref trace);
         Rooks(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, occupied, ref trace);
         Queens(board, ref score, ref mobilitySquares, kingZones, ref kingAttacks, ref kingAttacksCount, occupied, ref trace);
         Kings(board, ref score, ref kingAttacks, ref kingAttacksCount, ref trace);

         if (board.SideToMove == Color.Black)
         {
            score *= -1;
         }

         trace.score = (score.Mg * board.Phase + score.Eg * (24 - board.Phase)) / 24;

         return (trace, board.Phase);
      }

      private static Score Material(Board board, Color color, ref Trace trace)
      {
         Bitboard us = new(board.ColorBB[(int)color].Value);
         Score score = new();

         while (us)
         {
            int square = us.GetLSB();
            us.ClearLSB();
            Piece piece = board.Mailbox[square];

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

      private static void Knights(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ref Trace trace)
      {
         Bitboard knightsBB = board.PieceBB[(int)PieceType.Knight];

         while (knightsBB)
         {
            int square = knightsBB.GetLSB();
            knightsBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            // * (1 - 2 * (int)color) evaluates to 1 when color is white and to -1 when color is black (so that black score is subtracted)
            score += Evaluation.KnightMobility[new Bitboard(KnightAttacks[square] & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);
            trace.knightMobility[new Bitboard(KnightAttacks[square] & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((KnightAttacks[square] & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Knight] * new Bitboard(KnightAttacks[square] & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
               trace.kingAttackWeights[(int)PieceType.Knight][(int)color] += new Bitboard(KnightAttacks[square] & kingZones[(int)color ^ 1]).CountBits();
            }
         }
      }

      private static void Bishops(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied, ref Trace trace)
      {
         Bitboard bishopBB = board.PieceBB[(int)PieceType.Bishop];

         while (bishopBB)
         {
            int square = bishopBB.GetLSB();
            bishopBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = GetBishopAttacks(square, occupied);
            score += Evaluation.BishopMobility[new Bitboard(moves & ~(board.ColorBB[(int)color].Value & board.PieceBB[(int)PieceType.Pawn].Value) & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);
            trace.bishopMobility[new Bitboard(moves & ~(board.ColorBB[(int)color].Value & board.PieceBB[(int)PieceType.Pawn].Value) & mobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((moves & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Bishop] * new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
               trace.kingAttackWeights[(int)PieceType.Bishop][(int)color] += new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
            }
         }
      }

      private static void Rooks(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied, ref Trace trace)
      {
         Bitboard rookBB = board.PieceBB[(int)PieceType.Rook];

         while (rookBB)
         {
            int square = rookBB.GetLSB();
            rookBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = GetRookAttacks(square, occupied);
            score += Evaluation.RookMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);
            trace.rookMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((moves & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Rook] * new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
               trace.kingAttackWeights[(int)PieceType.Rook][(int)color] += new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
            }
         }
      }
      private static void Queens(Board board, ref Score score, ref ulong[] mobilitySquares, ulong[] kingZones, ref Score[] kingAttacks, ref int[] kingAttacksCount, ulong occupied, ref Trace trace)
      {
         Bitboard queenBB = board.PieceBB[(int)PieceType.Queen];

         while (queenBB)
         {
            int square = queenBB.GetLSB();
            queenBB.ClearLSB();
            Color color = board.Mailbox[square].Color;
            ulong moves = GetQueenAttacks(square, occupied);
            score += Evaluation.QueenMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()] * (1 - 2 * (int)color);
            trace.queenMobility[new Bitboard(moves & ~board.ColorBB[(int)color].Value & mobilitySquares[(int)color]).CountBits()][(int)color]++;

            if ((moves & kingZones[(int)color ^ 1]) != 0)
            {
               kingAttacks[(int)color] += Evaluation.KingAttackWeights[(int)PieceType.Queen] * new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
               kingAttacksCount[(int)color]++;
               trace.kingAttackWeights[(int)PieceType.Queen][(int)color] += new Bitboard(moves & kingZones[(int)color ^ 1]).CountBits();
            }
         }
      }

      private static void Kings(Board board, ref Score score, ref Score[] kingAttacks, ref int[] kingAttacksCount, ref Trace trace)
      {
         Bitboard kingBB = board.PieceBB[(int)PieceType.King];

         while (kingBB)
         {
            int kingSq = kingBB.GetLSB();
            kingBB.ClearLSB();
            Color color = board.Mailbox[kingSq].Color;
            ulong kingSquares = color == Color.White ? 0xD7C3000000000000 : 0xC3D7;

            if ((kingSquares & SquareBB[kingSq]) != 0)
            {
               ulong pawnSquares = color == Color.White ? (ulong)(kingSq % 8 < 3 ? 0x007000000000000 : 0x000E0000000000000) : (ulong)(kingSq % 8 < 3 ? 0x700 : 0xE000);

               Bitboard pawns = new(board.PieceBB[(int)PieceType.Pawn].Value & board.ColorBB[(int)color].Value & pawnSquares);
               score += Evaluation.PawnShield[Math.Min(pawns.CountBits(), 3)] * (1 - 2 * (int)color);
               trace.pawnShield[Math.Min(pawns.CountBits(), 3)][(int)color]++;
            }

            if (kingAttacksCount[(int)color ^ 1] >= 2)
            {
               score -= kingAttacks[(int)color ^ 1] * (1 - 2 * (int)color);
            }
         }
      }

      private static void Pawns(Board board, int[] kingSquares, ref ulong[] mobilitySquares, ref Score score, ref Trace trace)
      {
         Bitboard pawns = board.PieceBB[(int)PieceType.Pawn];
         Bitboard[] colorPawns = [pawns & board.ColorBB[(int)Color.White], pawns & board.ColorBB[(int)Color.Black]];
         int[] defended = [
            (colorPawns[(int)Color.White] & WhitePawnAttacks(colorPawns[(int)Color.White].Value)).CountBits(),
            (colorPawns[(int)Color.Black] & BlackPawnAttacks(colorPawns[(int)Color.Black].Value)).CountBits(),
         ];
         int[] connected = [0, 0];

         while (pawns)
         {
            int square = pawns.GetLSB();
            Color color = board.Mailbox[square].Color;
            pawns.ClearLSB();
            mobilitySquares[(int)color ^ 1] |= PawnAttacks[(int)color][square];

            // Passed pawns
            if ((PassedPawnMasks[(int)color][square] & colorPawns[(int)color ^ 1].Value) == 0)
            {
               score += Evaluation.PassedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1] * (1 - 2 * (int)color);
               trace.passedPawn[(color == Color.White ? 8 - (square >> 3) : 1 + (square >> 3)) - 1][(int)color]++;
               score += TaxiDistance[square][kingSquares[(int)color]] * Evaluation.FriendlyKingPawnDistance * (1 - 2 * (int)color);
               score += TaxiDistance[square][kingSquares[(int)color ^ 1]] * Evaluation.EnemyKingPawnDistance * (1 - 2 * (int)color);
               trace.friendlyKingPawnDistance[(int)color] += TaxiDistance[square][kingSquares[(int)color]];
               trace.enemyKingPawnDistance[(int)color] += TaxiDistance[square][kingSquares[(int)color ^ 1]];
            }

            // Connected pawn
            if ((((SquareBB[square] & ~FILE_MASKS[(int)File.H]) << 1) & colorPawns[(int)color].Value) != 0)
            {
               connected[(int)color]++;
            }

            // Isolated pawn
            if ((IsolatedPawnMasks[square & 7] & colorPawns[(int)color].Value) == 0)
            {
               // Penalty is based on file
               score -= Evaluation.IsolatedPawn[square & 7] * (1 - 2 * (int)color);
               trace.isolatedPawn[square & 7][(int)color]--;
            }
         }

         score += Evaluation.DefendedPawn[defended[(int)Color.White]] - Evaluation.DefendedPawn[defended[(int)Color.Black]];
         trace.defendedPawn[defended[(int)Color.White]][(int)Color.White]++;
         trace.defendedPawn[defended[(int)Color.Black]][(int)Color.Black]++;
         score += Evaluation.ConnectedPawn[connected[(int)Color.White]] - Evaluation.ConnectedPawn[connected[(int)Color.Black]];
         trace.connectedPawn[connected[(int)Color.White]][(int)Color.White]++;
         trace.connectedPawn[connected[(int)Color.Black]][(int)Color.Black]++;
         mobilitySquares[(int)Color.White] = ~mobilitySquares[(int)Color.White];
         mobilitySquares[(int)Color.Black] = ~mobilitySquares[(int)Color.Black];
      }

      private List<CoefficientEntry> GetCoefficients(Trace trace)
      {
         List<CoefficientEntry> entryCoefficients = new();
         int currentIndex = 0;

         AddCoefficientsAndEntries(ref entryCoefficients, trace.material, 6, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.pst, 384, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.knightMobility, 9, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.bishopMobility, 14, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.rookMobility, 15, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.queenMobility, 28, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.kingAttackWeights, 5, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.pawnShield, 4, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.passedPawn, 7, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.defendedPawn, 8, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.connectedPawn, 9, ref currentIndex);
         AddCoefficientsAndEntries(ref entryCoefficients, trace.isolatedPawn, 8, ref currentIndex);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.friendlyKingPawnDistance, ref currentIndex);
         AddSingleCoefficientAndEntry(ref entryCoefficients, trace.enemyKingPawnDistance, ref currentIndex);

         return entryCoefficients;
      }

      private void AddSingleCoefficientAndEntry(ref List<CoefficientEntry> entryCoefficients, double[] trace, ref int currentIndex)
      {
         if ((short)(trace[0] - trace[1]) != 0)
         {
            entryCoefficients.Add(new CoefficientEntry((short)(trace[0] - trace[1]), currentIndex));
         }
         currentIndex++;
      }

      private void AddCoefficientsAndEntries(ref List<CoefficientEntry> entryCoefficients, double[][] trace, int size, ref int currentIndex)
      {
         foreach (var item in trace)
         {
            if ((short)(item[0] - item[1]) != 0)
            {
               entryCoefficients.Add(new CoefficientEntry((short)(item[0] - item[1]), currentIndex));
            }
            currentIndex++;
         }
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
         PrintArray("king attack weights", ref index, 5, sw);
         PrintArray("pawn shield", ref index, 4, sw);
         PrintArray("passed pawn", ref index, 7, sw);
         PrintArray("defended pawn", ref index, 8, sw);
         PrintArray("connected pawn", ref index, 9, sw);
         PrintArray("isolated pawn", ref index, 8, sw);
         PrintSingle("friendly king pawn distance", ref index, sw);
         PrintSingle("enemy king pawn distance", ref index, sw);
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
