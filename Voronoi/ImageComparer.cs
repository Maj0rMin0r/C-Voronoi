using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace Voronoi
{
    public class ImageComparer
    {
        public List<double> CalculateRegionsDeltaEList(Bitmap originalBitmap, List<IntPoint2D> listOfPoints, IntPoint2D originPoint)
        {
            if (originalBitmap == null) throw new ArgumentNullException(nameof(originalBitmap));
            if (!listOfPoints.Any()) throw new ArgumentNullException(nameof(listOfPoints));
            if (originPoint == null) throw new ArgumentNullException(nameof(originPoint));

            var originPixelColor = originalBitmap.GetPixel(originPoint.X, originPoint.Y);
            var originPixelRgb = new Rgb {R = originPixelColor.R, B = originPixelColor.B, G = originPixelColor.G};
            return listOfPoints.Select(point => originalBitmap.GetPixel(point.X, point.Y))
                                .Select(imagePixelColor => new Rgb {R = imagePixelColor.R, B = imagePixelColor.B, G = imagePixelColor.G})
                                .Select(imageRgb => imageRgb.Compare(originPixelRgb, new CieDe2000Comparison())).ToList();
        }
    }
}