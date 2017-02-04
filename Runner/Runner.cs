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

        public static void Start(int iterations, Bitmap sourceBitmap, string newImageName, string newImageFileDirectory)
        {
            if (iterations < 1) throw new Exception(nameof(iterations) + " to be greater than 0.");
            if (sourceBitmap == null) throw new ArgumentNullException(nameof(sourceBitmap));
            if (newImageName == null) throw new ArgumentNullException(nameof(newImageName));
            if (newImageFileDirectory == null) throw new ArgumentNullException(nameof(newImageFileDirectory));
            Run(iterations, sourceBitmap, newImageName, newImageFileDirectory, (sourceBitmap.Width + sourceBitmap.Height) * 2);
        }

        private static void Run(int iterations, Bitmap newImage, string fileName, string fileDirectory, int numberOfPointsToPlot)
	    {
            ReadonlyBitmap.Set(newImage);
            var nums = Enumerable.Range(0, iterations).ToArray();
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
