using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static readonly Bitmap OriginalImage;
        
        private static void Main2()
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

        private static int Main(string[] args)
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
                ReadonlyBitmap.SetSnapshot(bmp, bmp.Width, bmp.Height);
                //readonlyBmp.SetSnapshot();
                // Retrieve the bitmap data from the the bitmap.
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                //Create a new bitmap.
                var newBitmap = new Bitmap(bmp.Width, bmp.Height, bmpData.Stride, bmp.PixelFormat, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
                //calls run
                Run(bmp, newBitmap, numberOfPointsToPlot);
            }
            catch (Exception e)
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

                    var averageDeltaE = voronoiOutput.CalculateAccuracy(ReadonlyBitmap.GetSnapshot(originalImage.Width, originalImage.Height));
                    //calculateDeltaE
                                         
                    //either of these work, we just have to choose which one at some point
                    result.TryAdd(voronoiOutput, averageDeltaE);
	                //result.AddOrUpdate(voronoiOutput, 0.0, (k,v) => 0.0);
	            });
	        //var bestVoronoi = result.OrderBy(r => r.Value).Min().Key;
            var bestVoronoi = result.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

            var writer = new Drawer(newImage);
            foreach (var site in bestVoronoi.Sites)
            {
                var lines = bestVoronoi.OutputLines(newImage.Width, newImage.Height);
                var intPoint2DList = bestVoronoi.OutputRegion(site, lines);
                /*
                 * Pseudocode
                 * For all the points in the list, color the points
                 */
                var originPixelColorBrush = new SolidBrush(originalImage.GetPixel((int)site.X, (int)site.Y));
                foreach (var point in intPoint2DList)
                {
                    writer.FillPoint(new Point(point.X, point.Y), originPixelColorBrush);
                    //finalBitmap.SetPixel(point.X, point.Y, originPixelColor);
                }



                // allDeltaEList.AddRange(imageComparer.CalculateRegionsDeltaEList(imageBitmap, OutputRegion(site, lines), new IntPoint2D(site)));
            }
            //writer.DrawLines();
            //writer.FillRegion();
        }
	}
}
