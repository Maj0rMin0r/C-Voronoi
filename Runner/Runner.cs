using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Voronoi;
using VoronoiDrawer;

namespace Runner
{
    internal class Runner
    {
        private const string MissingArgs = "Error: missing file location or number of plot points";
        private const int C = 50;

        private static int Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine(MissingArgs);
                return 1;
            }
            try
            {
                var numberOfPointsToPlot = int.Parse(args[0]);
                var bmp = new Bitmap(args[1]);
                ReadonlyBitmap.SetSnapshot(bmp, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                var newBitmap = new Bitmap(bmp.Width, bmp.Height, bmpData.Stride, bmp.PixelFormat, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
                Run(bmp, newBitmap, numberOfPointsToPlot, args[2], args[3]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            return 0;
        }

        private static void Run(Image originalImage, Bitmap newImage, int numberOfPointsToPlot, string fileName, string fileDirectory)
	    {
            var nums = Enumerable.Range(0, C).ToArray();
            var result = new ConcurrentDictionary<VoronoiOutput, double>();
	        Parallel.ForEach(nums, _ =>
	            {
                    var voronoiOutput = Fortunes.Run(originalImage.Width, originalImage.Height, numberOfPointsToPlot);
                    var averageDeltaE = voronoiOutput.CalculateAccuracy(ReadonlyBitmap.GetSnapshot(originalImage.Width, originalImage.Height));
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
