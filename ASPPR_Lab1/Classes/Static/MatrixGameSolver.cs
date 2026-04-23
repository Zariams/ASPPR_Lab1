using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ASPPR_Lab1;
using ASPPR_Lab1.ASPPR_Lab1;
using ASPPR_Lab2.Enums;

namespace ASPPR_Lab2.Classes.Static
{
    public static class MatrixGameSolver
    {
        internal static MatrixGameSolution Solve(Matrix matrix, int numberOfTries = 0, IComputationReportCompiler? compiler = null)
        {
            List<List<string>>? model = null;
            var maxMinStr = matrix.Rows.Select((row, index) => (Value: row.Min(), Row: index, Col: row.FindIndex(x => x == row.Min()))).MaxBy(x => x.Value);
            var minMaxStr = matrix.Columns.Select((col, index) => (Value: col.Max(), Row: col.FindIndex(x => x == col.Max()), Col: index)).MinBy(x => x.Value);
            compiler?.AddAction("Пошук сідлового елементу");
            compiler?.AddAction("Знайдено верхню ціну гри:", $"A[{maxMinStr.Row},{maxMinStr.Col}] = {maxMinStr.Value}");
            compiler?.AddAction("Знайдено нижню ціну гри:", $"A[{minMaxStr.Row},{minMaxStr.Col}] = {minMaxStr.Value}");
            if (maxMinStr.Value == minMaxStr.Value)
            {
                compiler.AddAction("Сідлову точку знайдено", $"Ціна гри: v = {maxMinStr.Value}");
                var vSimple = maxMinStr.Value;
                var xSimple = new List<double>(Enumerable.Repeat(0d, matrix.ColCount));
                var ySimple = new List<double>(Enumerable.Repeat(0d, matrix.RowCount));
                xSimple[maxMinStr.Row] = 1;
                ySimple[maxMinStr.Col] = 1;
                if (numberOfTries > 0)
                {
                    model = GetMatrixGameModel(matrix, xSimple, ySimple, vSimple, numberOfTries);
                }
                return new MatrixGameSolution(xSimple, ySimple, vSimple, model);
            }

            if (matrix.RowCount == 2 || matrix.ColCount == 2)
            {
                return Solve2xN(matrix, compiler);
            }
            compiler?.AddAction("Сідлову точку не знайдено");
            var k = Math.Abs(matrix.Rows.Select(row => row.Min()).Min());
            var initialSystem = matrix + k;
            initialSystem.AddRow(new List<double>(Enumerable.Repeat(-1d, matrix.ColCount)), "Z", "1");
            initialSystem.AddColumn(new List<double>(Enumerable.Repeat(1d, matrix.RowCount)), "1", "W");
            var A = new InequalitySystem(initialSystem, Sign.LessOrEqual);
            var Z = new GoalFunction(new List<double>(Enumerable.Repeat(1d, matrix.ColCount)));
            var solution = LinearInequalitySolver.Solve(A, Z, compiler);
            var vInv = solution.GoalFunctionValue;
            compiler.AddAction("Знайдено обернену ціну гри:", $"v^-1 = {vInv}");
            var v = 1 / vInv;
            compiler.AddAction("Знайдено ціну гри:", $"v = {v}");
            var X = solution.SolutionCoefficientsDual.Select(x => x * v).ToList();
            compiler.AddAction("Знайдено ймовірність стратегій гравця А:", $"X: ({string.Join(';', X)})");
            var Y = solution.SolutionCoefficients.Select(y => y * v).ToList();
            compiler.AddAction("Знайдено ймовірність стратегій гравця В:", $"Y: ({string.Join(';', Y)})");
            v = v - k;
            compiler.AddAction("Відновлено ціну гри:", $"v = {v}");
            if (numberOfTries > 0)
            {
                model = GetMatrixGameModel(matrix, X, Y, v, numberOfTries);
            }
            return new MatrixGameSolution(X, Y, v, model);
        }

