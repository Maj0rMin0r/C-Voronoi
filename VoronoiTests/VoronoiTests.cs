using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voronoi;

namespace VoronoiTests
{
    [TestClass]
    public class VoronoiTests
    {
        
        [TestMethod]
        public void OnePoint_Normal_CorrectSize()
        {
            var set = new Point2D[1];
            set[0] = new Point2D(5, 5);

            var output = Fortunes.Run(9, 9, set);
            var lines = output.OutputLines(9, 9);
            var origins = output.Sites;
            var region = output.OutputRegion(origins[0], lines);

            Assert.AreEqual(1, origins.Length);
            Assert.AreEqual(81, region.Count);
        }

        [TestMethod]
        public void TwoPoints_Horiz_CorrectSize()
        {
            var set = new Point2D[2];
            set[0] = new Point2D(3, 5);
            set[1] = new Point2D(7, 5);

            var output = Fortunes.Run(9, 9, set);
            var lines = output.OutputLines(9, 9);
            var origins = output.Sites;
            var region1 = output.OutputRegion(origins[0], lines);
            var region2 = output.OutputRegion(origins[1], lines);
            
            //Line endpoints are right, in 0,0 index
            Assert.IsTrue(lines[4, 0]);
            Assert.IsTrue(lines[4, 8]);

            Assert.AreEqual(2, origins.Length);
            Assert.AreEqual(72, region1.Count + region2.Count);//9*9-9(for the line) = 72
            Assert.AreEqual(36, region1.Count);
        }

        [TestMethod]
        public void TwoPoints_Vert_CorrectSize()
        {
            var set = new Point2D[2];
            set[0] = new Point2D(5, 3);
            set[1] = new Point2D(5, 7);

            var output = Fortunes.Run(9, 9, set);
            var lines = output.OutputLines(9, 9);
            var origins = output.Sites;
            var region1 = output.OutputRegion(origins[0], lines);
            var region2 = output.OutputRegion(origins[1], lines);

            //Line endpoints are right, in 0,0 index
            Assert.IsTrue(lines[0, 4]);
            Assert.IsTrue(lines[8, 4]);

            Assert.AreEqual(2, origins.Length);
            Assert.AreEqual(72, region1.Count + region2.Count);//9*9-9(for the line) = 72
            Assert.AreEqual(36, region1.Count);
        }

        [TestMethod]
        public void TwoPoints_Diag_CorrectSize()
        {
            var set = new Point2D[2];
            set[0] = new Point2D(3, 3);
            set[1] = new Point2D(7, 7);

            var output = Fortunes.Run(9, 9, set);
            var lines = output.OutputLines(9, 9);
            var origins = output.Sites;
            var region1 = output.OutputRegion(origins[0], lines);
            var region2 = output.OutputRegion(origins[1], lines);

            //Line endpoints are right, in 0,0 index
            Assert.IsTrue(lines[8, 0]);
            Assert.IsTrue(lines[0, 8]);

            Assert.AreEqual(2, origins.Length);
            Assert.AreEqual(72, region1.Count + region2.Count);//9*9-9(for the line) = 72
            Assert.AreEqual(36, region1.Count);
        }

        [TestMethod]
        public void TwoPoints_DuplicatePoints_Happy()
        {
            var set = new Point2D[2];
            set[0] = new Point2D(5, 5);
            set[1] = new Point2D(5, 5);

            var output = Fortunes.Run(9, 9, set);
            var lines = output.OutputLines(9, 9);
            var origins = output.Sites;
            var region = output.OutputRegion(origins[0], lines);

            Assert.AreEqual(2, origins.Length);
            Assert.AreEqual(81, region.Count);
        }

        [TestMethod]
        public void LotsaPoints_Random_VerifyRegions()
        {
            var output = Fortunes.Run(1000, 1000, 1000);
            var lines = output.OutputLines(1000, 1000);
            var origins = output.Sites;
            var count = origins.Count(site => output.OutputRegion(site, lines).Count == 0);

            Assert.IsTrue(count < 5);
        }
    }
}
