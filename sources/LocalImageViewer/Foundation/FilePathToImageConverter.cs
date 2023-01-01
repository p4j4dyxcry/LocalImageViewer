using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
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
                    return ImageSourceHelper.GetImageSource(filePath);
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

    public class FilePathToImageExtension: MarkupExtension
    {
        private static FilePathToImageConverter? _converter;
        public override object ProvideValue( IServiceProvider serviceProvider )
        {
            return _converter ??= new FilePathToImageConverter();
        }
    }
}
