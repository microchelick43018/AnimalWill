using System;
using System.Collections.Generic;
using static AnimalWill.Symbol;
using static AnimalWill.SlotInfo;
using static AnimalWill.SlotSimulation;
using static AnimalWill.SlotStats;
using static AnimalWill.PaylinesCounter;
using System.Linq;
using OfficeOpenXml;
using System.IO;
using System.Reflection;


namespace AnimalWill
{
    public static class RhinoFeature
    {
        public static Dictionary<int, List<Symbol>> ReelsSet = new Dictionary<int, List<Symbol>>();
        public static List<Symbol> RhinoInnerReel = new List<Symbol>();
        public static int RhinoSpinsCount = 0;
        public static int TotalWinPerRound = 0;
        public static double ChanceToUseOuterReels = 0;
        public static int RetriggerSpinsCount = 0;
        private static Symbol _selectedSymbol;

        public static void StartRhinoFreeSpins(out int win)
        {
            int temp = RhinoSpinsCount;
            TotalWinPerRound = 0;
            for (int i = 0; i < RhinoSpinsCount; i++)
            {
                MakeASpin();
            }
            FreeSpinsCountForFeature[Rhino] += RhinoSpinsCount;
            RhinoSpinsCount = temp;
            win = TotalWinPerRound;
        }

        private static void MakeASpin()
        {
            GenerateNewMatrix(ReelsSet);
            RealizeInnerSymbolsInRhinoFeature();

            int totalWinPerSpin = 0;
            int payLinesWin = 0;
            int collectorsWin = 0;

            if (ChanceToUseOuterReels >= Rand.NextDouble())
            {
                GenerateNewOuterMatrix();
                RealizeOuterSymbols();
                int collectorsCount = GetSymbolCountFromMatrix(Collector);
                if (collectorsCount != 0)
                {
                    TurnCollectorsIntoWilds();
                    for (int i = 0; i < collectorsCount * 2; i++)
                    {
                        collectorsWin += GetCollectorsWin(out int animalsAmount, out Symbol playedSymbol);
                    }
                }
            }

            if (GetSymbolCountFromMatrix(Scatter) == 3)
            {
                RhinoSpinsCount += RetriggerSpinsCount;
            }

            payLinesWin = GetPaylinesWins(Matrix);
            totalWinPerSpin = payLinesWin + collectorsWin;
            AddWinTo(totalWinPerSpin, WinsPerFeatureSpin[Rhino]);
            AddWinXToInterval(totalWinPerSpin / CostToPlay, IntervalFeaturesSpinWinsX[Rhino]);
            TotalWinPerRound += totalWinPerSpin;
        }

        private static void RealizeInnerSymbolsInRhinoFeature()
        {
            Symbol symbolInsteadInner = RhinoInnerReel[Rand.Next(0, RhinoInnerReel.Count)];
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (Matrix[i, j] == Inner)
                    {
                        Matrix[i, j] = symbolInsteadInner;
                    }
                }
            }
        }
    }
}
