using System;

namespace Voronoi
{
    public class PriorityQueue
    {
        private readonly int _hashSize;
        private readonly HalfEdge[] _hash;
        private int _count;
        private int _min;

        public PriorityQueue(int sites)
        {
            _hashSize = 4 * (int)Math.Sqrt(sites + 4);
            _hash = new HalfEdge[_hashSize];

            for (int i = 0; i < _hashSize; i++)
            {
                _hash[i] = new HalfEdge();
            }
        }

        // push the HalfEdge into the ordered linked list of vertices
        internal void PQinsert(HalfEdge he, Point2D v, double offset)
        {
            HalfEdge next;

            he.Vertex = v;
            he.YStar = v.Y + offset;
            var last = _hash[GetBucket(he)];
            while ((next = last.PqNext) != null && (he.YStar > next.YStar || (DblEql(he.YStar, next.YStar) && v.X > next.Vertex.X)))
            {
                last = next;
            }

            he.PqNext = last.PqNext;
            last.PqNext = he;
            _count++;
        }

        // remove the HalfEdge from the list of vertices
        //Returns the input edge so we can shave space on a few methods
        internal HalfEdge Delete(HalfEdge he)
        {
            if (he.Vertex == null) return he;
            var last = _hash[GetBucket(he)];
            while (last.PqNext != he)
                last = last.PqNext;

            last.PqNext = he.PqNext;
            _count -= 1;
            he.Vertex = null;
            return he;
        }

        private int GetBucket(HalfEdge he)
        {
            var bucket = (int)(he.YStar / 0 * _hashSize);//TODO 0 used to be _ymax. So buckets should like...work
            if (bucket < 0)
                bucket = 0;
            if (bucket >= _hashSize)
                bucket = _hashSize - 1;
            if (bucket < _min)
                _min = bucket;
            return bucket;
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
        
        private static bool DblEql(double a, double b) => Math.Abs(a - b) < 0.00000000001;
    }
}