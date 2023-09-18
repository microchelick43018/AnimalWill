using System;
using System.Collections.Generic;
using static AnimalWill.Symbol;
using static AnimalWill.SlotInfo;
using static AnimalWill.SlotSimulation;
using static AnimalWill.SlotStats;
using System.Linq;
using OfficeOpenXml;
using System.IO;
using System.Reflection;

namespace AnimalWill
{
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
        public static Dictionary<Symbol, Dictionary<int, int>> WinsPerFeatureSpin = new Dictionary<Symbol, Dictionary<int, int>>();
        public static Dictionary<int, int> PaylineWinsPerSpinCount = new Dictionary<int, int>();
        public static List<int> Intervals = new List<int> { -1, 0, 1, 2, 3, 5, 10, 15, 20, 30, 50, 75, 100, 150, 200, 250, 300, 400, 500, 1000 };
        public static Dictionary<int, int> IntervalTotalWinsX = new Dictionary<int, int>();
        public static Dictionary<Symbol, Dictionary<int, int>> IntervalFeaturesSpinWinsX = new Dictionary<Symbol, Dictionary<int, int>>();
        public static Dictionary<Symbol, Dictionary<int, int>> IntervalFeaturesRoundWinsX = new Dictionary<Symbol, Dictionary<int, int>>();
        public static Dictionary<int, int> CollectorsAmountHits = new Dictionary<int, int>();
        public static Dictionary<Symbol, int> FeaturesSelectedCount = new Dictionary<Symbol, int>() { { Lion, 0 }, { Elephant, 0 }, { Leopard, 0 }, { Rhino, 0 }, { WaterBuffalo, 0 } };
        public static Dictionary<Symbol, Dictionary<int, int>> WinsPerFeatureRound = new Dictionary<Symbol, Dictionary<int, int>>();

        public static int FSTriggersCount = 0;
        public static int CollectFeatureTriggersCount = 0;

        public static double StdDev = 0;
        public static double TotalRTP = 0;
        public static double PaylinesRTP = 0;
        public static double ScattersRTP = 0;
        public static double CollectorsRTP = 0;
        public static double ConfidenceInterval = 0;
        public static int SumOfElephantMultipliers = 0;

        static SlotStats()
        {
            foreach (Symbol item in Enum.GetValues(typeof(Symbol)))
            {
                IntervalFeaturesSpinWinsX.Add(item, new Dictionary<int, int>());
                IntervalFeaturesRoundWinsX.Add(item, new Dictionary<int, int>());
                WinsPerFeatureSpin.Add(item, new Dictionary<int, int>());
                WinsPerFeatureRound.Add(item, new Dictionary<int, int>());
            }
            foreach (var interval in Intervals)
            {
                foreach (Symbol item in Enum.GetValues(typeof(Symbol)))
                {
                    IntervalFeaturesSpinWinsX[item].Add(interval, 0);
                    IntervalFeaturesRoundWinsX[item].Add(interval, 0);
                }
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
                double slagaemoe = (item.Key / CostToPlay - TotalRTP) * (item.Key / CostToPlay - TotalRTP) * item.Value / CurrentIteration;
                dispa += slagaemoe;
            }
            StdDev = Math.Pow(dispa, 0.5f);
        }

        public static void CalculateTotalRTP()
        {
            TotalRTP = 0;
            foreach (var item in WinsPerTotalSpinCount)
            {
                TotalRTP += (double)item.Key * item.Value / CurrentIteration / CostToPlay;
            }
        }

