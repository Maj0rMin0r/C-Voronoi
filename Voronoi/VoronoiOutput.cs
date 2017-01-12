using System;
using System.Collections.Generic;

namespace Voronoi
{
    public class VoronoiOutput
    {
        private GraphEdge IteratorEdges { get; set; }
        private GraphEdge AllEdges { get; }
        private Point2D[] Sites { get; }

        internal VoronoiOutput(GraphEdge results, Point2D[] sites)
        {
            AllEdges = results;
            IteratorEdges = null;
            Sites = sites;
        }

        public void OutputConsole()
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

        private void ResetIterator() => IteratorEdges = AllEdges;

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