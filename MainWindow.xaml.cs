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

        //global text watermark list
        private TextWatermarkListWithSerchByUiLabel textWatermarks = new TextWatermarkListWithSerchByUiLabel();
        //private TextWatermark currentTextWatermark;

        //cancel processing token
        CancellationTokenSource cancelTS;

        public MainWindow()
        {
            //add watermark first watermark to have something for binding
            TextWatermark t = new TextWatermark((string)Application.Current.FindResource("newWatermarkText"),
                new FontFamily("Arial"), Colors.Red, 10, 0, 70, 3, 3);

            t.UiLabelOnImageInCanvas.MouseLeftButtonDown += WatermarkLabelOnCanvasTouched;
            textWatermarks.Add(t);

            
            InitializeComponent();

            //preview listbox items count changed handler
            //do not use XAML binding because of cant control items==null
            ((INotifyCollectionChanged)listBoxPreview.Items).CollectionChanged += listBoxPreviewItemsCollectionChanged;
                        
            tabItemTextWatermarks.DataContext = textWatermarks[0];
            
            //put placeholder on canvas to have image on it
            renderPreviewImage();

            //put added first watermark on initialized window
            t.SetLabelGeometryAccordingToImageAndCanvas(imagePreview.Width, imagePreview.Height, canvasMain.ActualWidth, canvasMain.ActualHeight);
            canvasMain.Children.Add(t.UiLabelOnImageInCanvas);
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
            tabItemTextWatermarks.IsEnabled = false;
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
            //image processing result counter
            int numOk = 0;

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
                    try
                    {
                        string s = p.savePath + im.imageFileDirectoryRelativePath;

                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.None;
                        bi.UriSource = new Uri(im.imageFileFullPath);
                        bi.EndInit();

                        ImageFiletypes curType = p.iType;
                        if (!p.needConvert) curType = im.imageFileType;

                        int pw = bi.PixelWidth;
                        int ph = bi.PixelHeight;
                        if (p.needResize)
                        {
                            pw = p.width;
                            ph = p.height;
                            if (bi.PixelWidth < bi.PixelHeight)
                            {
                                int tmp = pw;
                                pw = ph;
                                ph = tmp;
                            }
                        }
                        //???????????????????????????????
                        //if (Watermarking.WatermarkScaleAndSaveImageFromBitmapImage(curType, bi, pw, ph, watermarks, s, im.imageFileNameWithoutPathAndExtension))
                        //{
                        //    Interlocked.Increment(ref numOk);
                        //}
                        bi = null;

                        //update progress bar
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            //multithread safe increment
                            lock (progressBar)
                            { progressBar.Value++; }
                        }));
                    }
                    catch (Exception)
                    {
                        //bad image file?
                    }
                }
                );
            }
            catch (Exception)
            { }

            //enable controls and write message
            this.Dispatcher.Invoke(new Action (() => {
                buttonLoadSourceImages.IsEnabled = true;
                tabItemTextWatermarks.IsEnabled = true;
                buttonSave.Visibility = System.Windows.Visibility.Visible;

                labelMessage.Content = numOk.ToString() + " " + (string)Application.Current.FindResource("okProcessedImagesMessage");
                labelMessage.Content += ", " + (images.Count - numOk).ToString() + " " + (string)Application.Current.FindResource("failProcessedImagesMessage");
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
            BitmapImage bitmapForPreview = new BitmapImage();
            bitmapForPreview.BeginInit();
            if (images != null)
                if (images.Count == 0)
                    //no images loaded
                    bitmapForPreview.UriSource = new Uri("pack://application:,,,/Binary resources/placeholder.jpg");
                else
                    if (listBoxPreview.SelectedItem != null)
                        bitmapForPreview.UriSource = new Uri(((ImageFromFile)listBoxPreview.SelectedItem).imageFileFullPath);
                    else
                        bitmapForPreview.UriSource = new Uri(images[0].imageFileFullPath);
            else
                //no images selected
                bitmapForPreview.UriSource = new Uri("pack://application:,,,/Binary resources/placeholder.jpg");
            bitmapForPreview.EndInit();

            ImageSource isrc = bitmapForPreview;
            imagePreview.Source = isrc;
            FitImageAndWatermarksToCanvas();
        }
        
        private void FitImageAndWatermarksToCanvas()
        {
            if (imagePreview.Source == null) return;
            double canvasAspectRatio = canvasMain.ActualWidth / canvasMain.ActualHeight;
            double imageAspectRatio = imagePreview.Source.Width / imagePreview.Source.Height;
            if (canvasAspectRatio > imageAspectRatio)
            {
                imagePreview.Height = canvasMain.ActualHeight;
                imagePreview.Width = imagePreview.Height * imageAspectRatio;
                Canvas.SetLeft(imagePreview, (canvasMain.ActualWidth - imagePreview.Width) / 2);
                Canvas.SetTop(imagePreview, 0);
            }
            else
            {
                imagePreview.Width = canvasMain.ActualWidth;
                imagePreview.Height = imagePreview.Width / imageAspectRatio;
                Canvas.SetLeft(imagePreview, 0);
                Canvas.SetTop(imagePreview, (canvasMain.ActualHeight - imagePreview.Height) / 2);
            }

            foreach (TextWatermark t in textWatermarks)
                t.SetLabelGeometryAccordingToImageAndCanvas(imagePreview.Width, imagePreview.Height, canvasMain.ActualWidth, canvasMain.ActualHeight);

        }

        private void buttonAddWatermark_Click(object sender, RoutedEventArgs e)
        {
            TextWatermark t = new TextWatermark((string)Application.Current.FindResource("newWatermarkText"),
                new FontFamily("Arial"), Colors.Red, 10, 0, 70, 3, 3);
            t.UiLabelOnImageInCanvas.MouseLeftButtonDown += WatermarkLabelOnCanvasTouched;
            t.SetLabelGeometryAccordingToImageAndCanvas(imagePreview.Width, imagePreview.Height, canvasMain.ActualWidth, canvasMain.ActualHeight);
            canvasMain.Children.Add(t.UiLabelOnImageInCanvas);
            textWatermarks.Add(t);
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

        private void buttonDeleteWatermark_Click(object sender, RoutedEventArgs e)
        {
            //now delete all added watermarks
            //add delete selected watermark
            //????????????????????????????????????
            if (textWatermarks.Count > 1)
            {
                //always keep 1 watermark!!!
                canvasMain.Children.Remove(textWatermarks[textWatermarks.Count - 1].UiLabelOnImageInCanvas);
                textWatermarks.Remove(textWatermarks[textWatermarks.Count - 1]);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            //enable processing cancelation
            if (cancelTS != null )
                cancelTS.Cancel();
        }

        private void canvasMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //resize image and relocate watermarks
            FitImageAndWatermarksToCanvas();
        }

        private void WatermarkLabelOnCanvasTouched(object sender, RoutedEventArgs e)
        {
            //canvas childrens are: image (always is first and has zero index) and added dynamicaly labels (watermarks)
            //label index in textWatermarks list equal to canvas index -1
            //MessageBox.Show("You've touched n°" + canvasMain.Children.IndexOf(sender as UIElement));
            int i = textWatermarks.GetIndexByUiLabel((Label)sender);
            if (i >= 0) tabItemTextWatermarks.DataContext = textWatermarks[i];
        }

    }
}
