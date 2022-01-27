using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Configuration;
using System.Windows.Media.Animation;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;
using System.Windows.Data;
using MetaLite_Viewer.Helper;
using System.Windows.Controls.Primitives;

namespace MetaLite_Viewer.Subwindow
{
    
    
    public partial class PluginDialogBox : Window
    {
        public FunctionData functionData;
        public string ServerAddress { get; set; }
        public Dictionary<string, Column> outputColumns;
        public string FunctionName { get; set; }
        public Dictionary<string, List<object>> returnValue { get; set; } = new Dictionary<string, List<object>>();
        public PluginDialogBox(string PluginFunctionName)
        {
            
            InitializeComponent();
            using (StreamReader r = new StreamReader(@"Plugin/" + PluginFunctionName))
            {
                string json = r.ReadToEnd();
                functionData = JsonConvert.DeserializeObject<FunctionData>(json);
            }
            functionData.PostUri = "http://" +
                ConfigurationManager.AppSettings["METALITE_SERVER"] + ":" +
                ConfigurationManager.AppSettings["PORT"] +
                functionData.PostUri;
            functionData.FunctionName = PluginFunctionName;
            this.Title = Path.GetFileNameWithoutExtension(PluginFunctionName);
            foreach (var col in functionData.Content)
            {
                if (col.parameterType == nameof(Boolean))
                {
                    Label title = new Label() { Content = col.title };


                    CheckBox Box = new CheckBox()
                    {
                        IsChecked = col.boolean,
                        Margin = new Thickness(-7,6,0,0)
                    };
                    Box.SetBinding(CheckBox.IsCheckedProperty, new Binding()
                    {
                        Source = col,
                        Path = new PropertyPath("boolean"),                        
                        Mode = BindingMode.TwoWay
                    });
                    StackPanel outlayer = new StackPanel() { Orientation = Orientation.Horizontal };
                    outlayer.Children.Add(title);
                    outlayer.Children.Add(Box);
                    contentBody.Children.Add(outlayer);
                }
                else if (col.parameterType == nameof(String))
                {
                    Label title = new Label() { Content = col.title };
                    contentBody.Children.Add(title);
                }
                else if (col.parameterType == "RangeInt32")
                {
                    Label title = new Label() { Content = col.title };
                    contentBody.Children.Add(title);

                    Grid rangeBody = new Grid();
                    rangeBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    rangeBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    rangeBody.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                    TextBox from = new TextBox() { Text = "0", Name = "RangeMin", Uid = "0" };
                    Grid.SetColumn(from, 0);
                    from.PreviewTextInput += Number_PreviewTextInput;
                    from.TextChanged += RangeCheck_TextChanged;

                    from.SetBinding(TextBox.TextProperty, new Binding()
                    {
                        Source = col.range,
                        Path = new PropertyPath("From"),
                        Mode = BindingMode.TwoWay
                    });
                    TextBlock dash = new TextBlock()
                    {
                        Text = "-",
                        Foreground = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 0xFF, G = 0xDD, R = 0xDD, B = 0xDD })
                    };
                    Grid.SetColumn(dash, 1);

                    TextBox to = new TextBox() { Text = "23170", Name = "RangeMax", Uid = "23170" };
                    Grid.SetColumn(to, 2);
                    to.PreviewTextInput += Number_PreviewTextInput;
                    to.TextChanged += RangeCheck_TextChanged;
                    to.SetBinding(TextBox.TextProperty, new Binding()
                    {
                        Source = col.range,
                        Path = new PropertyPath("To"),
                        Mode = BindingMode.TwoWay
                    });

                    rangeBody.Children.Add(from);
                    rangeBody.Children.Add(dash);
                    rangeBody.Children.Add(to);

                    if (col.range != null)
                    {
                        if (col.range.From != null)
                            from.Text = col.range.From.ToString();
                        if (col.range.To != null)
                            to.Text = col.range.To.ToString();
                        if (col.range.Min != null)
                            from.Uid = col.range.Min.ToString();
                        if (col.range.Max != null)
                            to.Uid = col.range.Max.ToString();
                    }
                    
                    title.Content += " (" + from.Uid + "-" + to.Uid + ")";
                    if (col.auto != null)
                    {
                        CheckBox auto = new CheckBox()
                        {
                            Content = "Auto",
                            Foreground = ColorHelper.XamlStringColor("#FFDDDDDD"),
                            IsChecked = col.auto,
                            Name = col.parameterName + "CheckBox",
                            Margin = new Thickness() { Bottom = 5 }
                            
                        };

                        auto.SetBinding(CheckBox.IsCheckedProperty, new Binding()
                        {
                            Source = col,
                            Path = new PropertyPath("auto"),
                            Mode = BindingMode.TwoWay
                        });
                        contentBody.Children.Add(auto);
                        rangeBody.SetBinding(Slider.VisibilityProperty, new Binding()
                        {
                            Source = col,
                            Path = new PropertyPath("auto"),
                            Converter = new BooleanToVisibilityInvertConverter(),
                            Mode = BindingMode.TwoWay
                        });
                        rangeBody.SetBinding(Slider.ValueProperty, new Binding()
                        {
                            Source = col.range,
                            Path = new PropertyPath("From"),
                            Converter = new BooleanToVisibilityInvertConverter(),
                            Mode = BindingMode.TwoWay
                        });

                    }
                    contentBody.Children.Add(rangeBody);
                }
                else if (col.parameterType == nameof(ComboBox))
                {
                    Label title = new Label() { Content = col.title };
                    contentBody.Children.Add(title);

                    var selection = col.items;
                    
                    StackPanel ComboBoxBody = new StackPanel() { Orientation = Orientation.Vertical };
                    
                    for (int i = 0; i < col.datas.Count; i++)
                    {
                        if (col.datas.Count > 1)
                        {
                            TextBlock ItemTitle = new TextBlock()
                            {
                                Text = col.datas[i].varName,
                                Padding = new Thickness() { Bottom = 5 },
                                Foreground = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 0xFF, G = 0xDD, R = 0xDD, B = 0xDD })
                            };
                            contentBody.Children.Add(ItemTitle);
                        }                        
                        
