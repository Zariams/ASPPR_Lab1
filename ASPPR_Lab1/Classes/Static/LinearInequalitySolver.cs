using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ASPPR_Lab1;
using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab2.Classes.Static
{
    internal static class LinearInequalitySolver
    {
        public static InequalitySystemSolution Solve(InequalitySystem A, GoalFunction Z)
        {
            var solution1 =  GetReferenceSolution(A, Z);

            if (solution1 == null)
            {
                return new InequalitySystemSolution(new List<double>(), new Matrix(0,0),Z.Type, 0, false, false, true);
            }

            var solution2 = GetOptimalSolution(A, Z, solution1);
            if (solution2 == null)
            {
                return solution1;
            }
            return solution2;
        }

        public static InequalitySystemSolution? GetReferenceSolution(InequalitySystem A, GoalFunction Z)
        {
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            var success = false;
            do
            {
                var negativeNumbersInConstantCol = FindNumberRowsInColumn(matrix, matrix.ColCount - 1, x => x < 0);
                if (!negativeNumbersInConstantCol.Any())
                {
                    success = true;
                    break;
                }
                var firstNegNumPos = negativeNumbersInConstantCol.First();
                var solutionCols = FindNumberColsInRow(matrix, firstNegNumPos, x => x < 0).Where(col => col != matrix.ColCount - 1);
                if (!solutionCols.Any())
                {
                    success = false;
                    break;
                }
                var firstSolutionCol = solutionCols.First();
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount-1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    if (ratio < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }
                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
            } while(!success);
            var result = ParseSolution(matrix);
            return success? new InequalitySystemSolution(result,matrix,Z.Type, Z.CalculateResultWithValues(result), false, false, false): null;
        }
        public static InequalitySystemSolution? GetOptimalSolution(InequalitySystem A, GoalFunction Z, InequalitySystemSolution referenceSolution)
        {
            var matrix = referenceSolution.SolutionMatrix;
            var success = false;
            do
            {
                var negativeNumbersInZRow = FindNumberColsInRow(matrix, matrix.RowCount - 1, x => x < 0);
                if (!negativeNumbersInZRow.Any())
                {
                    success = true;
                    break;
                }
                var firstSolutionCol = negativeNumbersInZRow.First();
               
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1: 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }

                if (solutionRow == -1)
                {
                    success = false;
                    break;
                }
                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
            } while (!success);
            var result = ParseSolution(matrix);
            return success ? new InequalitySystemSolution(result, matrix, Z.Type, Z.CalculateResultWithValues(result), true, false, false) : null;
        }

        private static List<double> ParseSolution(Matrix A)
        {
            var solution = new List<double>(Enumerable.Repeat(0d, A.ColCount-1));
            for (int i = 0; i < A.RowCount - 1; i++)
            {
                var rowMarker = A.RowMarkers[i];
                if (rowMarker.StartsWith("x"))
                {
                    var variableIndex = int.Parse(rowMarker.Substring(1));
                    solution[variableIndex-1] = A[i, A.ColCount - 1];
                }
            }
            return solution;
        }

        private static Matrix ConvertInputToMatrix(InequalitySystem A, GoalFunction Z)
        {
            var matrix = A.ConvertToMatrix();
            var zRow = Z.ConvertToMatrix();
            matrix.AddRow(zRow[0]);
            Console.WriteLine(matrix);
            return matrix;
        }

        private static IEnumerable<int> FindNumberPositionsInList(List<double> list, Func<double, bool> predicate)
        {
            return list
                .Select((value, index) => new { value, index })
                .Where(x => predicate(x.value))
                .Select(x => x.index);
        }

        private static IEnumerable<int> FindNumberRowsInColumn(Matrix A,int col,Func<double, bool> predicate)
        {
            var column = A.GetColumnAsList(col);

            return FindNumberPositionsInList(column, predicate);
        }

        private static IEnumerable<int> FindNumberColsInRow(Matrix A, int row, Func<double, bool> predicate)
        {
            return FindNumberPositionsInList(A[row], predicate);
        }
    }
}
