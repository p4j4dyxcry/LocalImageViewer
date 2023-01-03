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
        public static async Task<ImageSource> GetThumbnailFromFilePathByPercentAsync(string filePath, double percent)
        {
            byte[] bytes = await File.ReadAllBytesAsync(filePath);

            using var magickImage = new MagickImage(bytes);
            magickImage.Thumbnail((int)(magickImage.Width * percent),  (int)(magickImage.Height * percent));

            await using var memoryStream = new MemoryStream();
            await magickImage.WriteAsync(memoryStream);

            return await Task.Run(() => MemoryStreamToImageSource(memoryStream));
        }

        public static ImageSource GetThumbnailFromFilePathByPercent(string filePath, double percent)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            using var magickImage = new MagickImage(bytes);
            magickImage.Thumbnail((int)(magickImage.Width * percent),  (int)(magickImage.Height * percent));

            using var memoryStream = new MemoryStream();
            magickImage.Write(memoryStream);

            return MemoryStreamToImageSource(memoryStream);
        }

        public static async Task<ImageSource> GetThumbnailFromByteAsync(byte[] bytes, int w, int h)
        {
            using var magickImage = new MagickImage(bytes);
            magickImage.Thumbnail(w, h);

            await using var memoryStream = new MemoryStream();
            await magickImage.WriteAsync(memoryStream);

            return MemoryStreamToImageSource(memoryStream);
        }

        public static ImageSource GetImageSource(byte[] bytes)
        {
            using var magickImage = new MagickImage(bytes);

            using var memoryStream = new MemoryStream();
            magickImage.Write(memoryStream);

            return MemoryStreamToImageSource(memoryStream);
        }

        public static ImageSource GetImageSource(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            return GetImageSource(bytes);
        }

        public static async Task<ImageSource> GetImageSourceAsync(byte[] bytes)
        {
            using var magickImage = new MagickImage(bytes);

            await using var memoryStream = new MemoryStream();
            await magickImage.WriteAsync(memoryStream);

            return MemoryStreamToImageSource(memoryStream);
        }

        public static async Task<ImageSource> GetImageSourceAsync(string filePath)
        {
            byte[] bytes = await File.ReadAllBytesAsync(filePath);
            return await GetImageSourceAsync(bytes);
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
            image.Freeze();

            return image;
        }

    }
}
