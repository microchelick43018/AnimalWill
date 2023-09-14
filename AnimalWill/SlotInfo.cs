using System;
using System.Collections.Generic;
using static AnimalWill.Symbol;
using static AnimalWill.SlotInfo;
using static AnimalWill.SlotSimulation;
using static AnimalWill.SlotStats;
using static AnimalWill.LionFeature;
using System.Linq;
using OfficeOpenXml;
using System.IO;
using System.Reflection;

namespace AnimalWill
{
    static class SlotInfo
    {
        public const double CostToPlay = 70;
        public const int SlotWidth = 5;
        public const int SlotHeight = 4;

        public static List<Dictionary<int, List<Symbol>>> BGReelsSets = new List<Dictionary<int, List<Symbol>>>();

        public static List<Symbol> _collectorsWheel = new List<Symbol> { Lion, Lion, Elephant, Leopard, Rhino, WaterBuffalo };
        public static Dictionary<Symbol, int> AnimalsStartWeightsForWheel = new Dictionary<Symbol, int>();
        public static Dictionary<Symbol, int> AnimalsAdditionalWeightsForWheel = new Dictionary<Symbol, int>();
        public static List<Symbol> InnerReel = new List<Symbol>();
        public static Dictionary<int, List<int>> Paylines = new Dictionary<int, List<int>>();
        public static Dictionary<Symbol, int[]> PayTable = new Dictionary<Symbol, int[]>();
        public static Dictionary<int, List<Symbol>> OuterReels = new Dictionary<int, List<Symbol>>();
        public static List<double> BaseGameReelsWeights = new List<double>();
        public static Dictionary<int, int> CollectorsPayTable = new Dictionary<int, int>();
        public static double ChanceToUseOuterReelsDuringBG = 0;
        public static Dictionary<int, int> LeopardFeatureWinsWeights = new Dictionary<int, int>();

        public static void ImportInfo()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            using (var package = new ExcelPackage(new FileInfo(@"C:\Users\konstantin.d\source\repos\AnimalWill\AnimalWill\AnimalWillMaths.xlsx")))
            {
                try
                {
                    var workbook = package.Workbook;
                    ImportPaylines(workbook.Worksheets["Paylines"]);
                    ImportPaytable(workbook.Worksheets["Paytable"]);
                    ImportBGReels(workbook);
                    ImportInnerReels(workbook.Worksheets["Base Game Reels 1"]);
                    ImportBGReelsWeights(workbook.Worksheets["Base Game Reel Weights"]);

                    ImportOuterCollectorReels(workbook.Worksheets["Outer Collector Reels"]);
                    ImportChanceToUseOuterReelsDuringBG(workbook.Worksheets["Outer Reels Weights"]);
                    ImportCollectorsPayTable(workbook.Worksheets["Collect Feature Paytable"]);
                    ImportWeightsForWheel(workbook.Worksheets["Formula of Animal Wheel Weights"]);

                    ImportLionFeatureReels(workbook.Worksheets["Lion Feature Reels"]);
                    ImportLionFeatureInfo(workbook.Worksheets["Lion Feature Info"]);

                    ImportLeopardFeatureWieghts(workbook.Worksheets["Leopard Feature Weights"]);

                    ImportRhinoReelSet(workbook.Worksheets["Rhino Feature Reels"]);
                    ImportRhinoInfo(workbook.Worksheets["Rhino Feature Info"]);

                    ImportWaterBuffaloReels(workbook.Worksheets["Buffalo Feature Reel Set1"], WaterBuffaloFeature.WaterBuffaloFeatureReels1stSpin);
                    ImportWaterBuffaloReels(workbook.Worksheets["Buffalo Feature Reel Set2"], WaterBuffaloFeature.WaterBuffaloFeatureReels2ndSpin);
                    ImportWaterBuffaloReels(workbook.Worksheets["Buffalo Feature Reel Set3"], WaterBuffaloFeature.WaterBuffaloFeatureReels4thSpin);
                    ImportWaterBuffaloSpinsCount(workbook.Worksheets["Buffalo Feature Info"]);
                }
                finally
                {
                    package.Dispose();
                }
            }
        }

        private static void ImportRhinoInfo(ExcelWorksheet worksheet)
        {
            RhinoFeature.RhinoSpinsCount = Convert.ToInt32(worksheet.Cells[8, 4].Value);
            RhinoFeature.ChanceToUseOuterReels = Convert.ToDouble(worksheet.Cells["E5"].Value) / Convert.ToDouble(worksheet.Cells["G5"].Value);
        }

        private static void ImportRhinoReelSet(ExcelWorksheet worksheet)
        {
            int i;
            for (i = 0; i < SlotWidth; i++)
            {
                RhinoFeature.ReelsSet.Add(i, new List<Symbol>());
                for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
                {
                    RhinoFeature.ReelsSet[i].Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
                }
            }
        }

        private static void ImportWaterBuffaloSpinsCount(ExcelWorksheet worksheet)
        {
            WaterBuffaloFeature.SpinsCount = Convert.ToInt32(worksheet.Cells["D2"].Value);
        }

