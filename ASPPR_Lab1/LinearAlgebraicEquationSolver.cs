namespace ASPPR_Lab1
{
    namespace ASPPR_Lab1
    {
        internal static class LinearAlgebraicEquationSolver
        {

            public static Matrix SolveFirstMethod(Matrix A, Matrix B)
            {
                var Ai = A.Invert();
                var res = Ai * B;
                return res;
            }
            public static Matrix SolveSecondMethod(Matrix A, Matrix B)
            {
                var inputMatrix = A.DeepCopy();

                //Build initial system
                var rowCount = inputMatrix.RowCount;

                for (int i = 0; i < rowCount; i++)
                {
                    inputMatrix.AddToRow(i, -B[i, 0]);
                }

                for (int i = 0; i < rowCount; i++)
                {
                    if (inputMatrix[i, i] == 0) continue;
                    inputMatrix = inputMatrix.JordanExclude(i, i);

                }
                //Cross out all columns but the last one
                var result = new Matrix(rowCount, 1);
                for (int i = 0; i < rowCount; i++)
                {
                    var row = inputMatrix[i].TakeLast(1);
                    result[i] = row.ToList();
                }
                return result;
            }
            public static Matrix SolveGauss(Matrix A, Matrix B)
            {
                var inputMatrix = A.DeepCopy();

                var rowCount = inputMatrix.RowCount;
                var formulas = new Matrix(rowCount, 1);

                //Build initial system
                for (int i = 0; i < rowCount; i++)
                {
                    inputMatrix.AddToRow(i, -B[i, 0]);
                }

                for (int i = 0; i < rowCount; i++)
                {
                    if (inputMatrix[i, i] == 0) continue;
                    inputMatrix = inputMatrix.JordanExclude(i, i);
                    //Save variable formula
                    formulas[i] = inputMatrix[i].Skip(i + 1).ToList();
                }
                var solutions = new Matrix(rowCount + 1, 1);
                //Set last element as 1 to simplify calculations - coefficient without a variable
                solutions[rowCount, 0] = 1;

                for (int i = rowCount - 1; i >= 0; i--)
                {
                    var formula = formulas[i];
                    var l = formula.Count();
                    var sum = 0d;
                    //Calculate variable value
                    for (int j = l - 1; j >= 0; j--)
                    {
                        sum += formula[j] * solutions[j + 1 + i, 0];
                    }
                    solutions[i, 0] = sum;
                }
                //Remove last row as it isn't part of the solution
                solutions.RemoveRow(rowCount);
                return solutions;
            }
        }
    }
}