        internal static List<List<string>> GetMatrixGameModel(Matrix matrix, List<double> X, List<double> Y, double v, int n)
        {
            var header = new List<string>() {
                "Номер партії",
                "Випадкове число гравця А",
                "Стратегія гравця А",
                "Випадкове число гравця В",
                "Стратегія гравця В",
                "Виграш гравця А",
                "Накопичений виграш А",
                "Середній виграш А (ціна гри)"
            };
            var rows = new List<List<string>>();
            rows.Add(header);
            var accumulatedWinA = 0d;
            for (int i = 1; i <= n; i++)
            {
                var randA = new Random().NextDouble();
                var randB = new Random().NextDouble();
                var stratA = 0;
                var r = randA;
                foreach (var strat in X.Select((x, index) => (Value: x, Index: index)).OrderByDescending(x => x.Value))
                {
                    if (r < strat.Value)
                    {
                        stratA = strat.Index + 1;
                        break;
                    }
                    r -= strat.Value;
                }
                var stratB = 0;
                r = randB;
                foreach (var strat in Y.Select((y, index) => (Value: y, Index: index)).OrderByDescending(y => y.Value))
                {
                    if (r < strat.Value)
                    {
                        stratB = strat.Index + 1;
                        break;
                    }
                    r -= strat.Value;
                }
                var winA = matrix[stratA - 1, stratB - 1];
                accumulatedWinA += winA;
                var avgWinA = accumulatedWinA / (i);
                var row = new List<string>() //Convert values to strings, double numbers must use dot separator instead of comma
                {
                    i.ToString(),
                    randA.ToString("F4",CultureInfo.InvariantCulture),
                    stratA.ToString(),
                    randB.ToString("F4",CultureInfo.InvariantCulture),
                    stratB.ToString(),
                    winA.ToString(CultureInfo.InvariantCulture),
                    accumulatedWinA.ToString(CultureInfo.InvariantCulture),
                    avgWinA.ToString(CultureInfo.InvariantCulture)
                };
                rows.Add(row);
            }
            return (rows);
        }

        internal static MatrixGameSolution Solve2xN(Matrix matrixOr, IComputationReportCompiler? compiler = null)
        {
            var X = new List<double>(Enumerable.Repeat(0d, matrixOr.RowCount));
            var Y = new List<double>(Enumerable.Repeat(0d, matrixOr.ColCount));
            var v = 0d;

            var matrix = matrixOr.DeepCopy();
            var removedRows = new List<int>();
            var removedCols = new List<int>();
            while (matrix.RowCount != matrix.ColCount)
            {
                compiler?.AddAction("Розмірність матриці не відповідає 2х2. Звуження матриці");
                if (matrix.RowCount > matrix.ColCount)
                {
                    var rowSums = matrix.Rows.Select((row, index) => (value: row.Sum(), index: index)).ToList();
                    var minRowIndex = rowSums.MinBy(x => x.value).index;
                    matrix.RemoveRow(minRowIndex);
                    compiler?.AddAction("Видалено рядок з найменшою сумою:", $"Рядок {minRowIndex + 1} (сума = {rowSums.MinBy(x => x.value).value})");
                    removedRows.Where(i => i >= minRowIndex).Select(i => minRowIndex++);
                    removedRows.Add(minRowIndex);
                }
                else
                {
                    var colSums = matrix.Columns.Select((col, index) => (value: col.Sum(), index: index)).ToList();
                    var maxColSum = colSums.MaxBy(x => x.value);
                    matrix.RemoveColumn(maxColSum.index);
                    compiler?.AddAction("Видалено стовпець з найбільшою сумою:", $"Стовпець {maxColSum.index + 1} (сума = {maxColSum.value})");
                    removedCols.Where(i => i >= maxColSum.index).Select(i => maxColSum.index++);
                    removedCols.Add(maxColSum.index);
                }
                compiler?.AddMatrix("Матриця після звуження", matrix);
            }

            var solution = Solve2x2(matrix, compiler);
            var x1 = solution.X.First();
            var x2 = solution.X.Last();
            var y1 = solution.Y.First();
            var y2 = solution.Y.Last();
            var unremovedXIndeces = Enumerable.Range(0, matrixOr.RowCount).Where(i => !removedRows.Contains(i)).ToList();
            var unremovedYIndeces = Enumerable.Range(0, matrixOr.ColCount).Where(i => !removedCols.Contains(i)).ToList();
            X[unremovedXIndeces.First()] = x1;
            X[unremovedXIndeces.Last()] = x2;
            Y[unremovedYIndeces.First()] = y1;
            Y[unremovedYIndeces.Last()] = y2;

            return new MatrixGameSolution(X, Y, solution.V);
        }

