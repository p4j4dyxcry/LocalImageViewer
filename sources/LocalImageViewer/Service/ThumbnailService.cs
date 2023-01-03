using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
using LocalImageViewer.DataModel;
using LocalImageViewer.Foundation;
using YiSA.WPF.Common;
namespace LocalImageViewer.Service
{
    /// <summary>
    /// サムネイル生成サービス
    /// ImageMagicを利用している
    /// </summary>
    public class ThumbnailService : DisposableBindable
    {
        public static ImageSource NoneImageSource { get; } = CreateLoadingImage();

        private static ImageSource CreateLoadingImage()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .First(x=> x == "LocalImageViewer.Resources.loading.png");

            var stream = assembly.GetManifestResourceStream(resource);
            return ImageSourceHelper.MemoryStreamToImageSource(stream);
        }
        
        private readonly Config _config;
        public ThumbnailService(Config config)
        {
            _config = config;
        }
        
        public async Task CreateThumbnail(ImageDocument imageDocument)
        {
            var originalPath = imageDocument.Pages[0];

            var thumbnailDirectory = Path.Combine(_config.ThumbnailDirectory, imageDocument.MetaData.Id.ToString());
            var smallThumbnailPath = Path.Combine(thumbnailDirectory, "small.png");
            var largeThumbnailPath = Path.Combine(thumbnailDirectory, "large.png");
            
            Directory.CreateDirectory(thumbnailDirectory);
            
            await Task.Run(async () =>
            {
                using var magick = new MagickImage(originalPath);
                magick.Strip();

                // 大きいサムネイルを作成
                {
                    magick.Thumbnail(256,256);
                    await using var stream = File.Create(largeThumbnailPath);
                    await magick.WriteAsync(stream);                    
                }

                // 小さいサムネイルを作成
                {
                    magick.Thumbnail(64,64);
                    await using var stream = File.Create(smallThumbnailPath);
                    await magick.WriteAsync(stream);                     
                }

            });
        }
    }
}
