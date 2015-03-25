using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Bulk_Image_Watermark
{
    public class InverseOneWayVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(System.Windows.Visibility))
                throw new InvalidOperationException("The target must be a visibility");

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
}
