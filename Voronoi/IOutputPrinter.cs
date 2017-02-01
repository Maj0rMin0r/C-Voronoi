namespace Voronoi
{
    public interface IOutputPrinter
    {
        void OutputConsole();
        void OutputFile(int width, int height);

        void PrintArray(bool[,] array, int width, int height);
        void PrintRegions(int width, int height);
    }
}
