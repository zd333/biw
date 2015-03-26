using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    class Imaging
    {
        public static async Task<BitmapImageCollectionForXaml> GetImages(string directoryPath, bool useSubDirectories, int maxThumbnailsToLoad)
        {
            BitmapImageCollectionForXaml images = new BitmapImageCollectionForXaml();

            if (Directory.Exists(directoryPath))
            {
                int numThumbnailsLoaded = 0;
                await Task.Run(() => ProcessDirectory(ref images, directoryPath, directoryPath, useSubDirectories, ref numThumbnailsLoaded, maxThumbnailsToLoad));
            }

            return images;
        }

        private static void ProcessDirectory(ref BitmapImageCollectionForXaml images, string directoryPath, string baseDirectoryPath, bool useSubDirectories, ref int thumnbnailsLoaded, int maxThumbnailsToLoad)
        //separate method to use recurse
        {
            string[] fileEntries = Directory.GetFiles(directoryPath);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(ref images, fileName, baseDirectoryPath, ref thumnbnailsLoaded, maxThumbnailsToLoad);
            }

            if (useSubDirectories)
            {
                // recurse into subdirectories 
                string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
                foreach (string subdirectory in subdirectoryEntries)
                    ProcessDirectory(ref images, subdirectory, baseDirectoryPath, useSubDirectories, ref thumnbnailsLoaded, maxThumbnailsToLoad);
            }
        }

        private static void ProcessFile(ref BitmapImageCollectionForXaml images, string filePath, string baseDirectoryPath, ref int thumnbnailsLoaded, int maxThumbnailsToLoad)
        {
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

                if (thumnbnailsLoaded > maxThumbnailsToLoad)
                    images.Add(new ImageFromFile(filePath, rp, fnwe, ft, null));
                else
                {
                    thumnbnailsLoaded++;

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    //decode for thumbnail
                    bi.DecodePixelWidth = 200;
                    //?????????????????????????????????
                    bi.CacheOption = BitmapCacheOption.OnDemand;
                    bi.UriSource = new Uri(filePath);
                    bi.EndInit();
                    //this is for usage in another thread
                    bi.Freeze();

                    images.Add(new ImageFromFile(filePath, rp, fnwe, ft, bi));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}