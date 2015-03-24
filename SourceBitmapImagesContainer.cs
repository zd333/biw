using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        private bool _keepSourceFilesInMemoryForUiPreview;

        public SourceBitmapImagesContainer(string sourceDirectoryPath, bool useSubDirectories, bool keepSourceFilesInMemoryForUiPreview)
        {
            baseDirectoryPath = string.Empty;
            images = new BitmapImageCollectionForXaml();
            _keepSourceFilesInMemoryForUiPreview = keepSourceFilesInMemoryForUiPreview;

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

                BitmapImage bi = null;
                if (_keepSourceFilesInMemoryForUiPreview)
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(path);
                    bi.EndInit();
                }

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

        public BitmapImage bitmapImage
        { get; private set; }

        public ImageFromFile(string directoryFullPath, string directoryRelativeToBaseDirectoryPath, string nameWithoutPathAndExtension, ImageFiletypes type, BitmapImage image)
        {
            imageFileFullPath = directoryFullPath;
            imageFileDirectoryRelativeToBaseDirectoryPath = directoryRelativeToBaseDirectoryPath;
            imageFileNameWithoutPathAndExtension = nameWithoutPathAndExtension;
            imageFileType = type;
            bitmapImage = image;
        }
    }
}