using System;
using System.Collections.Generic;

namespace Voronoi
{
    /**
     * Holy legacy code batman!
     * C @author Steven Fortune
     * C++ @author Shane O'Sullivan
     * Java @author Ryan Minor
     * C# @author Ryan Minor
     * 
     * Structs carried over from C code: 
     * 	Freenode. Knows next node. Implemented
     * 	FreeNodeArrayList. Knows current and next node. Implemented
     * 	FreeList. Knows head and size. Using Arraylist
     * 	Point. x,y. Using Java's 2D
     * 	Site. Stores a coord and 2 points Implemented
     * 	Edge. Stores 3 doubles, 2 arrays of size 2 of sites, and one int. Implemented
     * 	Graphedge. Stores 2 x/y coords and a next graphedge.
     */

    public class Fortunes
    {
        private int _siteidx;
        private Site[] _sites;
        private int _nsites;
        private Halfedge _elLeftend, _elRightend;
        private int _elHashsize;
        private double _xmax, _ymax, _deltax, _deltay;
        private int _sqrtNsites;
        private Site _bottomsite;
        private int _pqHashsize;
        private Halfedge[] _pqHash;
        private int _pqCount;
        private int _pqMin;
        private double _pxmin, _pxmax, _pymin, _pymax;
        private double _borderMinX, _borderMaxX, _borderMinY, _borderMaxY;
        private GraphEdge _allEdges;
        private GraphEdge _iteratorEdges;
        private double _minDistanceBetweenSites;
        private Halfedge[] _elHash;
        private readonly Edge _deleted;

        private class Site
        {
            public readonly Point2D Coord;

            public Site(Point2D p)
            {
                Coord = p;
            }
        }

        /**
         * Who makes a language without native 2D support?
         * We are using the vector library to do all the fancy math stuff. We need to make a private class and add our numbers to it
         */
        private class Point2D
        {
            internal double X { get;}
            internal double Y { get;}
            
            public Point2D(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double Distance(Point2D botCoord)
            {
                //A^2 + B^2 = C^2
                var a = Math.Pow(botCoord.X - X, 2);
                var b = Math.Pow(botCoord.Y - Y, 2);
                return Math.Sqrt(a + b);
            }
        }

        private class Edge
        {
            public double A;
            public double B;
            public double C;
            public readonly Site[] Ep;
            public readonly Site[] Reg;

            internal Edge()
            {
                Ep = new Site[2];
                Reg = new Site[2];
            }
        }

        private class GraphEdge
        {
            public readonly Point2D P1;
            public readonly Point2D P2;
            public readonly GraphEdge Next;

            public GraphEdge(Point2D p1, Point2D p2, GraphEdge n)
            {
                P1 = p1;
                P2 = p2;
                Next = n;
            }
        }

        private class Halfedge
        {
            public Halfedge ElLeft;
            public Halfedge ElRight;
            public Edge ElEdge;
            public int ElPm;
            public Site Vertex;
            public double YStar;
            public Halfedge PqNext;
        }

        public Fortunes()
        {
            _siteidx = 0;
            _deleted = new Edge();
            _allEdges = null;
            _iteratorEdges = null;
            _minDistanceBetweenSites = 0;
        }

