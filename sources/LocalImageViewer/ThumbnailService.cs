using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// サムネイル生成サービス
    /// ImageMagicを利用している
    /// </summary>
    public class ThumbnailService : DisposableBindable
    {
        public static ImageSource NoneImageSource { get; } = CreateImageSourceFromFile("Resources/loading.png");

        private static ImageSource CreateImageSourceFromFile(string path)
        {
            return new BitmapImage(new Uri(path,UriKind.Relative));
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