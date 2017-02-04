using System;

namespace Voronoi
{
    internal class HalfEdge
    {
        internal readonly int Midpoint;
        internal Edge Edge;
        internal HalfEdge Left;
        internal HalfEdge NextSite;
        internal HalfEdge Right;
        internal Point2D Vertex;
        internal double YStar;

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