        public static void Run()
        {
            //FileWriter writer = null;
            var fortune = new Fortunes();
            const double max = 10;
            const double minDist = 1;

            var xValues = new double[] { 1, 5, 9 };
            var yValues = new double[] { 1, 5, 9 };

            int count = xValues.Length;

            if (count != yValues.Length)
            {
                throw new ArgumentException("Bad data! Number of x values does not equal number of y values");
            }

            //Setup outputs
            var graph = new Dictionary<Point2D, LinkedList<Point2D>>();
            
            //Run
            fortune.GenerateVoronoi(xValues, yValues, count, 0, max, 0, max, minDist);

            //Get output
            fortune.ResetIterator();
            var line = fortune.GetNext();

            while (line != null)
            {
                LinkedList<Point2D> temp;

                //	Write to a hash
                //Make Point2D's
                var key1 = new Point2D(line[0], line[1]); //Point 1
                var key2 = new Point2D(line[2], line[3]); //Point 2

                //Put relation
                if (graph.ContainsKey(key1))
                {
                    //Get value, update, put back
                    graph.TryGetValue(key1, out temp);

                    if (temp == null)
                        throw new ArgumentNullException(nameof(temp), "Key was missing for some reason");
                    
                    temp.AddLast(key2);
                    graph.Remove(key1);
                    graph.Add(key1, temp);
                }
                else
                {
                    //Create value, put
                    temp = new LinkedList<Point2D>();
                    temp.AddLast(key2);
                    graph.Add(key1, temp);
                }

                line = fortune.GetNext();
            }
            
            Console.Out.WriteLine("Valid paths:");
            foreach (var key in graph.Keys)
            {
                Console.Out.Write("Got line [" + key.X + ", " + key.Y + "] -> [");

                LinkedList<Point2D> valueList;
                graph.TryGetValue(key, out valueList);

                if (valueList == null)
                    throw new ArgumentNullException(nameof(valueList), "Values not found");

                foreach (var value in valueList)
                {
                    Console.Out.WriteLine(value.X + ", " + value.Y + "], ");
                }
            }
        }

        private static double[] GetSet(int max, int size)
        {
            var set = new double[size];

            for (int i = 0; i < size; i++)
            {
                set[i] = new Random().NextDouble() * max;
            }
            return set;
        }

        public bool GenerateVoronoi(double[] xValues, double[] yValues, int numPoints, double minX, double maxX, double minY,
            double maxY, double minDist)
        {
            _minDistanceBetweenSites = minDist;
            _nsites = numPoints;
            _sites = new Site[_nsites];

            //We no longer need min/max like this
            double xmin = xValues[0];
            double ymin = yValues[0];
            double xmax = xValues[0];
            double ymax = yValues[0];

            for (int i = 0; i < _nsites; i++)
            {
                _sites[i] = new Site(new Point2D(xValues[i], yValues[i]));

                if (xValues[i] < xmin)
                    xmin = xValues[i];
                else if (xValues[i] > xmax)
                    xmax = xValues[i];

                if (yValues[i] < ymin)
                    ymin = yValues[i];
                else if (yValues[i] > ymax)
                    ymax = yValues[i];
            }
            //TODO Set max's using image x and y

            // This is where C++ does qsort and some magic stuff to sort these. We
            // do it the hard way. This sort is best-case n and stable, and moreso takes barely any lines

            // Sort x
            for (int n = 1; n < _sites.Length; n++)
            {
                int j = n - 1;
                var tem = _sites[n];
                while (j >= 0 && tem.Coord.X < _sites[j].Coord.X)
                {
                    _sites[j + 1] = _sites[j];
                    j--;
                }
                _sites[j + 1] = tem;
            }

            // Sort y
            for (int n = 1; n < _sites.Length; n++)
            {
                int j = n - 1;
                var tem = _sites[n];
                while (j >= 0 && tem.Coord.Y < _sites[j].Coord.Y)
                {
                    _sites[j + 1] = _sites[j];
                    j--;
                }
                _sites[j + 1] = tem;
            }

            _siteidx = 0;
            Geominit();
            double temp;
            if (minX > maxX)
            {
                temp = minX;
                minX = maxX;
                maxX = temp;
            }
            if (minY > maxY)
            {
                temp = minY;
                minY = maxY;
                maxY = temp;
            }

            _borderMinX = minX;
            _borderMinY = minY;
            _borderMaxX = maxX;
            _borderMaxY = maxY;

            _siteidx = 0;
            Voronoi();

            return true;
        }

        public void ResetIterator() => _iteratorEdges = _allEdges;

