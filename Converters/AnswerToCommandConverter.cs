// AnswerToCommandConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Regularnik.ViewModels;

namespace Regularnik.Converters
{
    public class AnswerToCommandConverter : IMultiValueConverter
    {
        public object Convert(object[] v, Type t, object p, CultureInfo c)
        {
            string label = (string)v[0];
            var vm = v[1] as CourseSessionViewModel;
            return label switch
            {
                "WIEM" => vm?.KnowCmd,
                "PRAWIE" => vm?.AlmostCmd,
                "NIE WIEM" => vm?.DontKnowCmd,
                _ => null
            };
        }
        public object[] ConvertBack(object v, Type[] t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }
}

