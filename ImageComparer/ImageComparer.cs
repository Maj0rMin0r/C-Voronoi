using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace ImageComparer
{
    public class ImageComparer
    {
        public double CompareImages(string voronoiPath, string originalImagePath)
        {
            var deltaEList = new List<double>();
            var voronoiBitmap = Image.FromFile(voronoiPath) as Bitmap;
            var originalBitmap = Image.FromFile(originalImagePath) as Bitmap;
//            try
//            {
            for (var x = 0; x < voronoiBitmap.Width; x++)
            {
                for (var y = 0; y < voronoiBitmap.Height; y++)
                {
                    var voronoiPixelColor = voronoiBitmap.GetPixel(x, y);
                    var voronoiRgb = new Rgb {R = voronoiPixelColor.R, B = voronoiPixelColor.B, G = voronoiPixelColor.G};
                    var originalPixelColor = originalBitmap.GetPixel(x, y);
                    var originalRgb = new Rgb {R = originalPixelColor.R, B = originalPixelColor.B, G = originalPixelColor.G};
                    var deltaE = voronoiRgb.Compare(originalRgb, new CieDe2000Comparison());
                    deltaEList.Add(deltaE);
                }
            }
//            }
//            catch (NullReferenceException nullRefExecption)
//            {
//                
//            }
            // TODO remove this foreach loop later. use it to look at the values we get back
//            foreach (var deltaE in deltaEList)
//            {
//                Console.WriteLine(deltaE);
//            }
            
            return deltaEList.Average();
        }

        public List<double> CalculateRegionsDeltaEList()
        {
            // TODO do something Linq like to compare them

            return new List<double>();
        }


    }
}
