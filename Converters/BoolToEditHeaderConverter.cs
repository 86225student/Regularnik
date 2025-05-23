using System;
using System.Globalization;
using System.Windows.Data;

namespace Regularnik.Converters
{
    public class BoolToEditHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEdit = value is bool b && b;
            return isEdit ? "Edytuj kurs" : "Dodaj nowy kurs";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
