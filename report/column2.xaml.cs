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
using System.Windows.Markup;

namespace report
{
	/// <summary>
	/// UserControl1.xaml 的互動邏輯
	/// </summary>
	
	public partial class column2 : UserControl
	{

		public static DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
			nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
			typeof(UIElementCollection),
			typeof(column2),
			new PropertyMetadata());

		public column2()
		{
			InitializeComponent();
			
			Children = Body.Children;
			Body.Children.Add(new AddTopicButton());
			
		}

		public UIElementCollection Children
		{
			get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
			private set { SetValue(ChildrenProperty, value); }
		}

		public class AddTopicButton : Button
		{
			public AddTopicButton()
			{
				this.Height = 20;
				this.Width = 20;
				this.HorizontalAlignment = HorizontalAlignment.Right;
				this.VerticalAlignment = VerticalAlignment.Bottom;
				
				this.Background = new SolidColorBrush(Color.FromArgb(22, 0, 0, 0));
				this.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0));
				this.Content = "+";
				this.VerticalContentAlignment = VerticalAlignment.Center;
				this.Click += new RoutedEventHandler(add_topic);
			}

			private void add_topic(object sender, RoutedEventArgs e)
			{

				var BodyGrid = VisualTreeHelper.GetParent(sender as Button) as UIElement;
				var ContentPresenter = VisualTreeHelper.GetParent(BodyGrid) as UIElement;
				var ShowerBorder = VisualTreeHelper.GetParent(ContentPresenter) as UIElement;
				var _UserControl = VisualTreeHelper.GetParent(ShowerBorder) as UIElement;
				var stackPanelBody = VisualTreeHelper.GetParent(_UserControl) as UIElement;
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(stackPanelBody); i++)
				{
					if (VisualTreeHelper.GetChild(stackPanelBody, i) == _UserControl)
					{
						var col = new column2();
						col.Children.Add(new DeleteButton() { Margin = new Thickness() { Right = 20}, VerticalAlignment = VerticalAlignment.Bottom } );
						col.Children.Add(new AddTopicButton());
						(stackPanelBody as StackPanel).Children.Insert(i + 1, col);
						return;
					}
				}

			}
		}
	}
}
