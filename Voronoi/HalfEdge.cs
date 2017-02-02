namespace Voronoi
{
    public class HalfEdge
    {
        public HalfEdge ElLeft;
        public HalfEdge ElRight;
        public Edge ElEdge;
        public readonly int ElPm;
        public Point2D Vertex;
        public double YStar;
        public HalfEdge PqNext;

        public HalfEdge()
        {
            //Generic constructor lets C# to use type defaults
        }

        public HalfEdge(Edge e, int pm)
        {
            ElEdge = e;
            ElPm = pm;
        }
    }
}