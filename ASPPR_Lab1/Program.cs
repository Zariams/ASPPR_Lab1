using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ASPPR_Lab1.ASPPR_Lab1;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ASPPR_Lab1
{
    internal partial class Program
    {


        static void Main(string[] args)
        {
            var fileDirectory = Directory.GetCurrentDirectory();
            Console.OutputEncoding = Encoding.UTF8;
            RunMenu();
        }

        static void RunMenu()
        {
            var iterate = true;
            while (iterate)
            {
                Console.WriteLine(
               """
                Що ви хочете зробити?
                1. Розрахувати ранг матриці;
                2. Отримати обернену матрицю;
                3. Розв'язати систему алгебраїчних лінійних рівнянь;
                0. Вихід.
                """
               );
                var choice = InputNumber();
                var compiler = new ComputationReport();
                switch (choice)
                {
                    case 0: 
                        iterate = false; 
                        break;
                    case 1:
                        {
                            var matrix = InputMatrix();
                            var rank = matrix.Rank;
                            Console.WriteLine($"Ранг матриці: {matrix.Rank}");
                            break;
                        }
                        
                    case 2:
                        {
                            var matrix = InputMatrix();
                            var inverse = matrix.Invert(compiler);
                            Console.WriteLine($"Обернена матриця: {inverse}");
                            Console.WriteLine("Показати деталі розрахунків?");
                            var showcompiler = InputBool();
                            if (showcompiler) Console.WriteLine(compiler.Compile());
                            break;
                        }
                    case 3:
                        {
                            var A = InputMatrix();
                            var B = InputMatrix(A.RowCount,1);
                            var first = LinearAlgebraicEquationSolver.SolveFirstMethod(A, B, compiler);
                            var second = LinearAlgebraicEquationSolver.SolveSecondMethod(A, B, compiler);
                            var gauss = LinearAlgebraicEquationSolver.SolveGauss(A, B, compiler);
                            Console.WriteLine($"Результат за першим способом: {first}");
                            Console.WriteLine($"Результат за другим способом: {second}");
                            Console.WriteLine($"Результат за третім способом: {gauss}");
                            Console.WriteLine("Показати деталі розрахунків?");
                            var showcompiler = InputBool();
                            if (showcompiler) Console.WriteLine(compiler.Compile());
                            break;
                        }
                }

            }
           
            
        }
        static Matrix InputMatrix()
        {
            Console.Write("\nВведіть кількість рядків: ");
            var rows = InputNumber();
            Console.Write("\nВведіть кількість стовпців: ");
            var cols = InputNumber();
            
            return InputMatrix(rows,cols);
        }
        static Matrix InputMatrix(int rows, int cols)
        {
            Console.Write($"Введіть елементи матриці {rows}x{cols}. Елементи одного рядка вводити через кому, новий рядок з Enter: ");
            var result = new List<List<double>>();
            for (int i = 0; i < rows; i++)
            {
                var row = InputRow(cols);
                result.Add(row);
            }
            return new Matrix(result);
        }
        public static List<double> InputRow(int cols)
        {
            
            var result = new List<double>();
            var success = true;
            do
            {
                var str = Console.ReadLine();
                var numbers = str.Split(',').Select(n => n.Trim());
                if (numbers.Count() != cols)
                {
                    Console.WriteLine("Некоректна кількість елементів у рядку");
                    continue;
                }
                foreach (var number in numbers)
                {
                    double num;
                    success = double.TryParse(number, out num);
                    if (!success) break;
                    result.Add(num);
                }
                if (!success)
                {
                    Console.WriteLine("Некоректний формат.");
                    result = new List<double>();
                }
            } while (!success);

            return result;
            
        }

        static int InputNumber()
        {
            var success = false;
            int num;
            do
            {
                var str = Console.ReadLine();
                success = int.TryParse(str, out num);
                if (!success) Console.WriteLine("Некоректний формат!");
            } while (!success);
            return num;
        }
    
        static bool InputBool()
        {
            Console.Write("(Y/N)");
            var ans = Console.ReadLine();
            if (ans.ToLowerInvariant().FirstOrDefault() != 'y') return false;
            return true;
        }
    }
}
