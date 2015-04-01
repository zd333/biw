using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Bulk_Image_Watermark
{
    public enum ImageFiletypes
    { jpg, png, bmp}
    static class Watermarking
    {
        public static bool WatermarkScaleAndSaveImageFromBitmapImage(ImageFiletypes fileFormat, BitmapImage sourceImage, int newPixelWidth, int newPixelHight, TextWatermarkListWithSerchByUiLabel watermarks, string saveDirectoryPath, string fileNameWithoutExtensionAndPath)
        {
            RenderTargetBitmap rtb = CreateWatermarkedBitmapEncoder(sourceImage, watermarks, newPixelWidth, newPixelHight);            
            return SaveImage(rtb, fileFormat, saveDirectoryPath, fileNameWithoutExtensionAndPath);
        }

        private static RenderTargetBitmap CreateWatermarkedBitmapEncoder(BitmapImage SourceImage, TextWatermarkListWithSerchByUiLabel Watermarks, int newPixelWidth, int newPixelHeight)
        {            
            DrawingVisual visual = new DrawingVisual();
            
            //resize koefficient
            double kw = 1;
            if (SourceImage.PixelWidth > 0) kw = (double)newPixelWidth / SourceImage.PixelWidth;
            double kh = 1;
            if (SourceImage.PixelHeight > 0) kh = (double)newPixelHeight / SourceImage.PixelHeight;

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(SourceImage, new Rect(0, 0, SourceImage.PixelWidth, SourceImage.PixelHeight));

                foreach (Watermark w in Watermarks)
                {
                    //rotate drawing context
                    RotateTransform rt = new RotateTransform();
                    rt.Angle = w.angle;
                    rt.CenterX = 0;
                    rt.CenterY = 0;
                    dc.PushTransform(rt);

                    //dc.PushOpacity(w.opacity);
                    double op = 1 - w.opacity / 100.0;
                    dc.PushOpacity(op);

                    TextWatermark tw = (TextWatermark)w;

                    int x, y;
                    double sin, cos;
                    sin = Math.Sin(w.angle * Math.PI / 180.0);
                    cos = Math.Cos(w.angle * Math.PI / 180.0);
                    x = Convert.ToInt16(tw.GetPixelXlocation(SourceImage.PixelWidth) * cos + tw.GetPixelYlocation(SourceImage.PixelHeight) * sin);
                    y = Convert.ToInt16( - tw.GetPixelXlocation(SourceImage.PixelWidth) * sin + tw.GetPixelYlocation(SourceImage.PixelHeight) * cos);

                    dc.DrawText(tw.GetFormattedText(SourceImage.PixelWidth, SourceImage.PixelHeight), new Point(x, y));

                    dc.Pop();//pop opacity
                    dc.Pop();//pop rotation
                }
            }

            //resize transform
            visual.Transform = new ScaleTransform(kw, kh);

            RenderTargetBitmap rtb = new RenderTargetBitmap(newPixelWidth, newPixelHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);

            return rtb;
        }

        private static bool SaveImage(RenderTargetBitmap rtb, ImageFiletypes FileFormat, string SaveDirectoryPath, string FileNameWithoutExtensionAndPath)
        {
            BitmapEncoder be;
            switch (FileFormat)
            {
                case ImageFiletypes.bmp:
                    be = new BmpBitmapEncoder();
                    break;

                case ImageFiletypes.png:
                    be = new PngBitmapEncoder();
                    break;

                case ImageFiletypes.jpg:
                default:
                    be = new JpegBitmapEncoder();
                    break;
            }

            be.Frames.Add(BitmapFrame.Create(rtb));
            
            string ext = ".jpeg";
            Type t = be.GetType();
            if (t == typeof(BmpBitmapEncoder))
                ext = ".bmp";
            else
                if (t == typeof(PngBitmapEncoder))
                    ext = ".png";
            try
            {
                //check derectory exists
                if (!Directory.Exists(SaveDirectoryPath))
                {
                    Directory.CreateDirectory(SaveDirectoryPath);
                }

                //write file to directory
                using (Stream stm = File.Create(SaveDirectoryPath + "\\" + FileNameWithoutExtensionAndPath + ext))
                {
                    be.Save(stm);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}