                        ComboBox comboBox = new ComboBox() { Name = col.datas[i].varName, SelectedValue = col.datas[i].value };

                        comboBox.ItemsSource = selection;
                        comboBox.SetBinding(ComboBox.SelectedValueProperty, new Binding()
                        {
                            Source = col.datas[i],
                            Path = new PropertyPath("value"),
                            Mode = BindingMode.TwoWay
                        });
                        contentBody.Children.Add(comboBox);
                    }
                }
                else if (col.parameterType == nameof(Slider) + "Int32")
                {
                    Label title = new Label() { Content = col.title };
                    contentBody.Children.Add(title);
                    if (col.auto != null)
                    {
                        CheckBox auto = new CheckBox() { Content = "Auto", 
                            Foreground = ColorHelper.XamlStringColor("#FFDDDDDD"),
                            IsChecked = col.auto,
                            Name = col.parameterName + "CheckBox",
                            Margin = new Thickness() { Bottom = 5 }
                        };
                        
                        auto.SetBinding(CheckBox.IsCheckedProperty, new Binding()
                        {
                            Source = col,
                            Path = new PropertyPath("auto"),
                            Mode = BindingMode.TwoWay
                        });
                        contentBody.Children.Add(auto);
                        Slider value = new Slider() { 
                            Minimum = col.range.Min, 
                            Maximum = col.range.Max, 
                            Value = col.range.From,
                            AutoToolTipPlacement =  AutoToolTipPlacement.TopLeft,
                        };
                        value.SetBinding(Slider.VisibilityProperty, new Binding() { 
                            Source = col,
                            Path = new PropertyPath("auto"),
                            Converter = new BooleanToVisibilityInvertConverter(),
                            Mode = BindingMode.TwoWay
                        });
                        value.SetBinding(Slider.ValueProperty, new Binding()
                        {
                            Source = col.range,                            
                            Path = new PropertyPath("From"),
                            Mode = BindingMode.TwoWay
                        });

                        contentBody.Children.Add(value);                        
                    }
                }

                contentBody.Children.Add(new Separator());
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
            (sender as TextBox).Text = Int32.Parse((sender as TextBox).Text).ToString();
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
                
            }

        }
    }
}