using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voronoi;

namespace VoronoiTests
{
    [TestClass]
    public class VoronoiTests
    {
        
        [TestMethod]
        public void OnePoint_CorrectSize()
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
        public void TwoPoints_CorrectSize()
        {
            var set = new Point2D[2];
            set[0] = new Point2D(3, 5);
            set[1] = new Point2D(7, 5);

            var output = Fortunes.Run(9, 9, set);
            var lines = output.OutputLines(9, 9);
            var origins = output.Sites;
            var region1 = output.OutputRegion(origins[0], lines);
            var region2 = output.OutputRegion(origins[1], lines);

            Assert.AreEqual(2, origins.Length);
            Assert.AreEqual(72, region1.Count + region2.Count);
            Assert.AreEqual(36, region1.Count);
        }
    }
}
