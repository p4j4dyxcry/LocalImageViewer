using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
namespace LocalImageViewer.Foundation
{
    public class FilePathToImageConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath)
            {
                try
                {
                    if (FilePathToImageCache.TryGet(filePath, out var result))
                    {
                        return result;
                    }

                    ImageSource imageSource = FilePathToImageCache.ConvertCore(filePath);
                    if (FilePathToImageCache.CanNotUsingSimpleConverter(filePath))
                    {
                        FilePathToImageCache.Register(filePath,imageSource);
                    }

                    return imageSource;
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return value;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class FilePathToImageCache
    {
        private const int CacheCapacity = 80;
        private static readonly YiSA.Markup.Converters.FilePathToImageConverter SimpleConverter = new();
        private static readonly LruCache<string, ImageSource> LruCache = new(CacheCapacity);

        public static bool TryGet(string path,out ImageSource result)
        {
            return LruCache.TryGet(path, out result);
        }

        public static void Register(string path,ImageSource value)
        {
            LruCache.Add(path, value);
        }

        public static async Task RegisterCacheAsync(string[] filePaths,string current)
        {
            int index = filePaths.ToList().IndexOf(current);

            if (index is not -1)
            {
                int previousPageLoadNum = CacheCapacity / 4;
                int start = Math.Max(0, index - CacheCapacity - previousPageLoadNum);
                filePaths = filePaths.Skip(start).Take(CacheCapacity).ToArray();
            }
            else
            {
                filePaths = filePaths.Take(CacheCapacity).ToArray();
            }

            foreach (var path in filePaths.Where(CanNotUsingSimpleConverter))
            {
                var img = await ConvertCoreAsync(path);
                LruCache.Add(path,img);
            }
        }

        public static ImageSource ConvertCore(string filePath)
        {
            if (CanSUsingSimpleConverter(filePath))
            {
                return SimpleConverter.Convert(filePath, typeof(ImageSource),default!,CultureInfo.CurrentCulture) as ImageSource;
            }

            return ImageSourceHelper.GetThumbnailFromFilePathByPercent(filePath,0.5d);
        }

        public static async Task<ImageSource> ConvertCoreAsync(string filePath)
        {
            if (CanSUsingSimpleConverter(filePath))
            {
                return SimpleConverter.Convert(filePath, typeof(ImageSource),default!,CultureInfo.CurrentCulture) as ImageSource;
            }

            return await ImageSourceHelper.GetThumbnailFromFilePathByPercentAsync(filePath,0.5d);
        }

        public static bool CanSUsingSimpleConverter(string filePath)
        {
            string lowerPath = filePath.ToLower();
            if (lowerPath.EndsWith("png") ||
                lowerPath.EndsWith("jpeg") ||
                lowerPath.EndsWith("jpg"))
            {
                return true;
            }
            return false;
        }

        public static bool CanNotUsingSimpleConverter(string filePath)
        {
            return !CanSUsingSimpleConverter(filePath);
        }
    }

    public class FilePathToImageExtension: MarkupExtension
    {
        private static FilePathToImageConverter? _converter;
        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            return _converter ??= new FilePathToImageConverter();
        }
    }
}
