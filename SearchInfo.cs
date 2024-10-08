﻿using System.Runtime.CompilerServices;
using static Puffin.Constants;

namespace Puffin
{
   internal class SearchInfo
   {
      Move[][] Pv = new Move[MAX_PLY][];
      int[] PvLength = new int[MAX_PLY];
      public readonly Move[][] KillerMoves = new Move[MAX_PLY][];
      readonly Move[] CounterMoves = new Move[64 * 64];
      public readonly int[] HistoryScores = new int[2 * 64 * 64];
      public int Nodes = 0;
      public int Score = -INFINITY;

      public SearchInfo()
      {
         for (int i = 0; i < MAX_PLY; i++)
         {
            Pv[i] = new Move[MAX_PLY];
            KillerMoves[i] = new Move[2];
         }
      }

      public void ResetAll()
      {
         Array.Clear(HistoryScores, 0, HistoryScores.Length);
         Pv = new Move[MAX_PLY][];
         PvLength = new int[MAX_PLY];
         Nodes = 0;
         Score = -INFINITY;

         for (int i = 0; i < MAX_PLY; i++)
         {
            Pv[i] = new Move[MAX_PLY];
            KillerMoves[i] = new Move[2];
         }
      }

      public void ResetForSearch()
      {
         PvLength = new int[MAX_PLY];
         Nodes = 0;
         Score = -INFINITY;

         for (int i = 0; i < MAX_PLY; i++)
         {
            Pv[i] = new Move[MAX_PLY];
            KillerMoves[i] = new Move[2];
         }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void InitPvLength(int ply)
      {
         PvLength[ply] = ply;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Move GetBestMove()
      {
         return Pv[0][0];
      }

      public string GetPv()
      {
         string pv = "";

         for (int i = 0; i < PvLength[0]; i++)
         {
            pv += Pv[0][i] + " ";
         }

         return pv;
      }

      public void UpdatePV(Move move, int ply)
      {
         Pv[ply][ply] = move;
         for (int i = ply + 1; i < PvLength[ply + 1]; i++)
         {
            Pv[ply][i] = Pv[ply + 1][i];
         }
         PvLength[ply] = PvLength[ply + 1];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void UpdateCountermove(Move previousMove, Move currentMove)
      {
         CounterMoves[previousMove.From * 64 + previousMove.To] = currentMove;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Move GetCountermove(Move previousMove)
      {
         return (previousMove.From < 64 && previousMove.To < 64) ? CounterMoves[previousMove.From * 64 + previousMove.To] : new();
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void UpdateHistory(Color color, Move move, int value)
      {
         HistoryScores[(int)color * 4096 + move.From * 64 + move.To] += value;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int GetHistory(Color color, Move move)
      {
         return HistoryScores[(int)color * 4096 + move.From * 64 + move.To];
      }
   }
}
