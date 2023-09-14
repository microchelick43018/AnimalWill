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
    public static class LeopardFeature
    {
        public static void RealizeFeature(out int FSRoundWin)
        {
            FSRoundWin = TableWeightsSelector.GetRandomObjectFromTableWithWeights(LeopardFeatureWinsWeights);
            AddWinXToInterval(FSRoundWin / CostToPlay, IntervalFeaturesSpinWinsX[Leopard]);
            AddWinXToInterval(FSRoundWin / CostToPlay, IntervalFeaturesRoundWinsX[Leopard]);
        }
    }
}
