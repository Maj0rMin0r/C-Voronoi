using System;
using System.Collections.Generic;
using System.Linq;

namespace Voronoi
{
    public class VoronoiOutput : IOutputPrinter
    {
        private GraphEdge IteratorEdges { get; set; }
        private GraphEdge AllEdges { get; }
        public Point2D[] Sites { get; }

        internal VoronoiOutput(GraphEdge results, Point2D[] sites)
        {
            AllEdges = results;
            IteratorEdges = null;
            Sites = sites;
        }
        /// <summary>
        /// This method seeks to return a collection of every point within a region, given the lines (from OutputLines) and one point (the origin)
        /// Using psuedocode pulled from 
        /// <source>https://en.wikipedia.org/wiki/Flood_fill#Alternative_implementations</source>
        /// </summary>
        public List<IntPoint2D> OutputRegion(Point2D origin, bool[,] array)
        {
            var intPointList = new List<IntPoint2D>();
            var intPointQueue = new Queue<IntPoint2D>();
            var originPoint = new IntPoint2D(origin);
            intPointQueue.Enqueue(originPoint);
            while (intPointQueue.Count > 0)
            {
                var point = intPointQueue.Dequeue();
                if (array[point.X, point.Y] && (point != originPoint)) continue;
                var eastX = point.X;
                var eastY = point.Y;
                var westX = point.X;
                var westY = point.Y;
                while (!(westX <= 0 || array[westX - 1, westY]))
                    westX = westX - 1;
                while (!(eastX >= array.GetUpperBound(0) || array[eastX + 1, eastY]))
                    eastX = eastX + 1;
                var above = false;
                var below = false;
                for (int x = westX; x <= eastX; x++)
                {
                    array[x, point.Y] = true;
                    intPointList.Add(new IntPoint2D(x, point.Y));
                    if (!above && !(point.Y == array.GetUpperBound(1) || array[x, point.Y + 1]))
                    {
                        intPointQueue.Enqueue(new IntPoint2D(x, point.Y + 1));
                        above = true;
                    }

                    if (below || point.Y == 0 || array[x, point.Y - 1]) continue;
                    intPointQueue.Enqueue(new IntPoint2D(x, point.Y - 1));
                    below = true;
                }
            }
            return intPointList;
        }
        /// <summary>
        /// This method will iterate through all the regions and grab the average of each deltaE value
        /// over all pixels in all regions.
        /// </summary>
        public double CalculateAccuracy()
        {
            var imageComparer = new ImageComparer();
            var originalBitmap = ReadonlyBitmap.Get();
            var lines = OutputLines(originalBitmap.Width, originalBitmap.Height);
            var allDeltaEList = new List<double>();
            foreach (var site in Sites)
                allDeltaEList.AddRange(imageComparer.CalculateRegionsDeltaEList(originalBitmap, OutputRegion(site, lines), new IntPoint2D(site)));
            return allDeltaEList.Average();
        }
        /// <summary>
        /// Generates a graphical interpretation of all pixels for the lines of the diagram
        /// </summary>
        public bool[,] OutputLines(int width, int height)
        {
            var array = BuildArray(width, height);

            //Get cmd output
            ResetIterator();
            var line = GetNext();

            while (line != null)
            {
                DrawLine(R(line.Point2D1.X), R(line.Point2D1.Y), R(line.Point2D2.X), R(line.Point2D2.Y), ref array);

                line = GetNext();
            }
            
            return array;
        }
        /// <summary>
        /// Bresenhams line theorem tells us all the points along a line. Neat!
        /// Slightly modifed from the below source
        /// <source>http://www.roguebasin.com/index.php?title=Bresenham%27s_Line_Algorithm</source>
        /// <author>
        ///     Jason Morley 
        ///     <source>http://www.morleydev.co.uk/blog/2010/11/18/generic-bresenhams-line-algorithm-in-visual-basic-net/</source>
        /// </author>
        /// </summary>
        private static void DrawLine(int x0, int y0, int x1, int y1, ref bool[,] array)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap(ref x0, ref y0); Swap(ref x1, ref y1); }
            if (x0 > x1) { Swap(ref x0, ref x1); Swap(ref y0, ref y1); }
            int dX = x1 - x0, dY = Math.Abs(y1 - y0), err = dX / 2, ystep = y0 < y1 ? 1 : -1, y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (steep)
                    Draw(y, x, ref array);
                else
                    Draw(x, y, ref array);

