using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bulk_Image_Watermark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetLanguage()
        {
            //http://www.codeproject.com/Articles/123460/Simplest-Way-to-Implement-Multilingual-WPF-Applica
            //System.Threading.Thread.CurrentThread.CurrentCulture.LCID
            //0x0419 russian
            //0x0422 ukrainian
            //0x0423 belarusian

            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("..\\Resources\\LocalisationStringsEN.xaml", UriKind.Relative);
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
