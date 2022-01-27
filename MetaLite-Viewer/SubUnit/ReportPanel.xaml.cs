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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MetaLite_Viewer.Model;
using LiveCharts;
using LiveCharts.Wpf;
using System.Dynamic;
using System.Runtime.Remoting.Messaging;

namespace MetaLite_Viewer.SubUnit
{
	/// <summary>
	/// Window1.xaml 的互動邏輯
	/// </summary>
	public partial class ReportPanel : UserControl
	{
        public ObservableCollection<ki67> Ki67Data = new ObservableCollection<ki67>();
        
        public ReportPanel()
		{
			InitializeComponent();
            chartBody.DataTooltip.Background = (Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#55000000");
        }
        
        public string AixsXName { get; set; }
        public string AixsYName { get; set; }
        public LiveCharts.Wpf.Separator Tick { get; set; } = new LiveCharts.Wpf.Separator // force the separator step to 1, so it always display all labels
        {
                    Step = 1,
                    IsEnabled = false //disable it to make it invisible.
                };
        public void AddGirdData(int index, string fileName, double percentage, ExpressionOpt expression)
        {
            Ki67Data.Add(new ki67()
            {
                Index = index,
                FileName = fileName,
                Percentage = percentage,
                Expression = expression
            });
        }
        public void InitColumn()
        {
            dataGrid.Items.Clear();
            dataGrid.Columns.Clear();
            dataGrid.DataContext = null;
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "Index", Binding = new Binding("Index") });
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "FileName", Binding = new Binding("FileName") });

        }

        public void AddColumn(string Header)
        {
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = Header, Binding = new Binding(Header) });
        }

        public void AddGirdData(string[] gridData)
        {
            dynamic row = new ExpandoObject();
            
            for (int i = 0; i < gridData.Length; i++)
                ((IDictionary<String, Object>)row)[dataGrid.Columns[i].Header.ToString()] = gridData[i];
            
            dataGrid.Items.Add(row);
        }

        public void ClearData()
        {
            Ki67Data.Clear();
        }

        public void PlotData(string aixsXName, string aixsYName, long from, Dictionary<string, ChartValues<double>> PD)
        {         
            
            SeriesCollection = new SeriesCollection();
            SeriesCollection.Clear();
            chartBody.Series = SeriesCollection;

            int dataLen = 0;
            foreach (var element in PD)
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
                        
            YFormatter = value => value.ToString("N");
            XFormatter = val => (val + from).ToString();
            chartBody.AxisY = new AxesCollection();
            chartBody.AxisY.Add(new Axis
            {
                Title = aixsYName,
                LabelFormatter = YFormatter,
                MinValue = 0
            });
            chartBody.AxisX = new AxesCollection();
            chartBody.AxisX.Add(new Axis
                {
                    Title = aixsXName,
                    Separator = new LiveCharts.Wpf.Separator // force the separator step to 1, so it always display all labels
                    {
                        Step = 5,
                        IsEnabled = false //disable it to make it invisible.
                    },
                    LabelFormatter = XFormatter,
                    MinValue = 0
                });
            
            DataContext = this;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> XFormatter { get; set; }

        private void chartBody_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void chartBody_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double result = 0;
            if (value is double)
                result = (double)value;
            return result.ToString("N2")+"%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
