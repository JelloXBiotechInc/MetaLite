using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;


namespace report
{
	/// <summary>
	/// TitleColumn.xaml 的互動邏輯
	/// </summary>
	public partial class TitleColumn : UserControl
	{
		public TitleColumn()
		{
			InitializeComponent();
			var Titledpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof(TitleColumn));
			string titleToShow = "";
			Titledpd.AddValueChanged(this, (sender, args) =>
			{
				titleToShow = this.Title;
			});

			

		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(TitleColumn));

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

		public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(Enum), typeof(TitleColumn));

		public Enum ItemSource
		{
			get
			{
				return GetValue(ItemSourceProperty) as Enum;
			}
			set
			{
				SetValue(ItemSourceProperty, value);
			}
		}

		public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register("DataType", typeof(Type), typeof(TitleColumn));

		public Type DataType
		{
			get
			{
				if (GetValue(DataTypeProperty) as Type == typeof(string))
					return typeof(string);
				else if (GetValue(DataTypeProperty) as Type == typeof(DateTime))
					return typeof(DateTime);
				else if (GetValue(DataTypeProperty) as Type == typeof(SexOpt))
					return typeof(SexOpt);
				return typeof(string);
			}
			set
			{
				SetValue(DataTypeProperty, value);
			}
		}

		public static readonly DependencyProperty IsTitleEditableProperty = DependencyProperty.Register("IsTitleEditable", typeof(bool), typeof(TitleColumn), new PropertyMetadata(false));

		public bool IsTitleEditable
		{
			get
			{
				return (bool)GetValue(IsTitleEditableProperty); 
			}
			set
			{
				SetValue(IsTitleEditableProperty, value);
			}
		}

		public static readonly DependencyProperty IndentLevelProperty = DependencyProperty.Register("IndentLevel", typeof(int), typeof(TitleColumn), new PropertyMetadata(0));

		public int IndentLevel
		{
			get
			{
				return (int)GetValue(IndentLevelProperty);
			}
			set
			{
				SetValue(IndentLevelProperty, value);
			}
		}

		private void mainGrid_Loaded(object sender, RoutedEventArgs e)
		{
			UIElement titleElement;
			UIElement dataElement = new TextBox() { };
			string Text = Title + ":";
			for (int i = 0; i < IndentLevel; i++)
				Text = "      " + Text;
			if (IsTitleEditable)
			{
				titleElement = new TextBox() { Text = Text, 
					Margin = new Thickness() { Left = -3 },
					TextWrapping = TextWrapping.Wrap,
					Name = "titleBlock",
					VerticalAlignment = VerticalAlignment.Center
				};
			}
			else
			{
				titleElement = new TextBlock() { Text = Text,
					Name = "titleBlock",
					VerticalAlignment = VerticalAlignment.Center
					

				};
			}
			Grid.SetColumn(titleElement, 0);

			mainGrid.Children.Add(titleElement);

			if (DataType == typeof(string))
			{
				dataElement = new TextBox() {
					Name = "dataBlock",
					VerticalAlignment = VerticalAlignment.Center,
					TextWrapping = TextWrapping.Wrap,
					BorderBrush = new SolidColorBrush(Colors.Transparent),
					
					Margin = new Thickness() { Right = 6}
				};
			}
			else if(DataType == typeof(DateTime))
			{
				dataElement = new DateSelector() {
					Name = "dataBlock",
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Right,
					Margin = new Thickness() { Right = 6}
				};
			}
			else if (DataType == typeof(SexOpt))
			{
				dataElement = new ComboBox()
				{
					Name = "dataBlock",
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Right,
					Margin = new Thickness() { Right = 6 }
				};
				(dataElement as ComboBox).ItemsSource = Enum.GetValues(typeof(SexOpt)).Cast<SexOpt>();
			}
			Grid.SetColumn(dataElement, 1);
			mainGrid.Children.Add(dataElement);
		}
	}
	public enum SexOpt { F, M };
}
