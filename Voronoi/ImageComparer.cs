using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace Voronoi
{
    public class ImageComparer
    {
        public double CompareImages(string voronoiPath, string originalImagePath)
        {
            var deltaEList = new List<double>();
            var voronoiBitmap = Image.FromFile(voronoiPath) as Bitmap;
            var originalBitmap = Image.FromFile(originalImagePath) as Bitmap;
            for (var x = 0; x < voronoiBitmap.Width; x++)
            {
                for (var y = 0; y < voronoiBitmap.Height; y++)
                {
                    var voronoiPixelColor = voronoiBitmap.GetPixel(x, y);
                    var voronoiRgb = new Rgb { R = voronoiPixelColor.R, B = voronoiPixelColor.B, G = voronoiPixelColor.G };
                    var originalPixelColor = originalBitmap.GetPixel(x, y);
                    var originalRgb = new Rgb { R = originalPixelColor.R, B = originalPixelColor.B, G = originalPixelColor.G };
                    var deltaE = voronoiRgb.Compare(originalRgb, new CieDe2000Comparison());
                    deltaEList.Add(deltaE);
                }
            }
            return deltaEList.Average();
        }

        public List<double> CalculateRegionsDeltaEList(Bitmap image, List<IntPoint2D> listOfPoints, IntPoint2D originPoint)
        {
            var originPixelColor = image.GetPixel(originPoint.X, originPoint.Y);
            var originPixelRgb = new Rgb { R = originPixelColor.R, B = originPixelColor.B, G = originPixelColor.G };
            return listOfPoints.Select(point => image.GetPixel(point.X, point.Y))
                .Select(imagePixelColor => new Rgb { R = imagePixelColor.R, B = imagePixelColor.B, G = imagePixelColor.G })
                .Select(imageRgb => imageRgb.Compare(originPixelRgb, new CieDe2000Comparison())).ToList();
        }
    }
}
