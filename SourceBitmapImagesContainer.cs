using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    public class BitmapImageCollectionForXaml : ObservableCollection<ImageFromFile>
    {
        public BitmapImageCollectionForXaml()
            : base()
        {}
    }

    class SourceBitmapImagesContainer
    {
        public string baseDirectoryPath
        { get; private set; }

        public BitmapImageCollectionForXaml images
        { get; set; }

        public SourceBitmapImagesContainer(string sourceDirectoryPath, bool useSubDirectories)
        {
            baseDirectoryPath = string.Empty;
            images = new BitmapImageCollectionForXaml();

            if (Directory.Exists(sourceDirectoryPath))
            {
                baseDirectoryPath = sourceDirectoryPath;
                ProcessDirectory(sourceDirectoryPath, useSubDirectories);
            }
        }

        //separate method to use recurse
        private void ProcessDirectory(string sourceDirectoryPath, bool useSubDirectories)
        {
            string[] fileEntries = Directory.GetFiles(sourceDirectoryPath);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName);
            }

            if (useSubDirectories)
            {
                // Recurse into subdirectories of this directory. 
                string[] subdirectoryEntries = Directory.GetDirectories(sourceDirectoryPath);
                foreach (string subdirectory in subdirectoryEntries)
                    ProcessDirectory(subdirectory, useSubDirectories);
            }
        }

        private void ProcessFile(string path)
        {
            //for testing
            //???????????????????????????????
            System.Threading.Thread.Sleep(500);

            try
            {
                string ext = Path.GetExtension(path).ToLower();
                ImageFiletypes ft;

                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                        ft = ImageFiletypes.jpg;
                        break;
                    case ".png":
                        ft = ImageFiletypes.png;
                        break;
                    case ".bmp":
                        ft = ImageFiletypes.bmp;
                        break;

                    default:
                        return;
                }

                string fnwe = Path.GetFileNameWithoutExtension(path);

                string s = Path.GetDirectoryName(path);
                string rp = s.Remove(s.IndexOf(baseDirectoryPath), baseDirectoryPath.Length);

                //???????????????????????????????????
                //add decode pixel to keep small thumbnail images in memory
                //to increase performance
                BitmapImage bi = null;
                bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(path);
                bi.EndInit();

                images.Add(new ImageFromFile(path, rp, fnwe, ft, bi));
            }
            catch (Exception)
            {
            }
        }
    }

    public class ImageFromFile
    {
        public string imageFileFullPath
        { get; private set; }

        public string imageFileDirectoryRelativeToBaseDirectoryPath
        { get; private set; }

        public string imageFileNameWithoutPathAndExtension
        { get; private set; }

        public ImageFiletypes imageFileType;

        public BitmapImage bitmapImageThumbnail
        { get; private set; }

        public ImageFromFile(string directoryFullPath, string directoryRelativeToBaseDirectoryPath, string nameWithoutPathAndExtension, ImageFiletypes type, BitmapImage image)
        {
            imageFileFullPath = directoryFullPath;
            imageFileDirectoryRelativeToBaseDirectoryPath = directoryRelativeToBaseDirectoryPath;
            imageFileNameWithoutPathAndExtension = nameWithoutPathAndExtension;
            imageFileType = type;
            bitmapImageThumbnail = image;
        }
    }
}