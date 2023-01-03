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
        public static async Task<ImageSource> GetImageSourceAsync(string filePath, double scale = 1.0)
        {
            return await Task.Run(async () =>
            {
                using MagickImage magickImage =  new MagickImage(filePath);
                if (scale is not 1.0)
                {
                    magickImage.Thumbnail((int)(magickImage.Width * scale), (int)(magickImage.Height * scale));
                }
                await using var memoryStream = new MemoryStream();
                await magickImage.WriteAsync(memoryStream);
                return MemoryStreamToImageSource(memoryStream);
            });
        }
        public static async Task<ImageSource> GetThumbnailFromByteAsync(byte[] bytes, int w, int h)
        {
            return await Task.Run(async () =>
            {
                using var magickImage = new MagickImage(bytes);
                magickImage.Thumbnail(w, h);
                await using var memoryStream = new MemoryStream();
                await magickImage.WriteAsync(memoryStream);
                return MemoryStreamToImageSource(memoryStream);
            });
        }

        public static ImageSource MemoryStreamToImageSource(Stream memoryStream)
        {
            var image = new BitmapImage();
            memoryStream.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = memoryStream;
            image.EndInit();
            image.Freeze();

            return image;
        }

    }
}
