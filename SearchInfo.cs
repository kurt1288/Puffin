﻿using System.Runtime.CompilerServices;

namespace Skookum
{
   internal class SearchInfo
   {
      readonly Move[][] Pv;
      readonly int[] PvLength;
      public int Nodes;

      public SearchInfo()
      {
         Pv = new Move[Constants.MAX_PLY][];
         PvLength = new int[Constants.MAX_PLY];
         Nodes = 0;

         for (int i = 0; i < Constants.MAX_PLY; i++)
         {
            Pv[i] = new Move[Constants.MAX_PLY];
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
   }
}
