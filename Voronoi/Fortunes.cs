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
     * Structs carried over from C++>Java code: 
     * 	Freenode. Knows next node. Implemented
     * 	FreeNodeArrayList. Knows current and next node. Implemented
     * 	FreeList. Knows head and size. Replaced with ArrayList
     * 	Point. x,y. Using Java's 2D > In C#, had to make own. Basically a quicker copy of it
     * 	Site. Stores a coord and 2 points. Implemented
     * 	Edge. Stores 3 doubles, 2 arrays of size 2 of sites, and one int. Implemented
     * 	Graphedge. Stores 2 x/y coords and a next graphedge.
     */
    public class Fortunes
    {
        private int _siteidx;
        private Point2D[] _sites;
        private Point2D _bottomsite;
        private GraphEdge _allEdges;
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        public int NumSites { get; private set; }

        public Fortunes()
        {
            _siteidx = 0;
            _allEdges = null;
        }

        /**
         * Creates object
         * Runs
         * Outputs
         */
        public static VoronoiOutput Run(int width, int height, int siteCount)
        {
            var fortune = new Fortunes
                          {
                              ImageWidth = width,
                              ImageHeight = height
                          };

            var values = GetSet(siteCount, width, height);

            //Run
            fortune.GenerateVoronoi(values);

            var output = new VoronoiOutput(fortune._allEdges, values);
            return output;
        }

        public bool GenerateVoronoi(Point2D[] values)
        {
            NumSites = values.Length;
            _sites = new Point2D[NumSites];

            double xmin = values[0].X;
            double ymin = values[0].Y;
            double xmax = values[0].X;
            double ymax = values[0].Y;

            for (int i = 0; i < NumSites; i++)
            {
                _sites[i] = values[i];

                if (values[i].X < xmin)
                    xmin = values[i].X;
                else if (values[i].X > xmax)
                    xmax = values[i].X;

                if (values[i].Y < ymin)
                    ymin = values[i].Y;
                else if (values[i].Y > ymax)
                    ymax = values[i].Y;
            }

            // This is where C++ does qsort and some magic stuff to sort these. We
            // do it the hard way. This sort is best-case n and stable, and moreso takes barely any lines

            // Sort x
            for (int n = 1; n < _sites.Length; n++)
            {
                int j = n - 1;
                var tem = _sites[n];
                while (j >= 0 && tem.X < _sites[j].X)
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
                while (j >= 0 && tem.Y < _sites[j].Y)
                {
                    _sites[j + 1] = _sites[j];
                    j--;
                }
                _sites[j + 1] = tem;
            }

            _siteidx = 0;

            _siteidx = 0;
            Voronoi();

            return true;
        }
        
        private Point2D Leftreg(HalfEdge he)
        {
            if (he.ElEdge == null)
                return _bottomsite;

            return he.ElPm == 0 ? he.ElEdge.Reg[0] : he.ElEdge.Reg[1];
        }

        private Point2D Rightreg(HalfEdge he)
        {
            if (he.ElEdge == null) //if this HalfEdge has no edge, return the bottom site (whatever that is)
                return _bottomsite;

            //if the elPm field is zero, return the site 0 that this edge bisects, otherwise return site number 1
            return he.ElPm == 0 ? he.ElEdge.Reg[1] : he.ElEdge.Reg[0];
        }
        
        private static Edge Bisect(Point2D s1, Point2D s2)
        {
            var newedge = new Edge();

            newedge.Reg[0] = s1; //store the sites that this edge is bisecting
            newedge.Reg[1] = s2;
            newedge.Ep[0] = null; //to begin with, there are no endpoints on the bisector - it goes to infinity
            newedge.Ep[1] = null;

            var dx = s2.X - s1.X;
            var dy = s2.Y - s1.Y;
            var adx = dx > 0 ? dx : -dx;
            var ady = dy > 0 ? dy : -dy;
            newedge.C = s1.X * dx + s1.Y * dy + (dx * dx + dy * dy) * 0.5; //get the slope of the line

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
            
            return newedge;
        }

        // create a new site where the HalfEdges el1 and el2 intersect - note that
        // the Point in the argument list is not used, don't know why it's there
        private static Point2D Intersect(HalfEdge el1, HalfEdge el2)
        {
            Edge e;
            HalfEdge el;

            var e1 = el1.ElEdge;
            var e2 = el2.ElEdge;
            if (e1 == null || e2 == null || e1.Reg[1] == e2.Reg[1])
                return null;

            var d = e1.A * e2.B - e1.B * e2.A;
            //This checks for the value being basically zero
            if (-0.0000000001 < d && d < 0.0000000001)
                return null;

            var xint = (e1.C * e2.B - e2.C * e1.B) / d;
            var yint = (e2.C * e1.A - e1.C * e2.A) / d;

            if ((e1.Reg[1].Y < e2.Reg[1].Y) || (DblEql(e1.Reg[1].Y, e2.Reg[1].Y) &&
                                                              e1.Reg[1].X < e2.Reg[1].X))
            {
                el = el1;
                e = e1;
            }
            else
            {
                el = el2;
                e = e2;
            }

            if ((xint >= e.Reg[1].X && el.ElPm == 0) || (xint < e.Reg[1].X && el.ElPm == 1))
                return null;

            //create a new site at the point of intersection - this is a new vector event waiting to happen
            return new Point2D(xint, yint);
        }

        private void Endpoint(Edge e, int lr, Point2D s)
        {
            e.Ep[lr] = s;
            if (e.Ep[1 - lr] == null)
                return;

            ClipLine(e);
        }
        
        private void PushGraphEdge(double x1, double y1, double x2, double y2)
        {
            var newEdge = new GraphEdge(new Point2D(x1, y1), new Point2D(x2, y2), _allEdges);
            _allEdges = newEdge;
        }

        private void ClipLine(Edge e)
        {
            Point2D s1, s2;

            var x1 = e.Reg[0].X;
            var x2 = e.Reg[1].X;
            var y1 = e.Reg[0].Y;
            var y2 = e.Reg[1].Y;

            //if the distance between the two points this line was created from is less than 
            //the square root of 2, then ignore it
            if (Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) < 1) return;

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
                y1 = 0;
                if (s1 != null && s1.Y > 0)
                    y1 = s1.Y;

                if (y1 > ImageHeight)
                    y1 = ImageHeight;
            
                x1 = e.C - e.B * y1;
                y2 = ImageHeight;
                if (s2 != null && s2.Y < ImageHeight)
                    y2 = s2.Y;

                if (y2 < 0)
                    y2 = 0;

                x2 = e.C - e.B * y2;
                if (((x1 > ImageWidth) && (x2 > ImageWidth)) || ((x1 < 0) && (x2 < 0)))
                    return;

                if (x1 > ImageWidth)
                {
                    x1 = ImageWidth;
                    y1 = (e.C - x1) / e.B;
                }

                if (x1 < 0)
                {
                    x1 = 0;
                    y1 = (e.C - x1) / e.B;
                }

                if (x2 > ImageWidth)
                {
                    x2 = ImageWidth;
                    y2 = (e.C - x2) / e.B;
                }

                if (x2 < 0)
                {
                    x2 = 0;
                    y2 = (e.C - x2) / e.B;
                }
            }
            else
            {
                x1 = 0;
                if (s1 != null && s1.X > 0)
                    x1 = s1.X;

                if (x1 > ImageWidth)
                    x1 = ImageWidth;

                y1 = e.C - e.A * x1;
                x2 = ImageWidth;
                if (s2 != null && s2.X < ImageWidth)
                    x2 = s2.X;

                if (x2 < 0)
                    x2 = 0;

                y2 = e.C - e.A * x2;

                if (((y1 > ImageHeight) & (y2 > ImageHeight)) | ((y1 < 0) & (y2 < 0)))
                    return;

                if (y1 > ImageHeight)
                {
                    y1 = ImageHeight;
                    x1 = (e.C - y1) / e.A;
                }

                if (y1 < 0)
                {
                    y1 = 0;
                    x1 = (e.C - y1) / e.A;
                }

                if (y2 > ImageHeight)
                {
                    y2 = ImageHeight;
                    x2 = (e.C - y2) / e.A;
                }

                if (y2 < 0)
                {
                    y2 = 0;
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
            HalfEdge lbnd;

            var queue = new PriorityQueue(NumSites);
            _bottomsite = NextSite();
            var list = new EdgeList(NumSites);

            var newsite = NextSite();

            while (true)
            {
                Point2D bot;
                Point2D p;
                HalfEdge rbnd;
                HalfEdge bisector;
                Edge e;

                if (!queue.IsEmpty())
                    newintstar = queue.Min();

                //if the lowest site has a smaller y value than the lowest vector intersection, process the site
                //otherwise process the vector intersection		

                if (newsite != null && (queue.IsEmpty() || newsite.Y < newintstar.Y || (DblEql(newsite.Y, newintstar.Y) && newsite.X < newintstar.X)))
                { /* new site is smallest - this is a site event*/
                    lbnd = list.LeftBound(newsite); //get the first HalfEdge to the LEFT of the new site
                    rbnd = lbnd.ElRight; //get the first HalfEdge to the RIGHT of the new site
                    bot = Rightreg(lbnd); //if this HalfEdge has no edge, , bot = bottom site (whatever that is)
                    e = Bisect(bot, newsite); //create a new edge that bisects 
                    bisector = new HalfEdge(e, 0); //create a new HalfEdge, setting its elPm field to 0			
                    EdgeList.ElInsert(lbnd, bisector); //insert this new bisector edge between the left and right vectors in a linked list	

                    if ((p = Intersect(lbnd, bisector)) != null)//if the new bisector intersects with the left edge, remove the left edge's vertex, and put in the new one
                        queue.PQinsert(queue.Delete(lbnd), p, p.Distance(newsite));

                    lbnd = bisector;
                    bisector = new HalfEdge(e, 1); //create a new HalfEdge, setting its elPm field to 1
                    EdgeList.ElInsert(lbnd, bisector); //insert the new HE to the right of the original bisector earlier in the IF stmt

                    if ((p = Intersect(bisector, rbnd)) != null)//if this new bisector intersects with the
                        queue.PQinsert(bisector, p, p.Distance(newsite)); //push the HE into the ordered linked list of vertices

                    newsite = NextSite();
                }
                else if (!queue.IsEmpty())
                { /* intersection is smallest - this is a vector event */
                    lbnd = queue.ExtractMin(); //pop the HalfEdge with the lowest vector off the ordered list of vectors				
                    var llbnd = lbnd.ElLeft;
                    rbnd = lbnd.ElRight; //get the HalfEdge to the right of the above HE
                    var rrbnd = rbnd.ElRight;
                    bot = Leftreg(lbnd); //get the Site to the left of the left HE which it bisects
                    var top = Rightreg(rbnd);

                    var v = lbnd.Vertex;
                    Endpoint(lbnd.ElEdge, lbnd.ElPm, v); //set the endpoint of the left HalfEdge to be this vector
                    Endpoint(rbnd.ElEdge, rbnd.ElPm, v); //set the endpoint of the right HalfEdge to be this vector
                    list.Delete(lbnd); //mark the lowest HE for deletion - can't delete yet because there might be pointers to it in Hash Map	
                    queue.Delete(rbnd); //remove all vertex events to do with the  right HE
                    list.Delete(rbnd); //mark the right HE for deletion - can't delete yet because there might be pointers to it in Hash Map	
                    var pm = 0;

                    if (bot.Y > top.Y)
                    { //if the site to the left of the event is higher than the Site to the right of it, then swap them and set the 'pm' variable to 1
                        var temp = bot;
                        bot = top;
                        top = temp;
                        pm = 1;
                    }

                    e = Bisect(bot, top); //create an Edge (or line) that is between the two Sites. This creates
                    //the formula of the line, and assigns a line number to it
                    bisector = new HalfEdge(e, pm); //create a HE from the Edge 'e', and make it point to that edge with its elEdge field
                    EdgeList.ElInsert(llbnd, bisector); //insert the new bisector to the right of the left HE
                    Endpoint(e, 1 - pm, v); //set one endpoint to the new edge to be the vector point 'v'.
                    //If the site to the left of this bisector is higher than the right
                    //Site, then this endpoint is put in position 0; otherwise in pos 1

                    //if left HE and the new bisector don't intersect, then delete the left HE, and reinsert it 
                    if ((p = Intersect(llbnd, bisector)) != null)
                        queue.PQinsert(queue.Delete(llbnd), p, p.Distance(bot));

                    //if right HE and the new bisector don't intersect, then reinsert it 
                    if ((p = Intersect(bisector, rrbnd)) != null)
                        queue.PQinsert(bisector, p, p.Distance(bot));
                }
                else break;
            }

        for (lbnd = list.LeftEnd.ElRight; lbnd != list.RightEnd; lbnd = lbnd.ElRight)
                ClipLine(lbnd.ElEdge);
        }

        private static bool DblEql(double a, double b) => Math.Abs(a - b) < 0.00000000001;

        /* Return a single in-storage site */
        private Point2D NextSite() => _siteidx >= NumSites ? null : _sites[_siteidx++];

        private static Point2D[] GetSet(int size, int x, int y)
        {
            var set = new Point2D[size];
            var rand = new Random();

            for (int i = 0; i < size; i++)
            {
                set[i] = new Point2D(rand.Next(0, x - 1), rand.Next(0, y - 1));
            }

            return set;
        }
    }
}