        internal static MatrixGameSolution Solve2x2(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Розмірність матриці 2х2. Застосування формул для розв'язання системи з двох рівнянь");
            var k11 = matrix[0, 0];
            var k12 = matrix[0, 1];
            var k21 = matrix[1, 0];
            var k22 = matrix[1, 1];

            var ky1 = k11 - k21;
            var ky2 = k12 - k22;

            var kx1 = k11 - k12;
            var kx2 = k21 - k22;

            var x1 = kx2 / (kx2 - kx1);
            var x2 = 1 - x1;

            var y1 = ky2 / (ky2 - ky1);
            var y2 = 1 - y1;

            var v = k11 * x1 + k21 * x2;
            compiler?.AddAction("Знайдено розв'язок гри за формулами", $"X: ({x1}; {x2}), Y: ({y1}; {y2}), v: {v}");
            var result = new MatrixGameSolution(new List<double>() { x1, x2 }, new List<double>() { y1, y2 }, v);
            return result;
        }

        internal static GameWithNatureSolution SolveGameWithNature(Matrix matrix, double? pessimismCoefficient = null, List<double>? probabilities = null, IComputationReportCompiler? compiler = null)
        {
            Dictionary<GameWithNatureAlgorithm, MatrixGameSolution> values = new Dictionary<GameWithNatureAlgorithm, MatrixGameSolution>();
            values[GameWithNatureAlgorithm.Vald] = SolveVald(matrix, compiler);
            values[GameWithNatureAlgorithm.Optimistic] = SolveOptimistic(matrix, compiler);
            if (pessimismCoefficient.HasValue)
            {
                values[GameWithNatureAlgorithm.Gurvits] = SolveGurvits(matrix, pessimismCoefficient.Value, compiler);
            }
            values[GameWithNatureAlgorithm.Savage] = SolveSavage(matrix, compiler);
            if (probabilities != null)
            {
                values[GameWithNatureAlgorithm.Bayesian] = SolveBayes(matrix, probabilities, compiler);
            }
            values[GameWithNatureAlgorithm.Laplace] = SolveLaplace(matrix, compiler);
            return new GameWithNatureSolution(values);
        }

        internal static MatrixGameSolution SolveVald(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            var minRows = matrix.Rows.Select((row, index) => (Value: row.Min(), Row: index, Col: row.FindIndex(x => x == row.Min())));
            var maxMinRows = minRows.GroupBy(x => x.Value).MaxBy(x => x.Key).Select(x => x.Row);
            compiler?.AddAction("Розв'язок методом Вальдa", $"{string.Join(";\n",minRows.Select(x => $"Мінімальне значення у рядку {x.Row}: {x.Value}"))}");
            compiler?.AddAction($"Максимальний елемент: {minRows.Select(x => x.Value).Max()}");
            var probabilityList = new List<double>(Enumerable.Repeat(0d, matrix.RowCount));
            probabilityList = probabilityList.Select((x, index) => maxMinRows.Contains(index) ? 1d / maxMinRows.Count() : 0).ToList();
            var solution = new MatrixGameSolution(probabilityList);
            compiler?.AddAction($"Оптимальні стратегії: ({string.Join(';', solution.X.Select((value, index) => (Value: value, Index: index)).Where(x => Math.Abs(x.Value) > 0.001).Select(x => $"A{x.Index + 1}"))})");
            return solution;
        }

        internal static MatrixGameSolution SolveOptimistic(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            var maxRows = matrix.Rows.Select((row, index) => (Value: row.Max(), Row: index, Col: row.FindIndex(x => x == row.Max())));
            var minMaxRows = maxRows.GroupBy(x => x.Value).MaxBy(x => x.Key).Select(x => x.Row);
            compiler?.AddAction("Розв'язок оптимістичним методом", $"{string.Join(";\n", maxRows.Select(x => $"Максимальне значення у рядку {x.Row}: {x.Value}"))}");
            compiler?.AddAction($"Максимальний елемент: {maxRows.Select(x => x.Value).Max()}");
            var probabilityList = new List<double>(Enumerable.Repeat(0d, matrix.RowCount));
            probabilityList = probabilityList.Select((x, index) => minMaxRows.Contains(index) ? 1d / maxRows.Count() : 0).ToList();
            var solution = new MatrixGameSolution(probabilityList);
            compiler?.AddAction($"Оптимальні стратегії: ({string.Join(';', solution.X.Select((value, index) => (Value: value, Index: index)).Where(x => Math.Abs(x.Value) > 0.001).Select(x => $"A{x.Index + 1}"))})");
            return solution;
        }

