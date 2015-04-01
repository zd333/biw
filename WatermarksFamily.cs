using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    public abstract class Watermark
    {
        protected int _xLocationInPercent;
        public abstract int xLocationInPercent
        { get; set; }

        protected int _yLocationInPercent;
        public abstract int yLocationInPercent
        { get; set; }

        public abstract int heightInPercent
        { get; set; }

        public abstract int angle
        { get; set;}

        public abstract int opacity
        { get; set; }

        public int GetPixelXlocation(int recipientImageWidth)
        //returns actual x location in pixels according to the image size
        {
            return (recipientImageWidth / 100) * _xLocationInPercent;
        }

        public int GetPixelYlocation(int recipientImageHeight)
        //returns actual y location in pixels according to the image size
        {
            return (recipientImageHeight / 100) * _yLocationInPercent;
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

    public class TextWatermark : Watermark, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        //control for UI
        public Label UiLabelOnImageInCanvas
        { get; private set; }

        private double containerImageWidth;
        private double containerImageHeight;
        private double containerCanvasWidth;
        private double containerCanvasHeight;

        public override int xLocationInPercent
        {
            get { return _xLocationInPercent; }
            set
            {
                if (value > 100)
                    _xLocationInPercent = 100;
                else
                    if (value < 0)
                        _xLocationInPercent = 0;
                    else
                        _xLocationInPercent = value;
                SetLabelGeometryAccordingToImageAndCanvas(containerImageWidth, containerImageHeight, containerCanvasWidth, containerCanvasHeight);
                RaisePropertyChanged("xLocationInPercent");
            }
        }

        public override int yLocationInPercent
        {
            get { return _yLocationInPercent; }
            set
            {
                if (value > 100)
                    _yLocationInPercent = 100;
                else
                    if (value < 0)
                        _yLocationInPercent = 0;
                    else
                        _yLocationInPercent = value;
                SetLabelGeometryAccordingToImageAndCanvas(containerImageWidth, containerImageHeight, containerCanvasWidth, containerCanvasHeight);
                RaisePropertyChanged("yLocationInPercent");
            }
        }

        private int _heightInPercent;
        public override int heightInPercent
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
                SetLabelGeometryAccordingToImageAndCanvas(containerImageWidth, containerImageHeight, containerCanvasWidth, containerCanvasHeight);
                RaisePropertyChanged("heightInPercent");
            }
        }

        private int _angle;
        public override int angle
        { 
            get { return _angle; }
            set
            {
                if (value < -90) _angle = -90;
                else
                    if (value > 90) _angle = 90;
                    else _angle = value;
                RotateTransform rt = new RotateTransform(Convert.ToDouble(_angle));
                UiLabelOnImageInCanvas.RenderTransform = rt;
                RaisePropertyChanged("angle");
            }
        }

        private int _opacity;
        public override int opacity
        {
            get { return _opacity; }
            set
            {
                if (value > 95)
                    _opacity = 95;//to avoid invisibility
                else
                    if (value < 0)
                        _opacity = 0;
                    else
                        _opacity = value;
                UiLabelOnImageInCanvas.Opacity = 1 - _opacity/100.0;
                RaisePropertyChanged("opacity");
            }
        }

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

                UiLabelOnImageInCanvas.Content = _text;
                RaisePropertyChanged("text");
            }
        }

        private FontFamily _fontFamily;
        public FontFamily fontFamily
        {
            get { return _fontFamily; } 
            set
            {
                _fontFamily = value;
                UiLabelOnImageInCanvas.FontFamily = _fontFamily;
                RaisePropertyChanged("fontFamily");
            }
        }

        private Color _foreground;
        public Color foreground
        { 
            get{return _foreground;} 
            set
            {
                _foreground = value;
                UiLabelOnImageInCanvas.Foreground = new SolidColorBrush(_foreground);
                RaisePropertyChanged("foreground");
            } 
        }

        public TextWatermark()
        // default constructor for xaml binding
        {
            UiLabelOnImageInCanvas = new Label();
            heightInPercent = 10;
            text = "";
            fontFamily = new FontFamily("Arial");
            foreground = Colors.Red;            
        }

        public TextWatermark(string Text, FontFamily Font, Color Foreground, int HeightInPercent, int Angle, int Opacity,
            int XlocationInPercent, int YlocationInPercent)
        {
            UiLabelOnImageInCanvas = new Label();

            heightInPercent = HeightInPercent;
            angle = Angle;
            opacity = Opacity;
            xLocationInPercent = XlocationInPercent;
            yLocationInPercent = YlocationInPercent;
            text = Text;
            fontFamily = Font;
            foreground = Foreground;
            //this is for usage in another thread???????????????????????
            //Foreground.Freeze();

            UiLabelOnImageInCanvas.Content = Text;
            UiLabelOnImageInCanvas.Foreground = new SolidColorBrush(Foreground);
            UiLabelOnImageInCanvas.FontFamily = fontFamily;
            UiLabelOnImageInCanvas.Opacity = 1 - Opacity/100.0;
            RotateTransform rt = new RotateTransform(Convert.ToDouble(Angle));
            UiLabelOnImageInCanvas.RenderTransform = rt;
        }

        public void SetLabelGeometryAccordingToImageAndCanvas(double imageWidth, double imageHeight, double canvasWidth, double canvasHeight)
        {
            //remember container geometry
            containerImageWidth = imageWidth;
            containerImageHeight = imageHeight;
            containerCanvasWidth = canvasWidth;
            containerCanvasHeight = canvasHeight;

            //set position on container
            double left = (canvasWidth - imageWidth) / 2;
            double top = (canvasHeight - imageHeight) / 2;
            Canvas.SetLeft(UiLabelOnImageInCanvas, left + imageWidth * _xLocationInPercent / 100);
            Canvas.SetTop(UiLabelOnImageInCanvas, top + imageHeight * _yLocationInPercent / 100);

            //set font hight
            double h = imageHeight;
            if (imageWidth < imageHeight) h = imageWidth;
            h = (h / 100) * _heightInPercent;
            if (h == 0) h = 10;//to avoid 0 font setting during base object creation
            UiLabelOnImageInCanvas.FontSize = Convert.ToInt16(h);
        }

        public FormattedText GetFormattedText(int recipientImageWidth, int recipientImageHeight)
        {
            return new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                GetPixelHeight(recipientImageWidth, recipientImageHeight),
                new SolidColorBrush(foreground));
        }
    }

    //class ImageWatermark : Watermark
    //{
    //    public BitmapImage image
    //    { get; private set; }

    //    public ImageWatermark(BitmapImage Image, int HeightInPercent, int Angle, double Opacity, int XlocationInPercent, int YlocationInPercent)
    //        :base(HeightInPercent, Angle, Opacity, XlocationInPercent, YlocationInPercent)
    //    {
    //        image = Image;
    //    }

    //    public void UpdateImageFromFile (string Path)
    //    {
    //        try
    //        {
    //            image.BeginInit();
    //            image.UriSource = new Uri(Path);
    //            image.EndInit();
    //        }
    //        catch (Exception)
    //        { }
    //    }
    //}
}
