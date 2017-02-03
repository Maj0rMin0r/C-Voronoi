using System;

namespace Voronoi
{
    internal class GraphEdge
    {
        internal readonly Point2D P1;
        internal readonly Point2D P2;
        internal readonly GraphEdge Next;

        internal GraphEdge(Point2D p1, Point2D p2, GraphEdge n)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            P1 = p1;
            P2 = p2;
            Next = n;
        }
    }
}