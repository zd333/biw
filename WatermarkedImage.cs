using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    enum ImageFiletypes
    { jpg, png, bmp}
    class WatermarkedImage
    {
        private BitmapEncoder be;

        public WatermarkedImage(ImageFiletypes FileFormat, BitmapImage SourceImage, List<Watermark> Watermarks)
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

            RenderTargetBitmap rtb = new RenderTargetBitmap(SourceImage.PixelWidth, SourceImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            be.Frames.Add(BitmapFrame.Create(rtb));            
        }


        public bool SaveImage(string SaveDirectoryPath, string FileNameWithoutExtensionAndPath)
        {
            if (be == null) return false;
            
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

        public ImageSource GetImageForUI()
        {
            if (be == null) return null;


            return null;
        }
    }
}
