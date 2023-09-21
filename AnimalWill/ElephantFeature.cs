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
    public static class ElephantFeature
    {
        public static List<int> Multipliers = new List<int>();
        public static int FreeSpinsCount = 0;
        public static int RetriggerSpinsCount = 0;
        public static Dictionary<int, List<Symbol>> ReelsSet = new Dictionary<int, List<Symbol>>();
        public static List<Symbol> ElephantInnerReel = new List<Symbol>();
        public static int TotalWinPerRound = 0;
        public static double ChanceToUseOuterReels = 0;
        public static int CurrentMultiplier = 0;
        public static int CurrentMultiplierNumber = 0;

        public static void StartElephantFreeSpins(out int win)
        {
            int temp = FreeSpinsCount;
            CurrentMultiplierNumber = 0;
            CurrentMultiplier = Multipliers[0];
            TotalWinPerRound = 0;
            for (int i = 0; i < FreeSpinsCount; i++)
            {
                MakeASpin();
            }
            FreeSpinsCountForFeature[Elephant] += FreeSpinsCount;
            FreeSpinsCount = temp;
            win = TotalWinPerRound;
        }

        private static void MakeASpin()
        {
            SlotStats.SumOfElephantMultipliers += CurrentMultiplier;

            GenerateNewMatrix(ReelsSet);
            RealizeInnerSymbolsInElephantFeature();

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
                    for (int i = 0; i < collectorsCount; i++)
                    {
                        collectorsWin += GetCollectorsWin(out int animalsAmount, out Symbol playedSymbol);
                    }
                }
            }

            if (GetSymbolCountFromMatrix(Scatter) == 3)
            {
                FreeSpinsCount += RetriggerSpinsCount;
            }

            payLinesWin = GetPaylinesWins(Matrix);
            totalWinPerSpin = payLinesWin + collectorsWin;
            totalWinPerSpin *= CurrentMultiplier;
            RealizeTotem(totalWinPerSpin);

            AddWinTo(totalWinPerSpin, WinsPerFeatureSpin[Elephant]);
            AddWinXToInterval(totalWinPerSpin / CostToPlay, IntervalFeaturesSpinWinsX[Elephant]);
            TotalWinPerRound += totalWinPerSpin;
        }

        private static void RealizeTotem(int totalWinPerSpin)
        {
            if (totalWinPerSpin > 0)
            {
                CurrentMultiplierNumber = Math.Min(CurrentMultiplierNumber + 1, Multipliers.Count - 1);
                CurrentMultiplier = Multipliers[CurrentMultiplierNumber];
            }
        }

        private static void RealizeInnerSymbolsInElephantFeature()
        {
            Symbol symbolInsteadInner = ElephantInnerReel[Rand.Next(0, ElephantInnerReel.Count)];
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
