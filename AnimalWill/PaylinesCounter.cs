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
    public static class PaylinesCounter
    {
        public static int GetPaylinesWins(Symbol[,] matrix)
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

                wildsInARow = CountWildsInARow(payline, matrix);

                if (wildsInARow != 0)
                    wildsWin = PayTable[Wild][wildsInARow - 1];

                if (TryGetWinSymbolExceptWild(payline, matrix, out winSymbol) == false)
                {
                    winSymbol = Wild;
                }
                else if (winSymbol != Scatter && winSymbol != Collector)
                {
                    substitutesSymbolsInARow = CountSubsituteSymbolsInARow(payline, matrix, winSymbol);
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

        private static int CountWildsInARow(List<int> payline, Symbol[,] matrix)
        {
            int result = 0;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (matrix[payline[i], i] == Wild)
                    result++;
                else
                    break;
            }
            return result;
        }
        private static bool TryGetWinSymbolExceptWild(List<int> payline, Symbol[,] matrix, out Symbol winSymbol)
        {
            bool winSymbolIsNotWild = false;
            winSymbol = default;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (matrix[payline[i], i] != Wild)
                {
                    winSymbol = matrix[payline[i], i];
                    winSymbolIsNotWild = true;
                    break;
                }
            }
            return winSymbolIsNotWild;
        }

        private static int CountSubsituteSymbolsInARow(List<int> payline, Symbol[,] matrix, Symbol symbolToSubstitute)
        {
            int result = 0;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (matrix[payline[i], i] == Wild || matrix[payline[i], i] == symbolToSubstitute)
                    result++;
                else
                    break;
            }
            return result;
        }

        public static void RealizeInnerSymbols()
        {
            Symbol symbolInsteadInner = InnerReel[Rand.Next(0, InnerReel.Count)];
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
