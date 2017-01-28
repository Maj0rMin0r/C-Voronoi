using System;

namespace Voronoi
{
    /**
    * Who makes a language without native 2D support?
    * We are using the vector library to do all the fancy math stuff. We need to make a private class and add our numbers to it
    */
    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }

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
        public override string ToString() => "[" + X + "," + Y + "]";
    }

    /**
     * An integer implementation
     */
    public class IntPoint2D
    {
        public int X { get; set; }
        public int Y { get; set; }

        public IntPoint2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IntPoint2D(Point2D p)
        {
            X = (int) p.X;
            Y = (int) p.Y;
        }

        public IntPoint2D(IntPoint2D p)
        {
            X = p.X;
            Y = p.Y;
        }
        public override string ToString() => "[" + X + "," + Y + "]";
    }
}
