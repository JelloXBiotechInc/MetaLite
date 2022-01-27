using ColorPickerWPF;
using MahApps.Metro.Controls;
using MetaLite_Viewer.Helper;
using MetaLite_Viewer.Model;
using MetaLite_Viewer.Subwindow;
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
using System.Windows.Controls.Primitives;

namespace MetaLite_Viewer
{
    /// <summary>
    /// Interaction logic for AnnotationSelector.xaml
    /// </summary>
    public partial class AnnotationSelector : Window
    {
        private TextBox? nowSelectedItem;
        private string TextBoxOldString;
        public AnnotationSelector()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImageChanged();
        }

        private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            Hide();
        }

        private void WindowTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);

            // Perform the hit test against a given portion of the visual object tree.
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);

            if (result != null)
            {
                if (!result.VisualHit.Equals(nowSelectedItem))
                    SetTitle(nowSelectedItem);
            }
        }

        private void SetTitle(TextBox? textBox)
        {
            if (textBox != null)
            {
                textBox.IsReadOnly = true;
                textBox.Background = Brushes.Transparent;
                textBox.Foreground = Brushes.White;
                textBox.BorderBrush = Brushes.Transparent;
                textBox.IsEnabled = false;
                textBox.IsEnabled = true;
                this.annotationSelector.ScrollIntoView(textBox);
            }
            
        }


        private void AnnotationLayerTitleBlock_KeyDown(object sender, KeyEventArgs e)
        {
            BindingExpression be = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
            switch (e.Key)
            {
                case Key.Escape:
                    (sender as TextBox).Text = TextBoxOldString;
                    SetTitle(nowSelectedItem);
                    break;
                case Key.Enter:
                    be.UpdateSource();
                    SetTitle(nowSelectedItem);
                    break;
            }
        }

        private void AddAnnotationButtonClick(object sender, RoutedEventArgs e)
        {
            if (Resource.isDataReaded)
            {
                Resource.nowImageData.AddAnnotationLayer();
                this.annotationSelector.SelectedIndex = Resource.nowImageData.AnnotationLayers.Count-1;
                if (Resource.nowImageData.AnnotationLayers.Count == 1)
                {
                    Resource.nowImageData.SelectedLayerIndex = 0;
                    this.annotationSelector.SelectedIndex = 0;
                }
            }            
        }

        public void ImageChanged()
        {
            if (Resource.isDataReaded)
            {
                annotationSelector.ItemsSource = Resource.nowImageData.AnnotationLayers;
                if (Resource.isLayerExist)
                {
                    
                    SetFocus(Resource.nowImageData.AnnotationLayers[Resource.nowImageData.SelectedLayerIndex].Uid);
                }                    
            }            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void AnnotationLayerBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem as AnnotationLayer;
            if (selectedItem == null)
            {
                Resource.MainWindow.DrawContour();
                return;
            }
                
            Resource.nowImageData.SelectedLayerIndex = Resource.nowImageData.AnnotationLayers.IndexOf(selectedItem);
            try
            {
                if (selectedItem.Layer.IsFrozen)
                {
                    // If any selected annotation layer is freeze then unfreeze all at once
                    foreach (ImageData ID in Resource.ImageDatas)
                    {
                        foreach (AnnotationLayer Ann in ID.AnnotationLayers)
                        {
                            if (Ann.Layer.IsFrozen)
                            {
                                var Temp = Ann.Layer.Clone();
                                Ann.Layer = Temp;

                            }
                        }
                    }

                }
                Resource.MainWindow.DrawContour();
            }
            catch
            {
                
            }
            
        }        

        private void AnnotationLayerTitleBlock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            

            (sender as TextBox).IsReadOnly = false;
            (sender as TextBox).CaretIndex = (sender as TextBox).Text.Length;
            (sender as TextBox).Background = Brushes.White;
            (sender as TextBox).Foreground = Brushes.Black;
            (sender as TextBox).BorderBrush = Brushes.Black;
            
            nowSelectedItem = (sender as TextBox);
            TextBoxOldString = nowSelectedItem.Text;
            (sender as TextBox).Focus();
            CaptureMouse();
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideElement);
            
        }

        private void OnMouseDownOutsideElement(object sender, MouseButtonEventArgs e)
        {
           
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideElement);
            ReleaseMouseCapture();
            SetTitle(nowSelectedItem);
        }

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            bool ok = ColorPickerWindow.ShowDialog(out Color color, (Color)(sender as Button).Background.GetValue(SolidColorBrush.ColorProperty));
            if (ok)
            {
                (sender as Button).Background = new SolidColorBrush(color);
                color.A = 255;
                (sender as Button).Foreground = new SolidColorBrush(color);
            }
            SetFocus((sender as Button).Uid);
            try
            {
                Resource.MainWindow.DrawContour();
            }
            catch
            {
                MessageBox.Show("Error code: 0005\nChange annotationcolor drawing contour exception.");
            }
        }

        public void SetFocus(String FoucsUid)
        {
            foreach (AnnotationLayer ann in annotationSelector.Items)
            {
                if (ann.Uid == FoucsUid)
                {
                    if (this.annotationSelector.SelectedItem == ann)
                        continue;
                    this.annotationSelector.SelectedItem = ann;
                    this.annotationSelector.ScrollIntoView(ann);
                    this.annotationSelector.Focus();

                    break;
                }
            }
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            SetFocus((sender as TextBox).Uid);
        }

        private void Annotation_Delete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = messagebox.Show("Deleting annotation cannot be recovered, do you want to continue?", "Deleting confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Cancel) return;
            var UIElement = sender;
            while ((UIElement as UIElement).GetType() != typeof(ListBoxItem))
            {
                UIElement = (UIElement as UIElement).GetParentObject();
            }
            UIElement = (UIElement as UIElement).GetParentObject();            
            UIElement = (UIElement as UIElement).GetParentObject();            
            UIElement = (UIElement as UIElement).GetParentObject();
            
            int LayerIndex = annotationSelector.ItemContainerGenerator.IndexFromContainer((UIElement as ListBoxItem));

            int setIndexOfSelectedIndex = this.annotationSelector.SelectedIndex;
            if (LayerIndex < Resource.nowImageData.SelectedLayerIndex)
            {
                Resource.nowImageData.SelectedLayerIndex = MathHelper.Clip(Resource.nowImageData.SelectedLayerIndex-1, 0, Resource.nowImageData.AnnotationLayers.Count-1);
                setIndexOfSelectedIndex -= 1;
            }
            else if (LayerIndex == Resource.nowImageData.SelectedLayerIndex)
            {
                if (LayerIndex == Resource.nowImageData.AnnotationLayers.Count - 1)
                {
                    setIndexOfSelectedIndex = Resource.nowImageData.AnnotationLayers.Count - 2;
                }
                else
                {
                    setIndexOfSelectedIndex = LayerIndex;
                    Resource.nowImageData.SelectedLayerIndex = LayerIndex;
                }
            }
            
            Resource.nowImageData.RemoveAnnotationLayer(LayerIndex);
            this.annotationSelector.SelectedIndex = MathHelper.Clip(setIndexOfSelectedIndex, 0, Resource.nowImageData.AnnotationLayers.Count - 1);
            Resource.MainWindow.DrawContour();
        }
        private void AnnotationLayerTitleBlock_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {            
            SetTitle((sender as TextBox));
        }

        private void AnnotationLayerTitleBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Resource.isDataReaded)
                return;
            Resource.nowImageData.IsAnnotationDirty = true;
        }

        private void ShowAllToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            
            if (!Resource.isDataReaded)
                return;
            foreach (var L in Resource.nowImageData.AnnotationLayers)
            {
                L.IsChecked = !(sender as ToggleButton).IsChecked.Value;
            }
        }


        private void HideAllAnnotation_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Resource.MainWindow.DrawContour();
        }
    }
}
