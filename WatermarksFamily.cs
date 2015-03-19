﻿using System;
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

        //returns actual x location in pixels according to the image size
        public int GetPixelXlocation(int recipientImageWidth)
        {
            return (recipientImageWidth / 100) * xLocationInPercent;
        }

        //returns actual y location in pixels according to the image size
        public int GetPixelYlocation(int recipientImageHeight)
        {
            return (recipientImageHeight / 100) * yLocationInPercent;
        }

        public int heightInPercent
        {
            get { return heightInPercent; }
            set
            {
                if (value < 1)
                {
                    heightInPercent = 5;//optimal default value
                }
                else if (value > 50)
                {
                    heightInPercent = 50;//too big number
                }
            }
        }

        //rotation angle
        public int angle
        {
            get;
            set;
        }

        public double opacity
        {
            get { return opacity; }
            set
            {
                if (value > 1)
                {
                    opacity = 1;
                }
                else if (value < 0.05)
                {
                    opacity = 0.05;//to avoid invisibility
                }
            }
        }

        public Watermark(int HeightInPercent, int Angle, double Opacity)
        {
            heightInPercent = heightInPercent;
            angle = Angle;
            opacity = Opacity;
        }


        //returns actual height in pixels according to the image size
        protected int GetPixelHeight(int recipientImageWidth, int recipientImageHeight)
        {
            int h = recipientImageHeight;
            if (recipientImageWidth < recipientImageHeight) h = recipientImageWidth;
            h = (h / 100) * heightInPercent;
            return h;
        }
    }

    class TextWatermark : Watermark
    {
        public string text
        {
            get
            {
                return text;
            }
            set
            {
                if (value.Length > 50)
                {
                    text = value.Substring(0, 50);
                }
            }
        }

        public Typeface typeface
        { get; set; }

        public Brush foreground
        { get; set; }

        public TextWatermark(string Text, Typeface Font, Brush Foreground, int HeightInPercent, int Angle, double Opacity)
            :base(HeightInPercent, Angle, Opacity)
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

        public ImageWatermark(BitmapImage Image, int HeightInPercent, int Angle, double Opacity)
            :base(HeightInPercent, Angle, Opacity)
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
