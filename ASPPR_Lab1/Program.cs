using System.Data;
using System.Diagnostics.CodeAnalysis;
using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab1
{
    internal class Program
    {


        static void Main(string[] args)
        {
            var A = new List<List<double>>()
            {
                new List<double>() {3, 1, 1 },
                new List<double>() {2, -2, 1 },
                new List<double>() {-1, -3, -2 },
            };

            var B = new List<List<double>>()
            {
                new List<double>() {1},
                new List<double>() {3},
                new List<double>() {4},
            };
            //var A = new List<List<double>>()
            //{
            //    new List<double>() {6, 2, 5 },
            //    new List<double>() {-3, 4, -1 },
            //    new List<double>() {1, 4, 3 },
            //};

            //var B = new List<List<double>>()
            //{
            //    new List<double>() {1},
            //    new List<double>() {6},
            //    new List<double>() {6},
            //};

            var res1 = LinearAlgebraicEquationSolver.SolveFirstMethod(new Matrix(A), new Matrix(B));
            Console.WriteLine($"First method:\n{res1}");

            var res2 = LinearAlgebraicEquationSolver.SolveSecondMethod(new Matrix(A), new Matrix(B));
            Console.WriteLine($"Second method:\n{res2}");

            var res3 = LinearAlgebraicEquationSolver.SolveGauss(new Matrix(A), new Matrix(B));
            Console.WriteLine($"Third method:\n{res3}");

            TestMatricesInversion();
            TestMatricesRankCalculation();
        }

        static void TestMatricesRankCalculation()
        {
            var testMatrices = new List<List<List<double>>>()
            {
                new List<List<double>>()
                {
                    new List<double>() {1, 2, 3, 4 },
                    new List<double>() {2, 4, 6, 8 },
                },
                new List<List<double>>()
                {
                    new List<double>() {1, 2},
                    new List<double>() {3, 6},
                    new List<double>() {5, 10},
                    new List<double>() {4, 8 },
                },
                new List<List<double>>()
                {
                    new List<double>() {6, 2, 5 },
                    new List<double>() {-3, 4, -1 },
                    new List<double>() {1, 4, 3 },
                },
                new List<List<double>>()
                {
                    new List<double>() {2, 5, 4 },
                    new List<double>() {-3, 1, -2 },
                    new List<double>() {-1, 6, 2 },
                },
                new List<List<double>>()
                {
                    new List<double>() {1, 2, 3, 4 },
                    new List<double>() {-2, 5, -1,3 },
                    new List<double>() {2, 4, 6, 8 },
                    new List<double>() {-1, 9, 2, 7 },
                },
                new List<List<double>>()
                {
                    new List<double>() {1, 2, 3, 4 },
                    new List<double>() {-2, 5, -1,3 },
                    new List<double>() {2, 4, 7, 8 },
                    new List<double>() {-1, 9, 2, 7 },
                },

            };

            int i = 0;
            foreach (var matrix in testMatrices.Select(m => new Matrix(m)))
            {
                i++;
                Console.WriteLine($"Test {i}");
                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Init matrix:\n {matrix}");
                Console.WriteLine($"Rank: {matrix.Rank}");
            }
        }

        static void TestMatricesInversion()
        {
            var testMatrices = new List<List<List<double>>>()
            {
                new List<List<double>>()
                {
                    new List<double>() {3, 1, 1 },
                    new List<double>() {2, -2, 1 },
                    new List<double>() {-1, -3, -2 },
                },
                new List<List<double>>()
                {
                    new List<double>() {5, -3, 7 },
                    new List<double>() {-1, 4, 3 },
                    new List<double>() {6, -2, 5 },
                },
                new List<List<double>>()
                {
                    new List<double>() {6, 2, 5 },
                    new List<double>() {-3, 4, -1 },
                    new List<double>() {1, 4, 3 },
                },
                new List<List<double>>()
                {
                    new List<double>() {2, -1, 3 },
                    new List<double>() {-1, 2, 2 },
                    new List<double>() {1, 1, 1 },
                },
            };
            int i = 0;
            foreach (var matrix in testMatrices.Select(m => new Matrix(m)))
            {
                i++;
                Console.WriteLine($"Test {i}");
                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Init matrix:\n {matrix}");
                var resultMatrix = matrix.Invert();
                Console.WriteLine($"Inverted matrix:\n {resultMatrix}");
            }
        }


    }
}
