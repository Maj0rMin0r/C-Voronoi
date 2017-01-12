using System;

namespace Voronoi
{
    internal class EdgeList
    {
        public HalfEdge LeftEnd { get; }
        public HalfEdge RightEnd { get; }
        private readonly int _hashSize;
        private readonly HalfEdge[] _hash;

        internal EdgeList(int sites)
        {
            _hashSize = 2 * (int)Math.Sqrt(sites + 4);
            _hash = new HalfEdge[_hashSize];

            for (int i = 0; i < _hashSize; i++) _hash[i] = null;
            LeftEnd = new HalfEdge(null, 0);
            RightEnd = new HalfEdge(null, 0);
            LeftEnd.ElLeft = null;
            LeftEnd.ElRight = RightEnd;
            RightEnd.ElLeft = LeftEnd;
            RightEnd.ElRight = null;
            _hash[0] = LeftEnd;
            _hash[_hashSize - 1] = RightEnd;
        }

        /* Get entry from hash table, pruning any deleted nodes */
        private HalfEdge GetHash(int b)
        {
            if (b < 0 || b >= _hashSize)
                return null;

            var he = _hash[b];

            if (he?.ElEdge == null || he.ElEdge != null)
                return he;

            /* Hash table points to deleted half edge.  Patch as necessary. */
            _hash[b] = null;
            return null;
        }

        internal HalfEdge LeftBound(Point2D p)
        {
            /* Use hash table to get close to desired HalfEdge */
            //var bucket = (int)(p.X / xmax * _HashSize);
            var bucket = 0;//TODO buckets are broken here too
            if (bucket < 0) bucket = 0; //make sure that the bucket position in within the range of the hash array
            if (bucket >= _hashSize) bucket = _hashSize - 1;

            var he = GetHash(bucket);
            if (he == null)
            { //if the HE isn't found, search backwards and forwards in the hash map for the first non-null entry
                int i;
                for (i = 1; ; i++)
                {
                    if ((he = GetHash(bucket - i)) != null)
                        break;
                    if ((he = GetHash(bucket + i)) != null)
                        break;
                }
            }

            /* Now search linear list of HalfEdges for the correct one */
            if (he == LeftEnd || (he != RightEnd && IsRightOf(he, p)))
            {
                do
                {
                    he = he.ElRight;
                } while (he != RightEnd && IsRightOf(he, p)); //keep going right on the list until either the end is reached, or you find the 1st edge which the point

                he = he.ElLeft; //isn't to the right of
            }
            else
            { //if the point is to the left of the HalfEdge, then search left for the HE just to the left of the point
                do
                {
                    he = he.ElLeft;
                } while (he != LeftEnd && !IsRightOf(he, p));
            }

            /* Update hash table and reference counts */
            if (bucket > 0 && bucket < _hashSize - 1)
            {
                _hash[bucket] = he;
            }
            return he;
        }

        /*
         * This delete routine can't reclaim node, since pointers from hash table
         * may be present.
         */

        internal void Delete(HalfEdge he)
        {
            he.ElLeft.ElRight = he.ElRight;
            he.ElRight.ElLeft = he.ElLeft;
            he.ElEdge = null;
        }

        /* returns 1 if p is to right of HalfEdge e */
        private static bool IsRightOf(HalfEdge el, Point2D p)
        {
            bool above;

            var e = el.ElEdge;
            var topsite = e.Reg[1];
            var rightOfSite = p.X > topsite.X;
            if (rightOfSite && el.ElPm == 0)
                return true;
            if (!rightOfSite && el.ElPm == 1)
                return false;

            if (DblEql(e.A, 1.0))
            {
                var dyp = p.Y - topsite.Y;
                var dxp = p.X - topsite.X;
                var fast = false;
                if ((!rightOfSite && (e.B < 0.0)) || (rightOfSite && (e.B >= 0.0)))
                {
                    above = dyp >= e.B * dxp;
                    fast = above;
                }
                else
                {
                    above = p.X + p.Y * e.B > e.C;
                    if (e.B < 0.0)
                        above = !above;
                    if (!above)
                        fast = true;
                }

                if (fast) return el.ElPm == 0 ? above : !above;
                var dxs = topsite.X - e.Reg[0].X;
                above = e.B * (dxp * dxp - dyp * dyp) < dxs * dyp * (1.0 + 2.0 * dxp / dxs + e.B * e.B);
                if (e.B < 0.0)
                    above = !above;
            }
            else
            {
                var yl = e.C - e.A * p.X;
                var t1 = p.Y - yl;
                var t2 = p.X - topsite.X;
                var t3 = yl - topsite.Y;
                above = t1 * t1 > t2 * t2 + t3 * t3;
            }
            return el.ElPm == 0 ? above : !above;
        }

        private static bool DblEql(double a, double b) => Math.Abs(a - b) < 0.00000000001;

        internal static void ElInsert(HalfEdge lb, HalfEdge newHe)
        {
            newHe.ElLeft = lb;
            newHe.ElRight = lb.ElRight;
            lb.ElRight.ElLeft = newHe;
            lb.ElRight = newHe;
        }
    }
}
