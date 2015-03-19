using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Data;


namespace Bulk_Image_Watermark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SourceBitmapImagesContainer sourceContainer;







        public ObservableCollection<BitmapImage> rrr;






        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (sourceContainer == null)
            {
                TryToShowMessageBoxFromResourceDictionary("warningNoResultsToSave", "warning", MessageBoxImage.Error);
                return;
            }
                string savePath = textBoxDestinationPath.Text;
                if (savePath.Length == 0)
                {
                    TryToShowMessageBoxFromResourceDictionary("warningNoDestinationPath", "warning", MessageBoxImage.Error);
                    return;
                }
                if (savePath.ToLower().Equals(sourceContainer.baseDirectoryPath.ToLower()))
                {
                    TryToShowMessageBoxFromResourceDictionary("warningDestinationSourcePathesTheSame", "warning", MessageBoxImage.Error);
                    return;                
                }
            
        }

        private void buttonSelectSourcePath_Click(object sender, RoutedEventArgs e)
        {
            rrr = new ObservableCollection<BitmapImage>();
            BitmapImage fff = new BitmapImage();
            fff.BeginInit();
            fff.UriSource = new Uri("C:\\111\\111.jpg");
            fff.EndInit();
            rrr.Add(fff);

            fff = new BitmapImage();
            fff.BeginInit();
            fff.UriSource = new Uri("C:\\111\\222.jpg");
            fff.EndInit();
            rrr.Add(fff);

            Binding b = new Binding();
            b.Source = rrr;
            listBoxPreview.SetBinding(ListBox.ItemsSourceProperty, b);

            MessageBox.Show(listBoxPreview.ItemsSource.ToString());
            

            ////initialise source images container

            ////??????????????????????????????????????????????
            ////to add - create own wpf folder browser dialog to avoid using winforms
            //var dialog = new System.Windows.Forms.FolderBrowserDialog();
            //dialog.ShowNewFolderButton = false;
            //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    textBoxSourcePath.Text = dialog.SelectedPath;
            //}

        }

        private void buttonSelectDestinationPath_Click(object sender, RoutedEventArgs e)
        {
            //??????????????????????????????????????????????
            //to add - create own wpf folder browser dialog to avoid using winforms
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxDestinationPath.Text = dialog.SelectedPath;
            }
        }

        private void TryToShowMessageBoxFromResourceDictionary(string messageResourceKey, string captionResourceKey, MessageBoxImage icon)
        {
            try
            {
                string msg = (string)Application.Current.FindResource(messageResourceKey);
                string cpt = (string)Application.Current.FindResource(captionResourceKey);
                MessageBox.Show(msg, cpt, MessageBoxButton.OK, icon);
            }
            catch (Exception)
            {
            }
        }

        private void textBoxSourcePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //??????????????????????????????????????????????
            //need to do this in external thread
            sourceContainer = new SourceBitmapImagesContainer(((TextBox)sender).Text,
                checkBoxUseSubdirectories.IsChecked.GetValueOrDefault(),
                !checkBoxDontLoadAllSourceImagesToMemory.IsChecked.GetValueOrDefault());
        }
    }
}
