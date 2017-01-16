using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace VoronoiWriter
{
    public class VoronoiWriter : IDisposable
    {
        private  Bitmap _bitmap;
        private Graphics _canvas;

        //@url -> http://stackoverflow.com/questions/17736160/good-way-to-check-if-file-extension-is-of-an-image-or-not
        private static readonly string[] ValidFileExtensions = { ".jpg", ".bmp", ".gif", ".png" };

        public VoronoiWriter(int width, int height)
        {
            _bitmap = new Bitmap(width, height);
            _canvas = Graphics.FromImage(_bitmap);
        }

        public VoronoiWriter(Bitmap bitmap)
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
            for (int i = 1; File.Exists(path); ++i)
            {
                if (!File.Exists(path))
                    break;
                path = Path.Combine(dir + fileName + "(" + i + ")" + fileExtension);
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

        public void DrawLine(double[] xyxy) => DrawLine(xyxy, new Pen(Color.Black));

        public void DrawLine(double[] xyxy, Color color) => DrawLine(xyxy, new Pen(color));

        public void DrawLine(double[] xyxy, Pen pen)
        {
            var pointA = new PointF((float)xyxy[0], (float)xyxy[1]);
            var pointB = new PointF((float)xyxy[2], (float)xyxy[3]);
            _canvas.DrawLine(pen, pointA, pointB);
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

        //only needed if there is an managedresources to be disposed of
//        ~VoronoiWriter()
//        {
//            Dispose(false);
//        }

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
