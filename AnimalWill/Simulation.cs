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
using System.Windows.Media.Imaging;

namespace AnimalWill
{
    class Simulation
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

    class SlotSimulation
    {
        public const int IterationsCount = (int)2E7;
        public static int CurrentIteration = 0;
        public static Dictionary<int, List<Symbol>> CurrentBGReels = new Dictionary<int, List<Symbol>>();
        private static Dictionary<Symbol, int> _animalsMeters = new Dictionary<Symbol, int>() { { Lion, 1 }, { Elephant, 1 }, { Leopard, 1 }, { Rhino, 1 }, { WaterBuffalo, 1 } };
        public static Symbol[,] Matrix = new Symbol[SlotHeight, SlotWidth];
        public static Symbol[,] OuterMatrix = new Symbol[SlotHeight, SlotWidth];
        public static Random Rand = new Random();
        public static List<int> StopPositions = new List<int>() { 0, 0, 0, 0, 0 };
        public static Dictionary<Symbol, int> AnimalsWeights = new Dictionary<Symbol, int>() { { Lion, 0 }, { Elephant, 0 }, { Leopard, 0 }, { Rhino, 0 }, { WaterBuffalo, 0 } };

        public void StartSimulation()
        {
            int intervalToUpdateStats = (int) 10E5;
            for (CurrentIteration = 0; CurrentIteration <= IterationsCount; CurrentIteration++)
            {
                MakeASpin();
                if (CurrentIteration % intervalToUpdateStats == 0)
                {
                    Console.Clear();
                    Console.WriteLine("\x1b[3J");
                    ShowStats();
                }
            }
        }

        private void ShowStats()
        {
            Console.WriteLine($"Iteration: {CurrentIteration}");
            CalculateConfidenceInterval();
            CalculateTotalRTP();
            CalculateBGOnlyRTP();
            CalculateScattersRTP();
            CalculateStdDev();
            CalculateCollectorsRTP();
            CalculateFSRTP();

            ShowFSTriggerCycle();
            ShowCFTriggerCycle();
            ShowRTPs();
            ShowStdDev(); 
            Console.WriteLine();
            ShowFeaturesTriggerCycle();
            Console.WriteLine();
            ShowFeaturesRTPs();
            Console.WriteLine();
            ShowFeaturesStdDevs();
            Console.WriteLine(); 


            ShowIntervalsHitRate(IntervalTotalWinsX, "Total Wins", WinsPerTotalSpinCount);

            ShowCollectorsHits();
            ShowAvgCollectorFeatureWin(); 
            ShowAvgAnimalsCollected();
            ShowSelectedFeaturesAmount();

            Console.WriteLine();
            foreach (var item in _animalsMeters.Keys)
            {
                Console.WriteLine();
                ShowIntervalsHitRate(IntervalFeaturesRoundWinsX[item], $"{item} Feature Wins Per Round", WinsPerFeatureRound[item]);
                Console.WriteLine();
            }

            ShowAvgMultiplierPerElephantSpin();
            Console.WriteLine();
            ShowAvgRoundWinXForEachFeature();
            Console.WriteLine();
            ShowAvgFreeSpinsCount();
        }

        public static void GenerateNewMatrix(Dictionary<int, List<Symbol>> reelSet)
        {
            GenerateNewStopPositions(reelSet);
            for (int i = 0; i < SlotWidth; i++)
            {
                for (int j = 0; j < SlotHeight; j++)
                {
                    Matrix[j, i] = reelSet[i][(StopPositions[i] + j) % reelSet[i].Count];
                }
            }
        }