        public double[] GetNext()
        {
            var returned = new double[4];

            if (_iteratorEdges == null)
                return null;

            returned[0] = _iteratorEdges.P1.X;
            returned[1] = _iteratorEdges.P1.Y;
            returned[2] = _iteratorEdges.P2.X;
            returned[3] = _iteratorEdges.P2.Y;

            _iteratorEdges = _iteratorEdges.Next;

            return returned;
        }

        private bool ElInitialize()
        {
            int i;
            _elHashsize = 2 * _sqrtNsites;
            _elHash = new Halfedge[_elHashsize];

            for (i = 0; i < _elHashsize; i++) _elHash[i] = null;
            _elLeftend = HEcreate(null, 0);
            _elRightend = HEcreate(null, 0);
            _elLeftend.ElLeft = null;
            _elLeftend.ElRight = _elRightend;
            _elRightend.ElLeft = _elLeftend;
            _elRightend.ElRight = null;
            _elHash[0] = _elLeftend;
            _elHash[_elHashsize - 1] = _elRightend;

            return true;
        }

        private static Halfedge HEcreate(Edge e, int pm)
        {
            var answer = new Halfedge
                         {
                             ElEdge = e,
                             ElPm = pm,
                             PqNext = null,
                             Vertex = null
                         };
            return (answer);
        }

        private static void ElInsert(Halfedge lb, Halfedge newHe)
        {
            newHe.ElLeft = lb;
            newHe.ElRight = lb.ElRight;
            lb.ElRight.ElLeft = newHe;
            lb.ElRight = newHe;
        }

        /* Get entry from hash table, pruning any deleted nodes */

        private Halfedge ElGethash(int b)
        {
            if (b < 0 || b >= _elHashsize)
                return null;

            var he = _elHash[b];

            if (he?.ElEdge == null || !he.ElEdge.Equals(_deleted))
                return (he);

            /* Hash table points to deleted half edge.  Patch as necessary. */
            _elHash[b] = null;
            return null;
        }

        private Halfedge Leftbnd(Point2D p)
        {
            /* Use hash table to get close to desired halfedge */
            var bucket = (int)(p.X / _deltax * _elHashsize);
            if (bucket < 0) bucket = 0; //make sure that the bucket position in within the range of the hash array
            if (bucket >= _elHashsize) bucket = _elHashsize - 1;

            var he = ElGethash(bucket);
            if (he == null)
            { //if the HE isn't found, search backwards and forwards in the hash map for the first non-null entry
                int i;
                for (i = 1;; i++)
                {
                    if ((he = ElGethash(bucket - i)) != null)
                        break;
                    if ((he = ElGethash(bucket + i)) != null)
                        break;
                }
            }

            /* Now search linear list of halfedges for the correct one */
            if (he == _elLeftend || (he != _elRightend && right_of(he, p)))
            {
                do
                {
                    he = he.ElRight;
                } while (he != _elRightend && right_of(he, p)); //keep going right on the list until either the end is reached, or you find the 1st edge which the point

                he = he.ElLeft; //isn't to the right of
            }
            else
            { //if the point is to the left of the HalfEdge, then search left for the HE just to the left of the point
                do
                {
                    he = he.ElLeft;
                } while (he != _elLeftend && !right_of(he, p));
            }

            /* Update hash table and reference counts */
            if (bucket > 0 && bucket < _elHashsize - 1)
            {
                _elHash[bucket] = he;
            }
            return he;
        }

        /*
         * This delete routine can't reclaim node, since pointers from hash table
         * may be present.
         */

        private void ElDelete(Halfedge he)
        {
            he.ElLeft.ElRight = he.ElRight;
            he.ElRight.ElLeft = he.ElLeft;
            he.ElEdge = _deleted;
        }

        private Site Leftreg(Halfedge he)
        {
            if (he.ElEdge == null)
                return (_bottomsite);
            return (he.ElPm == 0 ? he.ElEdge.Reg[0] : he.ElEdge.Reg[1]);
        }

