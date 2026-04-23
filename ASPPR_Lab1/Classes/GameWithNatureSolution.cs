using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPPR_Lab2.Enums;

namespace ASPPR_Lab2.Classes
{
    internal class GameWithNatureSolution
    {
        Dictionary<GameWithNatureAlgorithm, List<double>> Values { get; set; }

        public GameWithNatureSolution(Dictionary<GameWithNatureAlgorithm, List<double>> values)
        {
            Values = values;
        }

        public GameWithNatureSolution(Dictionary<GameWithNatureAlgorithm,MatrixGameSolution> values)
        {
            Values = values.ToDictionary(x => x.Key, x => x.Value.X.ToList());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Values)
            {
                sb.AppendLine($"{item.Key}: ({string.Join(';', item.Value.Select((value, index) => (Value: value, Index: index)).Where(x => Math.Abs(x.Value) > 0.001).Select(x => $"A{x.Index+1}"))})");
            }
            return sb.ToString();
        }
    }
}