        private void MakeASpin()
        {
            ChooseBGReelSet();
            GenerateNewMatrix(CurrentBGReels);
            RealizeInnerSymbols();

            int totalWin = 0;
            int payLinesWin = 0;
            int scattersAmount = 0;
            int scattersWin = 0;
            int collectorsWin = 0;
            int FSRoundWin = 0;

            if (ChanceToUseOuterReelsDuringBG >= Rand.NextDouble())
            {
                GenerateNewOuterMatrix();
                RealizeOuterSymbols();
                int collectorsCount = GetSymbolCountFromMatrix(Collector);
                if (collectorsCount != 0)
                {
                    for (int i = 0; i < collectorsCount; i++)
                    {
                        CollectFeatureTriggersCount++;
                        TurnCollectorsIntoWilds();
                        collectorsWin += GetCollectorsWin(out int animalsAmount, out Symbol playedSymbol);
                        _animalsMeters[playedSymbol] += animalsAmount;
                        CollectorsAmountHits[animalsAmount]++;
                    }
                }
            }

            payLinesWin = GetPaylinesWins(Matrix);
            scattersWin = GetScatterWin(out scattersAmount);
            if (scattersAmount == 3)
            {
                FSTriggersCount++;
                Symbol selectedSymbol = GetSymbolOfFeature();
                FeaturesSelectedCount[selectedSymbol]++;
                if (selectedSymbol == Lion)
                {
                    LionFeature.StartLionFreeSpins(out FSRoundWin);
                }
                else if (selectedSymbol == Elephant)
                {
                    ElephantFeature.StartElephantFreeSpins(out FSRoundWin);
                }
                else if (selectedSymbol == Leopard)
                {
                    LeopardFeature.RealizeFeature(out FSRoundWin);
                }
                else if (selectedSymbol == WaterBuffalo)
                {
                    WaterBuffaloFeature.StartFeature(out FSRoundWin);
                }
                else if (selectedSymbol == Rhino)
                {
                    RhinoFeature.StartRhinoFreeSpins(out FSRoundWin);
                }
                AddWinXToInterval(FSRoundWin / CostToPlay, IntervalFeaturesRoundWinsX[selectedSymbol]);
                AddWinTo(FSRoundWin, WinsPerFeatureRound[selectedSymbol]);
                ClearAnimalMeters();
            }

            totalWin = payLinesWin + scattersWin + collectorsWin + FSRoundWin;
            AddWinTo(payLinesWin, PaylineWinsPerSpinCount);
            AddWinTo(totalWin, WinsPerTotalSpinCount);
            AddWinTo(totalWin - FSRoundWin, WinsPerBGOnlySpinCount);
            AddWinXToInterval(totalWin / CostToPlay, IntervalTotalWinsX);
        }

        private Symbol GetSymbolOfFeature()
        {
            List<Symbol> highestMeters = new List<Symbol>();
            int highestScore = 0;
            foreach (var item in _animalsMeters)
            {
                if (highestScore < item.Value)
                {
                    highestScore = item.Value;
                }
            }
            foreach (var item in _animalsMeters)
            {
                if (item.Value == highestScore)
                {
                    highestMeters.Add(item.Key);
                }
            }
            Symbol selectedSymbol = highestMeters.ElementAt(Rand.Next(0, highestMeters.Count));
            return selectedSymbol;
        }

        private void ClearAnimalMeters()
        {
            for (int i = 0; i < _animalsMeters.Count; i++)
            {
                _animalsMeters[_animalsMeters.ElementAt(i).Key] = 0;
            }
        }

        private void ChooseBGReelSet()
        {
            int i = 0;
            double randomNumber = Rand.NextDouble();
            double sumOfWeightsOfWrongReelSets = BaseGameReelsWeights[0];
            while (randomNumber > sumOfWeightsOfWrongReelSets)
            {
                i++;
                sumOfWeightsOfWrongReelSets += BaseGameReelsWeights[i];
            }
            CurrentBGReels = BGReelsSets[i]; 
        }

        public static void TurnCollectorsIntoWilds()
        {
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (Matrix[i, j] == Collector)
                    {
                        Matrix[i, j] = Wild;
                    }
                }
            }
        }

        public static void RealizeOuterSymbols()
        {
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (OuterMatrix[i, j] != Blank)
                    {
                        Matrix[i, j] = OuterMatrix[i, j];
                    }
                }
            }
        }

        public static void GenerateNewStopPositions(Dictionary<int, List<Symbol>> reelSet)
        {
            for (int i = 0; i < SlotWidth; i++)
            {
                StopPositions[i] = Rand.Next(0, reelSet[i].Count);
            }
        }

        public static void GenerateNewOuterMatrix()
        {
            GenerateNewStopPositions(OuterReels);
            for (int i = 0; i < SlotWidth; i++)
            {
                for (int j = 0; j < SlotHeight; j++)
                {
                    OuterMatrix[j, i] = OuterReels[i][(StopPositions[i] + j) % OuterReels[i].Count];
                }
            }
        }

        public static int GetSymbolCountFromMatrix(Symbol symbol)
        {
            int result = 0;
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (Matrix[i, j] == symbol)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        public static int GetScatterWin(out int scattersAmount)
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

        public static int GetCollectorsWin(out int animalsAmount, out Symbol playedSymbol)
        {
            playedSymbol = GetAnimalFromCollectorsWheel();
            animalsAmount = GetSymbolCountFromMatrix(playedSymbol);
            return CollectorsPayTable[animalsAmount];
        }

        public static void CalculateAnimalWheelWeights()
        {
            for (int i = 0; i < AnimalsWeights.Count; i++)
            {
                var item = AnimalsWeights.ElementAt(i);
                int additionalWeight = AnimalsAdditionalWeightsForWheel[item.Key] * GetSymbolCountFromMatrix(item.Key);
                int startWeight = AnimalsStartWeightsForWheel[item.Key];
                AnimalsWeights[item.Key] = startWeight + additionalWeight;
            }
        }

        public static Symbol GetAnimalFromCollectorsWheel()
        {
            CalculateAnimalWheelWeights();
            
            return TableWeightsSelector.GetRandomObjectFromTableWithWeights(AnimalsWeights);
        }
    }   
}