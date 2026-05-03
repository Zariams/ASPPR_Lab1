using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab2.Classes
{
    internal class TransportSolution
    {
        public Matrix TransportPlan { get; set; }
        public double TotalCost { get; set; }
        public int LackOfSupply { get; set; }
        public int LackOfDemand { get; set; }
        public TransportSolution(Matrix transportPlan, double totalCost)
        {
            TransportPlan = transportPlan;
            TotalCost = totalCost;
        }
        public override string ToString()
        {
            return $"Transport Plan:\n{TransportPlan}\nTotal Cost: {TotalCost}\nLacking supplies: {LackOfSupply}. \t Demand: {LackOfDemand}";
        }
    }
}