        private Site Rightreg(Halfedge he)
        {
            if (he.ElEdge == null) //if this halfedge has no edge, return the bottom site (whatever that is)
                return (_bottomsite);

            //if the elPm field is zero, return the site 0 that this edge bisects, otherwise return site number 1
            return (he.ElPm == 0 ? he.ElEdge.Reg[1] : he.ElEdge.Reg[0]);
        }

        private void Geominit()
        {
            var sn = (double)_nsites + 4;
            _sqrtNsites = (int)Math.Sqrt(sn);
            _deltay = _ymax;
            _deltax = _xmax;
        }

        private Edge Bisect(Site s1, Site s2)
        {
            var newedge = new Edge();
       
            newedge.Reg[0] = s1; //store the sites that this edge is bisecting
            newedge.Reg[1] = s2;
            newedge.Ep[0] = null; //to begin with, there are no endpoints on the bisector - it goes to infinity
            newedge.Ep[1] = null;

            var dx = s2.Coord.X - s1.Coord.X;
            var dy = s2.Coord.Y - s1.Coord.Y;
            var adx = dx > 0 ? dx : -dx;
            var ady = dy > 0 ? dy : -dy;
            newedge.C = s1.Coord.X * dx + s1.Coord.Y * dy + (dx * dx + dy * dy) * 0.5; //get the slope of the line

            if (adx > ady)
            {
                newedge.A = 1.0;
                newedge.B = dy / dx;
                newedge.C /= dx; //set formula of line, with x fixed to 1
            }
            else
            {
                newedge.B = 1.0;
                newedge.A = dx / dy;
                newedge.C /= dy; //set formula of line, with y fixed to 1
            }
            
            //printf("\nbisect(%d) ((%f,%f) and (%f,%f)",nedges,s1.coord.x,s1.coord.y,s2.coord.x,s2.coord.y);

            return (newedge);
        }

        // create a new site where the HalfEdges el1 and el2 intersect - note that
        // the Point in the argument list is not used, don't know why it's there
        private Site Intersect(Halfedge el1, Halfedge el2)
        {
            Edge e;
            Halfedge el;

            var e1 = el1.ElEdge;
            var e2 = el2.ElEdge;
            if (e1 == null || e2 == null || e1.Reg[1] == e2.Reg[1])
                return null;

            var d = (e1.A * e2.B) - (e1.B * e2.A);
            //This checks for the value being basically zero
            if (-0.0000000001 < d && d < 0.0000000001)
                return null;

            var xint = (e1.C * e2.B - e2.C * e1.B) / d;
            var yint = (e2.C * e1.A - e1.C * e2.A) / d;

            if ((e1.Reg[1].Coord.Y < e2.Reg[1].Coord.Y) || (DblEql(e1.Reg[1].Coord.Y, e2.Reg[1].Coord.Y) &&
                                                              e1.Reg[1].Coord.X < e2.Reg[1].Coord.X))
            {
                el = el1;
                e = e1;
            }
            else
            {
                el = el2;
                e = e2;
            }

            if ((xint >= e.Reg[1].Coord.X && el.ElPm == 0) || (xint < e.Reg[1].Coord.X && el.ElPm == 1))
                return null;

            //create a new site at the point of intersection - this is a new vector event waiting to happen
            return new Site(new Point2D(xint, yint));
        }

        /* returns 1 if p is to right of halfedge e */

        private static bool right_of(Halfedge el, Point2D p)
        {
            bool above;

            var e = el.ElEdge;
            var topsite = e.Reg[1];
            var rightOfSite = p.X > topsite.Coord.X;
            if (rightOfSite && el.ElPm == 0)
                return (true);
            if (!rightOfSite && el.ElPm == 1)
                return (false);

            if (DblEql(e.A, 1.0))
            {
                var dyp = p.Y - topsite.Coord.Y;
                var dxp = p.X - topsite.Coord.X;
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

                if (fast) return (el.ElPm == 0 ? above : !above);
                var dxs = topsite.Coord.X - (e.Reg[0]).Coord.X;
                above = e.B * (dxp * dxp - dyp * dyp) < dxs * dyp * (1.0 + 2.0 * dxp / dxs + e.B * e.B);
                if (e.B < 0.0)
                    above = !above;
            }
            else
            {
                var yl = e.C - e.A * p.X;
                var t1 = p.Y - yl;
                var t2 = p.X - topsite.Coord.X;
                var t3 = yl - topsite.Coord.Y;
                above = t1 * t1 > t2 * t2 + t3 * t3;
            }
            return (el.ElPm == 0 ? above : !above);
        }

