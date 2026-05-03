using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab2.Classes
{
    internal class TransportInput
    {

        public Matrix CostMatrix { get; set; }
        public List<int> Supply { get; set; }
        public List<int> Demand { get; set; }
        public TransportInput(Matrix costMatrix, List<int> supply, List<int> demand)
        {
            CostMatrix = costMatrix;
            Supply = supply;
            Demand = demand;
            costMatrix.RowMarkers = supply.Select((s, i) => $"S{i + 1}").ToList();
            costMatrix.RowMarkersDual = supply.Select((s, i) => $"{s}").ToList();
            costMatrix.ColMarkers = demand.Select((d, j) => $"D{j + 1}").ToList();
            costMatrix.ColMarkersDual = demand.Select((d, j) => $"{d}").ToList();
        }

        public bool IsClosed()
        {
            return Supply.Sum() == Demand.Sum();
        }

        public int Close()
        {
            if (IsClosed()) return 0;

            var difference = Supply.Sum() - Demand.Sum();
            if (difference > 0)
            {
                Demand.Add(difference);
                var newColumn = new List<double>(Enumerable.Repeat(0d, Supply.Count()));
                CostMatrix.AddColumn(newColumn);
            }
            else
            {
                Supply.Add(-difference);
                var newRow = new List<double>(Enumerable.Repeat(0d, Demand.Count()));
                CostMatrix.AddRow(newRow);
            }
            return difference;
        }

        public override string ToString()
        {
            return CostMatrix.ToStringWithDualMarkers();
        }
    }
}
