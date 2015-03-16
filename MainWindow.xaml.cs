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
            SetLanguage();
            InitializeComponent();
        }

        private void SetLanguage()
        {
            //check current culture and add localisation if needed

            ResourceDictionary dict = new ResourceDictionary();

            switch (System.Threading.Thread.CurrentThread.CurrentCulture.LCID)
            {
                //russian
                case 0x0419://RU
                case 0x0422://UA
                case 0x0423://BE
                    dict.Source = new Uri("..\\Culture\\LocalisationStringsRU.xaml", UriKind.Relative);
                    break;
                //case XXX:
                //    break;
            }
            if (dict.Source != null)
            {
                this.Resources.MergedDictionaries.Add(dict);
            }
        }
    }
}
