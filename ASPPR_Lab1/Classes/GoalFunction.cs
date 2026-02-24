using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab1
{

    internal class GoalFunction
    {
        private List<double> _coefficients = [];
        public IReadOnlyList<double> Coefficients
        {
            get
            {
                return _coefficients;
            }
        }
        public GoalFunctionType Type { get; set; } = GoalFunctionType.Maximize;
        public GoalFunction(List<double> coefficients, GoalFunctionType type = GoalFunctionType.Maximize)
        {
            _coefficients = coefficients;
            Type = type;
        }
        public GoalFunction()
        {
        }
        public Matrix ConvertToMatrix()
        {
            var matrix = new Matrix(1, _coefficients.Count + 1);
            for (int i = 0; i < _coefficients.Count; i++)
            {
                matrix[0, i] = Type == GoalFunctionType.Minimize? _coefficients[i] : -_coefficients[i];
            }
            matrix[0, _coefficients.Count] = 0;
            return matrix;
        }

        public double CalculateResultWithValues(List<double> values)
        {
            if (values.Count != _coefficients.Count)
            {
                throw new Exception("Кількість значень не співпадає з кількістю змінних в цільовій функції");
            }
            var result = 0.0;
            for (int i = 0; i < _coefficients.Count; i++)
            {
                result += _coefficients[i] * values[i];
            }
            return result;
        }

        public override string ToString()
        {
            var str = "Цільова функція: ";
            for (int i = 0; i < _coefficients.Count; i++)
            {
                var coeff = _coefficients[i];
                if (Math.Abs(coeff) <= Double.Epsilon) continue;
                if (i > 0 && coeff > 0) str += "+";
                str += $"({coeff})*x{i + 1} ";
            }
            return str;
        }
    }

}
