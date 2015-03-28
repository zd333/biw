using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{

    public class TextWatermarkListWithSerchByUiLabel : List<TextWatermark>
    {
        public int GetIndexByUiLabel(Label label)
        {
            for (int i=0;i<this.Count;i++)
            {
                if (this[i].UiLabelOnImageInCanvas == label)
                    return i;
            }
            return -1;
        }

    }
    public class InverseOneWayVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((System.Windows.Visibility)value == System.Windows.Visibility.Visible)
                return System.Windows.Visibility.Collapsed;
            else
                return System.Windows.Visibility.Visible;

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class UriToBitmapOneWayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.DecodePixelWidth = 200;
                bi.CacheOption = BitmapCacheOption.OnDemand;
                bi.UriSource = new Uri(value.ToString());
                bi.EndInit();
                //this is for usage in another thread
                bi.Freeze();
                return bi;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
