using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    abstract class Watermark
    {
        public int xLocationInPercent
        {get;set;}

        public int yLocationInPercent
        { get; set; }

        public int GetPixelXlocation(int recipientImageWidth)
        //returns actual x location in pixels according to the image size
        {
            return (recipientImageWidth / 100) * xLocationInPercent;
        }

        public int GetPixelYlocation(int recipientImageHeight)
        //returns actual y location in pixels according to the image size
        {
            return (recipientImageHeight / 100) * yLocationInPercent;
        }

        private int _heightInPercent;
        public int heightInPercent
        {
            get { return _heightInPercent; }
            set
            {
                if (value < 1)
                    _heightInPercent = 5;//optimal default value
                else
                    if (value > 50)
                        _heightInPercent = 50;//too big number
                    else
                        _heightInPercent = value;
            }
        }

        //rotation angle
        public int angle
        { get; set;}

        private double _opacity;
        public double opacity
        {
            get { return _opacity; }
            set
            {
                if (value > 1)
                    _opacity = 1;
                else
                    if (value < 0.05)
                        _opacity = 0.05;//to avoid invisibility
                    else
                        _opacity = value;
            }
        }

        public Watermark(int HeightInPercent, int Angle, double Opacity, int XlocationInPercent, int YlocationInPercent)
        {
            heightInPercent = HeightInPercent;
            angle = Angle;
            opacity = Opacity;
            xLocationInPercent = XlocationInPercent;
            yLocationInPercent = YlocationInPercent;
        }


        protected int GetPixelHeight(int recipientImageWidth, int recipientImageHeight)
        //returns actual height in pixels according to the image size
        {
            int h = recipientImageHeight;
            if (recipientImageWidth < recipientImageHeight) h = recipientImageWidth;
            h = (h / 100) * heightInPercent;
            return h;
        }
    }

    class TextWatermark : Watermark
    {
        private string _text;
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value.Length > 50)
                    _text = value.Substring(0, 50);
                else
                    _text = value;
            }
        }

        public Typeface typeface
        { get; set; }

        public Brush foreground
        { get; set; }

        public TextWatermark(string Text, Typeface Font, Brush Foreground, int HeightInPercent, int Angle, double Opacity, int XlocationInPercent, int YlocationInPercent)
            :base(HeightInPercent, Angle, Opacity, XlocationInPercent, YlocationInPercent)
        {
            text = Text;
            typeface = Font;
            foreground = Foreground;
        }

        public FormattedText GetFormattedText(int recipientImageWidth, int recipientImageHeight)
        {
            return new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                GetPixelHeight(recipientImageWidth, recipientImageHeight),
                foreground);
        }
    }

    class ImageWatermark : Watermark
    {
        public BitmapImage image
        { get; private set; }

        public ImageWatermark(BitmapImage Image, int HeightInPercent, int Angle, double Opacity, int XlocationInPercent, int YlocationInPercent)
            :base(HeightInPercent, Angle, Opacity, XlocationInPercent, YlocationInPercent)
        {
            image = Image;
        }

        public void UpdateImageFromFile (string Path)
        {
            try
            {
                image.BeginInit();
                image.UriSource = new Uri(Path);
                image.EndInit();
            }
            catch (Exception)
            { }
        }
    }
}
