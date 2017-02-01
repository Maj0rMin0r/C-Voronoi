using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Voronoi
{
    public class Drawer : IDisposable
    {
        private Bitmap _bitmap;
        private Graphics _canvas;

        private static readonly string[] ValidFileExtensions = { ".jpg", ".bmp", ".gif", ".png" };

        public Drawer(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _canvas = Graphics.FromImage(_bitmap);
        }

        /// <summary>
        /// Attempts to always create a new image when creating a new voronoi drawer
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="fileDirectory">file directory</param>
        public void SaveToNewImageFile(string fileName, string fileDirectory)
        {
            if (!Directory.Exists(fileDirectory)) throw new IOException("File directory does not exist.");
            var fileExtension = Path.GetExtension(fileName);
            if(!ValidFileExtensions.Contains(fileExtension))
                throw new IOException("File extension not support. Only jpg or png are supported.");
            var path = Path.Combine(fileDirectory + fileName);
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileNameWithoutExtension(fileName);
            for (int i = 1; File.Exists(path); ++i)
                path = Path.Combine(dir + @"\" + file + "(" + i + ")" + fileExtension);
            _bitmap.Save(path);
        }

        /// <summary>
        /// Takes the VoronoiOuput and fills in the Bitmap accordingly
        /// </summary>
        /// <param name="voronoi">output to be drawn</param>
        public void DrawVoronoi(VoronoiOutput voronoi)
        {
            if (ReadonlyBitmap.Get() == null)
                throw new IOException();
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
