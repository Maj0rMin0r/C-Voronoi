using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoronoiWriter
{
    public class VoronoiWriter
    {
        //we will design this without a contructor to improve multi-threads
        public void SaveVoronoiDiagramToBitMap(string fileName,
            string fileDirectory, int canvasWidth, int canvasHeight)
        {
            //make sure file directory and file name are available
            ManageFile(fileName, fileDirectory);
            //create BitMap object
            //@url -> https://msdn.microsoft.com/en-us/library/system.drawing.bitmap(v=vs.110).aspx 
            var canvas = new Bitmap(canvasWidth, canvasHeight);
            //@url -> https://msdn.microsoft.com/en-us/library/system.drawing.graphics(v=vs.110).aspx
            var pen = Graphics.FromImage(canvas);
            //draw black lines on blank white canvas

            //fill in regions

            //save images
            //@url -> https://msdn.microsoft.com/en-us/library/9t4syfhh(v=vs.110).aspx
            //TODO -> accept multiple picture formats?
            //TODO -> what if the user passes a file name with the extension in the name?

            canvas.Save(fileDirectory + fileName, System.Drawing.Imaging.ImageFormat.Png);
            //clean up?
            canvas.Dispose();
            pen.Dispose();
        }

        public void ManageFile(string fileName, string fileDirectory)
        {
            if (!Directory.Exists(fileDirectory)) throw new Exception("File directory does not exist.");
            var fileExtension = Path.GetExtension(fileName);
            if (fileExtension != "png" || fileExtension != "jpg")
            {
                throw new Exception("File extension not support. Only jpg or png are supported.");
            }

            if (File.Exists(fileName))
                throw new Exception("File already exists.");
        }

        public void DrawLines()
        {

        }


        public void FillRegions()
        {

        }
    }
}
