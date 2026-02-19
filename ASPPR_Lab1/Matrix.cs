using System.Data;
using static ASPPR_Lab1.Program;

namespace ASPPR_Lab1
{
    namespace ASPPR_Lab1
    {
        internal class Matrix
        {
            private readonly List<List<double>> _data;

            public int RowCount => _data.Count;
            public int ColCount => _data[0].Count;
            public Matrix(int rows, int cols)
            {
                var data = new List<List<double>>();

                for (int i = 0; i < rows; i++)
                    data.Add(new List<double>(Enumerable.Repeat(0d, cols)));

                _data = data;
            }
            public Matrix(List<List<double>> data)
            {
                _data = data;
            }

            public double this[int r, int c]
            {
                get => _data[r][c];
                set => _data[r][c] = value;
            }

            public List<double> this[int r]
            {
                get => _data[r];
                set => _data[r] = value;
            }

            public List<List<double>> Rows
            {
                get => _data;
            }
            public int Rank
            {
                get {
                    var inputMatrix = this.DeepCopy();
                    var rowCount = inputMatrix.RowCount;
                    var colCount = inputMatrix.ColCount;
                    var rank = 0;
                    for (int i = 0; i < Math.Min(rowCount, colCount); i++)
                    {
                        if (Math.Abs(inputMatrix[i, i]) <= Double.Epsilon) continue;
                        inputMatrix = inputMatrix.JordanExclude(i, i);
                        rank++;
                    }

                    return rank;
                }
            }

            public static Matrix operator *(Matrix m1, Matrix m2)
            {
                int r1 = m1.RowCount;
                int c1 = m1.ColCount;
                int r2 = m2.RowCount;
                int c2 = m2.ColCount;

                if (c1 != r2)
                {
                    throw new ArgumentException("The number of columns of the first matrix max match the number of rows in the second matrix");
                }

                var res = new Matrix(r1, c2);

                for (int i = 0; i < r1; i++)
                {
                    for (int j = 0; j < c2; j++)
                    {
                        for (int k = 0; k < c1; k++)
                        {
                            res[i, j] += m1[i, k] * m2[k, j];
                        }
                    }
                }

                return res;
            }

            public Matrix JordanExclude(int row, int col)
            {
                var rowCount = this.RowCount;
                var colCount = this.ColCount;
                var solutionElement = this[row, col];
                var resultMatrix = new Matrix(rowCount, colCount);


                //Step 1: set solution element to 1
                resultMatrix[row, col] = 1 / solutionElement;

                //Step 2: entire row of solution element is filled with negative elemenets of input matrix
                for (int i = 0; i < colCount; i++)
                {
                    if (i == col) continue;
                    resultMatrix[row, i] = -this[row, i] / solutionElement;
                }

                //Step 3: copy elements of solution column from input matrix 
                for (int i = 0; i < rowCount; i++)
                {
                    if (i == row) continue;
                    resultMatrix[i, col] = this[i, col] / solutionElement;
                }

                //Step 4: all other elements are calculated according to the formula: bij = aij*ars - ais*arj

                for (int r = 0; r < rowCount; r++)
                {
                    if (r == row) continue;
                    for (int c = 0; c < colCount; c++)
                    {
                        if (c == col) continue;
                        resultMatrix[r, c] = (this[r, c] * this[row, col] - this[r, col] * this[row, c]) / solutionElement;
                    }
                }
                return resultMatrix;
            }
            
            public Matrix Invert(IComputationReportCompiler? compiler = null)
            {
                var inputMatrix = this.DeepCopy();
                compiler?.AddMatrix("Вхідна матриця", inputMatrix,1);
                var rowCount = inputMatrix.RowCount;
                for (int i = 0; i < rowCount; i++)
                {
                    compiler?.AddStep(i + 1, $"Розв'язувальний елемент A[{i + 1},{i + 1}] = {Math.Round(inputMatrix[i, i],3)}");
                    if (inputMatrix[i, i] == 0) continue;
                    inputMatrix = inputMatrix.JordanExclude(i, i);
                    compiler?.AddMatrix("Матриця після виконання ЗЖВ:", inputMatrix);
                }
                compiler?.AddMatrix("Обернена матриця", inputMatrix,1);
                return inputMatrix;
            }

            public void AddToRow(int row, double value)
            {
                _data[row].Add(value);
            }
            public void RemoveRow(int row)
            {
                _data.RemoveAt(row);
            }
            public void RemoveColumn(int column)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    _data[i].RemoveAt(column);
                }
            }
            public Matrix TakeLastColumn()
            {
                var result = new List<List<double>>();

                foreach (var row in _data)
                    result.Add(new List<double> { row.Last() });

                return new Matrix(result);
            }
            
            public Matrix DeepCopy()
            {
                var copy = _data
                    .Select(row => new List<double>(row))
                    .ToList();

                return new Matrix(copy);
            }
            public override string ToString()
            {
                var result = string.Empty;
                foreach (var row in _data)
                {
                    string r = string.Empty;
                    foreach (var col in row)
                    {
                        string v = $"{Math.Round(col, 3),20}|";
                        r += v;
                    }
                    result += r + '\n';
                    result += new string('-', ColCount * 21) + '\n';
                    
                }
               
                return result;
            }
        }
    }
}
