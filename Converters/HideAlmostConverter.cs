using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Regularnik.ViewModels;

namespace Regularnik.Converters
{
    /// <summary>
    /// Ukrywa przycisk „PRAWIE”, gdy aktywna kategoria to Reinforce.
    /// – z MultiBinding odbiera:  values[0] = ActiveCat, values[1] = Content („PRAWIE”)
    /// – z pojedynczego Bindingu odbiera tylko ActiveCat i parametr „PRAWIE”
    /// </summary>
    public class HideAlmostConverter : IMultiValueConverter, IValueConverter
    {
        /* ===== MultiValueConverter ===== */
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Visible;
            var cat = ExtractCategory(values[0]);
            var label = values[1] as string;
            return ShouldHide(cat, label) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        /* ===== ValueConverter (single binding) ===== */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cat = ExtractCategory(value);
            var label = parameter?.ToString();
            return ShouldHide(cat, label) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        /* ===== helpers ===== */
        private static Category ExtractCategory(object v)
            => v is Category c ? c : Category.New;

        private static bool ShouldHide(Category cat, string label)
            => cat == Category.Reinforce && label?.Contains("PRAWIE") == true;
    }
}
