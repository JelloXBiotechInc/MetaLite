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
	/// SignOut.xaml 的互動邏輯
	/// </summary>
	public partial class SignOut : UserControl
	{
		public static DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
			nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
			typeof(UIElementCollection),
			typeof(SignOut),
			new PropertyMetadata());

		public UIElementCollection Children
		{
			get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
			private set { SetValue(ChildrenProperty, value); }
		}

		public SignOut()
		{
			
			InitializeComponent();
			Children = Body.Children;
			Body.Children.Add(new AddItem());

		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SignOut));

		public string Title
		{
			get
			{
				return GetValue(TitleProperty) as string;
			}
			set
			{
				SetValue(TitleProperty, value);
			}
		}

		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(SignOut));

		public string Description
		{
			get
			{
				return GetValue(DescriptionProperty) as string;
			}
			set
			{
				SetValue(DescriptionProperty, value);
			}
		}

		public class AddItem : Button
		{
			public AddItem()
			{
				Grid.SetColumn(this, 2);
				Grid.SetRow(this, 2);

				this.Height = 20;
				this.Width = 20;
				this.HorizontalAlignment = HorizontalAlignment.Right;
				this.VerticalAlignment = VerticalAlignment.Bottom;

				this.Background = new SolidColorBrush(Color.FromArgb(22, 0, 0, 0));
				this.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0));
				this.Content = "+";
				this.VerticalContentAlignment = VerticalAlignment.Center;

				this.Click += new RoutedEventHandler(add_SignedOut);
			}
			private void add_SignedOut(object sender, RoutedEventArgs e)
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
						var signOut = new SignOut();
						var delete = new DeleteButton();
						Grid.SetColumn(delete, 2);
						signOut.Children.Add(delete);
						signOut.Children.Add(new AddItem());
						(stackPanelBody as StackPanel).Children.Insert(i + 1, signOut);
						return;
					}
				}

			}
		}
	}
}
