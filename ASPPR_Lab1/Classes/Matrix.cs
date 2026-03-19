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
            public List<string> RowMarkers { get; set; } = new List<string>();
            public List<string> ColMarkers { get; set; } = new List<string>();
            public Matrix(int rows, int cols, List<string>? rowMarkers = null, List<string>? colMarkers = null)
            {
                var data = new List<List<double>>();

                for (int i = 0; i < rows; i++)
                    data.Add(new List<double>(Enumerable.Repeat(0d, cols)));

                _data = data;
                if (rowMarkers != null)
                {
                    if (rowMarkers.Count != rows)
                        RowMarkers = [];
                        //throw new ArgumentException("The number of row markers must match the number of rows in the matrix.");
                    RowMarkers = rowMarkers;
                }
                else
                {
                    for (int i = 0; i < rows; i++)
                        RowMarkers.Add($"y{i + 1}");
                }
                if (colMarkers != null)
                {
                    if (colMarkers.Count != cols)
                        ColMarkers = [];
                      //  throw new ArgumentException("The number of column markers must match the number of columns in the matrix.");
                    ColMarkers = colMarkers;
                }
                else
                {
                    for (int i = 0; i < cols; i++)
                        ColMarkers.Add($"x{i + 1}");
                }
            }
            public Matrix(List<List<double>> data, List<string>? rowMarkers = null, List<string>? colMarkers = null)
            {
                _data = data;
                if (rowMarkers != null)
                {
                    if (rowMarkers.Count != data.Count)
                        throw new ArgumentException("The number of row markers must match the number of rows in the matrix.");
                    RowMarkers = rowMarkers;
                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                        RowMarkers.Add($"y{i + 1}");
                }

                if (colMarkers != null)
                {
                    if (colMarkers.Count != data[0].Count)
                        throw new ArgumentException("The number of column markers must match the number of columns in the matrix.");
                    ColMarkers = colMarkers;
                }
                else
                {
                    for (int i = 0; i < data[0].Count; i++)
                        ColMarkers.Add($"x{i + 1}");
                }
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
                var resultMatrix = new Matrix(rowCount, colCount, new List<string>(RowMarkers), new List<string>(ColMarkers));


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

                resultMatrix.RowMarkers[row] = ColMarkers[col];
                resultMatrix.ColMarkers[col] = RowMarkers[row];
                return resultMatrix;
            }

            public Matrix JordanExcludeModified(int row, int col)
            {
                var rowCount = this.RowCount;
                var colCount = this.ColCount;
                var solutionElement = this[row, col];
                var resultMatrix = new Matrix(rowCount, colCount,new List<string>(RowMarkers), new List<string>(ColMarkers));


                //Step 1: set solution element to 1
                resultMatrix[row, col] = 1 / solutionElement;

                //Step 2
                for (int i = 0; i < colCount; i++)
                {
                    if (i == col) continue;
                    resultMatrix[row, i] = this[row, i] / solutionElement; // modified compared to original jordan exclusion
                }

                //Step 3
                for (int i = 0; i < rowCount; i++)
                {
                    if (i == row) continue;
                    resultMatrix[i, col] = -this[i, col] / solutionElement; // modified compared to original jordan exclusion
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
                resultMatrix.RowMarkers[row] = ColMarkers[col];
                resultMatrix.ColMarkers[col] = RowMarkers[row];
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
            public void AddRow(List<double> row, string? marker = null)
            {
                _data.Add(row);
                if (marker != null)
                    RowMarkers.Add(marker);
                else
                    RowMarkers.Add($"y{RowCount}");
            }
            public void InsertRow(List<double> row, int index,  string? marker = null)
            {
                _data.Insert(index, row);
                if (marker != null)
                    RowMarkers.Insert(index, marker);
                else
                    RowMarkers.Insert(index, $"y{index + 1}");
            }
            public void RemoveRow(int row)
            {
                _data.RemoveAt(row);
                RowMarkers.RemoveAt(row);
            }
            public Matrix GetColumn(int col)
            {
                var result = new List<List<double>>();

                foreach (var row in _data)
                    result.Add(new List<double> { row[col] });

                return new Matrix(result);
            }
            public List<double> GetColumnAsList(int col)
            {
                var result = new List<double>();

                foreach (var row in _data)
                    result.Add( row[col] );

                return result;
            }
            public void RemoveColumn(int column)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    _data[i].RemoveAt(column);
                }
                ColMarkers.RemoveAt(column);
            }
            public Matrix TakeLastColumn()
            {
                return GetColumn(ColCount - 1);
            }
            
            public Matrix DeepCopy()
            {
                var copy = _data
                    .Select(row => new List<double>(row))
                    .ToList();

                var matrix = new Matrix(copy);
                matrix.RowMarkers = new List<string>();
                matrix.ColMarkers = new List<string>();
                foreach (var marker in RowMarkers)
                    matrix.RowMarkers.Add(marker);
                foreach (var marker in ColMarkers)
                    matrix.ColMarkers.Add(marker);
                return matrix;
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

            public string ToStringWithMarkers()
            {
                var result = $"{"",10}|";
                for (int i = 0; i < ColCount; i++)
                {
                    string v = $"{ColMarkers[i],20}|";
                    result += v;
                }
                result += '\n' + new string('-', (ColCount+1) * 21) + '\n';
                for (int i = 0 ; i < RowCount; i++)
                {
                    var row = _data[i];
                    string r = $"{RowMarkers[i],10}|";
                    foreach (var col in row)
                    {
                        string v = $"{Math.Round(col, 3),20}|";
                        r += v;
                    }
                    result += r + '\n';
                    result += new string('-', (ColCount + 1) * 21) + '\n';

                }

                return result;
            }

            public void RoundToDecimalPlaces(int decimalPlaces)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < ColCount; j++)
                    {
                        this[i, j] = Math.Round(this[i, j], decimalPlaces);
                    }
                }
            }
        }
    }
}
