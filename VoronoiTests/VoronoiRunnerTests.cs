using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VoronoiTests
{
    [TestClass]
    public class VoronoiRunnerTests
    {
        [TestMethod]
        public void Main_InvalidBitmapPath_Exception() => Runner.Runner.Main(new[] {"50", Directory.GetCurrentDirectory(), "a.png", Directory.GetCurrentDirectory() });

        [TestMethod]
        public void Main_InvalidNumberOfPoints_Exception() => Runner.Runner.Main(new[] { "a", Directory.GetCurrentDirectory(), "a.png", Directory.GetCurrentDirectory() });

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Main_NotEnoughArguments_ArgumentException() => Runner.Runner.Main(new[] { "50", "", "a.png" });

        [TestMethod]
        public void Start_HappyPath_IsHappy() => Runner.Runner.Start(1, new Bitmap(10, 10), "a.png", Directory.GetCurrentDirectory());

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Start_PointsLessThanOne_Exception() => Runner.Runner.Start(0, new Bitmap(10, 10), "a.png", Directory.GetCurrentDirectory());

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Start_NullBitmap_ArgumentNullException() => Runner.Runner.Start(1, null, "a.png", Directory.GetCurrentDirectory());

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Start_NullFileName_ArgumentNullException() => Runner.Runner.Start(1, new Bitmap(10, 10), null, Directory.GetCurrentDirectory());

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Start_NullFileDirectory_ArgumentNullException() => Runner.Runner.Start(1, new Bitmap(10, 10), "a.png", null);
    }
}
