using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace MetaLite_Viewer.Interpretor
{
	[ValueConversion(typeof(object), typeof(String))]
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formatString = string.IsNullOrEmpty(FormatString) ? "{0}" : FormatString;
            return string.Format(culture, FormatString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public string FormatString { get; set; }
    }
    
}
