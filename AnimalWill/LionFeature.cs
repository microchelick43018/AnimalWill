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
        public static Dictionary<Symbol, int> SymbolsToWildsWeights = new Dictionary<Symbol, int>();
        public static int LionSpinsCount;
        public static int TotalWinPerRound = 0;

        private static Symbol _selectedSymbol;

        public static void StartLionFreeSpins(out int win)
        {
            TotalWinPerRound = 0;
            SelectWildSymbol();
            for (int i = 0; i < LionSpinsCount; i++)
            {
                MakeASpin();
            }
            win = TotalWinPerRound;
        }

        private static void MakeASpin()
        {
            GenerateNewMatrix(LionFeatureReels);
            RealizeInnerSymbols();

            RealizeWildSymbols();

            int totalWin = 0;
            int payLinesWin = 0;

            payLinesWin = GetPaylinesWins(Matrix);
            totalWin = payLinesWin;
            AddWinTo(totalWin, WinsPerLionSpin);
            AddWinXToInterval(totalWin / CostToPlay, IntervalLionFeatureSpinWinsX);
            TotalWinPerRound += totalWin;
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
    }
}
