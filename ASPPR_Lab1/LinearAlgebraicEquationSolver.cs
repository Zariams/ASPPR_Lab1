using static ASPPR_Lab1.Program;

namespace ASPPR_Lab1
{
    namespace ASPPR_Lab1
    {
        internal static class LinearAlgebraicEquationSolver
        {

            public static Matrix SolveFirstMethod(Matrix A, Matrix B, IComputationReportCompiler? compiler = null)
            {
                compiler?.AddAction("Згенерований протокол обчислення", "Знаходження ров'язків СЛАР 1-м методом (за допомогою оберненої матриці)", 4 );
                compiler?.AddMatrix("Вхідна матриця А", A,1);
                compiler?.AddMatrix("Вхідна матриця B", B,1);
                compiler?.AddAction("Протокол обчислення",titleLevel:3);
                compiler?.AddAction("Знаходження оберненої матриці");
                var Ai = A.Invert(compiler);
                compiler?.AddAction("Знаходження розв'язків шляхом добутку матриць");
                var res = Ai * B;
                compiler?.AddMatrix("Матриця добутку Аi*B", res, 1);
                var i = 0;
                compiler?.AddAction("Результат", string.Join('\n',res.Rows.Select(r => $"X[{++i}] = {Math.Round(r[0],3)}")));
                return res;
            }
            public static Matrix SolveSecondMethod(Matrix A, Matrix B, IComputationReportCompiler? compiler = null)
            {
                compiler?.AddAction("Згенерований протокол обчислення", "Знаходження ров'язків СЛАР 1-м методом (за допомогою оберненої матриці)", 4);
                compiler?.AddMatrix("Вхідна матриця А", A, 1);
                compiler?.AddMatrix("Вхідна матриця B", B, 1);
                compiler?.AddAction("Протокол обчислення", titleLevel: 3);
                var inputMatrix = A.DeepCopy();

                //Build initial system
                var rowCount = inputMatrix.RowCount;

                for (int i = 0; i < rowCount; i++)
                {
                    inputMatrix.AddToRow(i, -B[i, 0]);
                }
                compiler?.AddMatrix("Переписана система", inputMatrix, 2);
                for (int i = 0; i < rowCount; i++)
                {
                    compiler?.AddStep(i + 1, $"Розв'язувальний елемент A[{i + 1},{i + 1}] = {Math.Round(inputMatrix[i, 0],3)}");
                    //if (inputMatrix[i, i] == 0) continue;
                    //inputMatrix = inputMatrix.JordanExclude(i, i);
                    if (inputMatrix[i, 0] == 0) continue;
                    inputMatrix = inputMatrix.JordanExclude(i, 0);
                    inputMatrix.RemoveColumn(0);
                    compiler?.AddMatrix("Матриця після виконання ЗЖВ:", inputMatrix);
                }
                //Cross out all columns but the last one
                var result = inputMatrix.TakeLastColumn();
                compiler?.AddMatrix("Розв'язки:", result, 2);
                return result;
            }
            public static Matrix SolveGauss(Matrix A, Matrix B, IComputationReportCompiler? compiler = null)
            {
                compiler?.AddAction("Згенерований протокол обчислення", "Знаходження ров'язків СЛАР 1-м методом (за допомогою оберненої матриці)", 4);
                compiler?.AddMatrix("Вхідна матриця А", A, 1);
                compiler?.AddMatrix("Вхідна матриця B", B, 1);
                compiler?.AddAction("Протокол обчислення", titleLevel: 3);
                var inputMatrix = A.DeepCopy();

                var rowCount = inputMatrix.RowCount;
                var formulas = new Matrix(rowCount, 1);

                //Build initial system
                for (int i = 0; i < rowCount; i++)
                {
                    inputMatrix.AddToRow(i, -B[i, 0]);
                }
                compiler?.AddMatrix("Переписана система", inputMatrix, 2);
                for (int i = 0; i < rowCount; i++)
                {
                    compiler?.AddStep(i + 1, $"Розв'язувальний елемент A[{i + 1},{i + 1}] = {Math.Round(inputMatrix[0, 0],3)}");

                    //if (inputMatrix[i, i] == 0) continue;
                    //inputMatrix = inputMatrix.JordanExclude(i, i);
                    ////Save variable formula
                    //formulas[i] = inputMatrix[i].Skip(i + 1).ToList();
                    if (inputMatrix[0, 0] == 0) continue;
                    inputMatrix = inputMatrix.JordanExclude(0, 0);
                    //Save variable formula
                    inputMatrix.RemoveColumn(0);
                    formulas[i] = inputMatrix[0].ToList();
                    int j = i+1;
                    compiler?.AddAction($"Розв'язувальний рядок X[{i + 1}] = {string.Join(" ", formulas[i].SkipLast(1).Select(c => $"({Math.Round(c,3)})*X[{++j}] + "))} ({Math.Round(formulas[i].Last(),3)})",titleLevel:0);
                    inputMatrix.RemoveRow(0);
                    compiler?.AddMatrix("Матриця після виконання ЗЖВ:", inputMatrix);
                }
                compiler?.AddAction("Обчислення розв'язків");
                var solutions = new Matrix(rowCount + 1, 1);
                //Set last element as 1 to simplify calculations - coefficient without a variable
                solutions[rowCount, 0] = 1;

                for (int i = rowCount - 1; i >= 0; i--)
                {
                    var formula = formulas[i];
                    var l = formula.Count();
                    var sum = 0d;
                    //Calculate variable value
                    var str = $"X[{i + 1}] = ";
                    for (int j = l - 1; j >= 0; j--)
                    {
                        sum += formula[j] * solutions[j + 1 + i, 0];
                        str += $"({Math.Round(formula[j], 3)} * ({Math.Round(solutions[j + 1 + i, 0], 3)}))";
                        if (j == 0)
                        {
                            str += " = ";
                            continue;
                        }
                        str += " + ";
                    }
                    str += Math.Round(sum,3);
                    compiler?.AddAction(str, titleLevel: 0);

                    solutions[i, 0] = sum;
                }
                //Remove last row as it isn't part of the solution
                solutions.RemoveRow(rowCount);
                return solutions;
            }
        }
    }
}
