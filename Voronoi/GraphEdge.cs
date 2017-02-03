using System;

namespace Voronoi
{
    public class GraphEdge
    {
        public readonly Point2D Point2D1;
        public readonly Point2D Point2D2;
        public readonly GraphEdge Next;

        internal GraphEdge(Point2D point2D1, Point2D point2D2, GraphEdge nextGraphEdge)
        {
            if (point2D1 == null) throw new ArgumentNullException(nameof(point2D1));
            if (point2D2 == null) throw new ArgumentNullException(nameof(point2D2));
            Point2D1 = point2D1;
            Point2D2 = point2D2;
            Next = nextGraphEdge;
        }
    }
}