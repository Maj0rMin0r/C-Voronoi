using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace Voronoi
{
    public class Drawer : IDisposable
    {
        private Bitmap _bitmap;
        private Graphics _canvas;

        private static readonly string[] ValidFileExtensions = { ".jpg", ".bmp", ".gif", ".png" };

        public Drawer(int width, int height, string fileName, string fileDirectory)
        {
            _bitmap = new Bitmap(width, height);
            _canvas = Graphics.FromImage(_bitmap);
            SaveToNewImageFile(fileName, fileDirectory);
        }

        public Drawer(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _canvas = Graphics.FromImage(_bitmap);
        }

        public void SaveToNewImageFile(string fileName, string fileDirectory)
        {
            if (!Directory.Exists(fileDirectory)) throw new Exception("File directory does not exist.");
            var fileExtension = Path.GetExtension(fileName);
            if(!ValidFileExtensions.Contains(fileExtension))
                throw new Exception("File extension not support. Only jpg or png are supported.");
            var path = Path.Combine(fileDirectory + fileName);
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileNameWithoutExtension(fileName);
            for (int i = 1; File.Exists(path); ++i)
            {
                if (!File.Exists(path))
                    break;
                path = Path.Combine(dir + @"\" + file + "(" + i + ")" + fileExtension);
            }
            _bitmap.Save(path);
        }

        public void DrawPoint(double[] xy) => DrawPoint(xy, new SolidBrush(Color.Black));

        public void DrawPoint(double[] xy, Color color) => DrawPoint(xy, new SolidBrush(color));

        public void DrawPoint(double[] xy, Brush brush)
        {
            var point = new RectangleF((float)xy[0], (float)xy[1], 1, 1);
            _canvas.FillRectangle(brush, point);
        }

        public void DrawPoints(Collection<RectangleF> points, Brush brush)
        {
            foreach (var point in points)
            {
                _canvas.FillRectangle(brush, point);
            }
        }

        public void DrawLines(Collection<PointF[]> lines, Pen pen)
        {
            foreach (var line in lines)
            {
                _canvas.DrawLines(pen, line);
            }     
        }

        public void FillRegion(Collection<PointF[]> pointsMakingUpRegions, Brush brush)
        {
            var path = new GraphicsPath();
            foreach (var points in pointsMakingUpRegions)
            {
                path.AddLine(points[0], points[1]);
            }
            var region = new Region(path);
            _canvas.FillRegion(brush, region);
        }

        public void DrawVoronoi(VoronoiOutput voronoi)
        {
            foreach (var site in voronoi.Sites)
            {
                var lines = voronoi.OutputLines(_bitmap.Width, _bitmap.Height);
                var intPoint2DList = voronoi.OutputRegion(site, lines);
                var regionColor = ReadonlyBitmap.Get().GetPixel((int)site.X, (int)site.Y);
                foreach (var point in intPoint2DList)
                {
                    _bitmap.SetPixel(point.X, point.Y, regionColor);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bitmap.Dispose();
                _canvas.Dispose();
            }
            _bitmap = null;
            _canvas = null;
        }
    }
}
