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
    internal static class MulticriteriaSolver
    {
        public static MulticriteriaSolution Solve(InequalitySystem system, List<GoalFunction> goalFunctions, IComputationReportCompiler? compiler = null)
        {
            var solutionsSimplex = SolveSimplex(system, goalFunctions, compiler);
            var solutionMatrix = BuildSolutionMatrix(solutionsSimplex);

            compiler?.AddMatrix($"Отримали k={solutionsSimplex.Count} оптимальних вектори", solutionMatrix);
            var goalFuncCoefMatrix = BuildGoalFuncCoefMatrix(goalFunctions);
            compiler?.AddMatrix($"Матриця коефіцієнтів цільових функцій", goalFuncCoefMatrix);
            var lossMatrix = BuildLossMatrix(solutionMatrix, goalFuncCoefMatrix);
            lossMatrix *= -1;
            compiler?.AddMatrix("Пошук матриці неоптимальних розв'язків", lossMatrix);
            var maxElem = lossMatrix.GetMaxElement().Value;
            compiler?.AddAction($"max = {maxElem}");
            var gameMatrix = (lossMatrix*(-1)).TransformElements(x=> { return x + maxElem; });
            var matrixGameSolution = MatrixGameSolver.Solve(gameMatrix,50, compiler);
            compiler?.AddAction($"Вагові коефіцієнти розв'язків: {string.Join("; ",matrixGameSolution.X)}");
            var compromiseSolution = CalculateCompromiseSolution(solutionsSimplex, matrixGameSolution.X.ToList());
            compiler?.AddAction($"Компромісне рішення X*(компр.): {string.Join("; ", compromiseSolution)}");
            return new MulticriteriaSolution(compromiseSolution,matrixGameSolution,gameMatrix,lossMatrix, solutionsSimplex);
        }

        private static List<InequalitySystemSolution> SolveSimplex(InequalitySystem system, List<GoalFunction> goalFunctions, IComputationReportCompiler? compiler = null)
        {
            var solutions = new List<InequalitySystemSolution>();
            foreach (var goalFunction in goalFunctions)
            {
                var solution = LinearInequalitySolver.Solve(system, goalFunction, compiler);
                solutions.Add(solution);
            }
            return solutions;
        }

        private static Matrix BuildSolutionMatrix(List<InequalitySystemSolution> solutions)
        {
            var matrix = new Matrix(solutions.Count, solutions[0].SolutionCoefficients.Count);
            for (int i = 0; i < solutions.Count; i++)
            {
                for (int j = 0; j < solutions[i].SolutionCoefficients.Count; j++)
                {
                    matrix[i, j] = solutions[i].SolutionCoefficients[j];
                }
            }
            return matrix;
        }
        private static Matrix BuildGoalFuncCoefMatrix(List<GoalFunction> goalFunctions)
        {
            var matrix = new Matrix(goalFunctions.Count, goalFunctions[0].Coefficients.Count);
            for (int i = 0; i < goalFunctions.Count; i++)
            {
                for (int j = 0; j < goalFunctions[i].Coefficients.Count; j++)
                {
                    matrix[i, j] = goalFunctions[i].Coefficients[j];
                }
            }
            return matrix;
        }

        private static Matrix BuildLossMatrix(Matrix solutionMatrix, Matrix goalFuncCoefMatrix)
        {
            var matrix = new Matrix(goalFuncCoefMatrix.RowCount, goalFuncCoefMatrix.RowCount);
            for (int i = 0; i < goalFuncCoefMatrix.RowCount; i++)
            {
                for (int j = 0; j < goalFuncCoefMatrix.RowCount; j++)
                {
                    var dotProduct = (List<double> x, List<double> y) =>
                    {
                        var sum = 0d;
                        foreach(var a in x.Zip(y, (a, b) => (a, b)))
                        {
                            sum += a.a * a.b;
                        }
                        return sum;
                    };
                    var q = (dotProduct(solutionMatrix[i], goalFuncCoefMatrix[j]) - dotProduct(solutionMatrix[j], goalFuncCoefMatrix[j])) / dotProduct(solutionMatrix[j], goalFuncCoefMatrix[j]);
                    matrix[i, j] = q;
                }
            }
            return matrix;
        }
        private static List<double> CalculateCompromiseSolution (List<InequalitySystemSolution> solutions, List<double> weights)
        {
            var compromiseSolution = new List<double>();
            for (int i = 0; i < solutions[0].SolutionCoefficients.Count; i++)
            {
                var sum = 0d;
                for (int j = 0; j < solutions.Count; j++)
                {
                    sum += weights[j] * solutions[j].SolutionCoefficients[i];
                }
                compromiseSolution.Add(sum);
            }
            return compromiseSolution;
        }
    }
}
