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
using System.Collections.Specialized;


namespace Bulk_Image_Watermark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //global source images container
        private SourceBitmapImagesContainer sourceContainer;

        //global watermark list
        private List<Watermark> watermarks = new List<Watermark>();

        //source property for preview
        private BitmapSource bitmapForPreview;

        //global current typeface
        //????????????????????????????????????????
        //will not need this with adorners
        Typeface curTypeface = new Typeface("Arial");

        //global current font color
        //????????????????????????????????????????
        //will not need this with adorners
        Brush curFontColor = Brushes.GreenYellow;

        public MainWindow()
        {
            InitializeComponent();

            //preview listbox items count changed handler
            //do not use XAML binding because of cant control items==null
            ((INotifyCollectionChanged)listBoxPreview.Items).CollectionChanged += listBoxPreviewItemsCollectionChanged;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            //check save settings
            if (sourceContainer == null)
            {
                ShowMessageBoxFromResourceDictionary("warningNoResultsToSave", "warning", MessageBoxImage.Error);
                return;
            }
            string savePath = textBoxDestinationPath.Text;
            if (savePath.Length == 0)
            {
                ShowMessageBoxFromResourceDictionary("warningNoDestinationPath", "warning", MessageBoxImage.Error);
                return;
            }
            if (savePath.ToLower().Equals(sourceContainer.baseDirectoryPath.ToLower()))
            {
                ShowMessageBoxFromResourceDictionary("warningDestinationSourcePathesTheSame", "warning", MessageBoxImage.Error);
                return;
            }

            //??????????????????????????????????????
            //image processing and saving
        }


        private void buttonSelectSourcePath_Click(object sender, RoutedEventArgs e)
        {
            //??????????????????????????????????????????????
            //to add - create own wpf folder browser dialog to avoid using winforms
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxSourcePath.Text = dialog.SelectedPath;
            }
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

        //custom method to show message box with strings from resources
        private void ShowMessageBoxFromResourceDictionary(string messageResourceKey, string captionResourceKey, MessageBoxImage icon)
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

        private void LoadSourceImagesToContainer()
        {
            //disable controls that can start this method
            //??????????????????????????????????????????????????
            //add cancelation and restart mechanism instead of disabling
            buttonLoadSourceImages.IsEnabled = false;
            checkBoxDontLoadAllSourceImagesToMemory.IsEnabled = false;

            sourceContainer = new SourceBitmapImagesContainer(textBoxSourcePath.Text,
                checkBoxUseSubdirectories.IsChecked.GetValueOrDefault(),
                !checkBoxDontLoadAllSourceImagesToMemory.IsChecked.GetValueOrDefault());

            listBoxPreview.Resources["previewImages"] = sourceContainer.images;

            buttonLoadSourceImages.IsEnabled = true;
            checkBoxDontLoadAllSourceImagesToMemory.IsEnabled = true;
            
            renderPreviewImage();
        }

        private void checkBoxDontLoadAllSourceImagesToMemory_Click(object sender, RoutedEventArgs e)
        {
            LoadSourceImagesToContainer();
        }

        //preview listbox context menu remove pressed handler
        //delete selected objects from source container, which property set as dictionary source for listbox
        private void DeleteSourceImageFromResource(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Collections.IList l = listBoxPreview.SelectedItems;
                foreach (object x in l)                    
                    sourceContainer.images.Remove((ImageFromFile)x);
            }
            catch (Exception)
            {}
        }

        private void listBoxPreview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //change image and render watermarks
            renderPreviewImage();
        }

        private void renderPreviewImage()
        {
            //put image on preview panel
            BitmapImage tbi = new BitmapImage();
            if (sourceContainer != null)
                if ((sourceContainer.images.Count == 0) || (checkBoxDontLoadAllSourceImagesToMemory.IsChecked == true))
                {
                    //no images selected or user admited not to load images to memory
                    tbi = GetResourcesPlaceholderMemoryClone();
                }
                else
                    if (listBoxPreview.SelectedItem != null)
                        tbi = ((ImageFromFile)listBoxPreview.SelectedItem).bitmapImage.CloneCurrentValue();
                    else
                        tbi = sourceContainer.images[0].bitmapImage.CloneCurrentValue();
            else
                //no images selected
                tbi = GetResourcesPlaceholderMemoryClone();

            //???????????????????????????????
            //this part of method will be not necessary with adorners
            
            //create watermarks and place them on preview image
            bitmapForPreview = Watermarking.GetImageFromBitmapImageForUi(tbi,watermarks);
            ImageSource isrc = bitmapForPreview;
            imagePreview.Source = isrc;
        }

        private BitmapImage GetResourcesPlaceholderMemoryClone()
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/Binary resources/placeholder.jpg");
            bi.EndInit();
            return bi.CloneCurrentValue();
        }

        private void buttonInsertWatermarks_Click(object sender, RoutedEventArgs e)
        {
            //change image and render watermarks
            //just testing now
            //????????????????????????????????????????????????
            int size, ang, op, x, y;
            size = 5;
            ang = 45;
            op = 50;
            int.TryParse(textBoxWatermarkSize.Text, out size);
            int.TryParse(textBoxWatermarkAngle.Text, out ang);
            int.TryParse(textBoxWatermarkOpacity.Text, out op);
            x = 50;
            y = 50;

            int numx = 1;
            int numy = 1;
            int.TryParse(textBoxNumX.Text,out numx);
            int.TryParse(textBoxNumY.Text,out numy);

            int stepx = 100/(numx+1);
            int stepy = 100/(numy+1);

            for (int xi = 1; xi <= numx; xi++)
                for (int yi = 1; yi <= numy; yi++)
                {
                    x = xi * stepx;
                    y = yi * stepy;
                    double dop =1.0 - op / 100.0;
                    watermarks.Add(new TextWatermark(textBoxWatermarkText.Text, curTypeface, curFontColor, size, ang, dop, x, y));
                }

            renderPreviewImage();
        }

        //hide/show preview listbox depending to items are emty
        private void listBoxPreviewItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                ItemCollection l = (ItemCollection)sender;
                if (l != null)
                    //there is no items in listbox or user admited not to load source images to memory
                    if ((l.Count > 0) && (checkBoxDontLoadAllSourceImagesToMemory.IsChecked == false))
                    {
                        listBoxPreview.Visibility = Visibility.Visible;
                        return;
                    }
                listBoxPreview.Visibility = Visibility.Collapsed;
            }
                catch(Exception)
            {}
        }
         
        private void buttonLoadSourceImages_Click(object sender, RoutedEventArgs e)
        {
            LoadSourceImagesToContainer();
        }

        private void buttonDeleteWatermarks_Click(object sender, RoutedEventArgs e)
        {
            //remove all watermarks and rewrite image
            watermarks.Clear();
            renderPreviewImage();
        }
    }    
}
