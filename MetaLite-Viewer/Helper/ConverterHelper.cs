using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace MetaLite_Viewer.Helper
{
	
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            False = trueValue;
            True = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }
    public sealed class BooleanToVisibilityInvertConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityInvertConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
    public sealed class VisibilityToBooleanConverter : BooleanConverter<Visibility>
    {
        public VisibilityToBooleanConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        {
            Console.WriteLine("kiji");
        }
    }

    public class StringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return String.IsNullOrEmpty(value as string) ? 0 : Double.Parse(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return value == null ? "0" : value.ToString();
        }
    }
    public sealed class StringToDoubleConverter : StringConverter
    {
        public StringToDoubleConverter() :
            base()
        { }
    }

    public class DoubleConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "0";
            }
            else
            {
                double doubleValue = (double)value;
                return doubleValue.ToString("#0");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return String.IsNullOrEmpty(value as string) ? 0 : Double.Parse(value as string); 
        }
    }
    public sealed class DoubleToStringConverter : DoubleConverter
    {
        public DoubleToStringConverter() :
            base()
        { }
    }


    /////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    public class MyColorConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tmp = (SolidColorBrush)value;

            return Color.FromArgb(tmp.Color.A, tmp.Color.R, tmp.Color.G, tmp.Color.B);
        }
    }

    public sealed class ColorToBrushConverter : MyColorConverter
    {
        public ColorToBrushConverter() :
            base()
        { }
    }
}
