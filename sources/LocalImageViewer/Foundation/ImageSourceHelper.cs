using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
namespace LocalImageViewer.Foundation
{
    /// <summary>
    /// ImageSourceの生成関連のヘルパー
    /// </summary>
    public static class ImageSourceHelper
    {
        public static async Task<ImageSource> LoadThumbnailFromByteAsync(byte[] bytes, int w, int h)
        {
            var magickImage = new MagickImage(bytes);
            magickImage.Thumbnail(w, h);

            await using var memoryStream = new MemoryStream();
            await magickImage.WriteAsync(memoryStream);

            return MemoryStreamToImageSource(memoryStream);
        }

        public static ImageSource GetImageSource(byte[] bytes)
        {
            var magickImage = new MagickImage(bytes);

            using var memoryStream = new MemoryStream();
            magickImage.Write(memoryStream);

            return MemoryStreamToImageSource(memoryStream);
        }

        public static ImageSource GetImageSource(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            return GetImageSource(bytes);
        }

        public static ImageSource MemoryStreamToImageSource(MemoryStream memoryStream)
        {
            var image = new BitmapImage();
            memoryStream.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = memoryStream;
            image.EndInit();

            return image;
        }

    }
}
