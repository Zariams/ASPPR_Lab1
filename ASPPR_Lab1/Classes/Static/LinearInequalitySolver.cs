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
        public static InequalitySystemSolution Solve(InequalitySystem A, GoalFunction Z, IComputationReportCompiler compiler = null)
        {
            if (A.Inequalities.Any(ineq => ineq.Sign == Sign.Equals))
            {
                return SolveMixedSystem(A, Z, compiler);
            }
            else
            {
                return SolveNormal(A, Z, compiler);
            }
        }
        public static InequalitySystemSolution SolveNormal(InequalitySystem A, GoalFunction Z, IComputationReportCompiler compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення",  titleLevel:4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);

            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);

            var solution1 =  GetReferenceSolution(matrix, Z, compiler);

            if (solution1 == null)
            {
                return new InequalitySystemSolution(new List<double>(), new Matrix(0,0),Z.Type, 0, false, false, true);
            }

            var solution2 = GetOptimalSolution(A, Z, solution1, compiler);
            if (solution2 == null)
            {
                return solution1;
            }
            return solution2;
        }
        private static InequalitySystemSolution? GetReferenceSolution(Matrix matrix, GoalFunction Z, IComputationReportCompiler? compiler = null, int varCount = 0)
        {
            compiler?.AddAction("Пошук опорного розв'язку:", titleLevel: 2);

            var success = false;
            do
            {
                var negativeNumbersInConstantCol = FindNumberRowsInColumn(matrix, matrix.ColCount - 1, x => x < 0);
                if (!negativeNumbersInConstantCol.Any())
                {
                    success = true;
                    compiler?.AddAction("Опорний розв'язок знайдено!", titleLevel: 1);
                    break;
                }
                var firstNegNumPos = negativeNumbersInConstantCol.First();
                var solutionCols = FindNumberColsInRow(matrix, firstNegNumPos, x => x < 0).Where(col => col != matrix.ColCount - 1);
                if (!solutionCols.Any())
                {
                    success = false;
                    compiler?.AddAction("Помилка! Система обмежень є суперечливою!", titleLevel: 1);
                    throw new Exception("Помилка! Система обмежень є суперечливою!");
                }
                var firstSolutionCol = solutionCols.First();
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }
                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithMarkers(), 0);
            } while (!success);
            if (!success) return null;
            var resultList = ParseSolution(matrix,varCount);
            var result = new InequalitySystemSolution(resultList, matrix, Z.Type, Z.CalculateResultWithValues(resultList), false, false, false);
            compiler?.AddAction("Знайдено опорний розв'язок:", result.ToString(), titleLevel: 1);
            return result;
        }

        public static InequalitySystemSolution? GetReferenceSolutionStandalone(InequalitySystem A, GoalFunction Z, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);
            if (A.Inequalities.Any(ineq => ineq.Sign == Sign.Equals))
            {
                matrix = CrossOutZeroRows(matrix, compiler);
            }
            var success = false;
            do
            {
                var negativeNumbersInConstantCol = FindNumberRowsInColumn(matrix, matrix.ColCount - 1, x => x < 0);
                if (!negativeNumbersInConstantCol.Any())
                {
                    success = true;
                    compiler?.AddAction("Опорний розв'язок знайдено!", titleLevel: 1);
                    break;
                }
                var firstNegNumPos = negativeNumbersInConstantCol.First();
                var solutionCols = FindNumberColsInRow(matrix, firstNegNumPos, x => x < 0).Where(col => col != matrix.ColCount - 1);
                if (!solutionCols.Any())
                {
                    success = false;
                    compiler?.AddAction("Помилка! Система обмежень є суперечливою!", titleLevel: 1);
                    throw new Exception("Помилка! Система обмежень є суперечливою!");
                }
                var firstSolutionCol = solutionCols.First();
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount - 1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
                    var signum2 = double.Sign(col2[i]) == 0 ? 1 : double.Sign(col2[i]);
                    if (ratio < 0 || signum1 * signum2 < 0) continue;
                    if (ratio < minimalNonNegativeRatio)
                    {
                        minimalNonNegativeRatio = ratio;
                        solutionRow = i;
                    }
                }
                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithMarkers(), 0);
            } while (!success);
            if (!success) return null;
            var resultList = ParseSolution(matrix, A.VariableCount);
            var result = new InequalitySystemSolution(resultList, matrix, Z.Type, Z.CalculateResultWithValues(resultList), false, false, false);
            compiler?.AddAction("Знайдено опорний розв'язок:", result.ToString(), titleLevel: 1);
            return result;
        }
        public static InequalitySystemSolution? GetOptimalSolution(InequalitySystem A, GoalFunction Z, InequalitySystemSolution referenceSolution, IComputationReportCompiler? compiler = null)
        {
            var matrix = referenceSolution.SolutionMatrix;
            compiler?.AddAction("Пошук оптимального розв'язку:", titleLevel: 2);
            var success = false;
            do
            {
                var negativeNumbersInZRow = FindNumberColsInRow(matrix, matrix.RowCount - 1, x => x < 0);
                if (!negativeNumbersInZRow.Any())
                {
                    success = true;
                    compiler?.AddAction("Оптимальний розв'язок вже знайдено!", titleLevel: 1);
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
                    compiler?.AddAction("Помилка! Функція не обмежена зверху!", titleLevel: 1);
                    throw new Exception("Помилка! Функція не обмежена зверху!");
                }
                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithMarkers(), 0);

            } while (!success);
            if (!success) throw new Exception("Оптимальне рішення не знайдено!");
            var resultList = ParseSolution(matrix,A.VariableCount);
            var result = new InequalitySystemSolution(resultList, matrix, Z.Type, Z.CalculateResultWithValues(resultList), true, false,  false);
            compiler?.AddAction("Знайдено оптимальний розв'язок:", result.ToString(), titleLevel: 1);
            return result;
        }

        private static List<double> ParseSolution(Matrix matrix, int varCount)
        {
            var solution = new List<double>(Enumerable.Repeat(0d, varCount));
            for (int i = 0; i < matrix.RowCount - 1; i++)
            {
                var rowMarker = matrix.RowMarkers[i];
                if (rowMarker.StartsWith("x"))
                {
                    var variableIndex = int.Parse(rowMarker.Substring(1));
                    solution[variableIndex-1] = matrix[i, matrix.ColCount - 1];
                }
            }
            return solution;
        }

        private static Matrix ConvertInputToMatrix(InequalitySystem A, GoalFunction Z)
        {
            var matrix = A.ConvertToMatrix();
            var zRow = Z.ConvertToMatrix();
            matrix.AddRow(zRow[0]);
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
    
        public static InequalitySystemSolution SolveMixedSystem(InequalitySystem A, GoalFunction Z, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);
            var crossedOutZeroRowsMatrix = CrossOutZeroRows(matrix, compiler);
            var solution1 = GetReferenceSolution(crossedOutZeroRowsMatrix, Z, compiler,A.VariableCount);

            if (solution1 == null)
            {
                return new InequalitySystemSolution(new List<double>(), new Matrix(0, 0), Z.Type, 0, false, false, true);
            }

            var solution2 = GetOptimalSolution(A, Z, solution1, compiler);
            if (solution2 == null)
            {
                return solution1;
            }
            return solution2;
        }
        public static Matrix CrossOutZeroRows(InequalitySystem A, GoalFunction Z, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення", titleLevel: 4);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);
            var matrix = ConvertInputToMatrix(A, Z);
            matrix.RowMarkers[matrix.RowCount - 1] = "Z";
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);
            var crossedOutZeroRowsMatrix = CrossOutZeroRows(matrix, compiler);
            compiler?.AddAction("Вихідна симлекс-таблиця:", crossedOutZeroRowsMatrix.ToStringWithMarkers(), 0);
            return crossedOutZeroRowsMatrix;
        }
        private static Matrix CrossOutZeroRows(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            matrix = matrix.DeepCopy();
            var success = false;
            do
            {
                var zeroRows = matrix.RowMarkers.Select((marker, index) => new { marker, index }).Where(x => x.marker.StartsWith("0")).Select(x => x.index);
                if (!zeroRows.Any())
                {
                    success = true; 
                    break;
                }
                int firstSolutionCol = -1;
                foreach (var row in zeroRows)
                {
                    var positiveNumbersInCoefRow = FindNumberColsInRow(matrix, row, x => x > 0);

                    firstSolutionCol = positiveNumbersInCoefRow.Any()? positiveNumbersInCoefRow.First(): firstSolutionCol;
                }
                if (firstSolutionCol == -1)
                {
                    success = false;
                    compiler?.AddAction("Помилка! Система обмежень є суперечливою!", titleLevel: 1);
                    throw new Exception("Система обмежень є суперечливою!");
                }
                var col1 = matrix.GetColumnAsList(firstSolutionCol);
                var col2 = matrix.GetColumnAsList(matrix.ColCount - 1);
                var solutionRow = -1;
                var minimalNonNegativeRatio = double.MaxValue;
                for (int i = 0; i < matrix.RowCount-1; i++)
                {
                    if (Math.Abs(col1[i]) <= Double.Epsilon) continue;
                    var ratio = col2[i] / col1[i];
                    var signum1 = double.IsNegative(col1[i]) ? -1 : 1;
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
                    success = true;
                    break;
                }
                

                compiler?.AddAction($"Розв'язувальний рядок:{solutionRow} ({matrix.RowMarkers[solutionRow]})", titleLevel: 0);
                compiler?.AddAction($"Розв'язувальний стовпець:{firstSolutionCol} ({matrix.ColMarkers[firstSolutionCol]})", titleLevel: 0);

                matrix = matrix.JordanExcludeModified(solutionRow, firstSolutionCol);
                if (matrix.ColMarkers[firstSolutionCol].StartsWith("0"))
                {
                    matrix.RemoveColumn(firstSolutionCol);
                    
                }
                compiler?.AddAction("Таблиця після виконання МЖВ:", matrix.ToStringWithMarkers(), 0);
            } while (!success);
            if (!success) return null;
            

            compiler?.AddAction("Вихідна симлекс-таблиця:", matrix.ToStringWithMarkers(), 0);
            return matrix;
        }
    
    }
}
