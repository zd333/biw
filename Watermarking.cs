using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    public enum ImageFiletypes
    { jpg, png, bmp}
    static class Watermarking
    {
        public static bool WatermarkBitmapImageAndSaveToFile(ImageFiletypes fileFormat, BitmapImage sourceImage, List<Watermark> watermarks, string saveDirectoryPath, string fileNameWithoutExtensionAndPath)
        {
            RenderTargetBitmap rtb = CreateWatermarkedBitmapEncoder(sourceImage, watermarks);
            return SaveImage(rtb, fileFormat, saveDirectoryPath, fileNameWithoutExtensionAndPath);
        }
        public static bool WatermarkImageFromFileAndSaveToFile(ImageFiletypes fileFormat, string path, List<Watermark> watermarks, string saveDirectoryPath, string fileNameWithoutExtensionAndPath)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path);
            bi.EndInit();
            return WatermarkBitmapImageAndSaveToFile(fileFormat, bi, watermarks, saveDirectoryPath, fileNameWithoutExtensionAndPath);            
        }

        public static BitmapImage GetImageFromBitmapImageForUi(BitmapImage SourceImage, List<Watermark> Watermarks)
        {
            //?????????????????????????????
            //change return type? (what is better for UIconctrol?)
            //after adding usercontrols on preview this method will become not necessary

            return null;
        }

        public static BitmapImage GetImageFromFileForUi(string path, List<Watermark> Watermarks)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path);
            bi.EndInit();
            return GetImageFromBitmapImageForUi(bi, Watermarks);
        }


        //??????????????????????????????
        //change return type? what is better for both UIcontrol source and saving to file?
        private static RenderTargetBitmap CreateWatermarkedBitmapEncoder(BitmapImage SourceImage, List<Watermark> Watermarks)
        {            
            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(SourceImage, new Rect(0, 0, SourceImage.PixelWidth, SourceImage.PixelHeight));

                foreach (Watermark w in Watermarks)
                {
                    //rotate drawing context
                    RotateTransform rt = new RotateTransform();
                    rt.Angle = w.angle;
                    rt.CenterX = SourceImage.PixelWidth / 2;
                    rt.CenterY = SourceImage.PixelHeight / 2;
                    dc.PushTransform(rt);

                    dc.PushOpacity(w.opacity);

                    Type t = w.GetType();
                    if (t == typeof(TextWatermark))
                    {
                        TextWatermark tw = (TextWatermark)w;
                        dc.DrawText(tw.GetFormattedText(SourceImage.PixelWidth, SourceImage.PixelHeight),
                            new Point(tw.GetPixelXlocation(SourceImage.PixelWidth), tw.GetPixelYlocation(SourceImage.PixelHeight))); 
                    }
                    else
                        if (t == typeof(ImageWatermark))
                        {
                            //?????????????????????????????????????????????
                            //will add image watermarks here
                        }
                    dc.Pop();
                }
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(SourceImage.PixelWidth, SourceImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
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
                using (Stream stm = File.Create(SaveDirectoryPath + FileNameWithoutExtensionAndPath + ext))
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