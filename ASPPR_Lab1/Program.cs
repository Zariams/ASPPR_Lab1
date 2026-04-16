using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ASPPR_Lab1.ASPPR_Lab1;
using ASPPR_Lab2.Classes.Static;
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
                4. Розв'язати систему лінійних нерівностей;
                5. Знайти опорний розв'язок нерівностей;
                6. Викреслити нульові стовпці;
                7. Розрахувати систему лінійних нерівностей з цілочисельними розв'язками;
                8. Розв'язати матричну гру з нульовою сумою;
                0. Вихід.
                """
               );
                try
                {
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
                                Console.WriteLine($"Обернена матриця:\n{inverse}");
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(compiler.Compile());
                                break;
                            }
                        case 3:
                            {
                                var A = InputMatrix();
                                var B = InputMatrix(A.RowCount, 1);
                                var first = LinearAlgebraicEquationSolver.SolveFirstMethod(A, B, compiler);
                                var second = LinearAlgebraicEquationSolver.SolveSecondMethod(A, B, compiler);
                                var gauss = LinearAlgebraicEquationSolver.SolveGauss(A, B, compiler);
                                Console.WriteLine($"Результат за першим способом: \n{first}");
                                Console.WriteLine($"Результат за другим способом: \n{second}");
                                Console.WriteLine($"Результат за третім способом: \n{gauss}");
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(compiler.Compile());
                                break;
                            }
                        case 4:
                            {
                                var A = InputInequalitySystem();
                                var Z = InputGoalFunction(A.VariableCount);
                                var comp = new ComputationReport();
                                var result = LinearInequalitySolver.Solve(A, Z, comp);
                                Console.WriteLine($"Розв'язок задачі лінійного програмування:\n{result}");
                                Console.WriteLine($"Розв'язок двоїстої задачі:\n{result.ToStringDual()}");
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(comp.Compile());
                                break;
                            }
                        case 5:
                            {
                                var A = InputInequalitySystem();
                                var Z = InputGoalFunction(A.VariableCount);
                                var comp = new ComputationReport();
                                var result = LinearInequalitySolver.GetReferenceSolutionStandalone(A, Z, comp);
                                Console.WriteLine($"Опорний розв'язок:\n{result}");
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(comp.Compile());
                                break;
                            }
                        case 6:
                            {
                                var A = InputInequalitySystem();
                                var Z = InputGoalFunction(A.VariableCount);
                                var comp = new ComputationReport();

                                var result = LinearInequalitySolver.CrossOutZeroRows(A, Z, comp);
                                Console.WriteLine($"Вихідна сімплекс-таблиця:\n{result.ToStringWithMarkers()}");
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(comp.Compile());
                                break;
                            }
                        case 7:
                            {
                                var A = InputInequalitySystem();
                                var Z = InputGoalFunction(A.VariableCount);
                                var comp = new ComputationReport();
                                var result = LinearInequalitySolver.SolveIntegerSystem(A, Z, comp);
                                Console.WriteLine($"Розв'язок задачі лінійного програмування:\n{result}");
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(comp.Compile());
                                break;
                            }
                        case 8:
                            {
                                Console.WriteLine("Введіть кількість ігор для генерації звіту (0 - без звіту)");
                                var tries = InputNumber();
                                var M = InputMatrix();
                                var comp = new ComputationReport();
                                var result = MatrixGameSolver.Solve(M,tries, comp);
                                Console.WriteLine($"Значення гри: \n{result}");
                                if (result.Model != null)
                                {
                                    var filePath = WriteToCsv(result.Model);
                                    Console.WriteLine($"Звіт з моделювання гри записано у файл: {filePath}");
                                }
                                Console.WriteLine("Показати деталі розрахунків?");
                                var showcompiler = InputBool();
                                if (showcompiler) Console.WriteLine(comp.Compile());
                                break;
                            }
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }
            }
        }

        static string WriteToCsv(List<List<string>> contents)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var row in contents)
            {
                sb.AppendLine(string.Join(',', row));
            }
            //currently the file uses wrong encoding  for cyrillic characters: РќРѕРјРµСЂ РїР°СЂС‚С–С—,Р’РёРїР°РґРєРѕРІРµ С‡РёСЃР»Рѕ РіСЂР°РІС†СЏ Рђ,РЎС‚СЂР°С‚РµРіС–СЏ РіСЂР°РІС†СЏ Рђ,Р’РёРїР°РґРєРѕРІРµ С‡РёСЃР»Рѕ РіСЂР°РІС†СЏ Р’,РЎС‚СЂР°С‚РµРіС–СЏ РіСЂР°РІС†СЏ Р’,Р’РёРіСЂР°С€ РіСЂР°РІС†СЏ Рђ,РќР°РєРѕРїРёС‡РµРЅРёР№ РІРёРіСЂР°С€ Рђ,РЎРµСЂРµРґРЅС–Р№ РІРёРіСЂР°С€ Рђ (С†С–РЅР° РіСЂРё)
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"report_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv");
            File.WriteAllText(filePath, sb.ToString(),Encoding.Unicode);
            return filePath;
        }
        static Matrix InputMatrix()
        {
            Console.Write("\nВведіть кількість рядків: ");
            var rows = InputNumber();
            Console.Write("\nВведіть кількість стовпців: ");
            var cols = InputNumber();

            return InputMatrix(rows, cols);
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


        static Sign InputSign()
        {
            var success = false;
            int num = 0;
            Sign sign = Sign.None;
            do
            {
                Console.WriteLine("""
                    Введіть знак операції:
                    1. - <=
                    2. - >=
                    3. - <
                    4. - >
                    5. - = 
                    """);
                var str = Console.ReadLine();
                success = int.TryParse(str, out num) && num > 0 && num < 6;

            } while (!success);
            return (Sign)(num-1);
        }
        public static Inequality InputInequality(int cols)
        {

            var coefficients = new List<double>();
            var success = true;
            do
            {
                Console.WriteLine($"Введіть {cols} коефіцієнти змінних нерівності, через кому:");
                var str = Console.ReadLine();
                var numbers = str.Split(',').Select(n => n.Trim());
                if (numbers.Count() != cols)
                {
                    Console.WriteLine("Некоректна кількість змінних");
                    continue;
                }
                foreach (var number in numbers)
                {
                    double num;
                    success = double.TryParse(number, out num);
                    if (!success) break;
                    coefficients.Add(num);
                }
                if (!success)
                {
                    Console.WriteLine("Некоректний формат.");
                    coefficients = new List<double>();
                }
            } while (!success);

            var sign = InputSign();
            Console.WriteLine("Введіть константу (праву частину нерівності):");
            var constant = InputNumber();

            var result = new Inequality(coefficients, constant, sign);
            return result;
        }
        static InequalitySystem InputInequalitySystem()
        {
            Console.Write("\nВведіть кількість змінних у системі: ");
            var cols = InputNumber();
            Console.Write("\nВведіть кількість нерівностей у системі: ");
            var rows = InputNumber();   

            return InputInequalitySystem(rows, cols);
        }
        static InequalitySystem InputInequalitySystem(int rows, int cols)
        {
            Console.WriteLine($"Для побудови системи, введіть {rows} нерівності:");
            var result = new List<Inequality>();
            for (int i = 0; i < rows; i++)
            {
                var row = InputInequality(cols);
                result.Add(row);
            }
            return new InequalitySystem(result);
        }
        
        static GoalFunction InputGoalFunction(int cols)
        {
            var coefficients = new List<double>();
            var success = true;

            Console.WriteLine("Введіть функцію мети.");
            do
            {
                Console.WriteLine($"Введіть {cols} коефіцієнтів змінних нерівності, через кому:");
                var str = Console.ReadLine();
                var numbers = str.Split(',').Select(n => n.Trim());
                if (numbers.Count() != cols)
                {
                    Console.WriteLine("Некоректна кількість змінних");
                    continue;
                }
                foreach (var number in numbers)
                {
                    double num;
                    success = double.TryParse(number, out num);
                    if (!success) break;
                    coefficients.Add(num);
                }
                if (!success)
                {
                    Console.WriteLine("Некоректний формат.");
                    coefficients = new List<double>();
                }
            } while (!success);
            
            Console.WriteLine("Чи хочете ви максимізувати чи мінімізувати цільову функцію? (Y/N, Y - максимізувати)");
            var maximize = InputBool();
            var goalFunctionType = maximize ? GoalFunctionType.Maximize : GoalFunctionType.Minimize;
            var result = new GoalFunction(coefficients, goalFunctionType);
            return result;
        }
    }
}
