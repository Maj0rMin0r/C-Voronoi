﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageComparerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CompareImagesSameImageExpectZero()
        {
            var imageComparer = new ImageComparer.ImageComparer();
            var averageDeltaE = imageComparer.CompareImages(@"C:\Users\aaron\Downloads\Turtle.png", @"C:\Users\aaron\Downloads\Turtle.png");
            Assert.AreEqual(0.0, averageDeltaE);
        }

        [TestMethod]
        public void CompareImagesDifferentImagesExpectNonZero()
        {
            var imageComparer = new ImageComparer.ImageComparer();
            var averageDeltaE = imageComparer.CompareImages(@"C:\Users\aaron\Downloads\Turtle.png", @"C:\Users\aaron\Downloads\1401661448.png");
            Assert.AreNotEqual(0.0, averageDeltaE);
        }
    }
}