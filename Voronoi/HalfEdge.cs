using System;

namespace Voronoi
{
    internal class HalfEdge
    {
        internal HalfEdge Left;
        internal HalfEdge Right;
        internal Edge Edge;
        internal readonly int Midpoint;
        internal Point2D Vertex;
        internal double YStar;
        internal HalfEdge NextSite;

        internal HalfEdge()
        {
            //Generic constructor lets C# to use type defaults
        }

        internal HalfEdge(Edge edge, int midpoint)
        {
            if (midpoint < 0) throw new ArgumentNullException(nameof(midpoint));
            Edge = edge;
            Midpoint = midpoint;
        }
    }
}