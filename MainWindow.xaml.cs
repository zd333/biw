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
using System.Windows.Documents;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;


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
            InitializeComponent();

            //preview listbox items count changed handler
            //do not use XAML binding because of cant control items==null
            ((INotifyCollectionChanged)listBoxPreview.Items).CollectionChanged += listBoxPreviewItemsCollectionChanged;
                                    
            //put placeholder on canvas to have image on it
            renderPreviewImage();
            
            //AddNewWatermark();            
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

                        if (Watermarking.WatermarkScaleAndSaveImageFromBitmapImage(curType, bi, pw, ph, textWatermarks, s, im.imageFileNameWithoutPathAndExtension))
                        {
                            Interlocked.Increment(ref numOk);
                        }
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
            AddNewWatermark();
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
            if (textWatermarks.Count > 0)
            {
                //remember selected watermark label
                TextWatermark foo = (TextWatermark)tabItemTextWatermarks.DataContext;
                
                int i = textWatermarks.GetIndexByUiLabel(foo.UiLabelOnImageInCanvas);
                if (i >= 0)
                {
                    tabItemTextWatermarks.DataContext = null;

                    canvasMain.Children.Remove(foo.UiLabelOnImageInCanvas);
                    textWatermarks.Remove(textWatermarks[i]);
                    if (textWatermarks.Count > 0)
                    {
                        tabItemTextWatermarks.DataContext = textWatermarks[textWatermarks.Count - 1];
                        AddAdornerToWatermarkLabel(textWatermarks[textWatermarks.Count - 1]);
                    }
                }
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
            int i = textWatermarks.GetIndexByUiLabel((Label)sender);
            if (i >= 0)
            {
                RemoveAllAdornersFromLabelWatermarks();
                tabItemTextWatermarks.DataContext = textWatermarks[i];
                AddAdornerToWatermarkLabel(textWatermarks[i]);
            }
        }

        private void RemoveAllAdornersFromLabelWatermarks()
        {
            foreach (TextWatermark t in textWatermarks)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(t.UiLabelOnImageInCanvas);
                Adorner[] adorners = layer.GetAdorners(t.UiLabelOnImageInCanvas);
                if (adorners != null)
                    if (adorners.Length > 0)
                        foreach (Adorner a in adorners)
                            layer.Remove(a);
            }
        }

        private void AddNewWatermark()
        {
            RemoveAllAdornersFromLabelWatermarks();

            TextWatermark t = new TextWatermark((string)Application.Current.FindResource("newWatermarkText"),
                new System.Windows.Media.FontFamily("Arial"), Colors.Red, 10, 0, 70, 3, 3);

            //selection
            t.UiLabelOnImageInCanvas.MouseLeftButtonDown += WatermarkLabelOnCanvasTouched;                          
            t.UiLabelOnImageInCanvas.Cursor = Cursors.Hand;
            t.SetLabelGeometryAccordingToImageAndCanvas(imagePreview.Width, imagePreview.Height, canvasMain.ActualWidth, canvasMain.ActualHeight);
            canvasMain.Children.Add(t.UiLabelOnImageInCanvas);

            AddAdornerToWatermarkLabel(t);

            textWatermarks.Add(t);
            tabItemTextWatermarks.DataContext = t;
        }

        private void AddAdornerToWatermarkLabel(TextWatermark watermark)
        {
            if (watermark != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(watermark.UiLabelOnImageInCanvas);
                WatermarkLabelAdorner a = new WatermarkLabelAdorner(watermark.UiLabelOnImageInCanvas);
                a.OnDrag += TextWatermarkDragHandler;
                layer.Add(a);   
            }
        }

        private void TextWatermarkDragHandler(WatermarkLabelAdorner sender, DragOperations operation, double diffX, double diffY)
        {
            Label l = sender.AdornedElement as Label;
            if (l == null) return;
            int i = textWatermarks.GetIndexByUiLabel(l);
            if (i < 0) return;
            TextWatermark t = textWatermarks[i];

            if (operation == DragOperations.Move)
            {
                int diffXinPercent = Convert.ToInt16((diffX / imagePreview.Width) * 100);
                int diffYinPercent = Convert.ToInt16((diffY / imagePreview.Height) * 100);
                t.xLocationInPercent += diffXinPercent;
                t.yLocationInPercent += diffYinPercent;
            }
            else
                if (operation == DragOperations.Rotate)
                {
                    double ox = sender.ActualWidth * Math.Cos((t.angle) * Math.PI / 180);
                    double oy = sender.ActualWidth * Math.Sin((t.angle) * Math.PI / 180);                    
                    double nx = ox + diffX;
                    if (nx < 0)
                        if (diffY < 0)
                            t.angle = -90;
                        else
                            t.angle = 90;
                    else
                    {
                        double ny = oy + diffY;
                        double na = Math.Atan(ny / nx) * (180.0 / Math.PI);
                        t.angle = Convert.ToInt16(na);
                    }
                }

        }

        private void buttonSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (textWatermarks.Count == 0)
            {
                ShowMessageBoxFromResourceDictionary("nothingToSaveInTemplate", "warning", MessageBoxImage.Error);
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "MyWatermarks";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Watermark templates (.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string path = dlg.FileName;
                //use helper class with stored properties to serialize text watermarks
                //serialization of original class is problematic because of cokplex properties (such as UI control, etc.)
                TextWatermarkSerializationHelper[] th = new TextWatermarkSerializationHelper[textWatermarks.Count];

                for (int i = 0; i < th.Length; i++)
                {
                    System.Drawing.Color c = System.Drawing.Color.FromArgb(textWatermarks[i].foreground.A,
                        textWatermarks[i].foreground.R, textWatermarks[i].foreground.G, textWatermarks[i].foreground.B);
                    
                    th[i] = new TextWatermarkSerializationHelper(
                        textWatermarks[i].text,
                        textWatermarks[i].fontFamily.ToString(),
                        c.ToArgb(),
                        textWatermarks[i].heightInPercent,
                        textWatermarks[i].angle,
                        textWatermarks[i].opacity,
                        textWatermarks[i].xLocationInPercent,
                        textWatermarks[i].yLocationInPercent);
                }

                XmlSerializer writer = new XmlSerializer(typeof(TextWatermarkSerializationHelper[]));                
                try
                {
                    System.IO.FileStream file = System.IO.File.Create(path);
                    writer.Serialize(file, th);
                    file.Close();
                }
                catch (Exception)
                { }
            } 
        }

        private void buttonLoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "MyWatermarks";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Watermark templates (.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string path = dlg.FileName;
                XmlSerializer reader = new XmlSerializer(typeof(TextWatermarkSerializationHelper[]));
                TextWatermarkSerializationHelper[] th = null;
                try
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(path);
                    th = (TextWatermarkSerializationHelper[])reader.Deserialize(file);
                }
                catch (Exception)
                { }
                if (th != null)
                {
                    RemoveAllAdornersFromLabelWatermarks();
                    textWatermarks = new TextWatermarkListWithSerchByUiLabel();
                    foreach (TextWatermarkSerializationHelper h in th)
                    {
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(h.colorArgb);
                        TextWatermark t = new TextWatermark(h.text,
                            new System.Windows.Media.FontFamily(h.fontFamilyName),
                            System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B),
                            h.height,
                            h.angle,
                            h.opacity,
                            h.x,
                            h.y);

                        t.UiLabelOnImageInCanvas.MouseLeftButtonDown += WatermarkLabelOnCanvasTouched;
                        t.UiLabelOnImageInCanvas.Cursor = Cursors.Hand;
                        t.SetLabelGeometryAccordingToImageAndCanvas(imagePreview.Width, imagePreview.Height, canvasMain.ActualWidth, canvasMain.ActualHeight);
                        canvasMain.Children.Add(t.UiLabelOnImageInCanvas);
                        
                        textWatermarks.Add(t);
                    }

                    AddAdornerToWatermarkLabel(textWatermarks[textWatermarks.Count - 1]);
                    tabItemTextWatermarks.DataContext = textWatermarks[textWatermarks.Count - 1];
                }
            }
        }
    }
}