        public static void CalculatePaylineRTP()
        {
            PaylinesRTP = 0;
            foreach (var item in PaylineWinsPerSpinCount)
            {
                PaylinesRTP += (double)item.Key * item.Value / CurrentIteration / CostToPlay;
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

        public static void CalculateCollectorsRTP()
        {
            CollectorsRTP = 0;
            foreach (var item in CollectorsAmountHits)
            {
                CollectorsRTP += item.Value * CollectorsPayTable[item.Key] / CostToPlay;
            }
            CollectorsRTP /= CurrentIteration;
        }

        public static void ShowRTPs()
        {
            Console.WriteLine($"Total RTP = {Math.Round(TotalRTP, 4) * 100}%");
            Console.WriteLine($"Scatter RTP = {Math.Round(ScattersRTP, 4) * 100}%");
            Console.WriteLine($"Paylines only RTP = {Math.Round(PaylinesRTP, 4) * 100}%");
            Console.WriteLine($"Collectors RTP = {Math.Round(CollectorsRTP, 4) * 100}%");
        }

        public static void ShowStdDev()
        {
            Console.WriteLine($"StdDev = {StdDev}");
            double stdDev2 = 0;
            foreach (var item in WinsPerTotalSpinCount)
            {
                stdDev2 += (item.Key / CostToPlay * item.Key / CostToPlay) * (double) item.Value / CurrentIteration;
            }
            stdDev2 -= TotalRTP * TotalRTP;
            stdDev2 = Math.Sqrt(stdDev2);
            Console.WriteLine($"StdDev2 = {stdDev2}");
        }

        public static void ShowIntervalsHitRate(Dictionary<int, int> intervalToShow, string intervalName, Dictionary<int, int> winsOfInterval)
        {
            int sumOfTheseSpins = 0;
            foreach (var interval in intervalToShow)
            {
                sumOfTheseSpins += interval.Value;
            }
            Console.WriteLine($"{intervalName} Intervals Hit Rate (RTP%):");
            Console.WriteLine($"0x: {1 / ((double)intervalToShow.ElementAt(0).Value / sumOfTheseSpins)} (0%)");
            double RTPofInterval = 0;
            for (int i = 1; i < IntervalTotalWinsX.Count - 1; i++)
            {
                RTPofInterval = GetSumOfWinsXFromTheInterval(intervalToShow.ElementAt(i).Key, intervalToShow.ElementAt(i + 1).Key, winsOfInterval);
                Console.WriteLine($"{intervalToShow.ElementAt(i).Key}x - {intervalToShow.ElementAt(i + 1).Key}x: " +
                    $"{1 / ((double)intervalToShow.ElementAt(i).Value / sumOfTheseSpins)} " +
                    $"({Math.Round(RTPofInterval / sumOfTheseSpins * 100, 4)}%)");
            }
            RTPofInterval = GetSumOfWinsXFromTheInterval(intervalToShow.ElementAt(intervalToShow.Count - 1).Key, 999999999, winsOfInterval);
            Console.WriteLine($" > {intervalToShow.ElementAt(intervalToShow.Count - 1).Key}x: " +
                $"{1 / ((double)intervalToShow.ElementAt(IntervalTotalWinsX.Count - 1).Value / sumOfTheseSpins)} ({Math.Round(RTPofInterval / sumOfTheseSpins * 100, 4)}%)");
        }

        public static double GetSumOfWinsXFromTheInterval(double leftBoardX, double rightBoardX, Dictionary<int, int> winsOfInterval)
        {
            double result = 0;
            foreach (var item in winsOfInterval)
            {
                if (leftBoardX < item.Key / CostToPlay && item.Key / CostToPlay <= rightBoardX)
                {
                    result += item.Key * item.Value;
                }
            }
            result /= CostToPlay;
            return result;
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

        public static void ShowCollectorsHits()
        {
            Console.WriteLine();
            Console.WriteLine("Collectors Hits Amount - Hits - Value (%)");
            foreach (var item in CollectorsAmountHits)
            {
                Console.WriteLine($"{item.Key} - {item.Value} - {CollectorsPayTable[item.Key]} ({Math.Round((double)item.Value / CollectFeatureTriggersCount, 6) * 100}%)");
            }
        }

        public static void ShowAvgCollectorFeatureWin()
        {
            double avgWin = 0;
            foreach (var item in CollectorsAmountHits)
            {
                avgWin += (double)item.Value * CollectorsPayTable[item.Key] / CostToPlay;
            }
            avgWin /= CollectFeatureTriggersCount;
            Console.WriteLine($"Collect Feature Avg Win = {avgWin}x");
        }

        public static void ShowAvgAnimalsCollected()
        {
            double avgAmount = 0;
            foreach (var item in CollectorsAmountHits)
            {
                avgAmount += (double)item.Value * item.Key;
            }
            avgAmount /= CollectFeatureTriggersCount;
            Console.WriteLine($"Collect Feature avg Animals collected = {avgAmount}");
        }

        public static void ShowSelectedFeaturesAmount()
        {
            Console.WriteLine();
            Console.WriteLine("Selected features Amount: Feature - Amount - %");
            foreach (var item in FeaturesSelectedCount)
            {
                Console.WriteLine($"{item.Key} - {item.Value} - {Math.Round((double) item.Value / FSTriggersCount, 4) * 100}%");
            }
            Console.WriteLine();
        }

        public static void ShowFeaturesTriggerCycle()
        {
            Console.WriteLine();
            Console.WriteLine("Selected features cycle: Feature - cycle");
            foreach (var item in FeaturesSelectedCount)
            {
                Console.WriteLine($"{item.Key} - {Math.Round(CurrentIteration / (double)item.Value, 4)}");
            }
            Console.WriteLine();
        }

        public static void ShowFeaturesRTPs()
        {
            double FeatureRTP = 0;
            foreach (Symbol animal in FeaturesSelectedCount.Keys)
            {
                FeatureRTP = 0;
                foreach (var item in WinsPerFeatureRound[animal])
                {
                    FeatureRTP += item.Value * item.Key / CostToPlay;
                }
                FeatureRTP /= CurrentIteration;
                Console.WriteLine($"{animal} Feature RTP = {Math.Round(FeatureRTP, 4) * 100}%");
            }
        }

        public static void ShowFeaturesStdDevs()
        {
            double stdDev = 0;
            foreach (Symbol animal in FeaturesSelectedCount.Keys)
            {
                stdDev = 0;
                foreach (var item in WinsPerFeatureRound[animal])
                {
                    stdDev += item.Value * (item.Key * item.Key / CostToPlay / CostToPlay);
                }
                stdDev /= CurrentIteration;
                stdDev -= TotalRTP * TotalRTP;
                stdDev = Math.Sqrt(stdDev);
                Console.WriteLine($"{animal} Feature StdDev = {Math.Round(stdDev, 6)}");
            }
        }

        public static void ShowAvgWinPerLionSpin()
        {
            double avgWin = 0;
            foreach (var item in WinsPerFeatureSpin[Lion])
            {
                avgWin += item.Value * item.Key / CostToPlay;
            }
            int lionSpinsCount = 0;
            foreach (var item in WinsPerFeatureSpin[Lion])
            {
                lionSpinsCount += item.Value;
            }
            avgWin /= lionSpinsCount;
            Console.WriteLine($"Lion Feature Avg Win Per Spin = {Math.Round(avgWin, 4)}x");
        }

        public static void ShowAvgMultiplierPerElephantSpin()
        {
            double avgMultiplier = 0;
            int sumOfElephantSpins = 0;
            foreach (var item in WinsPerFeatureSpin[Elephant])
            {
                sumOfElephantSpins += item.Value;
            }
            avgMultiplier = (double) SumOfElephantMultipliers / sumOfElephantSpins;
            Console.WriteLine($"Elephant Feature Avg multiplier = {Math.Round(avgMultiplier, 4)}x");
        }
    }
}
