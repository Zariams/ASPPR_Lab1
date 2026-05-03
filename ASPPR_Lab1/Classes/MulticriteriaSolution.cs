using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPPR_Lab1;
using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab2.Classes
{
    internal class MulticriteriaSolution
    {
        public List<double> CompromiseSolution { get; set; }
        public MatrixGameSolution GameSolution { get; set; }
        public Matrix GameMatrix { get; set; }
        public Matrix LossMatrix { get; set; }
        public List<InequalitySystemSolution> SimplexSolutions { get; set; }

        public MulticriteriaSolution(List<double> compromiseSolution, MatrixGameSolution gameSolution, Matrix gameMatrix, Matrix lossMatrix, List<InequalitySystemSolution> simplexSolutions)
        {
            CompromiseSolution = compromiseSolution;
            GameSolution = gameSolution;
            GameMatrix = gameMatrix;
            LossMatrix = lossMatrix;
            SimplexSolutions = simplexSolutions;
        }

        public override string ToString()
        {
            return $"Компромісне рішення: ({string.Join("; ", CompromiseSolution)})\n" +
                   $"Розв'язок матриці гри:\n{GameSolution}\n" +
                   $"Матриця гри:\n{GameMatrix}\n" +
                   $"Матриця неоптимальних розв'язків:\n{LossMatrix}\n" +
                   $"Розв'язки симплекс-методу для кожної цільової функції:\n{string.Join("\n", SimplexSolutions.Select(s => s.ToString()))}";
        }
    }
}