                err = err - dY;
                if (err >= 0) continue;
                y += ystep; err += dX;
            }
        }
        /// <summary>
        /// Swapper helper for the line algorithm
        /// Part of Bresenhams code
        /// <author>
        ///     Jason Morley
        ///     <source>http://www.morleydev.co.uk/blog/2010/11/18/generic-bresenhams-line-algorithm-in-visual-basic-net/</source>
        /// </author>
        /// </summary>
        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        /// <summary>
        /// Draws a single point
        /// </summary>
        private static void Draw(int x, int y, ref bool[,] array) => array[x, y] = true;
        /// <summary>
        /// Quick helper to round double to int
        /// Converts from 1,1 index to 0,0
        /// Makes sure we don't round too low
        /// </summary>
        private static int R(double input) => input >= 1 ? (int)Math.Round(input - 1, MidpointRounding.AwayFromZero) : 0;
        private void ResetIterator() => IteratorEdges = AllEdges;
        private static bool[,] BuildArray(int width, int height) => new bool[width, height];
        private GraphEdge GetNext()
        {
            if (IteratorEdges == null)
                return null;
            var returned = IteratorEdges;
            IteratorEdges = IteratorEdges.Next;
            return returned;
        }
        /// <summary>
        /// Visual output of voronoi.
        /// </summary>
        /// <param name="width">canvas width</param>
        /// <param name="height">canvas height</param>
        /// <param name="pathToSaveTo">path to save file to</param>
        void IOutputPrinter.OutputFile(int width, int height, string pathToSaveTo)
        {
            var writer = new System.IO.StreamWriter(@pathToSaveTo);
            writer.WriteLine("<!DOCTYPE html>\n<html>\n<head>\n<title>\nTitle</title>\n</head>\n<body>\n<canvas id=\"myCanvas\" width=\"" + width + "\" height=\"" + height + "\" style=\"border:1px solid #d3d3d3;\">\nWords"
                    + "</canvas>\n\n<script>\nvar c = document.getElementById(\"myCanvas\");\nvar ctx = c.getContext(\"2d\");\n\n<!--Points-->");
            foreach (var t in Sites)
            {
                writer.WriteLine("ctx.beginPath();\nctx.arc(" + t.X + "," + t.Y + ",1,0,2*Math.PI);\nctx.stroke();\n\n");
            }
            ResetIterator();
            var line = GetNext();
            while (line != null)
            {
                writer.WriteLine("ctx.moveTo(" + line.Point2D1.X + "," + line.Point2D1.Y + ");\nctx.lineTo(" + line.Point2D2.X + "," + line.Point2D2.Y + ");\nctx.stroke();");
                line = GetNext();
            }
            writer.WriteLine("</script>\n</body>\n</html>");
            writer.Close();
        }
        void IOutputPrinter.PrintRegions(int width, int height)
        {
            var origins = Sites;
            var booArray = OutputLines(width, height);
            var array = new string[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (booArray[i, j])
                        array[i, j] = "X";
                }
            }
            foreach (var site in origins)
            {
                var region = OutputRegion(site, booArray);
                foreach (var point in region)
                    array[point.X, point.Y] = "/";
                array[(int)site.X, (int)site.Y] = "O";
            }
            Console.Out.WriteLine("Regions:");
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Out.Write(array[x, y]);
                }
                Console.Out.WriteLine();
            }
        }
        void IOutputPrinter.PrintArray(bool[,] array, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Out.Write(array[x, y] ? "X" : " ");
                }
                Console.Out.WriteLine();
            }
        }
    }
}