        private void Endpoint(Edge e, int lr, Site s)
        {
            e.Ep[lr] = s;
            if (e.Ep[1 - lr] == null)
                return;

            clip_line(e);
        }

        // push the HalfEdge into the ordered linked list of vertices
        private void PQinsert(Halfedge he, Site v, double offset)
        {
            Halfedge next;

            he.Vertex = v;
            he.YStar = v.Coord.Y + offset;
            var last = _pqHash[PQbucket(he)];
            while ((next = last.PqNext) != null && (he.YStar > next.YStar || (DblEql(he.YStar, next.YStar) && v.Coord.X > next.Vertex.Coord.X)))
            {
                last = next;
            }

            he.PqNext = last.PqNext;
            last.PqNext = he;
            _pqCount++;
        }

        // remove the HalfEdge from the list of vertices
        //Returns the input edge so we can shave space on a few methods
        private Halfedge PQdelete(Halfedge he)
        {
            if (he.Vertex == null) return he;
            var last = _pqHash[PQbucket(he)];
            while (last.PqNext != he)
                last = last.PqNext;

            last.PqNext = he.PqNext;
            _pqCount -= 1;
            he.Vertex = null;
            return he;
        }

        private int PQbucket(Halfedge he)
        {
            var bucket = (int)(he.YStar / _deltay * _pqHashsize);
            if (bucket < 0)
                bucket = 0;
            if (bucket >= _pqHashsize)
                bucket = _pqHashsize - 1;
            if (bucket < _pqMin)
                _pqMin = bucket;
            return bucket;
        }

        private bool PQempty() => (_pqCount == 0);

        private Point2D PQ_min()
        {
            while (_pqHash[_pqMin].PqNext == null)
                _pqMin += 1;

            return new Point2D(_pqHash[_pqMin].PqNext.Vertex.Coord.X, _pqHash[_pqMin].PqNext.YStar);
        }

        private Halfedge PQextractmin()
        {
            var curr = _pqHash[_pqMin].PqNext;
            _pqHash[_pqMin].PqNext = curr.PqNext;
            _pqCount -= 1;
            return curr;
        }

        private void PQinitialize()
        {
            int i;

            _pqCount = 0;
            _pqMin = 0;
            _pqHashsize = 4 * _sqrtNsites;
            _pqHash = new Halfedge[_pqHashsize];

            if (_pqHash == null)
                return;

            for (i = 0; i < _pqHashsize; i++)
            {
                _pqHash[i] = new Halfedge {PqNext = null};
            }
        }

        private void PushGraphEdge(double x1, double y1, double x2, double y2)
        {
            var newEdge = new GraphEdge(new Point2D(x1, y1), new Point2D(x2, y2), _allEdges);
            _allEdges = newEdge;
        }

        private void clip_line(Edge e)
        {
            Site s1, s2;

            var x1 = e.Reg[0].Coord.X;
            var x2 = e.Reg[1].Coord.X;
            var y1 = e.Reg[0].Coord.Y;
            var y2 = e.Reg[1].Coord.Y;

            //if the distance between the two points this line was created from is less than 
            //the square root of 2, then ignore it
            if (Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) < _minDistanceBetweenSites) return;

            _pxmin = _borderMinX;
            _pxmax = _borderMaxX;
            _pymin = _borderMinY;
            _pymax = _borderMaxY;

            if (DblEql(e.A, 1.0) && e.B >= 0.0)
            {
                s1 = e.Ep[1];
                s2 = e.Ep[0];
            }
            else
            {
                s1 = e.Ep[0];
                s2 = e.Ep[1];
            }

