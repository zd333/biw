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
using System.Windows.Media.Animation;
using System.Threading;


namespace Bulk_Image_Watermark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //global source images container
        //private Imaging sourceContainer;
        private BitmapImageCollectionForXaml images;

        //global watermark list
        private List<Watermark> watermarks = new List<Watermark>();

        //source property for preview
        private BitmapSource bitmapForPreview;

        //cancel processing token
        CancellationTokenSource cancelTS;

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
            if (images == null)
            {
                ShowMessageBoxFromResourceDictionary("warningNoResultsToSave", "warning", MessageBoxImage.Error);
                return;
            }
            if (images.Count == 0)
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

            //disable controls to avoid parallel invoke or processing data update
            buttonLoadSourceImages.IsEnabled = false;
            buttonSave.Visibility = System.Windows.Visibility.Collapsed;
            labelMessage.Content = (string)Application.Current.FindResource("processingImagesMessage");

            //prepare resize dimensions if they are necessary in next processing
            //1024*768 will be default values if validation not passed
            int width = 1024;
            int height = 768;
            try
            {                
                string[] s;
                if (comboBoxResultResolution.SelectedItem != null)
                {
                    ComboBoxItem c = (ComboBoxItem)comboBoxResultResolution.SelectedItem;
                    s = c.Content.ToString().Split('*');
                }
                else
                    s = comboBoxResultResolution.Text.ToString().Split('*');
                
                width = int.Parse(s[0]);
                height = int.Parse(s[1]);
            }
            catch (Exception)
            {
            }
            if (height > width)
            {
                int t = width;
                width = height;
                height = t;
            }

            //prepare for convertion if necessary
            ImageFiletypes iType = ImageFiletypes.jpg;
            if (checkBoxChangeFormat.IsChecked.GetValueOrDefault())
            {
                switch (((ComboBoxItem)comboBoxFileType.SelectedItem).Content.ToString().ToLower())
                {
                    case "png":
                        iType = ImageFiletypes.png;
                        break;
                    case "bmp":
                        iType = ImageFiletypes.bmp;
                        break;
                    case "jpg":
                    default:
                        iType = ImageFiletypes.jpg;
                        break;
                }
            }

            progressBar.Maximum = images.Count;
            progressBar.Value = 0;

            //start processing and saving in other thread to avoid UI lock
            Thread th = new Thread(new ParameterizedThreadStart(ProcessAndSaveResults));
            th.Start(new ProcessAndSaveResultsParams(checkBoxResizeResults.IsChecked.GetValueOrDefault(), width, height,
                checkBoxChangeFormat.IsChecked.GetValueOrDefault(),iType, savePath));
        }

        private class ProcessAndSaveResultsParams
            //params for parametrized thread invoke
        {
            public bool needResize; public int width; public int height; public bool needConvert; public ImageFiletypes iType; public string savePath;
            public ProcessAndSaveResultsParams(bool NeedResize, int Width, int Height, bool NeedConvert, ImageFiletypes IType, string SavePath)
            {
                needResize = NeedResize; width = Width; height = Height; needConvert = NeedConvert; iType = IType; savePath = SavePath;
            }
        }
        private void ProcessAndSaveResults(object parameters)
        {
            //image processing result counters;
            int numOk = 0;
            int numFail = 0;

            if (parameters.GetType() != typeof(ProcessAndSaveResultsParams)) return;//do not throw exception, just do nothing

            ProcessAndSaveResultsParams p = (ProcessAndSaveResultsParams)parameters;
            ParallelOptions po = new ParallelOptions();
            cancelTS = new CancellationTokenSource();
            po.CancellationToken = cancelTS.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            try
            { 
                Parallel.ForEach(images, po, im =>
                {
                    po.CancellationToken.ThrowIfCancellationRequested();

                    string s = p.savePath + im.imageFileDirectoryRelativePath;

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(im.imageFileFullPath);
                    bi.EndInit();

                    ImageFiletypes curType = p.iType;
                    if (!p.needConvert) curType = im.imageFileType;

                    int pw = bi.PixelWidth;
                    int ph = bi.PixelHeight;
                    if (p.needResize)
                        if (bi.PixelWidth < bi.PixelHeight)
                        {
                            int tmp = pw;
                            pw = ph;
                            ph = tmp;
                        }
                    if (Watermarking.WatermarkScaleAndSaveImageFromBitmapImage(curType, bi, pw, ph, watermarks, s, im.imageFileNameWithoutPathAndExtension))
                        Interlocked.Increment(ref numOk);
                    else
                        Interlocked.Increment(ref numFail);

                    //update progress bar
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        //multithread safe increment
                        lock (progressBar)
                        { progressBar.Value++; }
                    }));
                }
                );
            }
            catch (OperationCanceledException)
            { }

            //enable controls and write message
            this.Dispatcher.Invoke(new Action (() => {
                buttonLoadSourceImages.IsEnabled = true;
                buttonSave.Visibility = System.Windows.Visibility.Visible;

                labelMessage.Content = numOk.ToString() + " " + (string)Application.Current.FindResource("okProcessedImagesMessage");
                labelMessage.Content += ", " + numFail.ToString() + " " + (string)Application.Current.FindResource("failProcessedImagesMessage");
            }));
        }


        private void buttonSelectSourcePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxSourcePath.Text = dialog.SelectedPath;
            }
        }

        private void buttonSelectDestinationPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxDestinationPath.Text = dialog.SelectedPath;
            }
        }

        private void ShowMessageBoxFromResourceDictionary(string messageResourceKey, string captionResourceKey, MessageBoxImage icon)
        //custom method to show message box with strings from resources
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

        private async void LoadSourceImagesToContainer()
        {
            
            //disable controls that can start this method or image processing and saving
            buttonLoadSourceImages.IsEnabled = false;
            buttonSave.IsEnabled = false;

            labelMessage.Content = (string)Application.Current.FindResource("loadingImagesMessage");

            //start progress bar animation
            progressBar.Maximum = 100;
            progressBar.IsIndeterminate = true;

            //reset listbox and image source
            listBoxPreview.Resources["previewImages"] = null;
            images = null;
            renderPreviewImage();

            images  = new BitmapImageCollectionForXaml();
            images = await Imaging.GetImages(textBoxSourcePath.Text, checkBoxUseSubdirectories.IsChecked.GetValueOrDefault());

            listBoxPreview.Resources["previewImages"] = images;

            labelMessage.Content = images.Count.ToString() + " " + (string)Application.Current.FindResource("loadedImagesQuantityMessage");

            //enable controls
            buttonLoadSourceImages.IsEnabled = true;
            buttonSave.IsEnabled = true;
            
            renderPreviewImage();

            //reset progress bar animation
            progressBar.IsIndeterminate = false;
        }

        private void DeleteSourceImageFromResource(object sender, RoutedEventArgs e)
        //preview listbox context menu remove pressed handler
        //delete selected objects from source container, which property set as dictionary source for listbox
        {
            try
            {
                System.Collections.IList l = listBoxPreview.SelectedItems;
                foreach (object x in l)                    
                    images.Remove((ImageFromFile)x);
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
            if (images != null)
                if (images.Count == 0)
                {
                    //no images loaded
                    bitmapForPreview = Watermarking.GetImageFromBitmapImageForUi(GetResourcesPlaceholder(), watermarks);
                }
                else
                    if (listBoxPreview.SelectedItem != null)
                        //tbi = ((ImageFromFile)listBoxPreview.SelectedItem).bitmapImage;
                        bitmapForPreview = Watermarking.GetImageFromFileForUi(((ImageFromFile)listBoxPreview.SelectedItem).imageFileFullPath, watermarks);
                    else
                        //bitmapForPreview = sourceContainer.images[0].bitmapImage;
                        bitmapForPreview = Watermarking.GetImageFromFileForUi(images[0].imageFileFullPath, watermarks);
            else
                //no images selected
                bitmapForPreview = Watermarking.GetImageFromBitmapImageForUi(GetResourcesPlaceholder(), watermarks);

            ImageSource isrc = bitmapForPreview;
            imagePreview.Source = isrc;
        }

        private BitmapImage GetResourcesPlaceholder()
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/Binary resources/placeholder.jpg");
            bi.EndInit();
            return bi;
        }

        private void buttonInsertWatermarks_Click(object sender, RoutedEventArgs e)
        {
            //change image and render watermarks

            //get selected font family
            Typeface f = new Typeface("Arial");
            try
            {
                f = new Typeface(comboBoxFontFamily.SelectedItem.ToString());
            }
            catch (Exception)
            {}

            //get selected color
            Brush c = new SolidColorBrush(colorPickerFontColor.SelectedColor);
            

            //set geometrical parameters
            int size, ang, op, x, y;
            size = 5;
            ang = 45;
            op = 50;
            int.TryParse(textBoxWatermarkSize.Text, out size);
            int.TryParse(textBoxWatermarkAngle.Text, out ang);
            int.TryParse(textBoxWatermarkOpacity.Text, out op);

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
                    watermarks.Add(new TextWatermark(textBoxWatermarkText.Text, f, c , size, ang, dop, x, y));
                }

            renderPreviewImage();
        }

        private void listBoxPreviewItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //hide/show preview listbox depending to items are emty
        {
            try
            {
                ItemCollection l = (ItemCollection)sender;
                if (l != null)
                    //there is some items in listbox
                    if (l.Count > 0)
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

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            //enable processing cancelation
            if (cancelTS != null )
                cancelTS.Cancel();
        }
    }    
}
