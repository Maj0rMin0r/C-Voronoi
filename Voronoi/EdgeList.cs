using System;

namespace Voronoi
{
    internal class EdgeList
    {
        internal HalfEdge LeftEnd { get; }
        internal HalfEdge RightEnd { get; }
        internal readonly int HashSize;
        internal readonly HalfEdge[] Hash;

        /// <summary>
        /// Initializes the array of HalfEdge's based on the number of sites available.
        /// </summary>
        /// <param name="sites">number of sites to create</param>
        internal EdgeList(int sites)
        {
            HashSize = 2 * (int)Math.Sqrt(sites + 4);
            Hash = new HalfEdge[HashSize];

            LeftEnd = new HalfEdge(null, 0);
            RightEnd = new HalfEdge(null, 0);
            LeftEnd.Right = RightEnd;
            RightEnd.Left = LeftEnd;
            Hash[0] = LeftEnd;
            Hash[HashSize - 1] = RightEnd;
        }

        /// <summary>
        /// Get entry from hash table, prunes any deleted nodes.
        /// </summary>
        /// <param name="b">entry location</param>
        /// <returns>half edge from table that corresponds to b</returns>
        private HalfEdge GetHash(int b)
        {
            if (b < 0 || b >= HashSize)
                return null;
            var he = Hash[b];
            if (he?.Edge == null || he.Edge != null)
                return he;
            Hash[b] = null;
            return null;
        }

        /// <summary>
        /// Determines the HalfEdge nearest left to the given point.
        /// </summary>
        /// <param name="p">Point to find left bound HalfEdge from</param>
        /// <returns>left bound half edge of point</returns>
        internal HalfEdge LeftBound(Point2D p)
        {
            var he = GetHash(0);
            if (he == null)
            { 
                int i;
                for (i = 1; ; i++)
                {
                    if ((he = GetHash(0 - i)) != null)
                        break;
                    if ((he = GetHash(0 + i)) != null)
                        break;
                }
            }
            if (he == LeftEnd || (he != RightEnd && IsRightOf(he, p)))
            {
                do
                {
                    he = he.Right;
                } while (he != RightEnd && IsRightOf(he, p));

                he = he.Left;
            }
            else
            {
                do
                {
                    he = he.Left;
                } while (he != LeftEnd && !IsRightOf(he, p));
            }
            return he;
        }

        /// <summary>
        /// Deletes HalfEdge. This delete routine cannot reclaim the node, since
        /// hash table pointers may still be present.
        /// </summary>
        /// <param name="he">node to delete</param>
        internal void Delete(HalfEdge he)
        {
            he.Left.Right = he.Right;
            he.Right.Left = he.Left;
            he.Edge = null;
        }

        /// <summary>
        /// Determines if the Point is to the right of the HalfEdge.
        /// </summary>
        /// <param name="el">edge</param>
        /// <param name="p">point</param>
        /// <returns>1 if p is to the right of HalfEdge</returns>
        private static bool IsRightOf(HalfEdge el, Point2D p)
        {
            bool above;
            var e = el.Edge;
            var topsite = e.Reg[1];
            var rightOfSite = p.X > topsite.X;
            if (rightOfSite && el.Midpoint == 0)
                return true;
            if (!rightOfSite && el.Midpoint == 1)
                return false;
            if (DoubleComparison.IsEqual(e.A, 1.0))
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
                if (fast) return el.Midpoint == 0 ? above : !above;
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
            return el.Midpoint == 0 ? above : !above;
        }

        /// <summary>
        /// Inserts a new HalfEdge next to the previous one
        /// </summary>
        /// <param name="leftBoundHalfEdge"></param>
        /// <param name="newHalfEdge"></param>
        internal static void ElInsert(HalfEdge leftBoundHalfEdge, HalfEdge newHalfEdge)
        {
            newHalfEdge.Left = leftBoundHalfEdge;
            newHalfEdge.Right = leftBoundHalfEdge.Right;
            leftBoundHalfEdge.Right.Left = newHalfEdge;
            leftBoundHalfEdge.Right = newHalfEdge;
        }
    }
}
