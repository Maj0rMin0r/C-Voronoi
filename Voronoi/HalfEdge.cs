using System;

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

        public HalfEdge(Edge edge, int pm)
        {
            if (pm < 0) throw new ArgumentNullException(nameof(pm));
            ElEdge = edge;
            ElPm = pm;
        }
    }
}