        private static void ImportWaterBuffaloReels(ExcelWorksheet worksheet, Dictionary<int, List<Symbol>> reelSet)
        {
            int i;
            for (i = 0; i < SlotWidth; i++)
            {
                reelSet.Add(i, new List<Symbol>());
                for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
                {
                    reelSet[i].Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
                }
            }
            for (int j = 4; worksheet.Cells[j, 8].Value != null; j++)
            {
                RhinoFeature.RhinoInnerReel.Add(ConvertCellToSymbol(worksheet.Cells[j, 8].Value));
            }
        }

        private static void ImportLeopardFeatureWieghts(ExcelWorksheet worksheet)
        {
            for (int i = 0; worksheet.Cells[13 + i, 3].Value != null; i++)
            {
                if (LeopardFeatureWinsWeights.ContainsKey(Convert.ToInt32(worksheet.Cells[13 + i, 3].Value)))
                {
                    LeopardFeatureWinsWeights[Convert.ToInt32(worksheet.Cells[13 + i, 3].Value)] += Convert.ToInt32(worksheet.Cells[13 + i, 5].Value);
                }
                else
                {
                    LeopardFeatureWinsWeights.Add(Convert.ToInt32(worksheet.Cells[13 + i, 3].Value), Convert.ToInt32(worksheet.Cells[13 + i, 5].Value));
                }
            }
        }

        private static void ImportLionFeatureInfo(ExcelWorksheet worksheet)
        {
            LionSpinsCount = Convert.ToInt32(worksheet.Cells[2, 3].Value);
            for (int i = 0; i < 5; i++)
            {
                SymbolsToWildsWeights.Add(ConvertCellToSymbol(worksheet.Cells[5 + i, 2].Value), Convert.ToInt32(worksheet.Cells[5 + i, 3].Value));
            }
        }

        private static void ImportLionFeatureReels(ExcelWorksheet worksheet)
        {
            int i;
            for (i = 0; i < SlotWidth; i++)
            {
                LionFeatureReels.Add(i, new List<Symbol>());
                for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
                {
                    LionFeatureReels[i].Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
                }
            }
            for (int j = 4; worksheet.Cells[j, 8].Value != null; j++)
            {
                LionFeatureInnerReels.Add(ConvertCellToSymbol(worksheet.Cells[j, 8].Value));
            }
        }

        private static void ImportCollectorsPayTable(ExcelWorksheet worksheet)
        {
            for (int i = 0; i <= 20; i++)
            {
                CollectorsPayTable.Add(i, Convert.ToInt32(worksheet.Cells[3 + i, 3].Value));
                CollectorsAmountHits.Add(Convert.ToInt32(worksheet.Cells[3 + i, 2].Value), 0);
            }
        }

        private static void ImportBGReelsWeights(ExcelWorksheet worksheet)
        {
            BaseGameReelsWeights.Add((double)worksheet.Cells[2, 3].Value / (double)worksheet.Cells[4, 3].Value);
            BaseGameReelsWeights.Add((double)worksheet.Cells[3, 3].Value / (double)worksheet.Cells[4, 3].Value);
        }

        private static void ImportBGReels(ExcelWorkbook workbook)
        {
            string worksheetName = "Base Game Reels ";
            ExcelWorksheet worksheet;
            for (int worksheetNumber = 1; worksheetNumber <= 2; worksheetNumber++)
            {
                worksheet = workbook.Worksheets[worksheetName + worksheetNumber.ToString()];
                int i;
                BGReelsSets.Add(new Dictionary<int, List<Symbol>>());
                for (i = 0; i < SlotWidth; i++)
                {
                    BGReelsSets[worksheetNumber - 1].Add(i, new List<Symbol>());
                    for (int j = 4; worksheet.Cells[j, i + 3].Value != null; j++)
                    {
                        BGReelsSets[worksheetNumber - 1][i].Add(ConvertCellToSymbol(worksheet.Cells[j, i + 3].Value));
                    }
                }
            }

        }

        private static void ImportInnerReels(ExcelWorksheet worksheet)
        {
            for (int j = 4; worksheet.Cells[j, 8].Value != null; j++)
            {
                InnerReel.Add(ConvertCellToSymbol(worksheet.Cells[j, 8].Value));
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

        private static void ImportChanceToUseOuterReelsDuringBG(ExcelWorksheet worksheet)
        {
            ChanceToUseOuterReelsDuringBG = (double)worksheet.Cells[3, 3].Value / (double)worksheet.Cells[3, 5].Value;
        }

        private static void ImportWeightsForWheel(ExcelWorksheet worksheet)
        {
            for (int i = 3; i <= 7; i++)
            {
                AnimalsStartWeightsForWheel.Add(ConvertCellToSymbol(worksheet.Cells[i, 2].Value), Convert.ToInt32(worksheet.Cells[i, 3].Value));
                AnimalsAdditionalWeightsForWheel.Add(ConvertCellToSymbol(worksheet.Cells[i, 2].Value), Convert.ToInt32(worksheet.Cells[i, 4].Value));
            }
        }
    }
}
