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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace report
{
	/// <summary>
	/// UserControl1.xaml 的互動邏輯
	/// </summary>
	public partial class PdfProducer : UserControl
	{
		public StackPanel BodyStackPanel		{
			get
			{
				return this.all;
			}
		}
		private ObservableCollection<rowItem> DataCollection = new ObservableCollection<rowItem>();
		public Visibility functionObjectVisibility;
		public PdfProducer()
		{
			InitializeComponent();

			/*DataCollection.Add(new rowItem() {title= "Specimen Type:", content="lumpectomy" });
			DataCollection.Add(new rowItem() {title= "Laterality:", content="right" });
			DataCollection.Add(new rowItem() {title= "Tumor Site:", content="not specified" });
			DataCollection.Add(new rowItem() {title= "Tumor Size:", content="1.5 x 1.2 x 0.8 cm" });
			DataCollection.Add(new rowItem() {title= "Histologic Score:", content="" });
			DataCollection.Add(new rowItem() {title= "Nottingham Histologic Score:", content="" });
			DataCollection.Add(new rowItem() {title= "Total Nottingham invasion:", content="" });
			DataCollection.Add(new rowItem() {title= "Lymphatic/vascular invasion:", content="" });
			DataCollection.Add(new rowItem() {title= "Ductal carcinoma in situ:", content="" });
			DataCollection.Add(new rowItem() {title= "Architectural type:", content="" });
			DataCollection.Add(new rowItem() {title= "Van Nuys Grade:", content="" });
			DataCollection.Add(new rowItem() {title= "Extensive intraductar:", content="" });
			DataCollection.Add(new rowItem() {title= "component:", content="" });
			DataCollection.Add(new rowItem() {title= "Micricalcifications:", content="" });
			DataCollection.Add(new rowItem() {title= "Method of margin evaluation:", content="" });
			DataCollection.Add(new rowItem() {title= "Margin Status:", content="" });
			DataCollection.Add(new rowItem() {title= "Invasive carcinoma:", content="" });
			DataCollection.Add(new rowItem() {title= "Ductal carcinoma in situ:", content="" });
			DataCollection.Add(new rowItem() {title= "Method of margin evaluation:", content="" });
			DataCollection.Add(new rowItem() {title= "Axillary lymph nodes:", content="" });
			DataCollection.Add(new rowItem() {title= "Number evaluated:", content="" });
			DataCollection.Add(new rowItem() {title= "Number positive:", content="" });
			DataCollection.Add(new rowItem() {title= "Size of largest metastasis:", content="" });
			DataCollection.Add(new rowItem() {title= "Pathologic Stage:", content="" });
			mainData.DataContext = DataCollection;
			sss.ItemsSource = DataCollection;*/
		}

		

		

		private void Button_Click(object sender, RoutedEventArgs e)
		{

			var parent = VisualTreeHelper.GetParent(sender as Button) as UIElement;
			StackPanel stackpanelShell = VisualTreeHelper.GetChild(parent, 0) as StackPanel;
			StackPanel stackpanelBody = VisualTreeHelper.GetChild(stackpanelShell, 1) as StackPanel;
			
			var col = new column1();
			var delete = new DeleteButton();
			//var colBody = VisualTreeHelper.GetChild(col, 0) as Grid;
			
			col.Children.Add(delete);
			stackpanelBody.Children.Add(col);
		}


		private void Add_SignedOut(object sender, RoutedEventArgs e)
		{
			var StackPanelParent = VisualTreeHelper.GetParent(sender as Button) as UIElement;
			var signOut = new SignOut() { };
			var delete = new DeleteButton();
			Grid.SetColumn(delete, 2);
			signOut.Children.Add(delete);
			int insertIndex = (StackPanelParent as StackPanel).Children.IndexOf(sender as UIElement);
			(StackPanelParent as StackPanel).Children.Insert(insertIndex, signOut);
		}

		private void UserControl_LayoutUpdated(object sender, EventArgs e)
		{
			
			double ColumnMaxWidth = 0;
			double targetMaxWidth = 0;
			
			foreach (TitleColumn TC in FindVisualChildren<TitleColumn>(all))
			{
				if (TC.mainGrid.Children.Count < 1) return;


				if (TC.mainGrid.Children[0].GetType() == typeof(TextBox))
				{
					targetMaxWidth = (TC.mainGrid.Children[0] as TextBox).ActualWidth;
				}
				else if (TC.mainGrid.Children[0].GetType() == typeof(TextBlock))
				{
					targetMaxWidth = (TC.mainGrid.Children[0] as TextBlock).ActualWidth;
				}
				if (ColumnMaxWidth < targetMaxWidth)
				{
					ColumnMaxWidth = targetMaxWidth;
				}
			}
			foreach (TitleColumn TC in FindVisualChildren<TitleColumn>(all))
			{
				TC.mainGrid.ColumnDefinitions[0].Width = new GridLength(ColumnMaxWidth, GridUnitType.Pixel);
			}
			
			
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

		private void addDataGrid_Click(object sender, RoutedEventArgs e)
		{

			var parent = VisualTreeHelper.GetParent(sender as Button) as UIElement;
			StackPanel stackpanelShell = VisualTreeHelper.GetChild(parent, 0) as StackPanel;
			StackPanel stackpanelBody = VisualTreeHelper.GetChild(stackpanelShell, 1) as StackPanel;

			var col = new column2();
			//var delete = new DeleteButton() { Margin = new Thickness() { Right = 20 } };
			//var colBody = VisualTreeHelper.GetChild(col, 0) as Grid;
			
			//col.Children.Add(delete);
			stackpanelBody.Children.Add(col);
			//delete.Margin = new Thickness() { Right = 20 };
			//delete.VerticalAlignment = VerticalAlignment.Bottom;
		}

		private void AddBlock_Click(object sender, RoutedEventArgs e)
		{
			//var RichTextBox = new EditRichTextBox();
			//var stack = new StackPanel() { Orientation = Orientation.Vertical};

			
			var StackPanelParent = VisualTreeHelper.GetParent(sender as Button) as UIElement;
			var RichTextBox = new EditBlock() { MinHeight = 40 };
			
			int insertIndex = (StackPanelParent as StackPanel).Children.IndexOf(sender as UIElement);
			(StackPanelParent as StackPanel).Children.Insert(insertIndex, RichTextBox);
		}

		/*private void DataGrid_Delete(object sender, RoutedEventArgs e)
		{
			var button = (sender as Button);
			var contian = (button.TemplatedParent as ContentPresenter);
			
			var datacell = (contian.Parent as DataGridCell);
			var dataGridCellsPanel = VisualTreeHelper.GetParent(datacell) as DataGridCellsPanel;
			var itemsPresenter = VisualTreeHelper.GetParent(dataGridCellsPanel) as ItemsPresenter;
			var dataGridCellsPresenter = VisualTreeHelper.GetParent(itemsPresenter) as DataGridCellsPresenter;
			var selectiveScrollingGrid = VisualTreeHelper.GetParent(dataGridCellsPresenter) as SelectiveScrollingGrid;
			var border = VisualTreeHelper.GetParent(selectiveScrollingGrid) as Border;
			var dataGridRow = VisualTreeHelper.GetParent(border) as DataGridRow;
			var dataGridRowsPresenter = VisualTreeHelper.GetParent(dataGridRow) as DataGridRowsPresenter;
			
			var itemsPresenterRow = VisualTreeHelper.GetParent(dataGridRowsPresenter) as ItemsPresenter;
			
			
			var scrollContentPresenter = VisualTreeHelper.GetParent(itemsPresenterRow) as ScrollContentPresenter;
			
			var grid = VisualTreeHelper.GetParent(scrollContentPresenter) as Grid;
			//prognosticGrid.Items.RemoveAt(dataGridRowsPresenter.Children.IndexOf(dataGridRow));
			prognosticData.RemoveAt(dataGridRowsPresenter.Children.IndexOf(dataGridRow));
			Console.WriteLine(VisualTreeHelper.GetParent(grid));
			//Console.WriteLine(datacell.TemplatedParent);
		}*/
	}
	public class rowItem
	{
		public string title { get; set; }
		public string content { get; set; }

	}
	public class EditBlock : EditRichTextBox
	{
		public EditBlock()
		{
			var sep = new Separator()
			{
				Background = Brushes.Black,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness() { Top = 0 }

			};
			sep.RenderTransform = new ScaleTransform() { ScaleY = 1 };
			var delete = new DeleteButton();
			var add = new AddTopicButton();
			//Grid.SetColumn(delete, 2);
			this.Children.Insert(this.Children.IndexOf(this.mainRTB) - 1, sep);
			this.Children.Add(delete);
			this.Children.Add(add);
		}
	}
	public class DeleteButton : Button
	{
		public DeleteButton()
		{
			this.Height = 20;
			this.Width = 20;
			this.HorizontalAlignment = HorizontalAlignment.Right;
			this.VerticalAlignment = VerticalAlignment.Top;
			this.Background = new SolidColorBrush(Color.FromArgb(22, 0, 0, 0));
			this.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x55, 0x55));
			this.Content = "";
			this.FontFamily = new FontFamily("Segoe UI Symbol");
			this.Click += new RoutedEventHandler(delete_self);
		}

		private void delete_self(object sender, RoutedEventArgs e)
		{

			var BodyGrid = VisualTreeHelper.GetParent(sender as Button) as UIElement;
			Console.WriteLine(BodyGrid);
			var ContentPresenter = VisualTreeHelper.GetParent(BodyGrid) as UIElement;
			Console.WriteLine(ContentPresenter);
			var ShowerBorder = VisualTreeHelper.GetParent(ContentPresenter) as UIElement;
			Console.WriteLine(ShowerBorder);
			var UserControl = VisualTreeHelper.GetParent(ShowerBorder) as UIElement;
			Console.WriteLine(UserControl);
			var stackPanelBody = VisualTreeHelper.GetParent(UserControl) as UIElement;
			Console.WriteLine(stackPanelBody);
			(stackPanelBody as StackPanel).Children.Remove(UserControl);

		}
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
			//this.FontFamily = new FontFamily("Segoe UI Symbol");
			this.Click += new RoutedEventHandler(add_topic);
		}

		private void add_topic(object sender, RoutedEventArgs e)
		{

			var BodyGrid = VisualTreeHelper.GetParent(sender as Button) as UIElement;
			Console.WriteLine(BodyGrid);
			var ContentPresenter = VisualTreeHelper.GetParent(BodyGrid) as UIElement;
			Console.WriteLine(ContentPresenter);
			var ShowerBorder = VisualTreeHelper.GetParent(ContentPresenter) as UIElement;
			Console.WriteLine(ShowerBorder);
			var _UserControl = VisualTreeHelper.GetParent(ShowerBorder) as UIElement;
			Console.WriteLine(_UserControl);
			var stackPanelBody = VisualTreeHelper.GetParent(_UserControl) as UIElement;
			Console.WriteLine(stackPanelBody);
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(stackPanelBody); i++)
			{
				if (VisualTreeHelper.GetChild(stackPanelBody, i) == _UserControl)
				{
					var Edit = new EditBlock() { MinHeight = 40 };
					Edit.Children.Add(new DeleteButton());
					Edit.Children.Add(new AddTopicButton());
					(stackPanelBody as StackPanel).Children.Insert(i + 1, Edit);
					return;
				}
			}

		}
	}

}
