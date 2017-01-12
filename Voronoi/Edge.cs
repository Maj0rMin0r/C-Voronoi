namespace Voronoi
{
    public class Edge
    {
        public double A;
        public double B;
        public double C;
        public readonly Point2D[] Ep;
        public readonly Point2D[] Reg;

        internal Edge()
        {
            Ep = new Point2D[2];
            Reg = new Point2D[2];
        }
    }
}