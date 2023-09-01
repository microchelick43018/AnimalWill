using System;
using System.Collections.Generic;
using static AnimalWill.Symbol;
using static AnimalWill.SlotInfo;
using static AnimalWill.SlotSimulation;
using static AnimalWill.SlotStats;
using System.Linq;
using OfficeOpenXml;
using System.IO;

namespace AnimalWill
{
    class Program
    {
        static void Main(string[] args)
        {
            ImportInfo();
            SlotSimulation simulation = new SlotSimulation();
            simulation.StartSimulation();
            Console.ReadKey();
        }
    }

    public enum Symbol
    {
        Ten,
        Jack,
        Queen,
        King,
        Ace,
        WaterBuffalo,
        Rhino,
        Leopard,
        Elephant,
        Lion,
        Wild,
        Scatter,
        Collector,
        Inner,
        Blank,
    }

    static class SlotInfo
    {
        public const double CostToPlay = 50;
        public const int SlotWidth = 5;
        public const int SlotHeight = 4;

        public static Dictionary<int, List<Symbol>> BGReels = new Dictionary<int, List<Symbol>>();

        public static List<Symbol> InnerReel = new List<Symbol>();
        public static Dictionary<int, List<int>> Paylines = new Dictionary<int, List<int>>();
        public static Dictionary<Symbol, int[]> PayTable = new Dictionary<Symbol, int[]>();
        public static Dictionary<int, List<Symbol>> OuterReels = new Dictionary<int, List<Symbol>>();

        public static void ImportInfo()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (var package = new ExcelPackage(new FileInfo(@"C:\Users\AMD\source\repos\microchelick43018\AnimalWill\AnimalWill\AnimalWillMaths.xlsx")))
            {
                try
                {
                    var workbook = package.Workbook;
                    //var worksheet = workbook.Worksheets[0];
                    ImportPaylines(workbook.Worksheets["Paylines"]);
                    ImportPaytable(workbook.Worksheets["Paytable"]);
                    ImportBGReels(workbook.Worksheets["Base Game Reels"]);
                    ImportOuterCollectorReels(workbook.Worksheets["Outer Collector Reels"]);
                }
                finally
                {
                    package.Dispose();
                }
            }
        }

        private static void ImportBGReels(ExcelWorksheet worksheet)
        {
            int i;
            for (i = 0; i < SlotWidth; i++)
            {
                BGReels.Add(i, new List<Symbol>());
                for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
                {
                    BGReels[i].Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
                }
            }
            for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
            {
                InnerReel.Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
            }
        }

        private static void ImportPaytable(ExcelWorksheet worksheet)
        {
            for (int i = 4; worksheet.Cells[i, 2].Value != null; i++)
            {
                Symbol symbol = ConvertCellToSymbol(worksheet.Cells[i, 2].Value);
                PayTable.Add(symbol, new int[SlotWidth]);
                for (int j = 0; j < SlotWidth; j++)
                {
                    PayTable[symbol][j] = Convert.ToInt32(worksheet.Cells[i, j + 3].Value);
                }
            }
            PayTable[Scatter][2] *= Paylines.Count;
        }

        private static Symbol ConvertCellToSymbol(object cellValue)
        {
            string cellsString = cellValue.ToString();
            foreach (Symbol symbol in Enum.GetValues(typeof(Symbol)))
            {
                if (cellsString == symbol.ToString())
                {
                    return symbol;
                }
            }
            return default;
        }

        private static void ImportPaylines(ExcelWorksheet worksheet)
        {
            for (int i = 10; worksheet.Cells[i, 1].Value != null; i++)
            {
                int paylineNumber = Convert.ToInt32(worksheet.Cells[i, 1].Value);
                Paylines.Add(paylineNumber, new List<int>());
                for (int j = 2; worksheet.Cells[i, j].Value != null; j++)
                {
                    int paylineDigit = Convert.ToInt32(worksheet.Cells[i, j].Value);
                    Paylines[paylineNumber].Add(paylineDigit);
                }
            }
        }

