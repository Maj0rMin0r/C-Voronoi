using System;

namespace Voronoi
{
    /**
    * Who makes a language without native 2D support?
    * We are using the vector library to do all the fancy math stuff. We need to make a private class and add our numbers to it
    */
    public class Point2D
    {
        internal double X { get; }
        internal double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Distance(Point2D botCoord)
        {
            var a = Math.Pow(botCoord.X - X, 2);
            var b = Math.Pow(botCoord.Y - Y, 2);
            return Math.Sqrt(a + b);
        }
    }
}
