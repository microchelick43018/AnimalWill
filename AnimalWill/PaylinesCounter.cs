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
            int substitutesOtherSymbolInAPayline = 0;
            int substitutesWildInAPayline = 0;
            int wildsWin = 0;
            int otherSymbolWin = 0;
            Symbol winSymbol;
            foreach (var payline in Paylines.Values)
            {
                substitutesOtherSymbolInAPayline = 0;
                substitutesWildInAPayline = 0;
                wildsWin = 0;
                otherSymbolWin = 0;
                TryGetWinSymbolExceptWild(payline, matrix, out winSymbol);
                if (winSymbol != Collector && winSymbol != Scatter)
                    otherSymbolWin = GetSymbolWin(payline, matrix, winSymbol, out substitutesOtherSymbolInAPayline);
                wildsWin = GetSymbolWin(payline, matrix, Wild, out substitutesWildInAPayline);
                if (wildsWin + otherSymbolWin != 0)
                {
                    if (wildsWin >= otherSymbolWin)
                    {
                        SymbolsHitsCount[Wild][substitutesWildInAPayline - 1]++;
                        paylinesTotalWin += wildsWin;
                    }
                    else
                    {
                        paylinesTotalWin += otherSymbolWin;
                        SymbolsHitsCount[winSymbol][substitutesOtherSymbolInAPayline - 1]++;
                    }
                }
            }
            return paylinesTotalWin;
        }

        private static int GetSymbolWin(List<int> payline, Symbol[,] matrix, Symbol symbol, out int symbolAmount)
        {
            symbolAmount = 0;
            for (int i = 0; i < SlotWidth; i++)
            {
                if (matrix[payline[i], i] == Wild || matrix[payline[i], i] == symbol)
                {
                    symbolAmount++;
                }
                else
                {
                    break;
                }
            }
            if (symbolAmount == 0)
                return 0;
            return PayTable[symbol][symbolAmount - 1];
        }

        private static bool TryGetWinSymbolExceptWild(List<int> payline, Symbol[,] matrix, out Symbol winSymbol)
        {
            bool winSymbolIsNotWild = false;
            winSymbol = Wild;
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
