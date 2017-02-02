using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Voronoi;

namespace Runner
{
    public class Runner
    {
        private const string MissingArgs = "Error Args: {Number Of Points To Plot} {Image To Voronoi} {File Name} {File Directory}";
        private const int C = 50;

        public static int Main(string [] args)
        {
            if (args.Length < 4)
                throw new ArgumentException(MissingArgs);
            try
            {
                Start(int.Parse(args[0]), new Bitmap(args[1]), args[2], args[3]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
            return 0;
        }

        public static void Start(int numberOfPointsToPlot, Bitmap sourceBitmap, string newImageName, string newImageFileDirectory)
        {
            if (numberOfPointsToPlot < 1) throw new Exception(nameof(numberOfPointsToPlot) + " to be greater than 0.");
            if (sourceBitmap == null) throw new ArgumentNullException(nameof(sourceBitmap));
            if (newImageName == null) throw new ArgumentNullException(nameof(newImageName));
            if (newImageFileDirectory == null) throw new ArgumentNullException(nameof(newImageFileDirectory));
            Run(numberOfPointsToPlot, sourceBitmap, newImageName, newImageFileDirectory);
        }

        private static void Run(int numberOfPointsToPlot, Bitmap newImage, string fileName, string fileDirectory)
	    {
            ReadonlyBitmap.Set(newImage);
            var nums = Enumerable.Range(0, C).ToArray();
            var result = new ConcurrentDictionary<VoronoiOutput, double>();
	        Parallel.ForEach(nums, _ =>
	            {
                    var voronoiOutput = Fortunes.Run(ReadonlyBitmap.Get().Width, ReadonlyBitmap.Get().Height, numberOfPointsToPlot);
                    var averageDeltaE = voronoiOutput.CalculateAccuracy();
                    result.TryAdd(voronoiOutput, averageDeltaE);
	            });
            var bestVoronoi = result.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
	        using (var writer = new Drawer(newImage))
	        {
                writer.DrawVoronoi(bestVoronoi);
                writer.SaveToNewImageFile(fileName, @fileDirectory);
            }
        }
    }
}
