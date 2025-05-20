using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Regularnik.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object param, CultureInfo c)
            => value?.ToString() == (string)param ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type t, object param, CultureInfo c)
            => throw new NotImplementedException();
    }
}