            if (DblEql(e.A, 1.0))
            {
                y1 = _pymin;
                if (s1 != null && s1.Coord.Y > _pymin)
                    y1 = s1.Coord.Y;

                if (y1 > _pymax)
                    y1 = _pymax;

                x1 = e.C - e.B * y1;
                y2 = _pymax;
                if (s2 != null && s2.Coord.Y < _pymax)
                    y2 = s2.Coord.Y;

                if (y2 < _pymin)
                    y2 = _pymin;

                x2 = e.C - e.B * y2;
                if (((x1 > _pxmax) && (x2 > _pxmax)) || ((x1 < _pxmin) && (x2 < _pxmin)))
                    return;

                if (x1 > _pxmax)
                {
                    x1 = _pxmax;
                    y1 = (e.C - x1) / e.B;
                }

                if (x1 < _pxmin)
                {
                    x1 = _pxmin;
                    y1 = (e.C - x1) / e.B;
                }

                if (x2 > _pxmax)
                {
                    x2 = _pxmax;
                    y2 = (e.C - x2) / e.B;
                }

                if (x2 < _pxmin)
                {
                    x2 = _pxmin;
                    y2 = (e.C - x2) / e.B;
                }
            }
            else
            {
                x1 = _pxmin;
                if (s1 != null && s1.Coord.X > _pxmin)
                    x1 = s1.Coord.X;

                if (x1 > _pxmax)
                    x1 = _pxmax;

                y1 = e.C - e.A * x1;
                x2 = _pxmax;
                if (s2 != null && s2.Coord.X < _pxmax)
                    x2 = s2.Coord.X;

                if (x2 < _pxmin)
                    x2 = _pxmin;

                y2 = e.C - e.A * x2;

                if (((y1 > _pymax) & (y2 > _pymax)) | ((y1 < _pymin) & (y2 < _pymin)))
                    return;

                if (y1 > _pymax)
                {
                    y1 = _pymax;
                    x1 = (e.C - y1) / e.A;
                }

                if (y1 < _pymin)
                {
                    y1 = _pymin;
                    x1 = (e.C - y1) / e.A;
                }

                if (y2 > _pymax)
                {
                    y2 = _pymax;
                    x2 = (e.C - y2) / e.A;
                }

                if (y2 < _pymin)
                {
                    y2 = _pymin;
                    x2 = (e.C - y2) / e.A;
                }
            }

            PushGraphEdge(x1, y1, x2, y2);
        }

        /*
         * implicit parameters: nsites, sqrt_nsites, xmin, xmax, ymin, ymax, deltax,
         * deltay (can all be estimates). Performance suffers if they are wrong;
         * better to make nsites, deltax, and deltay too big than too small. (?)
         */