        internal static MatrixGameSolution SolveGurvits(Matrix matrix, double pessimismCoefficient, IComputationReportCompiler? compiler = null)
        {
            var maxInRows = matrix.Rows.Select(row => row.Max()).ToList();
            var minInRows = matrix.Rows.Select(row => row.Min()).ToList();
            compiler?.AddAction("Розв'язок методом Гурвіца", $"{string.Join(";\n",maxInRows.Select((value, index) => $"{index}: max = {value}, min = {minInRows[index]}"))}");
            var gurvitsValues = maxInRows.Select((max, index) => pessimismCoefficient * minInRows[index] + (1 - pessimismCoefficient) * max).ToList();
            compiler?.AddAction($"{string.Join(";\n", gurvitsValues.Select((value, index) => $"s{index + 1}={value}"))}");
            var maxGurvits = gurvitsValues.Select((value, index) => (Value: value, Row: index)).GroupBy(x => x.Value).MaxBy(x => x.Key).Select(x => x.Row);
            compiler?.AddAction($"Максимальний елемент: {gurvitsValues.Max()}");
            var probabilityList = new List<double>(Enumerable.Repeat(0d, matrix.RowCount));
            probabilityList = probabilityList.Select((x, index) => maxGurvits.Contains(index) ? 1d / maxGurvits.Count() : 0).ToList();
            var solution = new MatrixGameSolution(probabilityList);
            compiler?.AddAction($"Оптимальні стратегії: ({string.Join(';', solution.X.Select((value, index) => (Value: value, Index: index)).Where(x => Math.Abs(x.Value) > 0.001).Select(x => $"A{x.Index + 1}"))})");
            return solution;
        }

        internal static MatrixGameSolution SolveSavage(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            var colMaxes = matrix.Columns.Select(col => col.Max()).ToList();
            var regretMatrix = new Matrix(matrix.RowCount, matrix.ColCount);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColCount; j++)
                {
                    regretMatrix[i, j] = colMaxes[j] - matrix[i, j];
                }
            }
            compiler?.AddAction("Розв'язок методом Савіджа");
            compiler?.AddMatrix("Матриця ризиків",regretMatrix);
            compiler?.AddAction($"{string.Join(";\n",regretMatrix.Rows.Select((value, index) => $"max в рядку {index + 1}: {value.Max()}"))}");
            var minMaxRegret = regretMatrix.Rows.Select(row => row.Max()).Min();
            compiler?.AddAction($"Мінімальний елемент: {minMaxRegret}");
            var bestStrategies = Enumerable.Range(0, matrix.RowCount).Where(i => regretMatrix.Rows[i].Max() == minMaxRegret).ToList();
            var probabilityList = new List<double>(Enumerable.Repeat(0d, matrix.RowCount));
            probabilityList = probabilityList.Select((x, index) => bestStrategies.Contains(index) ? 1d / bestStrategies.Count() : 0).ToList();
            var solution = new MatrixGameSolution(probabilityList);
            compiler?.AddAction($"Оптимальні стратегії: ({string.Join(';', solution.X.Select((value, index) => (Value: value, Index: index)).Where(x => Math.Abs(x.Value) > 0.001).Select(x => $"A{x.Index + 1}"))})");
            return solution;
        }

        internal static MatrixGameSolution SolveBayes(Matrix matrix, List<double> probabilities, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Розв'язок методом Байєса",$"Ймовірності застосування природою своїх стратегій: {string.Join("; ",probabilities.Select((value, index) => $"p{index} = {value}"))}");
            var expectedValues = matrix.Rows.Select(row => row.Select((x,index) => x * probabilities[index]).Sum()).ToList();
            compiler?.AddAction($"{string.Join(";\n", expectedValues.Select((value, index) => $"s{index} = {value}"))}",$"Максимальний елемент: {expectedValues.Max()}",0);
            var bestStrategies = expectedValues.Select((value, index) => (Value: value, Row: index)).GroupBy(x => x.Value).MaxBy(x => x.Key).Select(x => x.Row);
            var probabilityList = new List<double>(Enumerable.Repeat(0d, matrix.RowCount));
            probabilityList = probabilityList.Select((x, index) => bestStrategies.Contains(index) ? 1d / bestStrategies.Count() : 0).ToList();
            var solution = new MatrixGameSolution(probabilityList);
            compiler?.AddAction($"Оптимальні стратегії: ({string.Join(';', solution.X.Select((value, index) => (Value: value, Index: index)).Where(x => Math.Abs(x.Value) > 0.001).Select(x => $"A{x.Index + 1}"))})");
            return solution;
        }

        internal static MatrixGameSolution SolveLaplace(Matrix matrix, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Розв'язок методом Лапласа");
            var probabilities = new List<double>(Enumerable.Repeat(1d / matrix.ColCount, matrix.ColCount));
            return SolveBayes(matrix, probabilities, compiler);
        }
    }
}
