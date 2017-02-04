namespace Voronoi
{
    internal class Edge
    {
        internal readonly Point2D[] EndPoints;
        internal readonly Point2D[] Reg;
        internal double A;
        internal double B;
        internal double C;

        internal Edge()
        {
            EndPoints = new Point2D[2];
            Reg = new Point2D[2];
        }
    }
}