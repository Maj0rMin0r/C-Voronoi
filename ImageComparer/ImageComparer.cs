using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace ImageComparer
{
    public class ImageComparison
    {
        public double CompareImages(string voronoiPath, string originalImagePath)
        {
            var deltaEList = new List<double>();
            var voronoiImage = (Bitmap)Image.FromFile(voronoiPath);
            var originalImage = (Bitmap)Image.FromFile(originalImagePath);
            for (var x = 0; x < voronoiImage.Width; x++)
            {
                for (var y = 0; y < voronoiImage.Height; y++)
                {
                    var voronoiPixelColor = voronoiImage.GetPixel(x, y);
                    var voronoiRgb = new Rgb {R = voronoiPixelColor.R, B = voronoiPixelColor.B, G = voronoiPixelColor.G};
                    var originalPixelColor = originalImage.GetPixel(x, y);
                    var originalRgb = new Rgb { R = originalPixelColor.R, B = originalPixelColor.B, G = originalPixelColor.G };
                    var deltaE = voronoiRgb.Compare(originalRgb, new CieDe2000Comparison());
                    deltaEList.Add(deltaE);
                }
            }
            // TODO remove this foreach loop later. use it to look at the values we get back
            foreach (var deltaE in deltaEList)
            {
                Console.WriteLine(deltaE);
            }
            
            return deltaEList.Average();
        }


    }
}
