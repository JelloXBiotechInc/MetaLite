using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace report
{
	/// <summary>
	/// DateSelector.xaml 的互動邏輯
	/// </summary>
	public partial class DateSelector : UserControl
	{
		public DateSelector()
		{
			InitializeComponent();
			var dpd = DependencyPropertyDescriptor.FromProperty(TextForegroundProperty, typeof(DateSelector));
			dpd.AddValueChanged(this, (sender, args) =>
			{
				datePicker.Foreground = new BrushConverter().ConvertFromString(this.TextForeground) as SolidColorBrush;
			});
		}
		public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register("TextForeground", typeof(string), typeof(DateSelector));

		public string TextForeground
		{
			get
			{
				return GetValue(TextForegroundProperty) as string;
			}
			set
			{
				SetValue(TextForegroundProperty, value);
			}
		}
	}
    public class DateToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            return ((DateTime)value).ToString("dd/MM/yyyy");
        }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
	
}
