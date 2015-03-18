using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    class SourceBitmapImagesContainer
    {
        public List<string> imageFileDirectoryPaths
        { get; private set; }

        //????????????????????????
        //not necessary?????????
        public List<string> imageFileNamesWithoutExtension
        { get; private set; }


        public List<BitmapImage> bitmapImages
        { get; private set; }

        public SourceBitmapImagesContainer(string sourceDirectoryPath, bool useSubDirectories)
        {
            //SourceImage.BeginInit();
            //SourceImage.UriSource = new Uri("C:\\Documents and Settings\\Zenbook\\Desktop\\111.jpg", UriKind.Absolute);
            //SourceImage.EndInit();

        }

        public void RemoveAt(int i)
        {
            try
            {
                bitmapImages.RemoveAt(i);
                imageFileDirectoryPaths.RemoveAt(i);
                imageFileNamesWithoutExtension.RemoveAt(i);
            }
            catch (Exception)
            {
                //do not transit exception
                //just do nothing
            }
        }
    }
}