        private static void ImportOuterCollectorReels(ExcelWorksheet worksheet)
        {
            int i;
            for (i = 0; i < SlotWidth; i++)
            {
                OuterReels.Add(i, new List<Symbol>());
                for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
                {
                    OuterReels[i].Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
                }
            }
        }
    }

    class SlotSimulation
    {
        public const int IterationsCount = (int) 10E7;
        public static int CurrentIteration = 0;
        private Symbol[,] _matrix = new Symbol[SlotHeight, SlotWidth];
        private Symbol[,] _outerMatrix = new Symbol[SlotHeight, SlotWidth];
        private Random _random = new Random();
        private List<int> _stopPositions = new List<int>() { 0, 0, 0, 0, 0 };
        private List<Symbol> _symbolsFromPayline = new List<Symbol>(5);
        private List<Symbol> _collectorsWheel = new List<Symbol> { Lion, Lion, Elephant, Leopard, Rhino, WaterBuffalo };
        private int _freeSpinsLeft = 0;

        public void StartSimulation()
        {
            int intervalToUpdateStats = (int) 10E5;
            for (CurrentIteration = 0; CurrentIteration < IterationsCount; CurrentIteration++)
            {
                MakeASpin();
                if (CurrentIteration % intervalToUpdateStats == 0)
                {
                    Console.Clear();
                    ShowStats();
                }
            }
        }

        private void ShowStats()
        {
            Console.WriteLine($"Iteration: {CurrentIteration}");
            CalculateConfidenceInterval();
            CalculateTotalRTP();
            CalculateScattersRTP();
            CalculateStdDev();
            ShowFSTriggerCycle();
            ShowRTPs();
            ShowStdDev();
            //ShowSymbolsRTPs();
            ShowTotalWinsXIntervalsHitFrequency();
        }

        private void GenerateNewMatrix()
        {
            GenerateNewStopPositions(BGReels);
            for (int i = 0; i < SlotWidth; i++)
            {
                for (int j = 0; j < SlotHeight; j++)
                {
                    _matrix[j, i] = BGReels[i][(_stopPositions[i] + j) % BGReels[i].Count];
                }
            }
        }

        private void RealizeInnerSymbols()
        {
            Symbol symbolInsteadInner = InnerReel[_random.Next(0, InnerReel.Count)];
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (_matrix[i, j] == Inner)
                    {
                        _matrix[i, j] = symbolInsteadInner;
                    }
                }
            }
        }

        private void MakeASpin()
        {
            GenerateNewMatrix();
            GenerateNewOuterMatrix();
            RealizeInnerSymbols();
            RealizeOuterSymbols();

            int totalWin = 0;
            int payLinesWin = 0;
            int scattersAmount = 0;
            int scattersWin = 0;
            int collectorsWin = 0;
            int FSRoundWin = 0;

            payLinesWin = GetPaylinesWins();
            scattersWin = GetScatterWin(out scattersAmount);
            if (scattersAmount == 3)
            {
                FSTriggersCount++;
            }
            if (CollectFeatureTriggersCount == 3)
            {

            }
            collectorsWin = GetCollectorsWin(out int collectorsAmount);
            totalWin = payLinesWin + scattersWin + collectorsWin + FSRoundWin;
            AddWinTo(totalWin, WinsPerTotalSpinCount);
            AddWinXToInterval((double)totalWin / CostToPlay, IntervalTotalWinsX);
        }

        private void RealizeOuterSymbols()
        {
            for (int i = 1; i < SlotHeight - 1; i++)
            {
                for (int j = 1; j < SlotWidth - 1; j++)
                {
                    if (_outerMatrix[i, j] == Collector)
                    {
                        _matrix[i, j] = Collector;
                    }
                }
            }
        }

        public void GenerateNewStopPositions(Dictionary<int, List<Symbol>> reelSet)
        {
            for (int i = 0; i < SlotWidth; i++)
            {
                _stopPositions[i] = _random.Next(0, reelSet[i].Count);
            }
        }

        private void GenerateNewOuterMatrix()
        {
            GenerateNewStopPositions(OuterReels);
            for (int i = 1; i < SlotWidth - 1; i++)
            {
                for (int j = 0; j < SlotHeight; j++)
                {
                    _outerMatrix[j, i] = OuterReels[i][(_stopPositions[i] + j) % OuterReels[i].Count];
                }
            }
        }

        private int GetPaylinesWins()
        {
            int paylinesTotalWin = 0;
            int wildsInARow = 0;
            int substitutesSymbolsInARow = 0;
            int wildsWin = 0;
            int otherSymbolWin = 0;
            Symbol winSymbol;
            foreach (var payline in Paylines.Values)
            {
                wildsInARow = 0;
                substitutesSymbolsInARow = 0;
                wildsWin = 0;
                otherSymbolWin = 0;

                wildsInARow = CountWildsInARow(payline);
                if (wildsInARow != 0)
                    wildsWin = PayTable[Wild][wildsInARow - 1];
                if (TryGetWinSymbolExceptWild(payline, out winSymbol) == false)
                {
                    winSymbol = Wild;
                }
                else if (winSymbol != Scatter && winSymbol != Collector)
                {
                    substitutesSymbolsInARow = CountSubsituteSymbolsInARow(payline, winSymbol);
                    otherSymbolWin = PayTable[winSymbol][substitutesSymbolsInARow - 1];
                }
                if (wildsWin != 0 || otherSymbolWin != 0)
                {
                    if (wildsWin >= otherSymbolWin)
                    {
                        SymbolsHitsCount[Wild][wildsInARow - 1]++;
                        paylinesTotalWin += wildsWin;
                    }
                    else
                    {
                        SymbolsHitsCount[winSymbol][substitutesSymbolsInARow - 1]++;
                        paylinesTotalWin += otherSymbolWin;
                    }
                }
            }
            return paylinesTotalWin;
        }

        private bool TryGetWinSymbolExceptWild(List<int> payline, out Symbol winSymbol)
        {
            bool winSymbolIsNotWild = false;
            winSymbol = default;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (_matrix[payline[i], i] != Wild)
                {
                    winSymbol = _matrix[payline[i], i];
                    winSymbolIsNotWild = true;
                    break;
                }
            }
            return winSymbolIsNotWild;
        }

        private int CountWildsInARow(List<int> payline)
        {
            int result = 0;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (_matrix[payline[i], i] == Wild)
                    result++;
                else
                    break;
            }
            return result;
        }

        private int CountSubsituteSymbolsInARow(List<int> payline, Symbol symbolToSubstitute)
        {
            int result = 0;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (_matrix[payline[i], i] == Wild || _matrix[payline[i], i] == symbolToSubstitute)
                    result++;
                else
                    break;
            }
            return result;
        }

        private int GetSymbolCountFromMatrix(Symbol symbol)
        {
            int result = 0;
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (_matrix[i, j] == symbol)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        private int GetScatterWin(out int scattersAmount)
        {
            scattersAmount = GetSymbolCountFromMatrix(Scatter);
            if (scattersAmount == 3)
            {
                SymbolsHitsCount[Scatter][scattersAmount - 1]++;
                return PayTable[Scatter][scattersAmount - 1];
            }
            else
            {
                return 0;
            }
        }

        private int GetCollectorsWin(out int collectorsAmount)
        {

            Symbol animalFromWheel = GetAnimalFromCollectorsWheel();
            collectorsAmount = GetSymbolCountFromMatrix(animalFromWheel);
            return 0; //PayTable[Collector][animalsCount - 1];
        }

        private Symbol GetAnimalFromCollectorsWheel()
        {
            return _collectorsWheel[_random.Next(0, _collectorsWheel.Count)];
        }
    }

    static class SlotStats
    {
        public static Dictionary<Symbol, double[]> SymbolsHitsCount = new Dictionary<Symbol, double[]>
        {
            {Wild, new double[]{ 0, 0, 0, 0, 0 } },
            {Lion, new double[]{ 0, 0, 0, 0, 0 } },
            {Elephant, new double[]{ 0, 0, 0, 0, 0 } },
            {Leopard, new double[]{ 0, 0, 0, 0, 0 } },
            {Rhino, new double[]{ 0, 0, 0, 0, 0 } },
            {WaterBuffalo, new double[]{ 0, 0, 0, 0, 0 } },
            {Ace, new double[]{ 0, 0, 0, 0, 0 } },
            {King, new double[]{ 0, 0, 0, 0, 0 } },
            {Queen, new double[]{ 0, 0, 0, 0, 0 } },
            {Jack, new double[]{ 0, 0, 0, 0, 0 } },
            {Ten, new double[]{ 0, 0, 0, 0, 0 } },
            {Scatter, new double[]{ 0, 0, 0, 0, 0 } },
        };

        public static Dictionary<int, int> WinsPerTotalSpinCount = new Dictionary<int, int>();
        public static Dictionary<int, int> WinsPerBGSpinCount = new Dictionary<int, int>();
        public static Dictionary<int, int> WinsPerFSRoundCount = new Dictionary<int, int>();
        public static Dictionary<int, int> WinsPer1FSCount = new Dictionary<int, int>();
        public static List<int> Intervals = new List<int> { -1, 0, 1, 2, 3, 5, 10, 15, 20, 30, 50, 75, 100, 150, 200, 250, 300, 400, 500, 1000 };
        public static Dictionary<int, int> IntervalTotalWinsX = new Dictionary<int, int>();

        public static int FSTriggersCount = 0;
        public static int CollectFeatureTriggersCount = 0;

        public static double StdDev = 0;
        public static double TotalRTP = 0;
        public static double ScattersRTP = 0;
        public static double CollectorsRTP = 0;
        public static double ConfidenceInterval = 0;
        static SlotStats()
        {
            foreach (var interval in Intervals)
            {
                IntervalTotalWinsX.Add(interval, 0);
            }
        }

        public static void AddWinTo(int win, Dictionary<int, int> winsCountToAdd)
        {
            if (winsCountToAdd.ContainsKey(win))
            {
                winsCountToAdd[win]++;
            }
            else
            {
                winsCountToAdd.Add(win, 1);
            }
        }

        public static void AddWinXToInterval(double winX, Dictionary<int, int> intervals)
        {
            if (winX == 400)
            {

            }
            if (winX == 0)
            {
                intervals[-1]++;
                return;
            }
            else if (winX > intervals.ElementAt(intervals.Count - 1).Key)
            {
                intervals[intervals.ElementAt(intervals.Count - 1).Key]++;
                return;
            }
            for (int i = 1; i < intervals.Count - 1; i++)
            {
                if (winX <= intervals.ElementAt(i + 1).Key)
                {
                    intervals[intervals.ElementAt(i).Key]++;
                    break;
                }
            }
        }

        public static void CalculateStdDev()
        {
            double dispa = 0;
            foreach (var item in WinsPerTotalSpinCount)
            {
                dispa += Math.Pow(item.Key / CostToPlay - TotalRTP, 2) * item.Value / CurrentIteration;
            }
            StdDev = Math.Pow(dispa, 0.5f);
        }

        public static void CalculateTotalRTP()
        {
            TotalRTP = 0;
            foreach (var item in WinsPerTotalSpinCount)
            {
                TotalRTP += (double) item.Key * item.Value / CurrentIteration / CostToPlay;
            }
        }

        public static void CalculateScattersRTP()
        {
            ScattersRTP = SymbolsHitsCount[Scatter][2] * PayTable[Scatter][2] / CurrentIteration / CostToPlay;
        }

        public static void CalculateConfidenceInterval()
        {
            ConfidenceInterval = TotalRTP * StdDev * 1.95 / Math.Pow(ConfidenceInterval, 1 / 2); //95% CI
        }

        public static void ShowRTPs()
        {
            Console.WriteLine($"Total RTP = {Math.Round(TotalRTP, 4) * 100}%");
            Console.WriteLine($"Scatter RTP = {Math.Round(ScattersRTP, 4) * 100}%");
            Console.WriteLine($"Paylines only RTP = {Math.Round(TotalRTP - ScattersRTP, 4) * 100}%");
        }

        public static void ShowStdDev()
        {
            Console.WriteLine($"StdDev = {StdDev}");
        }

        public static void ShowTotalWinsXIntervalsHitFrequency()
        {
            Console.WriteLine("TotalWinsXIntervals %:");
            Console.WriteLine($"0x: {(double) IntervalTotalWinsX.ElementAt(0).Value / CurrentIteration * 100}%");
            for (int i = 1; i < IntervalTotalWinsX.Count - 1; i++)
            {
                Console.WriteLine($"{IntervalTotalWinsX.ElementAt(i).Key}x - {IntervalTotalWinsX.ElementAt(i + 1).Key}x: {(double) IntervalTotalWinsX.ElementAt(i).Value / CurrentIteration * 100}%");
            }
            Console.WriteLine($" > {IntervalTotalWinsX.ElementAt(IntervalTotalWinsX.Count - 1).Key}x: {(double) IntervalTotalWinsX.ElementAt(IntervalTotalWinsX.Count - 1).Value / CurrentIteration * 100}%");
        }

        public static void ShowSymbolsRTPs()
        {
            Console.WriteLine($"Symbol\t\t1\t\t2\t\t3\t\t4\t\t5");
            foreach (var item in SymbolsHitsCount)
            {
                Console.Write($"{item.Key}     ");
                for (int i = 0; i < 5; i++)
                {
                    Console.Write($"\t{Math.Round(item.Value[i] * PayTable[item.Key][i] / CurrentIteration / CostToPlay, 5) * 100}\t");
                }
                Console.WriteLine();
            }
        }

        public static void ShowFSTriggerCycle()
        {
            Console.WriteLine($"FS Cycle = {Math.Round((double)CurrentIteration / FSTriggersCount, 2)}");
        }

        public static void ShowCFTriggerCycle()
        {
            Console.WriteLine($"Collect Feature Cycle = {Math.Round((double)CurrentIteration / CollectFeatureTriggersCount, 2)}");
        }
    }
}