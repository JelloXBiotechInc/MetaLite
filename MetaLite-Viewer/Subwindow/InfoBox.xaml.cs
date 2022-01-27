using MetaLite_Viewer.Helper;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// MessageBox.xaml 的互動邏輯
	/// </summary>
	public partial class InfoBox : Window
	{
		public InfoBox(
			string Content = "Message",
			string Title = "MessageBox", 			
			MessageBoxButton messageBoxButton = MessageBoxButton.OK, 
			MessageBoxDefaultButton messageBoxDefaultButton = MessageBoxDefaultButton.Button1,
			MessageBoxImage messageBoxImage = MessageBoxImage.Information,
			string button0Text = "",
			string button1Text = ""
			)
		{
			InitializeComponent();
			this.Title = Title;
			var Icon = new System.Windows.Controls.TextBlock()
			{
				FontFamily = new FontFamily("Segoe MDL2 Assets"),
				FontSize = 48,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness() { Left = 20 }
			};
			TextContent.Children.Add(Icon);
			switch (messageBoxImage)
			{
				case MessageBoxImage.Question:
					Icon.Text = "";
					Icon.Foreground = ColorHelper.XamlStringColor("#FFF3BEFF");
					break;
				case MessageBoxImage.Information:
					Icon.Text = "";
					Icon.Foreground = ColorHelper.XamlStringColor("#FF9AD1FF");
					break;
				case MessageBoxImage.Error:
					Icon.Text = "";
					Icon.Foreground = ColorHelper.XamlStringColor("#FFFF7373");
					break;
				case MessageBoxImage.Warning:
					Icon.Text = "";
					Icon.Foreground = ColorHelper.XamlStringColor("#FFFFDB38");
					break;
				default:
					Icon.Text = "";
					break;
			}
			TextContent.Children.Add(new System.Windows.Controls.TextBlock() { 
				Text = Content, 
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness() { Right = 20}
			});
			var padding = new Thickness() { Left = 30, Top = 5, Right = 30, Bottom = 5 };
			var margin = new Thickness() { Left = 0, Top = 10, Right = 15, Bottom = 10 };
			
			System.Windows.Controls.Button OK = new System.Windows.Controls.Button();
			OK.Padding = padding;
			OK.Margin = margin;
			OK.Click += OK_ButtonClick;
			OK.Content = "OK";
			System.Windows.Controls.Button Yes = new System.Windows.Controls.Button();
			Yes.Padding = padding;
			Yes.Margin = margin;
			Yes.Click += Yes_ButtonClick;
			if (String.IsNullOrWhiteSpace(button0Text))
				Yes.Content = "Yes";
			else
				Yes.Content = button0Text;
			System.Windows.Controls.Button No = new System.Windows.Controls.Button();
			No.Padding = padding;
			No.Margin = margin;
			No.Click += No_ButtonClick;
			if (String.IsNullOrWhiteSpace(button1Text))
				No.Content = "No";
			else
				No.Content = button1Text;
			System.Windows.Controls.Button Cancel = new System.Windows.Controls.Button();
			Cancel.Padding = padding;
			Cancel.Margin = margin;
			Cancel.Click += Cancel_ButtonClick;
			Cancel.Content = "Cancel";

			switch (messageBoxButton)
			{
				case MessageBoxButton.OK:
					ButtonPanel.Children.Add(OK);
					break;
				case MessageBoxButton.OKCancel:
					ButtonPanel.Children.Add(OK);
					ButtonPanel.Children.Add(Cancel);
					break;
				case MessageBoxButton.YesNo:
					ButtonPanel.Children.Add(Yes);
					ButtonPanel.Children.Add(No);
					break;
				case MessageBoxButton.YesNoCancel:
					ButtonPanel.Children.Add(Yes);
					ButtonPanel.Children.Add(No);
					ButtonPanel.Children.Add(Cancel);
					break;
				default:
					break;
			}
			
		}

		public MessageBoxResult InfoBoxResult = MessageBoxResult.OK;

		private void OK_ButtonClick(object sender, RoutedEventArgs e)
		{
			InfoBoxResult = MessageBoxResult.OK;
			Close();
		}

		private void Yes_ButtonClick(object sender, RoutedEventArgs e)
		{
			InfoBoxResult = MessageBoxResult.Yes;
			Close();
		}

		private void No_ButtonClick(object sender, RoutedEventArgs e)
		{
			InfoBoxResult = MessageBoxResult.No;
			Close();
		}

		private void Cancel_ButtonClick(object sender, RoutedEventArgs e)
		{
			InfoBoxResult = MessageBoxResult.Cancel;
			Close();
		}
	}

	public static class messagebox
	{
		public static MessageBoxResult Show(string Content = "Message")
		{
			// using construct ensures the resources are freed when form is closed
			var form = new InfoBox(Content);
			
			form.ShowDialog();
			form.Close();
			return form.InfoBoxResult;
		}
		public static MessageBoxResult Show(
			string Content = "Message",
			string Title = "MessageBox"
			)
		{
			// using construct ensures the resources are freed when form is closed
			var form = new InfoBox(Content, Title);

			form.ShowDialog();
			form.Close();
			return form.InfoBoxResult;
		}
		public static MessageBoxResult Show(
			string Content = "Message",
			string Title = "MessageBox",
			MessageBoxButton messageBoxButton = MessageBoxButton.OK
			)
		{
			// using construct ensures the resources are freed when form is closed
			var form = new InfoBox(Content, Title, messageBoxButton);

			form.ShowDialog();
			form.Close();
			return form.InfoBoxResult;
		}
		public static MessageBoxResult Show(
			string Content = "Message",
			string Title = "MessageBox",
			MessageBoxButton messageBoxButton = MessageBoxButton.OK,
			MessageBoxDefaultButton messageBoxDefaultButton = MessageBoxDefaultButton.Button1
			)
		{
			// using construct ensures the resources are freed when form is closed
			var form = new InfoBox(Content, Title, messageBoxButton, messageBoxDefaultButton);

			form.ShowDialog();
			form.Close();
			return form.InfoBoxResult;
		}
		public static MessageBoxResult Show(
			string Content = "Message",
			string Title = "MessageBox",			
			MessageBoxButton messageBoxButton = MessageBoxButton.OK,
			MessageBoxImage messageBoxImage = MessageBoxImage.Information
			)
		{
			// using construct ensures the resources are freed when form is closed
			var form = new InfoBox(Content, Title, messageBoxButton, MessageBoxDefaultButton.Button1, messageBoxImage);

			form.ShowDialog();
			form.Close();
			return form.InfoBoxResult;
		}

		public static MessageBoxResult Show(
			string Content = "Message",
			string Title = "MessageBox",
			MessageBoxButton messageBoxButton = MessageBoxButton.OK,
			MessageBoxImage messageBoxImage = MessageBoxImage.Information,
			string YesBoxText = "Yes",
			string NoBoxText = "No"
			)
		{
			// using construct ensures the resources are freed when form is closed
			var form = new InfoBox(Content, Title, messageBoxButton, MessageBoxDefaultButton.Button1, messageBoxImage, YesBoxText, NoBoxText);

			form.ShowDialog();
			form.Close();
			return form.InfoBoxResult;
		}
	}
}
