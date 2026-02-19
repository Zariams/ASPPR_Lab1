using ASPPR_Lab1.ASPPR_Lab1;

namespace ASPPR_Lab1
{
    internal partial class Program
    {
        internal interface IComputationReportCompiler
        {
            public void AddMatrix(string title, Matrix matrix, int titleLevel = 0);
            public void AddStep(int number, string description = "", int titleLevel = 1);
            public void AddAction(string title, string? description = "", int titleLevel = 2);
            public string Compile();
            public void Flush();
        }


    }
}
