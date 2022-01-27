using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetaLite_Viewer.Helper;
using System.Diagnostics;
using wpfpslib;
using ColorPickerWPF;

namespace MetaLite_Viewer.SubUnit
{
	/// <summary>
	/// UserControl1.xaml 的互動邏輯
	/// </summary>
	public partial class ChannelPanel : UserControl
	{
		private bool _isDown;
		private bool _isDragging;
		private UIElement _originalElement;
		private UIElement touchedElement;
		private int indexToShow;
		private double _originalLeft;
		private double _originalTop;
		private SimpleCircleAdorner _overlayElement;
		private Point _startPoint;
		
		public int RenderMode
		{
			get { return (int)GetValue(RenderModeProperty); }
			set { SetValue(RenderModeProperty, value); }
		}

		public static DependencyProperty RenderModeProperty =
		   DependencyProperty.Register("RenderMode", typeof(int), typeof(ChannelPanel));
		
		public ChannelPanel()
		{
			InitializeComponent();
			Channels = RGBChannels;
			channel1 = Red;
			channel2 = Green;
			channel3 = Blue;
		}

		private void window1_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && _isDragging)
			{
				DragFinished(true);
			}
		}

		private void DragStarted()
		{
			_isDragging = true;
			var toshowG = _originalElement as ListBoxItem;
			_originalLeft = Canvas.GetLeft(toshowG);
			_originalTop = Canvas.GetTop(toshowG);

			_overlayElement = new SimpleCircleAdorner(toshowG);
			var layer = AdornerLayer.GetAdornerLayer(toshowG);
			layer.Add(_overlayElement);

		}

		private void DragMoved()
		{
			var currentPosition = Mouse.GetPosition(Channels);

			_overlayElement.LeftOffset = currentPosition.X - _startPoint.X;
			_overlayElement.TopOffset = currentPosition.Y - _startPoint.Y;


		}

		private void DragFinished(bool cancelled)
		{
			Mouse.Capture(null);
			if (_isDragging)
			{
				AdornerLayer.GetAdornerLayer(_overlayElement.AdornedElement).Remove(_overlayElement);

				if (cancelled == false)
				{
					Canvas.SetTop(_originalElement, _originalTop + _overlayElement.TopOffset);
					Canvas.SetLeft(_originalElement, _originalLeft + _overlayElement.LeftOffset);
				}
				_overlayElement = null;
			}
			_isDragging = false;
			_isDown = false;
			Mouse.OverrideCursor = null;
		}
		private Point _biasPoint;
		private void MyCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Source.GetType() == typeof(ListBoxItem))
			{
				Mouse.OverrideCursor = this.Resources["CloseHandCursor"] as Cursor;
				_isDown = true;
				_startPoint = e.GetPosition(Channels);
				_biasPoint = e.GetPosition(e.Source as UIElement);
				_originalElement = e.Source as UIElement;
				touchedElement = _originalElement;
				oriIndex = Channels.Items.IndexOf(_originalElement);
				touchedIndex = oriIndex;
				Channels.SelectedIndex = touchedIndex;
				Channels.CaptureMouse();
				e.Handled = true;
			}
			else
			{

			}
		}
		private int oriIndex = 0;
		private int touchedIndex = 0;

		private void MyCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
		{

			if (_isDown)
			{
				if ((!_isDragging) &&
					((Math.Abs(e.GetPosition(Channels).X - _startPoint.X) >
					  SystemParameters.MinimumHorizontalDragDistance) ||
					 (Math.Abs(e.GetPosition(Channels).Y - _startPoint.Y) >
					  SystemParameters.MinimumVerticalDragDistance)))
				{
					DragStarted();
				}
				if (_isDragging)
				{
					var HitElement = Channels.InputHitTest(Mouse.GetPosition(Channels));
					if (HitElement == null)
						return;

					touchedElement = VisualTreeHelper.GetParent((HitElement as UIElement)) as UIElement;
					object temp;


					if (touchedElement != _originalElement && touchedElement.GetType() == typeof(ListBoxItem))
					{
						oriIndex = Channels.Items.IndexOf(_originalElement);
						touchedIndex = Channels.Items.IndexOf(touchedElement);

						if (oriIndex > touchedIndex)
						{
							_startPoint.Y = e.GetPosition(Channels).Y - ((touchedElement as ListBoxItem).ActualHeight - _biasPoint.Y);
						}
						else
						{
							_startPoint.Y = e.GetPosition(Channels).Y + _biasPoint.Y;

						}

						temp = Channels.Items.GetItemAt(oriIndex);
						DragMoved();
						temp = Channels.Items.GetItemAt(oriIndex);
						Channels.Items.Remove(temp);
						Channels.Items.Insert(touchedIndex, temp);
						Channels.SelectedIndex = touchedIndex;

						string selection = (ColorChannels.SelectedValue as ComboBoxItem).Content as string;

						if (selection == "RGB")
						{
							if (Channels.ItemContainerGenerator.IndexFromContainer(channel1) >= 0)
								Resource.RGBEffect.RIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel1);
							else
								Resource.RGBEffect.RIndex = touchedIndex;

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel2) >= 0)
								Resource.RGBEffect.GIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel2);
							else
								Resource.RGBEffect.GIndex = touchedIndex;

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel3) >= 0)
								Resource.RGBEffect.BIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel3);
							else
								Resource.RGBEffect.BIndex = touchedIndex;

							if (RedIntensity.Value + BlueIntensity.Value + GreenIntensity.Value == 3 &&
								(Resource.RGBEffect.RIndex == 0 && Resource.RGBEffect.GIndex == 1 && Resource.RGBEffect.BIndex == 2))
								Resource.MainWindow.scrollViewer.Effect = null;
							else
								Resource.MainWindow.scrollViewer.Effect = Resource.RGBEffect;

						}
						else if (selection == "Pseudo H&E")
						{
							if (Channels.ItemContainerGenerator.IndexFromContainer(channel1) >= 0)
							{
								Resource.HEEffect.EosinIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel1);
							}
							else
							{
								Resource.HEEffect.EosinIndex = touchedIndex;
							}

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel2) >= 0) 
							{ 
								Resource.HEEffect.BackgroundIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel2);
							}
							else
							{
								Resource.HEEffect.BackgroundIndex = touchedIndex;
							}

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel3) >= 0)
							{
								Resource.HEEffect.HematoxylinIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel3);
							}
							else
							{
								Resource.HEEffect.HematoxylinIndex = touchedIndex;
							}
						}
						else if (selection == "Pseudo IHC")
						{
							if (Channels.ItemContainerGenerator.IndexFromContainer(channel2) >= 0)
								Resource.IHCEffect.DabIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel2);
							else
								Resource.IHCEffect.DabIndex = touchedIndex;

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel1) >= 0)
								Resource.IHCEffect.BackgroundIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel1);
							else
								Resource.IHCEffect.BackgroundIndex = touchedIndex;

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel3) >= 0)
								Resource.IHCEffect.HematoxylinIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel3);
							else
								Resource.IHCEffect.HematoxylinIndex = touchedIndex;
						}
						else if (selection == "Pseudo Staining")
						{
							if (Channels.ItemContainerGenerator.IndexFromContainer(channel3) >= 0)
							{
								Resource.PseudoStainingEffect.EosinIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel3);
							}
							else
							{
								Resource.PseudoStainingEffect.EosinIndex = touchedIndex;
							}

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel2) >= 0)
							{
								Resource.PseudoStainingEffect.DabIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel2);
							}
							else
							{
								Resource.PseudoStainingEffect.DabIndex = touchedIndex;
							}

							if (Channels.ItemContainerGenerator.IndexFromContainer(channel1) >= 0)
							{
								Resource.PseudoStainingEffect.HematoxylinIndex = Channels.ItemContainerGenerator.IndexFromContainer(channel1);
							}
							else
							{
								Resource.PseudoStainingEffect.HematoxylinIndex = touchedIndex;
							}
						}
					}
					else
					{
						DragMoved();
					}

				}
			}
		}



		private void MyCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_isDown)
			{
				DragFinished(false);
				e.Handled = true;
			}
		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			var elementName = (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent((sender as ToggleButton)) as Grid) as UIElement) as UIElement) as ListBoxItem).Name;

			if ((sender as ToggleButton).IsChecked.Value)
			{
				// Code for Checked state
				if (elementName == "Red")
				{
					RedIntensity.Value = 1.0;
				}
				else if (elementName == "Green")
				{
					GreenIntensity.Value = 1.0;
				}
				else if (elementName == "Blue")
				{
					BlueIntensity.Value = 1.0;
				}
				else if (elementName == "PSEosin")
				{
					PSEosinIntensity.Value = 1.0;
				}
				else if (elementName == "PSHematoxylin")
				{
					PSHematoxylinIntensity.Value = 1.0;
				}
				else if (elementName == "PSDab")
				{
					PSDabIntensity.Value = 1.0;
				}
			}
			else
			{
				// Code for Un-Checked state
				if (elementName == "Red")
				{
					RedIntensity.Value = 0.0;
				}
				else if (elementName == "Green")
				{
					GreenIntensity.Value = 0.0;
				}
				else if (elementName == "Blue")
				{
					BlueIntensity.Value = 0.0;
				}
				else if (elementName == "PSEosin")
				{
					PSEosinIntensity.Value = 0.0;
				}
				else if (elementName == "PSHematoxylin")
				{
					PSHematoxylinIntensity.Value = 0.0;
				}
				else if (elementName == "PSDab")
				{
					PSDabIntensity.Value = 0.0;
				}
			}
		}

		private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			var elementName = (sender as Slider).Name;

			if (e.NewValue > 0)
			{
				if (elementName == "RedIntensity")
				{
					RedToggle.IsChecked = true;
				}
				else if (elementName == "GreenIntensity")
				{
					GreenToggle.IsChecked = true;
				}
				else if (elementName == "BlueIntensity")
				{
					BlueToggle.IsChecked = true;
				}
				else if (elementName == "PSEosinIntensity")
				{
					PSEosinToggle.IsChecked = true;
				}
				else if (elementName == "PSHematoxylinIntensity")
				{
					PSHematoxylinToggle.IsChecked = true;
				}
				else if (elementName == "PSDabIntensity")
				{
					PSDabToggle.IsChecked = true;
				}

			}
			else if (e.NewValue == 0)
			{
				if (elementName == "RedIntensity")
				{
					RedToggle.IsChecked = false;
				}
				else if (elementName == "GreenIntensity")
				{
					GreenToggle.IsChecked = false;
				}
				else if (elementName == "BlueIntensity")
				{
					BlueToggle.IsChecked = false;
				}
				else if (elementName == "PSEosinIntensity")
				{
					PSEosinToggle.IsChecked = false;
				}
				else if (elementName == "PSHematoxylinIntensity")
				{
					PSHematoxylinToggle.IsChecked = false;
				}
				else if (elementName == "PSDabIntensity")
				{
					PSDabToggle.IsChecked = false;
				}
			}
			
		}

		private void IntensityMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			
		}

		private void IntensityMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			
		}

		private void MouseEnter(object sender, MouseEventArgs e)
		{
			
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			
		}
		private ListBox Channels;
		private ListBoxItem channel1;
		private ListBoxItem channel2;
		private ListBoxItem channel3;
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			string selection = ((e.Source as ComboBox).SelectedValue as ComboBoxItem).Content as string;
			Channels = this.FindName(selection.Replace('&', ' ').Replace(' ', '_') + "Channels") as ListBox;
			if (RGB == null || HE == null || IHC == null) return;
			
			RGB.Visibility = Visibility.Collapsed;
			HE.Visibility = Visibility.Collapsed;
			IHC.Visibility = Visibility.Collapsed;
			PseudoStaining.Visibility = Visibility.Collapsed;

			if (selection == "RGB")
			{
				Resource.MainWindow.scrollViewer.Effect = Resource.RGBEffect;
				RGB.Visibility = Visibility.Visible;

				channel1 = Channels.Items[Channels.Items.IndexOf(Channels.FindName("Red"))] as ListBoxItem;
				channel2 = Channels.Items[Channels.Items.IndexOf(Channels.FindName("Green"))] as ListBoxItem;
				channel3 = Channels.Items[Channels.Items.IndexOf(Channels.FindName("Blue"))] as ListBoxItem;
			}
			else if (selection == "Pseudo H&E")
			{
				
				Resource.MainWindow.scrollViewer.Effect = Resource.HEEffect;
				HE.Visibility = Visibility.Visible;
				channel1 = HEEosin; 
				channel2 = HEBackground;
				channel3 = HEHematoxylin;
			}
			else if (selection == "Pseudo IHC")
			{
				Resource.MainWindow.scrollViewer.Effect = Resource.IHCEffect;
				IHC.Visibility = Visibility.Visible;
				channel1 = IHCBackground;
				channel2 = IHCDab;
				channel3 = IHCHematoxylin;
			}
			else if (selection == "Pseudo Staining")
			{
				Resource.MainWindow.scrollViewer.Effect = Resource.PseudoStainingEffect;
				PseudoStaining.Visibility = Visibility.Visible;
				channel1 = PSHematoxylin;
				channel2 = PSDab;
				channel3 = PSEosin;
			}
		}

		private void MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var elementName = (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent((sender as Slider)) as Grid) as UIElement) as UIElement) as ListBoxItem).Name;
			if (elementName == "Red")
			{
			}
			else if (elementName == "Green")
			{
			}
			else if (elementName == "Blue")
			{
			}			
			else if (elementName == "PSEosin")
			{
				var tmp = (Color)(sender as Slider).Foreground.GetValue(SolidColorBrush.ColorProperty);
				tmp.A = (byte)(PSEosinIntensity.Value * 255);
				bool ok = ColorPickerWindow.ShowDialog(out Color color, tmp);
				if (ok)
				{
					PSEosinIntensity.Value = (double)color.A / 255;
					color.A = 255;
					(sender as Slider).Foreground = new SolidColorBrush(color);
				}
			}
			else if (elementName == "PSHematoxylin")
			{
				var tmp = (Color)(sender as Slider).Foreground.GetValue(SolidColorBrush.ColorProperty);
				tmp.A = (byte)(PSHematoxylinIntensity.Value * 255);
				bool ok = ColorPickerWindow.ShowDialog(out Color color, tmp);
				if (ok)
				{
					PSHematoxylinIntensity.Value = (double)color.A / 255;
					color.A = 255;
					(sender as Slider).Foreground = new SolidColorBrush(color);
				}
			}
			else if (elementName == "PSDab")
			{

				var tmp = (Color)(sender as Slider).Foreground.GetValue(SolidColorBrush.ColorProperty);
				tmp.A = (byte)(PSDabIntensity.Value * 255);
				bool ok = ColorPickerWindow.ShowDialog(out Color color, tmp);
				if (ok)
				{
					PSDabIntensity.Value = (double)color.A / 255;
					color.A = 255;
					(sender as Slider).Foreground = new SolidColorBrush(color);
				}
			}
		}
	}
}
