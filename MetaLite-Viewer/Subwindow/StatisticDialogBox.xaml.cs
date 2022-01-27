using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System;
using System.Text.RegularExpressions;
using System.Windows.Data;
using MahApps.Metro.Controls;

namespace MetaLite_Viewer.Subwindow
{
    public partial class StatisticDialogBox : Window
    {
        public StatisticDialogBox()
        {
            InitializeComponent();
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
            if ((sender as RadioButton).GroupName == "BreastCancer")
            {
                SelectedModel = (sender as RadioButton).Name;
            }
        }
        private void RangeCheck_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty((sender as TextBox).Text))
            {
                (sender as TextBox).Text = "0";
                return;
            }
            

            if ((sender as TextBox).Name == "RangeMin")
            {
                TextBox max = (sender as TextBox).Parent.FindChild<TextBox>("RangeMax");
                if (max != null)
                {
                    if (Int32.Parse((sender as TextBox).Text) < Int32.Parse((sender as TextBox).Uid))
                        (sender as TextBox).Text = (sender as TextBox).Uid;
                    else if (Int32.Parse((sender as TextBox).Text) > Int32.Parse(max.Uid) || Int32.Parse((sender as TextBox).Text) > Int32.Parse(max.Text))
                        (sender as TextBox).Text = Math.Min(Int32.Parse(max.Uid), Int32.Parse(max.Text)).ToString();
                }                
            }
            else if ((sender as TextBox).Name == "RangeMax")
            {
                TextBox min = (sender as TextBox).Parent.FindChild<TextBox>("RangeMin");
                if (min != null)
                {
                    if (Int32.Parse((sender as TextBox).Text) > Int32.Parse((sender as TextBox).Uid))
                        (sender as TextBox).Text = (sender as TextBox).Uid;
                    else if (Int32.Parse((sender as TextBox).Text) < Int32.Parse(min.Uid) || Int32.Parse((sender as TextBox).Text) < Int32.Parse(min.Text))
                        (sender as TextBox).Text = Math.Max(Int32.Parse(min.Uid), Int32.Parse(min.Text)).ToString();
                }                
            }
            (sender as TextBox).Text = Int32.Parse((sender as TextBox).Text).ToString(); // force remove zero before text
        }

        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            if (string.IsNullOrEmpty(e.Text))
            {
                (sender as TextBox).Text = "1";
            }
            else
            {
                e.Handled = re.IsMatch(e.Text);
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
                Console.WriteLine(RedComboBox.Text);
                Console.WriteLine(GreenComboBox.Text);
                Console.WriteLine(BlueComboBox.Text);
            }
            

        }
    }
}