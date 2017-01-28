using System;
using System.Collections.Generic;

namespace Voronoi
{
    public class VoronoiOutput
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

        public void OutputConsole()
        {
            //Setup outputs
            var graph = GenerateGraph();

            Console.Out.WriteLine("Valid paths:");
            foreach (var key in graph.Keys)
            {
                LinkedList<Point2D> valueList;
                graph.TryGetValue(key, out valueList);

                if (valueList == null)
                    throw new ArgumentNullException(nameof(valueList), "Values not found");

                foreach (var value in valueList)
                {
                    Console.Out.WriteLine("Got line [" + key.X + ", " + key.Y + "] -> [" + value.X + ", " + value.Y + "], ");
                }
            }
        }

        public void OutputFile(int width, int height)
        {
            //Visual output
            var writer = new System.IO.StreamWriter(@"D:\MyDocs\Documents\output.html");
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
                writer.WriteLine("ctx.moveTo(" + line[0] + "," + line[1] + ");\nctx.lineTo(" + line[2] + "," + line[3] + ");\nctx.stroke();");
                line = GetNext();
            }

            writer.WriteLine("</script>\n</body>\n</html>");
            writer.Close();
        }

        /**
         * This method seeks to return a collection of every point within a region, given the lines (from OutputLines) and one point (the origin)
         * Using psuedocode pulled from https://en.wikipedia.org/wiki/Flood_fill#Alternative_implementations
         */
        public List<IntPoint2D> OutputRegion(Point2D origin, bool[,] array)
        {
            var intPointList = new List<IntPoint2D>();
            var intPointQueue = new Queue<IntPoint2D>();
            var originPoint = new IntPoint2D(origin);
            intPointQueue.Enqueue(originPoint);

            while (intPointQueue.Count > 0)
            {
                var point = intPointQueue.Dequeue();

                //To deal with odd shapes, we might add something to the queue then process the row already
                //If we do this row, we already have the row above/below in the queue. So we can skip this whole thing
                if (array[point.X,point.Y]) continue;

                // Set w and e equal to p
                //East and west should represent bounds of valid, inclusive
                var eastPoint = new IntPoint2D(point);
                var westPoint = new IntPoint2D(point);
                
                // Move w to the west until the color of the node to the west of w no longer matches (or OOB)
                while (!(westPoint.X <= 0 || array[westPoint.X-1, westPoint.Y]))
                    westPoint.X = westPoint.X - 1;

                // Move e to the east until the color of the node to the east of e no longer matches (or OOB)
                while (!(eastPoint.X >= array.GetUpperBound(0) || array[eastPoint.X+1, eastPoint.Y]))
                    eastPoint.X = eastPoint.X + 1;

                // For each node n between w and e:
                // TODO might be able to use LINQ for this for loop
                for (int x = westPoint.X; x <= eastPoint.X; x++)
                {
                    array[x, point.Y] = true;//Set node
                    intPointList.Add(new IntPoint2D(x,point.Y));

                    // If the node above/below is empty, add that node to pq
                    if (!(point.Y == array.GetUpperBound(1) || array[x, point.Y + 1]))
                        intPointQueue.Enqueue(new IntPoint2D(x, point.Y + 1));

                    if (!( point.Y == 0 || array[x, point.Y - 1]))
                        intPointQueue.Enqueue(new IntPoint2D(x, point.Y - 1));
                }
            }
            return intPointList;
        }

        /**
         * Generates a graphical interpretation of all pixels for the lines of the diagram
         */
        public bool[,] OutputLines(int width, int height)
        {
            var array = BuildArray(width, height);
            var graph = GenerateGraph();

            foreach (var key in graph.Keys)
            {
                LinkedList<Point2D> valueList;
                graph.TryGetValue(key, out valueList);

                if (valueList == null)
                    throw new ArgumentNullException(nameof(valueList), "Values not found");

                foreach (var value in valueList)
                {
                    DrawLine(R(key.X), R(key.Y), R(value.X), R(value.Y), ref array);
                }
            }

            return array;
        }

        /**
         * For sanity-checking region output. And it looks kinda neat
         */
        public void PrintRegions(int w, int h)
        {
            var origins = Sites;
            var booArray = OutputLines(w, h);
            var array = new string[w,h];

            //Carry over lines
            for (int i = 0; i<w; i++)
            {
                for (int j = 0; j<h; j++)
                {
                    if (booArray[i, j])
                        array[i, j] = "X";
                }
            }

            //Add regions
            foreach (var site in origins)
            {
                var region = OutputRegion(site, booArray);

                foreach (var point in region)
                    array[point.X, point.Y] = "/";
                array[(int)site.X, (int)site.Y] = "O";
            }
            

            //Print block
            Console.Out.WriteLine("Regions:");
            for (int y = h-1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++)
                {
                    Console.Out.Write(array[x,y]);
                }
                Console.Out.WriteLine();
            }


        }
        
        /**
         * To sanity-check the line-drawing code
         */
        public static void PrintArray(bool[,] array, int width, int height)
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

        /**
         * Bresenhams line theorem tells us all the points along a line. Neat!
         * Slightly modifed from the below source
         * http://www.roguebasin.com/index.php?title=Bresenham%27s_Line_Algorithm
         * @author Jason Morley (Source: http://www.morleydev.co.uk/blog/2010/11/18/generic-bresenhams-line-algorithm-in-visual-basic-net/)
         */
        private static void DrawLine(int x0, int y0, int x1, int y1, ref bool[,] array)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap(ref x0, ref y0); Swap(ref x1, ref y1); }
            if (x0 > x1) { Swap(ref x0, ref x1); Swap(ref y0, ref y1); }
            int dX = x1 - x0, dY = Math.Abs(y1 - y0), err = dX / 2, ystep = y0 < y1 ? 1 : -1, y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (steep)
                {
                    Draw(y, x, ref array);
                }
                else
                {
                    Draw(x, y, ref array);
                }

                err = err - dY;
                if (err >= 0) continue;
                y += ystep; err += dX;
            }
        }

        /**
         * Swapper helper for the line algorithm
         * @author Jason Morley (Source: http://www.morleydev.co.uk/blog/2010/11/18/generic-bresenhams-line-algorithm-in-visual-basic-net/)
         */
        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private static void Draw(int x, int y, ref bool[,] array) => array[x, y] = true;

        /**
         * Quick helper to round double to int
         * Converts from 1,1 index to 0,0.
         * Makes sure we don't round too low
         */
        private static int R(double input) => input >= 1 ? (int)Math.Round(input - 1) : 0;

        private void ResetIterator() => IteratorEdges = AllEdges;

        private Dictionary<Point2D, LinkedList<Point2D>> GenerateGraph()
        {
            //Setup outputs
            var graph = new Dictionary<Point2D, LinkedList<Point2D>>();

            //Get cmd output
            ResetIterator();
            var line = GetNext();

            while (line != null)
            {
                LinkedList<Point2D> temp;

                //	Write to a hash
                //Make Point2D's
                var key1 = new Point2D(line[0], line[1]); //Point 1
                var key2 = new Point2D(line[2], line[3]); //Point 2

                //Put relation
                if (graph.ContainsKey(key1))
                {
                    //Get value, update, put back
                    graph.TryGetValue(key1, out temp);

                    if (temp == null)
                        throw new ArgumentNullException(nameof(temp), "Key was missing for some reason");

                    temp.AddLast(key2);
                    graph.Remove(key1);
                    graph.Add(key1, temp);
                }
                else
                {
                    //Create value, put
                    temp = new LinkedList<Point2D>();
                    temp.AddLast(key2);
                    graph.Add(key1, temp);
                }

                line = GetNext();
            }

            return graph;
        }

        private static bool[,] BuildArray(int width, int height)
        {
            var array = new bool[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    array[i, j] = false;
                }
            }
            return array;
        }

        private double[] GetNext()
        {
            var returned = new double[4];

            if (IteratorEdges == null)
                return null;

            returned[0] = IteratorEdges.P1.X;
            returned[1] = IteratorEdges.P1.Y;
            returned[2] = IteratorEdges.P2.X;
            returned[3] = IteratorEdges.P2.Y;

            IteratorEdges = IteratorEdges.Next;

            return returned;
        }
    }
}