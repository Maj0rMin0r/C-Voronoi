using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoronoiDrawer;

namespace VoronoiTests
{
    [TestClass]
    public class VoronoiWriterTest
    {
        private const string FileName = @"\a.png";
        private const string FileDirectory = @"C:\Users\Jim\MSOE\C-Voronoi\images";

        [TestMethod]
        public void TestMethod1()
        {
            var writer = new Drawer(420, 420, FileName, FileDirectory);
//            writer.DrawLine(new double[] { 0, 210, 420, 210 });
            //writer.SaveToNewImageFile(FileName, FileDirectory);
        }
    }
}
