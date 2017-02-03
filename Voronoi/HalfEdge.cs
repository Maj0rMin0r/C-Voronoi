using System;

namespace Voronoi
{
    internal class HalfEdge
    {
        internal HalfEdge ElLeft;
        internal HalfEdge ElRight;
        internal Edge ElEdge;
        internal readonly int ElPm;
        internal Point2D Vertex;
        internal double YStar;
        internal HalfEdge PqNext;

        internal HalfEdge()
        {
            //Generic constructor lets C# to use type defaults
        }

        internal HalfEdge(Edge edge, int pm)
        {
            if (pm < 0) throw new ArgumentNullException(nameof(pm));
            ElEdge = edge;
            ElPm = pm;
        }
    }
}