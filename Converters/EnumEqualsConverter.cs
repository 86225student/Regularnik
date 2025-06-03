using System;
using System.Globalization;
using System.Windows.Data;

namespace Regularnik.Converters
{
    /// <summary>
    /// Zwraca true, gdy value (enum) == parameter (enum w postaci string).
    /// Używane do zaznaczania aktywnego kafelka.
    /// </summary>
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            // parameter przychodzi ze XAML-a jako string z nazwą wartości enuma
            var wanted = Enum.Parse(value.GetType(), parameter.ToString());
            return wanted.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;   // jednostronne bindowanie
    }
}
