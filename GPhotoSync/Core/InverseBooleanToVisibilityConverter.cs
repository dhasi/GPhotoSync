using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPhotoSync
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private readonly BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _converter.Convert(!(bool)value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)_converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
