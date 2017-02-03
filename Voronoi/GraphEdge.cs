namespace Voronoi
{
    public class GraphEdge
    {
        public readonly Point2D P1;
        public readonly Point2D P2;
        public readonly GraphEdge Next;

        internal GraphEdge(Point2D p1, Point2D p2, GraphEdge n)
        {
            P1 = p1;
            P2 = p2;
            Next = n;
        }
    }
}