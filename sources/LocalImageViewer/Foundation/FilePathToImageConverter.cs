using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
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
                    FilePathToImageCache.Register(filePath,imageSource);

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
        private static readonly YiSA.Markup.Converters.FilePathToImageConverter SimpleConverter = new();
        private static readonly LruCache<string, ImageSource> LruCache = new(200);

        public static bool TryGet(string path,out ImageSource result)
        {
            return LruCache.TryGet(path, out result);
        }

        public static void Register(string path,ImageSource value)
        {
            LruCache.Add(path, value);
        }

        public static async Task RegisterCacheAsync(string[] filePaths)
        {
            foreach (var path in filePaths)
            {
                var img = await ConvertCoreAsync(path);
                LruCache.Add(path,img);
            }
        }

        public static ImageSource ConvertCore(string filePath)
        {
            string lowerPath = filePath.ToLower();
            ImageSource result = null;
            if (lowerPath.EndsWith("png") ||
                lowerPath.EndsWith("jpeg") ||
                lowerPath.EndsWith("jpg"))
            {
                return SimpleConverter.Convert(filePath, typeof(ImageSource),default!,CultureInfo.CurrentCulture) as ImageSource;
            }

            return ImageSourceHelper.GetImageSource(filePath);
        }

        public static async Task<ImageSource> ConvertCoreAsync(string filePath)
        {
            string lowerPath = filePath.ToLower();
            if (lowerPath.EndsWith("png") ||
                lowerPath.EndsWith("jpeg") ||
                lowerPath.EndsWith("jpg"))
            {
                return SimpleConverter.Convert(filePath, typeof(ImageSource),default!,CultureInfo.CurrentCulture) as ImageSource;
            }

            return await ImageSourceHelper.GetImageSourceAsync(filePath);
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
