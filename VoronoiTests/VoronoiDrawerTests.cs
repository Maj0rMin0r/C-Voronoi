using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;
using Voronoi;

namespace VoronoiTests
{
    [TestClass]
    public class VoronoiDrawerTest
    {
        [TestMethod]
        public void Drawer_HappyPath_IsHappy()
        {
            ReadonlyBitmap.Set(new Bitmap(10, 10));
            using (var writer = new Drawer(ReadonlyBitmap.Get()))
            {
                writer.DrawVoronoi(Fortunes.Run(9, 9, new[] {new Point2D(0,0), new Point2D(5, 5) }));
                writer.SaveToNewImageFile("a.png", Directory.GetCurrentDirectory());
                writer.SaveToNewImageFile("a.png", Directory.GetCurrentDirectory());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Drawer_NullReadonlyVoronoi_IOException()
        {
            ReadonlyBitmap.Set(null);
            using (var writer = new Drawer(new Bitmap(10, 10)))
            {
                writer.DrawVoronoi(Fortunes.Run(9, 9, new Point2D[] {}));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Drawer_InvalidDirectory_IOException()
        {
            using (var writer = new Drawer(new Bitmap(10, 10)))
            {
                writer.SaveToNewImageFile("a.png", "");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Drawer_InvalidFileExtension_IOException()
        {
            using (var writer = new Drawer(new Bitmap(10, 10)))
            {
                writer.SaveToNewImageFile("a.g", Directory.GetCurrentDirectory());
            }
        }
    }
}
