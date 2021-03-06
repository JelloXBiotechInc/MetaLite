using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Data;
using System.Diagnostics;
using MetaLite_Viewer.Helper;

namespace MetaLite_Viewer.Subwindow
{
    public partial class AIDialogBox : Window
    {
        public AIDialogBox()
        {
            InitializeComponent();
            if (bool.Parse(ConfigurationManager.AppSettings["IS_ON_HH_MACHINE"]))
            {
                HonHaiAIModule.IsEnabled = true;
                HonHaiAIModule.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDDDDDD"));
            }
            if (Resource.ImageDatas.Count == 1)
            {
                SliderPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                RangeSelector.LowerValue = Resource.SelectedImageIndex + 1;
                RangeSelector.UpperValue = Resource.SelectedImageIndex + 1;
                RangeSelector.Maximum = Resource.ImageDatas.Count;
            }
        }

        public string SelectedModel { get; set; }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Dialog box canceled
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't accept the dialog box if there is invalid data
            if (!IsValid(this)) return;

            // Dialog box accepted
            DialogResult = true;
        }

        // Validate all dependency objects in a window
        private bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                var isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            return LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().All(IsValid);

            // All dependency objects are valid
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).GroupName == "Cancer")
            {
                SelectedModel = (sender as RadioButton).Uid;
            }
        }

        private bool ConnectAble = false;
        private long latency = -1;

        private void CheckServerAlive()
        {

            try
            {
                IPEndPoint tIPEndPoint = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["METALITE_SERVER"]), int.Parse(ConfigurationManager.AppSettings["PORT"]));
                TcpClient tClient = new TcpClient();
                tClient.Connect(tIPEndPoint);
                ConnectAble = tClient.Connected;
                latency = tClient.LingerState.LingerTime;
                tClient.Close();
                if (!ConnectAble)
                {
                    latency = -1;
                }
            }
            catch
            {
                latency = -2;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Dumd way to check duplicate selection
            if (this.IsLoaded && e.AddedItems.Count != 0)
            {
                var changedata = (e.AddedItems[0] as ComboBoxItem).Content;
                if ((string)RedComboBox.Text == (string)changedata && !sender.Equals(RedComboBox))
                {
                    RedComboBox.SelectedIndex = 0;
                }
                if ((string)GreenComboBox.Text == (string)changedata && !sender.Equals(GreenComboBox))
                {
                    GreenComboBox.SelectedIndex = 0;
                }
                if ((string)BlueComboBox.Text == (string)changedata && !sender.Equals(BlueComboBox))
                {
                    BlueComboBox.SelectedIndex = 0;
                }
                (sender as ComboBox).SelectedItem = e.AddedItems[0];
            }

        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
            waitingBlocker.SetBinding(Grid.VisibilityProperty, new Binding() { Source = Resource.MainWindow.logoGray, Path = new PropertyPath("Visibility") });
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) => {
                int span = 0;
                if (MainWindow.ProbServer() < 0)
                    MainWindow.ReloadMetaLiteServer();
            };
            bgWorker.RunWorkerCompleted += (s, e) => {
                if (Resource.MainWindow.MetaLiteServerStatus.Equals("dead"))
                {

                    DialogResult = false;
                    Close();
                    MessageBox.Show("MetaLite-Server is unavailable.\n Please reboot the whole program or restart MetaLite-Server Manually.");
                }
                // Enable here the UI
                // You can get the login-result via the e.Result. Make sure to check also the e.Error for errors that happended during the login-operation
            };
            bgWorker.RunWorkerAsync();
        }

        private void ImageTypeTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if ((sender as TabControl).SelectedIndex == 0)
            {
                if (JelloXAIModule_HBS != null)
                    JelloXAIModule_HBS.IsChecked = true;
                if (ApparentMagnification != null)
                    ApparentMagnification.SelectedIndex = 1;
            }
            else if ((sender as TabControl).SelectedIndex == 1)
            {
                if (JelloXAIModule_CamelyonHE != null)
                    JelloXAIModule_CamelyonHE.IsChecked = true;
                if (ApparentMagnification != null)
                    ApparentMagnification.SelectedIndex = 0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (InternetAvailability.IsInternetAvailable())
            {
                Process.Start(ConfigurationManager.AppSettings["CUSTOMIZE_AI_MODEL"]);
            }
            else
            {
                messagebox.Show("Your device are not connect to internet, \nplease check the internet connection.", "Disconnection error");
            }
        }
    }
}
