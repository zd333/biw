using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Bulk_Image_Watermark
{
    public class BitmapImageCollectionForXaml : ObservableCollection<ImageFromFile>
    {
        public BitmapImageCollectionForXaml()
            : base()
        { }
    }

    public class ImageFromFile
    {
        public string imageFileFullPath
        { get; private set; }

        public string imageFileDirectoryRelativePath
        { get; private set; }

        public string imageFileNameWithoutPathAndExtension
        { get; private set; }

        public ImageFiletypes imageFileType;

        public BitmapImage bitmapImageThumbnail
        { get; private set; }

        public ImageFromFile(string directoryFullPath, string directoryRelativePath, string nameWithoutPathAndExtension, ImageFiletypes type, BitmapImage image)
        {
            imageFileFullPath = directoryFullPath;
            imageFileDirectoryRelativePath = directoryRelativePath;
            imageFileNameWithoutPathAndExtension = nameWithoutPathAndExtension;
            imageFileType = type;
            bitmapImageThumbnail = image;
        }
    }
}
