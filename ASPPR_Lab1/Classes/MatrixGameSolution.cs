using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPPR_Lab2.Classes
{
    internal class MatrixGameSolution
    {
        public IEnumerable<double> X { get; set; }
        public IEnumerable<double>? Y { get; set; }
        public double? V { get; set; }

        public List<List<string>>? Model { get; set; }

        public MatrixGameSolution(IEnumerable<double> x, IEnumerable<double>? y = null, double? v = null, List<List<string>>? model = null)
        {
            X = x;
            Y = y;
            V = v;
            Model = model;
        }

        public override string ToString()
        {
            return $"X: ({string.Join(';', X)})\n {(Y is not null? $"Y: ({string.Join(';', Y)})\n" : "")}{(V is not null? $"v = {V}" : "")}";
        }
    }
}
