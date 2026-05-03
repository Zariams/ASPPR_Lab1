using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPPR_Lab1;
using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab2.Classes.Static
{
    internal static class TransportSolver
    {

        public static TransportSolution Solve(TransportInput input, int referenceSolutionMethod = 1, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Згенерований протокол обчислення");
            compiler?.AddAction("Вхідні дані", input.ToString());
            var difference = 0;
            if (!input.IsClosed())
            {
                compiler?.AddAction("Система не є закритою, додаємо фіктивні склади/точки збуту");
                difference = input.Close();
                compiler?.AddAction("Вхідні дані після закриття системи", input.ToString());
            }
            var referenceSolution = FindReferenceSolution(input, referenceSolutionMethod, compiler);
            compiler?.AddMatrix("Початкове опорне рішення", referenceSolution.TransportPlan);
            compiler?.AddAction("Ціна початкового опорного рішення", referenceSolution.TotalCost.ToString());
            compiler?.AddAction("Переходимо до пошуку оптимального рішення методом потенціалів");
            var optimalSolution = FindOptimalSolution(input, referenceSolution, compiler);
            optimalSolution.LackOfSupply = difference > 0 ? difference : 0;
            optimalSolution.LackOfDemand = difference < 0 ? -difference : 0;
            return optimalSolution;
        }

        public static TransportSolution FindReferenceSolution(TransportInput input, int method = 1, IComputationReportCompiler? compiler = null)
        {
            if (method == 1) return ReferenceNorthWestCorner(input, compiler);
            else if (method == 2) return ReferenceLeastCost(input, compiler);
            else if (method == 3) return ReferenceSimplexMethod(input, compiler);
            else throw new ArgumentException("Invalid method for finding reference solution. Use 1 for North-West Corner and 2 for Least Cost.");
        }

        public static TransportSolution ReferenceNorthWestCorner(TransportInput input, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Знаходимо початкове опорне рішення методом Північно-Західного кута");
            var transportPlan = new Matrix(input.CostMatrix.RowCount, input.CostMatrix.ColCount);
            bool canContinue = true;
            int row = 0, col = 0;
            var sb = new StringBuilder();
            while (canContinue)
            {
                var maxVal = MaxValueInTransportPlanCell(transportPlan, input, row, col);
                transportPlan[row][col] = maxVal;
                sb.Append($"x{row}{col} = {maxVal}; ");
                var unfulfilledDemand = GetUnfulfilledDemand(transportPlan, input, col);
                if (unfulfilledDemand > 0 && row < transportPlan.RowCount-1)
                {
                    row++;
                }
                else if (col < transportPlan.ColCount-1)
                {
                    col++;
                }
                else
                {
                    canContinue = false;
                }
            }
            compiler?.AddAction("Послідовність заповнення таблиці:", sb.ToString());
            var totalCost = (int)transportPlan.MultiplyElementwise(input.CostMatrix).Sum();
            return new TransportSolution(transportPlan, totalCost);
        }

        public static TransportSolution ReferenceLeastCost(TransportInput input, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Знаходимо початкове опорне рішення методом Найменшої вартості");
            var transportPlan = new Matrix(input.CostMatrix.RowCount, input.CostMatrix.ColCount);
            bool canContinue = true;
            var minElement = 0;
            var sb = new StringBuilder();
            
            while (canContinue)
            {
                var minCostCell = input.CostMatrix.GetMinElement(x => x > minElement);
                if (minCostCell is null)
                {
                    canContinue = false;
                    break;
                }
                sb.Append($"| min = {minCostCell}, ");
                var matchingCells = input.CostMatrix.GetMatchingElements(x => Math.Abs(x - minCostCell.Value) <= Double.Epsilon);
                foreach (var cell in matchingCells)
                {
                    var i = cell.Item1;
                    var j = cell.Item2;
                    var maxVal = MaxValueInTransportPlanCell(transportPlan, input, i, j);
                    if (maxVal > 0)
                    {
                        sb.Append($"x{i}{j} = {maxVal}; ");
                        transportPlan[i][j] = maxVal;
                    }

                }
                minElement = (int)minCostCell.Value;
            }
            compiler?.AddAction("Послідовність заповнення таблиці:", sb.ToString());
            var totalCost = (int)transportPlan.MultiplyElementwise(input.CostMatrix).Sum();
            return new TransportSolution(transportPlan, totalCost);
        }

        public static TransportSolution ReferenceSimplexMethod(TransportInput input, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction("Знаходимо початкове опорне рішення симплекс-методом");
            var transportPlan = new Matrix(input.CostMatrix.RowCount, input.CostMatrix.ColCount);
            var coefficients = input.CostMatrix.Rows.SelectMany(r => r).ToList();
            var Z = new GoalFunction(coefficients, GoalFunctionType.Minimize);
            var inequalities = new List<Inequality>();
            for (int i = 0; i < input.Supply.Count; i++)
            {
                var coefficientsForInequality = new List<double>();
                for (int j = 0; j < coefficients.Count; j++)
                {
                    var row = j / input.CostMatrix.ColCount;
                    coefficientsForInequality.Add(row == i ? 1 : 0);
                }
                inequalities.Add(new Inequality(coefficientsForInequality, input.Supply[i], Sign.LessOrEqual));
            }
            for (int i = 0; i < input.Demand.Count; i++)
            {
                var coefficientsForInequality = new List<double>();
                for (int j = 0; j < coefficients.Count; j++)
                {
                    var col = j % input.CostMatrix.ColCount;
                    coefficientsForInequality.Add(col == i ? 1 : 0);
                }
                inequalities.Add(new Inequality(coefficientsForInequality, input.Demand[i], Sign.GreaterOrEqual));
            }
            var A = new InequalitySystem(inequalities);
            compiler?.AddAction("Постановка задачі:", Z.ToString(), 0);
            compiler?.AddAction("При обмеженнях:", A.ToString(), 0);
            compiler?.AddAction("Перепишемо систему обмежень:", A.ToStringWithZeroes(), 0);

            var matrix = LinearInequalitySolver.ConvertInputToMatrix(A, Z);
            compiler?.AddAction("Вхідна симлекс-таблиця:", matrix.ToStringWithDualMarkers(), 0);
            var referenceSolution = LinearInequalitySolver.GetReferenceSolution(matrix, Z, compiler,coefficients.Count);
            var totalCost = referenceSolution.GoalFunctionValue;
            for (int i = 0; i < referenceSolution.SolutionCoefficients.Count; i++)
            {
                var row = i / input.CostMatrix.ColCount;
                var col = i % input.CostMatrix.ColCount;
                transportPlan[row][col] = (int)referenceSolution.SolutionCoefficients[i];
            }
            return new TransportSolution(transportPlan, totalCost);
        }

        public static TransportSolution FindOptimalSolution(TransportInput input, TransportSolution referenceSolution, IComputationReportCompiler? compiler = null)
        {
            var transportPlan = referenceSolution.TransportPlan;
           
            bool isOptimal = false;
            var cost = referenceSolution.TotalCost;
            while (!isOptimal)
            {
                var (supplyPotentials, demandPotentials) = CalculatePotentials(input, transportPlan);
                compiler?.AddAction($"Потенціали складів: {string.Join("; ",supplyPotentials)}");
                compiler?.AddAction($"Потенціали точок збуту: {string.Join("; ", demandPotentials)}");
                var cellsForOptimization = GetCellsForOptimization(input, transportPlan, supplyPotentials, demandPotentials);
                if (cellsForOptimization.Count == 0)
                {
                    compiler?.AddAction("Умова оптимальності виконується. План оптимальний");
                    isOptimal = true;
                    break;
                }
                compiler?.AddAction($"Проблемні клітинки: {string.Join("; ", cellsForOptimization.Select(x => $"({x.r},{x.c})"))}");
                compiler?.AddAction("Умова оптимальності не виконується.");
                var bestCell = cellsForOptimization.OrderByDescending(c => c.v).First();
                compiler?.AddAction($"Вибираємо клітинку для оптимізації: ({bestCell.r},{bestCell.c}) з різницею {bestCell.v}");
                var optimizedPlan = GetOptimizedTransportPlan(transportPlan, bestCell.r, bestCell.c,compiler);
                transportPlan = optimizedPlan;
                compiler?.AddMatrix("Новий план",transportPlan);
                cost = (int)transportPlan.MultiplyElementwise(input.CostMatrix).Sum();
                compiler?.AddAction($"Вартість перевезень за новим планом: {cost}");
            }
            return new TransportSolution(transportPlan, cost);
        }

        private static int MaxValueInTransportPlanCell(Matrix transportPlan, TransportInput input, int row, int col)
        {
            var maxVal = 0;
            var leftOverSupply = GetLeftoverSupply(transportPlan, input, row);
            var unfulfilledDemand = GetUnfulfilledDemand(transportPlan, input, col);
            if (leftOverSupply > 0 && unfulfilledDemand > 0)
            {
                maxVal = Math.Min(leftOverSupply, unfulfilledDemand);
            }
            return maxVal;
        }

        private static int GetLeftoverSupply(Matrix transportPlan, TransportInput input, int row)
        {
            var rowSum = (int)transportPlan[row].Sum();
            return input.Supply[row] - rowSum;
        }
        private static int GetUnfulfilledDemand(Matrix transportPlan, TransportInput input, int col)
        {
            var colSum = (int)transportPlan.GetColumnAsList(col).Sum();
            return input.Demand[col] - colSum;
        }
        private static (List<int> SupplyPotentials, List<int> DemandPotentials) CalculatePotentials(TransportInput input, Matrix transportPlan)
        {
            var supplyPotentials = new List<int>(new int[input.Supply.Count]);
            var demandPotentials = new List<int>(new int[input.Demand.Count]);
            var filledCells = transportPlan.GetMatchingElements(x => x > 0);
            

            for (int i = 0; i < supplyPotentials.Count; i++)
            {
                for (int j = 0; j < demandPotentials.Count; j++)
                {
                    if (!filledCells.Any(c => c.Item1 == i && c.Item2 == j)) continue;
                    var cost = (int)input.CostMatrix[i][j];
                    if (demandPotentials[j] == 0)
                    {
                        demandPotentials[j] = cost - supplyPotentials[i];
                    }
                    else
                    {
                        supplyPotentials[i] = cost - demandPotentials[j];
                    }
                }
            }
            return (supplyPotentials, demandPotentials);
        }

        private static List<(int r, int c, int v)> GetCellsForOptimization(TransportInput input, Matrix transportPlan, List<int> supplyPotentials, List<int> demandPotentials)
        {
            var optimizationPlan = new List<(int r, int c, int v)>();
            for (int i = 0; i < transportPlan.RowCount; i++)
            {
                for (int j = 0; j < transportPlan.ColCount; j++)
                {
                    if (transportPlan[i][j] == 0)
                    {
                        var cost = (int)input.CostMatrix[i][j];
                        var potentialCost = supplyPotentials[i] + demandPotentials[j];
                        var diff = potentialCost - cost;
                        if (diff > 0)
                        {
                            optimizationPlan.Add((i, j, diff));
                        }
                    }
                }
            }
            return optimizationPlan;
        }

        private static Matrix GetOptimizedTransportPlan(Matrix transportPlan, int enteringRow, int enteringCol, IComputationReportCompiler? compiler = null)
        {
            compiler?.AddAction($"Будуємо цикл для оптимізації проблемної точки x{enteringRow}{enteringCol}");
            var optimizedPlan = transportPlan.DeepCopy();
            optimizedPlan[enteringRow][enteringCol] = 1;
            var traversedCells = new List<(int r, int c, int v, int s)>();
            var traversalMatrix = new Matrix(transportPlan.RowCount, transportPlan.ColCount);
            
            for (int i = 0; i < optimizedPlan.RowCount; i++)
            {
                for (int j = 0; j < optimizedPlan.ColCount; j++)
                {
                    if (optimizedPlan[i][j] == 0)
                    {
                        traversalMatrix[i][j] = 100;
                        continue;
                    }
                    var otherNumbersInRow = optimizedPlan[i].Select(x => x).ToList();
                    otherNumbersInRow.RemoveAt(j);
                    otherNumbersInRow = otherNumbersInRow.Where(x => x > 0).ToList();
                    var otherNumbersInCol = optimizedPlan.GetColumnAsList(j);
                    otherNumbersInCol.RemoveAt(i);
                    otherNumbersInCol = otherNumbersInCol.Where(x => x > 0).ToList();
                    if (otherNumbersInRow.Count > 0 && otherNumbersInCol.Count > 0)
                    {
                        traversalMatrix[i][j] = 1;
                    }
                    else if (otherNumbersInRow.Count > 0 || otherNumbersInCol.Count > 0)
                    {
                        traversalMatrix[i][j] = 100;
                    }
                }
            }
            traversalMatrix[enteringRow,enteringCol] = 0;
            compiler?.AddMatrix("Матриця ваг для побудови циклу оптимізації (0 - початкова точка, 1 - найоптимальніша вершина циклу, 100 - найменш оптимальна)", traversalMatrix);
            var cellTraversalStack = new Stack<(int r, int c, int v, int s, int d)>(); // d - direction: 1 for row, -1 for column
            var cellsToVisit = traversalMatrix.GetMatchingElements(x => true).Select(c => (c.Item1, c.Item2, (int)traversalMatrix[c.Item1,c.Item2])).ToList();
            if (cellsToVisit.Where(c => c.Item3 <= 1).Count() > 3) cellsToVisit = cellsToVisit.Where(c => c.Item3 <= 1).ToList();
            cellTraversalStack.Push((enteringRow, enteringCol, 0, 1, 0));
            do
            {
                var cell = cellTraversalStack.Pop();
                traversedCells.Add((cell.r, cell.c, cell.v, cell.s));
                
                List<(int, int,int) > nextCells;
                if (cell.d == 0) //any direction
                {
                    nextCells = cellsToVisit.Where(c => (c.Item1 == cell.r || c.Item2 == cell.c) && !(c.Item1 == cell.r && c.Item2 == cell.c)).ToList();
                }
                else if (cell.d > 0) //from same column
                {
                    nextCells = cellsToVisit.Where(c => c.Item1 == cell.r && c.Item2 != cell.c).ToList();
                }
                else //from same row
                {
                    nextCells = cellsToVisit.Where(c => c.Item2 == cell.c && c.Item1 != cell.r).ToList();
                }
                
                nextCells = nextCells.OrderByDescending(c => c.Item3 + Math.Abs(c.Item1 - enteringRow) + Math.Abs(c.Item2 - enteringCol)).ToList();

                for (int i = 0; i < nextCells.Count(); i++)
                {
                    var nextCell = nextCells[i];
                    var direction = nextCell.Item1 == cell.r ? -1 : 1;
                    cellTraversalStack.Push((nextCell.Item1, nextCell.Item2, (int)transportPlan[nextCell.Item1,nextCell.Item2], -cell.s, direction));
                }


            } while (cellTraversalStack.Peek().r != enteringRow || cellTraversalStack.Peek().c != enteringCol);
            var minLambda = traversedCells.Where(c => c.s < 0).Min(c => c.v);

            compiler?.AddAction($"Цикл побудовано. Пройдені точки: {string.Join(" -> ", traversedCells.Select(x => $"x{x.r}{x.c} = {x.v} {(x.s < 0? '-': '+')} λ"))}");
            compiler?.AddAction($"λ = {minLambda}");
            foreach (var cell in traversedCells)
            {
                if (cell.s > 0)
                {
                    optimizedPlan[cell.r][cell.c] += minLambda;
                }
                else
                {
                    optimizedPlan[cell.r][cell.c] -= minLambda;
                }
            }
            optimizedPlan[enteringRow][enteringCol] -= 1;
            return optimizedPlan;
        }
    }
}
