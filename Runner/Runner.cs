using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voronoi;
using VoronoiDrawer;

namespace Runner
{
    internal class Runner
    {
        private const string MissingArgs = "Error: missing file location or number of plot points";
        private const int Width = 10;
	    private const int Height = 10;
        private const int NumSites = 2;

        private const int C = 50;
        
        private static void Main()
        {
            Console.Out.WriteLine("Started");
            var output = Fortunes.Run(Width, Height, NumSites);
            var lines = output.OutputLines(Width, Height);
            var origins = output.Sites;

            output.OutputConsole();

            output.OutputFile(Width, Height);
            Console.Out.WriteLine("File Created");


            foreach (var site in origins)
            {
                var region = output.OutputRegion(site, lines);
                //TODO Process regions
            }

            output.PrintRegions(Width, Height);

            Console.Out.WriteLine("Finished");
        }

        private static int Main2(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(MissingArgs);
                return 1;
            }
            try
            {
                var numberOfPointsToPlot = int.Parse(args[0]);
                // Create a bitmap.
                var bmp = new Bitmap(args[1]);
                // Retrieve the bitmap data from the the bitmap.
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, bmp.PixelFormat);
                //Create a new bitmap.
                var newBitmap = new Bitmap(bmp.Width, bmp.Height, bmpData.Stride, bmp.PixelFormat, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
                //calls run
                Run(bmp, newBitmap, numberOfPointsToPlot);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                return 1;
            }

            return 0;
        }

        private static void Run(Bitmap originalImage, Bitmap newImage, int numberOfPointsToPlot)
	    {
            var nums = Enumerable.Range(0, C).ToArray();
            var result = new ConcurrentDictionary<VoronoiOutput, double>();
	        Parallel.ForEach(nums, _ =>
	            {
                    var voronoiOutput = Fortunes.Run(originalImage.Width, originalImage.Height, numberOfPointsToPlot);
                    //call regions thing to get the double
	                var averageDeltaE = voronoiOutput.CalculateAccuracy(originalImage);
                    //calculateDeltaE
                                         
                    //either of these work, we just have to choose which one at some point
                    result.TryAdd(voronoiOutput, averageDeltaE);
	                //result.AddOrUpdate(voronoiOutput, 0.0, (k,v) => 0.0);
	            });
	        var bestVoronoi = result.OrderBy(r => r.Value).Min().Key;
            //var bestVoronoi = result.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

            var writer = new Drawer(newImage);
            //writer.DrawLines();
            //writer.FillRegion();
	    }
	}
}
