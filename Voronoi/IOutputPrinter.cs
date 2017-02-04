namespace Voronoi
{
    public interface IOutputPrinter
    {
        void OutputFile(int width, int height, string pathToSaveTo);

        void PrintArray(bool[,] array, int width, int height);
        void PrintRegions(int width, int height);
    }
}