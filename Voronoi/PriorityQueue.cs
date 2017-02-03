using System;

namespace Voronoi
{
    internal class PriorityQueue
    {
        private readonly HalfEdge[] _hash;
        private int _count;
        private int _min;

        internal PriorityQueue(int sites)
        {
            var hashSize = 4 * (int)Math.Sqrt(sites + 4);
            _hash = new HalfEdge[hashSize];

            for (int i = 0; i < hashSize; i++)
                _hash[i] = new HalfEdge();
        }

        // push the HalfEdge into the ordered linked list of vertices
        internal void PQinsert(HalfEdge he, Point2D v, double offset)
        {
            HalfEdge next;

            he.Vertex = v;
            he.YStar = v.Y + offset;
            var last = _hash[0];
            _min = 0;
            while ((next = last.PqNext) != null && (he.YStar > next.YStar || (DoubleComparison.IsEqual(he.YStar, next.YStar) && v.X > next.Vertex.X)))
                last = next;

            he.PqNext = last.PqNext;
            last.PqNext = he;
            _count++;
        }

        // remove the HalfEdge from the list of vertices
        //Returns the input edge so we can shave space on a few methods
        internal HalfEdge Delete(HalfEdge he)
        {
            if (he.Vertex == null) return he;
            var last = _hash[0];
            _min = 0;
            while (last.PqNext != he)
                last = last.PqNext;

            last.PqNext = he.PqNext;
            _count -= 1;
            he.Vertex = null;
            return he;
        }

        internal bool IsEmpty() => _count == 0;

        internal Point2D Min()
        {
            while (_hash[_min].PqNext == null)
                _min += 1;

            return new Point2D(_hash[_min].PqNext.Vertex.X, _hash[_min].PqNext.YStar);
        }

        internal HalfEdge ExtractMin()
        {
            var curr = _hash[_min].PqNext;
            _hash[_min].PqNext = curr.PqNext;
            _count -= 1;
            return curr;
        }        
    }
}