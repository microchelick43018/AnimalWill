using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AnimalWill.SlotInfo;
using static AnimalWill.SlotSimulation;
using static AnimalWill.SlotStats;
using static AnimalWill.PaylinesCounter;

namespace AnimalWill
{
    public static class TableWeightsSelector
    {
        public static T GetRandomObjectFromTableWithWeights<T>(Dictionary<T, int> weightsTable)
        {
            int sumOfWeights = 0;
            foreach (var item in weightsTable)
            {
                sumOfWeights += item.Value;
            }
            int i = 0;
            int randomNumber = Rand.Next(0, sumOfWeights);
            int sumOfWrongWeights = weightsTable.ElementAt(i).Value;
            while (randomNumber > sumOfWrongWeights)
            {
                i++;
                sumOfWrongWeights += weightsTable.ElementAt(i).Value;
            }
            return weightsTable.ElementAt(i).Key;
        }
    }
}
