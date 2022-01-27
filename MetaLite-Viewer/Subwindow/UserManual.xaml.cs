using System;
using System.Windows;
using System.Configuration;
using System.IO;
using MetaLite_Viewer.Helper;
namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// UserManual.xaml 的互動邏輯
	/// </summary>
	public partial class UserManual : Window
	{
		public UserManual()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (InternetAvailability.IsInternetAvailable())
			{
				offline.Visibility = Visibility.Collapsed;
				body.Navigate(ConfigurationManager.AppSettings["USER_MANUAL"]);
			}
			else
			{
				body.Visibility = Visibility.Collapsed;
				string curDir = Directory.GetCurrentDirectory();
				offline.Navigate(new Uri(String.Format("file:///{0}/MetaLite User’s Guide.html", curDir)));
			}			
		}
	}
}
