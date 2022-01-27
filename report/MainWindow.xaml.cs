using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace report
{
	/// <summary>
	/// MainWindow.xaml 的互動邏輯
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var ss  = this.FindResource("ttttt");
			
			var toprint = (sss.FindName("all") as StackPanel);
			
			foreach(Button b in FindVisualChildren<Button>(toprint))
			{
				b.Visibility = Visibility.Hidden;
			}

			foreach (ToggleButton b in FindVisualChildren<ToggleButton>(toprint))
				b.Visibility = Visibility.Hidden;

			foreach (Line b in FindVisualChildren<Line>(toprint))
				b.Visibility = Visibility.Hidden;


			pdfwriter.ExportReportAsPdf(toprint, null, new Thickness(40), ReportOrientation.Vertical, null, null, ss as DataTemplate, true, (this.FindResource("ReportFooterDataTemplate") as DataTemplate));

			foreach (Button b in FindVisualChildren<Button>(toprint))
			{
				b.Visibility = Visibility.Visible;
			}

			foreach (ToggleButton b in FindVisualChildren<ToggleButton>(toprint))
				b.Visibility = Visibility.Visible;

			foreach (Line b in FindVisualChildren<Line>(toprint))
				b.Visibility = Visibility.Visible;
		}

		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}
	}
	
}