        private void Voronoi()
        {
            Point2D newintstar = null;
            Halfedge lbnd;
    
            PQinitialize();
            _bottomsite = Nextone();
            bool retval = ElInitialize();

            if (!retval)
                return;

            var newsite = Nextone();

            while (true)
            {
                if (!PQempty())
                    newintstar = PQ_min();

                //if the lowest site has a smaller y value than the lowest vector intersection, process the site
                //otherwise process the vector intersection		

                Site bot;
                Site p;
                Halfedge rbnd;
                Halfedge bisector;
                Edge e;
                if (newsite != null && (PQempty() || newsite.Coord.Y < newintstar.Y || (DblEql(newsite.Coord.Y, newintstar.Y) && newsite.Coord.X < newintstar.X)))
                { /* new site is smallest - this is a site event*/
                    //out_site(newsite);						//output the site
                    lbnd = Leftbnd(newsite.Coord); //get the first HalfEdge to the LEFT of the new site
                    rbnd = lbnd.ElRight; //get the first HalfEdge to the RIGHT of the new site
                    bot = Rightreg(lbnd); //if this halfedge has no edge, , bot = bottom site (whatever that is)
                    e = Bisect(bot, newsite); //create a new edge that bisects 
                    bisector = HEcreate(e, 0); //create a new HalfEdge, setting its elPm field to 0			
                    ElInsert(lbnd, bisector); //insert this new bisector edge between the left and right vectors in a linked list	

                    if ((p = Intersect(lbnd, bisector)) != null)//if the new bisector intersects with the left edge, remove the left edge's vertex, and put in the new one
                        PQinsert(PQdelete(lbnd), p, p.Coord.Distance(newsite.Coord));

                    lbnd = bisector;
                    bisector = HEcreate(e, 1); //create a new HalfEdge, setting its elPm field to 1
                    ElInsert(lbnd, bisector); //insert the new HE to the right of the original bisector earlier in the IF stmt

                    if ((p = Intersect(bisector, rbnd)) != null)//if this new bisector intersects with the
                        PQinsert(bisector, p, p.Coord.Distance(newsite.Coord)); //push the HE into the ordered linked list of vertices

                    newsite = Nextone();
                }
                else if (!PQempty())
                { /* intersection is smallest - this is a vector event */
                    lbnd = PQextractmin(); //pop the HalfEdge with the lowest vector off the ordered list of vectors				
                    var llbnd = lbnd.ElLeft;
                    rbnd = lbnd.ElRight; //get the HalfEdge to the right of the above HE
                    var rrbnd = rbnd.ElRight;
                    bot = Leftreg(lbnd); //get the Site to the left of the left HE which it bisects
                    var top = Rightreg(rbnd);

                    var v = lbnd.Vertex;
                    Endpoint(lbnd.ElEdge, lbnd.ElPm, v); //set the endpoint of the left HalfEdge to be this vector
                    Endpoint(rbnd.ElEdge, rbnd.ElPm, v); //set the endpoint of the right HalfEdge to be this vector
                    ElDelete(lbnd); //mark the lowest HE for deletion - can't delete yet because there might be pointers to it in Hash Map	
                    PQdelete(rbnd); //remove all vertex events to do with the  right HE
                    ElDelete(rbnd); //mark the right HE for deletion - can't delete yet because there might be pointers to it in Hash Map	
                    var pm = 0;

                    if (bot.Coord.Y > top.Coord.Y)
                    { //if the site to the left of the event is higher than the Site to the right of it, then swap them and set the 'pm' variable to 1
                        var temp = bot;
                        bot = top;
                        top = temp;
                        pm = 1;
                    }

                    e = Bisect(bot, top); //create an Edge (or line) that is between the two Sites. This creates
                    //the formula of the line, and assigns a line number to it
                    bisector = HEcreate(e, pm); //create a HE from the Edge 'e', and make it point to that edge with its elEdge field
                    ElInsert(llbnd, bisector); //insert the new bisector to the right of the left HE
                    Endpoint(e, 1 - pm, v); //set one endpoint to the new edge to be the vector point 'v'.
                    //If the site to the left of this bisector is higher than the right
                    //Site, then this endpoint is put in position 0; otherwise in pos 1

                    //if left HE and the new bisector don't intersect, then delete the left HE, and reinsert it 
                    if ((p = Intersect(llbnd, bisector)) != null)
                        PQinsert(PQdelete(llbnd), p, p.Coord.Distance(bot.Coord));

                    //if right HE and the new bisector don't intersect, then reinsert it 
                    if ((p = Intersect(bisector, rrbnd)) != null)
                        PQinsert(bisector, p, p.Coord.Distance(bot.Coord));
                }
                else break;
            }


            for (lbnd = _elLeftend.ElRight; lbnd != _elRightend; lbnd = lbnd.ElRight)
                clip_line(lbnd.ElEdge);
        }

        private static bool DblEql(double a, double b) => Math.Abs(a - b) < 0.00000000001;

        /* return a single in-storage site */
        private Site Nextone()
        {
            if (_siteidx >= _nsites) return null;
            _siteidx ++;
            return _sites[_siteidx-1];
        }
    }
}