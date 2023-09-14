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
    public static class WaterBuffaloFeature
    {
        public static Dictionary<int, List<Symbol>> WaterBuffaloFeatureReels1stSpin = new Dictionary<int, List<Symbol>>();
        public static Dictionary<int, List<Symbol>> WaterBuffaloFeatureReels2ndSpin = new Dictionary<int, List<Symbol>>();
        public static Dictionary<int, List<Symbol>> WaterBuffaloFeatureReels4thSpin = new Dictionary<int, List<Symbol>>();
        public static Dictionary<int, List<Symbol>> CurrentReelSet = new Dictionary<int, List<Symbol>>();
        public static int SpinsCount = 5;
        public static int TotalWinPerRound = 0;

        public static void StartFeature(out int win)
        {
            TotalWinPerRound = 0;
            for (int i = 0; i < SpinsCount; i++)
            {
                if (i == 0)
                {
                    CurrentReelSet = WaterBuffaloFeatureReels1stSpin;
                }
                else if (i == 1)
                {
                    CurrentReelSet = WaterBuffaloFeatureReels2ndSpin;
                }
                else if (i == 3)
                {
                    CurrentReelSet = WaterBuffaloFeatureReels4thSpin;
                }
                MakeASpin();
            }
            win = TotalWinPerRound;
        }

        private static void MakeASpin()
        {
            GenerateNewMatrix(CurrentReelSet);
            RealizeInnerSymbols();

            int totalWinPerSpin = 0;
            int payLinesWin = 0;

            payLinesWin = GetPaylinesWins(Matrix);
            totalWinPerSpin = payLinesWin;
            AddWinTo(totalWinPerSpin, WinsPerFeatureSpin[WaterBuffalo]);
            AddWinXToInterval(totalWinPerSpin / CostToPlay, IntervalFeaturesSpinWinsX[WaterBuffalo]);
            TotalWinPerRound += totalWinPerSpin;
        }
    }
}
