using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using report;

namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// ReportPreview.xaml 的互動邏輯
	/// </summary>
	public partial class ReportPreview : Window
	{
        public DataGrid DataGrid;
		public ReportPreview(DataGrid data, string aixsXName, string aixsYName, long from, Dictionary<string, ChartValues<double>> PD)
		{
			InitializeComponent();
            
            Resource.MainWindow.rightPanel.SelectedIndex = 1;

            DataGrid = data;

        }

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> XFormatter { get; set; }
        private double[] MaxWidths = { 0, 0, 0, 0 };
        public void PlotData(DataGrid data)
        {
            var _ExportStackPanel = ExportStackPanel.BodyStackPanel;
            var Header = new Grid();
            Header.HorizontalAlignment = HorizontalAlignment.Left;
            
            
            var border = new Thickness()
            {
                Bottom = 3,
                Top = 3,
                Right = 3,
                Left = 3
            };
            
            Header.RowDefinitions.Add(new RowDefinition());
            Header.RowDefinitions.Add(new RowDefinition());
            
            for (int i = 0; i < ((IDictionary<String, Object>)data.Items[0]).Keys.Count; i ++)            
                Header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 100});

            var title = new TextBlock() {
                Text = "Expression: ki67",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeight.FromOpenTypeWeight(999)
            };
            Grid.SetRow(title, 0);
            Grid.SetColumnSpan(title, 4);
            Header.Children.Add(title);
            
            foreach (var row in data.Items)
            {
                int col_iter = 0;
                foreach (var key in ((IDictionary<String, Object>)row).Keys)
                {
                    var columnValue = new TextBlock() { Text = key, VerticalAlignment = VerticalAlignment.Center, Margin = border };
                    Grid.SetColumn(columnValue, col_iter);
                    Grid.SetRow(columnValue, 1);
                    Header.Children.Add(columnValue);
                    col_iter++;
                }
                
            }

            _ExportStackPanel.Children.Add(new System.Windows.Controls.Separator());
            _ExportStackPanel.Children.Add(Header);

            foreach (var item in data.Items)
            {
                
                var row = new Grid();
                row.HorizontalAlignment = HorizontalAlignment.Left;
                row.Uid = "StatisticColumn";
                border = new Thickness()
                {
                    Bottom = 3,
                    Top = 3,
                    Right = 3,
                    Left = 3
                };
                int col_iter = 0;
                foreach (var key in ((IDictionary<String, Object>)item).Keys)
                {
                    row.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 100 });
                    var columnValue = new TextBlock() { Text = ((IDictionary<String, Object>)item)[key].ToString(), VerticalAlignment = VerticalAlignment.Center, Margin = border };
                    Grid.SetColumn(columnValue, col_iter);
                    columnValue.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    row.Children.Add(columnValue);
                    col_iter++;
                }
                _ExportStackPanel.Children.Add(row);
            }

            CartesianChart Chart = new CartesianChart() {
                Zoom = ZoomingOptions.X,
                LegendLocation = LegendLocation.Top,
                Name = "chartBody",
                
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Left };

            SeriesCollection = new SeriesCollection();
            SeriesCollection.Clear();
            Chart.Series = SeriesCollection;

            int dataLen = 0;
            foreach (var element in Resource.MainWindow.StatisticDataDictionary)
            {
                SeriesCollection.Add(
                    new LineSeries
                    {
                        Title = element.Key,
                        Values = element.Value
                    }
                );
                if (element.Value.Count > dataLen)
                    dataLen = element.Value.Count;
            }

            for (int i = 0; i < Resource.MainWindow.statisticPanel.chartBody.AxisX.Count; i++)
            {
                Chart.AxisY = new AxesCollection();
                Chart.AxisY.Add(new Axis
                {
                    Title = Resource.MainWindow.statisticPanel.chartBody.AxisY[i].Title,
                    LabelFormatter = Resource.MainWindow.statisticPanel.YFormatter,
                    MinValue = 0
                });
                Chart.AxisX = new AxesCollection();
                Chart.AxisX.Add(new Axis
                {
                    Title = Resource.MainWindow.statisticPanel.chartBody.AxisX[i].Title,
                    Separator = new LiveCharts.Wpf.Separator // force the separator step to 1, so it always display all labels
                    {
                        Step = 5,
                        IsEnabled = false //disable it to make it invisible.
                    },
                    LabelFormatter = Resource.MainWindow.statisticPanel.XFormatter,
                    MinValue = 0
                });
            }            

            Chart.SetBinding(
                CartesianChart.HeightProperty, new Binding()
                {
                    Source = Resource.MainWindow.statisticPanel.statisticsDataPlot,
                    Path = new PropertyPath("ActualHeight"),
                    Mode = BindingMode.OneWay
                });
            Chart.SetBinding(
                CartesianChart.WidthProperty, new Binding()
                {
                    Source = Resource.MainWindow.statisticPanel.statisticsDataPlot,
                    Path = new PropertyPath("ActualWidth"),
                    Mode = BindingMode.OneWay
                });

            _ExportStackPanel.Children.Add(Chart);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            foreach (UIElement UI in ExportStackPanel.BodyStackPanel.Children)
            {
                if (UI.GetType() == typeof(Grid) && UI.Uid == "StatisticColumn")
                {
                    for (int i = 0; i <4; i++)
                    {
                        if (((UI as Grid).Children[i] as TextBlock).ActualWidth+ 
                            (((UI as Grid).Children[i] as TextBlock).Margin.Right + ((UI as Grid).Children[i] as TextBlock).Margin.Left )> MaxWidths[i])
                            MaxWidths[i] = ((UI as Grid).Children[i] as TextBlock).ActualWidth +
                            (((UI as Grid).Children[i] as TextBlock).Margin.Right + ((UI as Grid).Children[i] as TextBlock).Margin.Left);
                    }
                }
            }
            foreach (UIElement UI in ExportStackPanel.BodyStackPanel.Children)
            {
                if (UI.GetType() == typeof(Grid) && UI.Uid == "StatisticColumn")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        (UI as Grid).ColumnDefinitions[i].Width = new GridLength(MaxWidths[i], GridUnitType.Pixel);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reportHeader = this.FindResource("reportHeader");

            foreach (Button b in FindVisualChildren<Button>(ExportStackPanel))
            {
                b.Visibility = Visibility.Hidden;
            }

            foreach (ToggleButton b in FindVisualChildren<ToggleButton>(ExportStackPanel))
                b.Visibility = Visibility.Hidden;

            foreach (Line b in FindVisualChildren<Line>(ExportStackPanel))
                b.Visibility = Visibility.Hidden;


            pdfwriter.ExportReportAsPdf(ExportStackPanel.BodyStackPanel, null, new Thickness(40), ReportOrientation.Vertical, null, null, reportHeader as DataTemplate, true, (this.FindResource("ReportFooterDataTemplate") as DataTemplate));

            foreach (Button b in FindVisualChildren<Button>(ExportStackPanel))
            {
                b.Visibility = Visibility.Visible;
            }

            foreach (ToggleButton b in FindVisualChildren<ToggleButton>(ExportStackPanel))
                b.Visibility = Visibility.Visible;

            foreach (Line b in FindVisualChildren<Line>(ExportStackPanel))
                b.Visibility = Visibility.Visible;
        }

        private void AttachStatistic_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.Items.Count == 0) return;
            PlotData(DataGrid);
            (sender as Button).Visibility = Visibility.Collapsed;
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
