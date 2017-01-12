using System;

namespace Voronoi
{
    public class PriorityQueue
    {
        private int _queueHashSize;
        private HalfEdge[] _queueHash;
        private int _queueCount;
        private int _queueMin;

        public PriorityQueue(int sites)
        {
            _queueHashSize = 4 * (int)Math.Sqrt(sites + 4);
            _queueHash = new HalfEdge[_queueHashSize];

            if (_queueHash == null)
                return;

            for (int i = 0; i < _queueHashSize; i++)
            {
                _queueHash[i] = new HalfEdge();
            }
        }

        // push the HalfEdge into the ordered linked list of vertices
        internal void PQinsert(HalfEdge he, Point2D v, double offset)
        {
            HalfEdge next;

            he.Vertex = v;
            he.YStar = v.Y + offset;
            var last = _queueHash[PQbucket(he)];
            while ((next = last.PqNext) != null && (he.YStar > next.YStar || (DblEql(he.YStar, next.YStar) && v.X > next.Vertex.X)))
            {
                last = next;
            }

            he.PqNext = last.PqNext;
            last.PqNext = he;
            _queueCount++;
        }

        // remove the HalfEdge from the list of vertices
        //Returns the input edge so we can shave space on a few methods
        internal HalfEdge PQdelete(HalfEdge he)
        {
            if (he.Vertex == null) return he;
            var last = _queueHash[PQbucket(he)];
            while (last.PqNext != he)
                last = last.PqNext;

            last.PqNext = he.PqNext;
            _queueCount -= 1;
            he.Vertex = null;
            return he;
        }

        private int PQbucket(HalfEdge he)
        {
            var bucket = (int)(he.YStar / 0 * _queueHashSize);//TODO 0 used to be _ymax. So buckets should like...work
            if (bucket < 0)
                bucket = 0;
            if (bucket >= _queueHashSize)
                bucket = _queueHashSize - 1;
            if (bucket < _queueMin)
                _queueMin = bucket;
            return bucket;
        }

        internal bool QueueEmpty() => _queueCount == 0;

        internal Point2D QueueMin()
        {
            while (_queueHash[_queueMin].PqNext == null)
                _queueMin += 1;

            return new Point2D(_queueHash[_queueMin].PqNext.Vertex.X, _queueHash[_queueMin].PqNext.YStar);
        }

        internal HalfEdge QueueGetMin()
        {
            var curr = _queueHash[_queueMin].PqNext;
            _queueHash[_queueMin].PqNext = curr.PqNext;
            _queueCount -= 1;
            return curr;
        }
        
        private static bool DblEql(double a, double b) => Math.Abs(a - b) < 0.00000000001;
    }
}