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
    public static class LionFeature
    {
        public static Dictionary<int, List<Symbol>> LionFeatureReels = new Dictionary<int, List<Symbol>>();
        public static List<Symbol> LionFeatureInnerReels = new List<Symbol>();
        public static Dictionary<Symbol, int> SymbolsToWildsWeights = new Dictionary<Symbol, int>();
        public static int LionSpinsCount;
        public static int RetriggerSpins = 0;
        public static int TotalWinPerRound = 0;
        public static double ChanceToChooseSelectedAsInner;

        private static Symbol _selectedSymbol;

        public static void StartLionFreeSpins(out int win)
        {
            TotalWinPerRound = 0;
            int temp = LionSpinsCount;
            for (int i = 0; i < LionSpinsCount; i++)
            {
                MakeASpin();
            }
            FreeSpinsCountForFeature[Lion] += LionSpinsCount;
            LionSpinsCount = temp;
            win = TotalWinPerRound;
        }

        private static void MakeASpin()
        {
            SelectWildSymbol();
            GenerateNewMatrix(LionFeatureReels);
            RealizeInnerSymbolsInLionFeature();

            RealizeWildSymbols();

            int totalWinPerSpin = 0;
            int payLinesWin = 0;
            if (GetSymbolCountFromMatrix(Scatter) == 3)
            {
                LionSpinsCount += RetriggerSpins;
            }

            payLinesWin = GetPaylinesWins(Matrix);
            totalWinPerSpin = payLinesWin;
            AddWinTo(totalWinPerSpin, WinsPerFeatureSpin[Lion]);
            AddWinXToInterval(totalWinPerSpin / CostToPlay, IntervalFeaturesSpinWinsX[Lion]);
            TotalWinPerRound += totalWinPerSpin;
        }

        private static void RealizeWildSymbols()
        {
            for (int i = 0; i < SlotWidth; i++)
            {
                for (int j = 0; j < SlotHeight; j++)
                {
                    if (Matrix[j, i] == _selectedSymbol)
                    {
                        Matrix[j, i] = Wild;
                    }
                }
            }
        }

        private static void SelectWildSymbol()
        {
            _selectedSymbol = TableWeightsSelector.GetRandomObjectFromTableWithWeights(SymbolsToWildsWeights);
        }

        private static void RealizeInnerSymbolsInLionFeature()
        {
            Symbol symbolInsteadInner = LionFeatureInnerReels[Rand.Next(0, LionFeatureInnerReels.Count)];
            if (Rand.NextDouble() < ChanceToChooseSelectedAsInner)
            {
                symbolInsteadInner = _selectedSymbol;
            }
            else
            {
                symbolInsteadInner = LionFeatureInnerReels[Rand.Next(0, LionFeatureInnerReels.Count)];
            }
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
