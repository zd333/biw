using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    class Imaging
    {
        //public BitmapImageCollectionForXaml images
        //{ get; set; }

        public static BitmapImageCollectionForXaml GetImages(string directoryPath, bool useSubDirectories)
        {
            BitmapImageCollectionForXaml images = new BitmapImageCollectionForXaml();

            if (Directory.Exists(directoryPath))
            {
                ProcessDirectory(ref images, directoryPath, directoryPath, useSubDirectories);
            }

            return images;
        }

        //separate method to use recurse
        private static void ProcessDirectory(ref BitmapImageCollectionForXaml images, string directoryPath, string baseDirectoryPath, bool useSubDirectories)
        {
            string[] fileEntries = Directory.GetFiles(directoryPath);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(ref images, fileName, baseDirectoryPath);
            }

            if (useSubDirectories)
            {
                // recurse into subdirectories 
                string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
                foreach (string subdirectory in subdirectoryEntries)
                    ProcessDirectory(ref images, subdirectory, baseDirectoryPath, useSubDirectories);
            }
        }

        private static void ProcessFile(ref BitmapImageCollectionForXaml images, string filePath, string baseDirectoryPath)
        {
            //for testing
            //???????????????????????????????
            System.Threading.Thread.Sleep(500);

            try
            {
                string ext = Path.GetExtension(filePath).ToLower();
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

                string fnwe = Path.GetFileNameWithoutExtension(filePath);

                string s = Path.GetDirectoryName(filePath);
                string rp = s.Remove(s.IndexOf(baseDirectoryPath), baseDirectoryPath.Length);

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                //???????????????????????????????????
                //need to test memory usage with and without decodepixel
                //decode for thumbnail
                bi.DecodePixelWidth = 200;
                bi.UriSource = new Uri(filePath);
                bi.EndInit();

                images.Add(new ImageFromFile(filePath, rp, fnwe, ft, bi));
            }
            catch (Exception)
            {
            }
        }
    }
}