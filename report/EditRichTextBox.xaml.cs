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
	/// EditRichTextBox.xaml 的互動邏輯
	/// </summary>
	public partial class EditRichTextBox : UserControl
	{
		public static DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
			nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
			typeof(UIElementCollection),
			typeof(EditRichTextBox),
			new PropertyMetadata());

		public UIElementCollection Children
		{
			get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
			private set { SetValue(ChildrenProperty, value); }
		}

		public EditRichTextBox()
		{
			InitializeComponent();
			Children = mainPanel.Children;
			mainToolBar.Visibility = Visibility.Collapsed;
			var dpd = DependencyPropertyDescriptor.FromProperty(RichTextBoxContentProperty, typeof(EditRichTextBox));
			dpd.AddValueChanged(this, (sender, args) =>
			{
				textBody.Inlines.Add(new Run(this.RichTextBoxContent));
			});
			var RichTextFontSizedpd = DependencyPropertyDescriptor.FromProperty(RichTextFontSizeProperty, typeof(EditRichTextBox));
			RichTextFontSizedpd.AddValueChanged(this, (sender, args) =>
			{
				textBody.FontSize = this.RichTextFontSize;
			});
		}

		public static readonly DependencyProperty RichTextBoxContentProperty = DependencyProperty.Register("RichTextBoxContent", typeof(string), typeof(EditRichTextBox));

		public string RichTextBoxContent
		{
			get
			{
				return GetValue(RichTextBoxContentProperty) as string;
			}
			set
			{
				SetValue(RichTextBoxContentProperty, value);
			}
		}

		public static readonly DependencyProperty RichTextFontSizeProperty = DependencyProperty.Register("RichTextFontSize", typeof(double), typeof(EditRichTextBox), new PropertyMetadata(12.0));

		public double RichTextFontSize
		{
			get
			{
				return (double)GetValue(RichTextFontSizeProperty);
			}
			set
			{
				SetValue(RichTextFontSizeProperty, value);
			}
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource == mainToolBar || e.OriginalSource == mainRTB || e.OriginalSource == textBody)
			{
				mainToolBar.Visibility = Visibility.Visible;
			}
			else
			{
				mainToolBar.Visibility = Visibility.Collapsed;
			}
		}

		private void mainPanel_GotFocus(object sender, RoutedEventArgs e)
		{
			mainToolBar.Visibility = Visibility.Visible;
		}

		private void mainPanel_LostFocus(object sender, RoutedEventArgs e)
		{
			mainToolBar.Visibility = Visibility.Collapsed;
		}

		private void mainRTB_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mainToolBar.Visibility = Visibility.Visible;
		}
	}
}
