using ASPPR_Lab1.ASPPR_Lab1;
using static ASPPR_Lab1.Program;

namespace ASPPR_Lab1
{
    internal class InequalitySystemSolution
    {
        public List<double> SolutionCoefficients { get; set; }
        public double GoalFunctionValue { get; set; }
        public GoalFunctionType GoalFunctionType { get; set; }
        public bool IsOptimal { get; set; }
        public bool IsUnbounded { get; set; }
        public bool IsInfeasible { get; set; }
        public Matrix? SolutionMatrix { get; set; }
        public InequalitySystemSolution(List<double> solution, Matrix solutionMatrix, GoalFunctionType type , double goalFunctionValue, bool isOptimal, bool isUnbounded, bool isInfeasible)
        {
            SolutionCoefficients = solution;
            GoalFunctionValue = goalFunctionValue;
            GoalFunctionType = type;
            IsOptimal = isOptimal;
            IsUnbounded = isUnbounded;
            IsInfeasible = isInfeasible;
            SolutionMatrix = solutionMatrix;
        }
    }
}
