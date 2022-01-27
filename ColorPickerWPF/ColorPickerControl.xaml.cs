using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorPickerWPF.Code;
using ColorPickerWPF.Properties;
using UserControl = System.Windows.Controls.UserControl;

namespace ColorPickerWPF
{
    /// <summary>
    /// Interaction logic for ColorPickerControl.xaml
    /// </summary>
    public partial class ColorPickerControl : UserControl
    {
        public Color Color = Colors.White;

        public delegate void ColorPickerChangeHandler(Color color);

        public event ColorPickerChangeHandler OnPickColor;

        internal List<ColorSwatchItem> ColorSwatch1 = new List<ColorSwatchItem>();
        internal List<ColorSwatchItem> ColorSwatch2 = new List<ColorSwatchItem>();

        public bool IsSettingValues = false;

        protected const int NumColorsFirstSwatch = 39;
        protected const int NumColorsSecondSwatch = 112;

        internal static ColorPalette ColorPalette;


        public ColorPickerControl()
        {
            InitializeComponent();

            ColorPickerSwatch.ColorPickerControl = this;

            // Load from file if possible
            if (ColorPickerSettings.UsingCustomPalette && File.Exists(ColorPickerSettings.CustomPaletteFilename))
            {
                try
                {
                    ColorPalette = ColorPalette.LoadFromXml(ColorPickerSettings.CustomPaletteFilename);
                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            }

            if (ColorPalette == null)
            {
                ColorPalette = new ColorPalette();
                ColorPalette.InitializeDefaults();
            }


            ColorSwatch1.AddRange(ColorPalette.BuiltInColors.Take(NumColorsFirstSwatch).ToArray());

            ColorSwatch2.AddRange(ColorPalette.BuiltInColors.Skip(NumColorsFirstSwatch).Take(NumColorsSecondSwatch).ToArray());
            
            Swatch1.SwatchListBox.ItemsSource = ColorSwatch1;
            Swatch2.SwatchListBox.ItemsSource = ColorSwatch2;

            if (ColorPickerSettings.UsingCustomPalette)
            {
                CustomColorSwatch.SwatchListBox.ItemsSource = ColorPalette.CustomColors;
            }
            else
            {
                customColorsLabel.Visibility = Visibility.Collapsed;
                CustomColorSwatch.Visibility = Visibility.Collapsed;
            }


            RSlider.Slider.Maximum = 255;
            GSlider.Slider.Maximum = 255;
            BSlider.Slider.Maximum = 255;
            ASlider.Slider.Maximum = 255;
            HSlider.Slider.Maximum = 360;
            SSlider.Slider.Maximum = 1;
            VSlider.Slider.Maximum = 1;


            RSlider.Label.Content = "R";
            RSlider.ToolTip = "Red";
            RSlider.Slider.TickFrequency = 1;
            RSlider.Slider.IsSnapToTickEnabled = true;
            GSlider.Label.Content = "G";
            GSlider.ToolTip = "Green";
            GSlider.Slider.TickFrequency = 1;
            GSlider.Slider.IsSnapToTickEnabled = true;
            BSlider.Label.Content = "B";
            BSlider.ToolTip = "Blue";
            BSlider.Slider.TickFrequency = 1;
            BSlider.Slider.IsSnapToTickEnabled = true;

            ASlider.Label.Content = "A";
            ASlider.ToolTip = "Alpha";
            ASlider.Slider.Minimum = 1;
            ASlider.Slider.TickFrequency = 1;
            ASlider.Slider.IsSnapToTickEnabled = true;

            HSlider.Label.Content = "H";
            HSlider.ToolTip = "Hue";

            HSlider.Slider.TickFrequency = 1;
            HSlider.Slider.IsSnapToTickEnabled = true;
            SSlider.Label.Content = "S";
            SSlider.ToolTip = "Saturation";

            //SSlider.Slider.TickFrequency = 1;
            //SSlider.Slider.IsSnapToTickEnabled = true;
            VSlider.Label.Content = "V";
            VSlider.ToolTip = "Value";

            //LSlider.Slider.TickFrequency = 1;
            //LSlider.Slider.IsSnapToTickEnabled = true;


            SetColorRGB(Color);
        }


        public void SetColorRGB(Color color)
        {
            Color = color;

            CustomColorSwatch.CurrentColor = color;

            IsSettingValues = true;

            RSlider.Slider.Value = Color.R;
            GSlider.Slider.Value = Color.G;
            BSlider.Slider.Value = Color.B;
            Console.WriteLine(Color.A);
            ASlider.Slider.Value = Color.A;
            //var img = SampleImage.Source as BitmapSource;
            //System.Drawing.Color HSV = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            //float hue = color.GetHue()/360f;
            //Console.WriteLine(ColorCanvas.ActualWidth+","+ hue);
            //float saturation = color.GetSaturation();
            //float lightness = color.GetBrightness();


            //

            //SSlider.Slider.Value = Color.GetSaturation();
            //LSlider.Slider.Value = Color.GetBrightness();
            //HSlider.Slider.Value = Color.GetHue();

            ColorDisplayBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B));

            IsSettingValues = false;
            OnPickColor?.Invoke(color);
            
        }

        public void SetColorHSV(Color color)
        {
            Color = color;

            CustomColorSwatch.CurrentColor = color;

            IsSettingValues = true;

            //RSlider.Slider.Value = Color.R;
            //GSlider.Slider.Value = Color.G;
            //BSlider.Slider.Value = Color.B;
            //ASlider.Slider.Value = Color.A;
            //var img = SampleImage.Source as BitmapSource;
            //System.Drawing.Color HSV = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            //float hue = color.GetHue()/360f;
            //Console.WriteLine(ColorCanvas.ActualWidth+","+ hue);
            //float saturation = color.GetSaturation();
            //float lightness = color.GetBrightness();


            //
            HSlider.Slider.Value = Color.GetHue();
            SSlider.Slider.Value = Color.GetSaturation();
            VSlider.Slider.Value = Color.GetBrightness();
            Aim.Margin = new Thickness() { Left = HSlider.Slider.Value - Aim.ActualWidth / 2, Top = (1.0 - VSlider.Slider.Value) * 255 - Aim.ActualHeight / 2 };

            ColorDisplayBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, color.R, color.G, color.B));

            IsSettingValues = false;
            OnPickColor?.Invoke(color);
            
        }

        public void SetColorHSV(double h, double s, double v)
        {
            HSlider.Slider.Value = h;
            SSlider.Slider.Value = s;
            VSlider.Slider.Value = v;

            Color = Util.FromHSV(h, s, v);

            CustomColorSwatch.CurrentColor = Color;

            IsSettingValues = true;

            //RSlider.Slider.Value = Color.R;
            //GSlider.Slider.Value = Color.G;
            //BSlider.Slider.Value = Color.B;
            //ASlider.Slider.Value = Color.A;
            //var img = SampleImage.Source as BitmapSource;
            //System.Drawing.Color HSV = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            //float hue = color.GetHue()/360f;
            //Console.WriteLine(ColorCanvas.ActualWidth+","+ hue);
            //float saturation = color.GetSaturation();
            //float lightness = color.GetBrightness();


            //
            
            Aim.Margin = new Thickness() { Left = HSlider.Slider.Value - Aim.ActualWidth / 2, Top = (1.0 - VSlider.Slider.Value) * 255 - Aim.ActualHeight / 2 };

            ColorDisplayBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, Color.R, Color.G, Color.B));

            IsSettingValues = false;
            OnPickColor?.Invoke(Color);

        }

        internal void CustomColorsChanged()
        {
            if (ColorPickerSettings.UsingCustomPalette)
            {
                SaveCustomPalette(ColorPickerSettings.CustomPaletteFilename);
            }
        }


        /*protected void SampleImageClick(System.Windows.Controls.Grid img, Point pos)
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/82a5731e-e201-4aaf-8d4b-062b138338fe/getting-pixel-information-from-a-bitmapimage?forum=wpf

            int stride = (int) img.Width*4;
            int size = (int) img.Height*stride;
            byte[] pixels = new byte[(int) size];

            //img.CopyPixels(pixels, stride, 0);


            // Get pixel
            //var x = (int) (pos.X * img.Width /360.0);
            //var y = (int) (pos.Y* img.Height / 255.0);
            HSlider.Slider.Value = pos.X;
            //LSlider.Slider.Value = 1.0 - (pos.Y) / 255;
            //LSlider.Slider.Value = (1.0 - pos.Y / 255);
            if (pos.Y < 127.5)
            {
                SSlider.Slider.Value = 1.0 - Math.Abs(pos.Y - 127.5) / 127.5;
                LSlider.Slider.Value = 1.0;
            }
            else
            {
                LSlider.Slider.Value = 1.0 - (pos.Y - 127.5) / 127.5;
                SSlider.Slider.Value = 1.0;
            }
            //LSlider.Slider.Value = 1.0 - (pos.Y - 127.5) / 127.5;
            //SSlider.Slider.Value = 1.0 - Math.Abs(pos.Y - 127.5) / 127.5;
            //SSlider.Slider.Value = 1.0;
            //LSlider.Slider.Value = 1.0 - pos.Y / 255;

            var RGB = Util.FromHSV(HSlider.Slider.Value, SSlider.Slider.Value, LSlider.Slider.Value);

            //int index = (y)*stride + 4*x;

            //byte red = pixels[index];
            //byte green = pixels[index + 1];
            //byte blue = pixels[index + 2];
            //byte alpha = pixels[index + 3];

            var color = Color.FromArgb(Color.A, RGB.R, RGB.G, RGB.B);
            SetColorRGB(color);
            SetColorHSV(color);
        }*/


        private void SampleImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            UpdateMousePos(e);
            this.MouseMove += ColorPickerControl_MouseMove;
            this.MouseUp += ColorPickerControl_MouseUp;
        }

        

        private void ColorPickerControl_MouseMove(object sender, MouseEventArgs e)
        {
            /*var pos = e.GetPosition(SampleImage);
            var img = SampleImage.Source as BitmapSource;

            var left = Math.Max(0, Math.Min(img.PixelWidth - 1, pos.X));
            var top = Math.Max(0, Math.Min(img.PixelHeight - 1, pos.Y));
            pos.X = left;
            pos.Y = top;
            //if (pos.X > 0 && pos.Y > 0 && pos.X < img.PixelWidth && pos.Y < img.PixelHeight)
            //{
            SampleImageClick(img, pos);
            Aim.Margin = new Thickness() { Left = pos.X - Aim.ActualWidth / 2, Top = pos.Y - Aim.ActualHeight / 2 };
            //}                        */

            
            UpdateMousePos(e);
        }

        private void UpdateMousePos(MouseEventArgs e)
        {
            var pos = e.GetPosition(SampleImage);
            //var img = SampleImage.Source as BitmapSource;

            var left = Math.Max(0, Math.Min(ColorCanvas.ActualWidth, pos.X));
            var top = Math.Max(0, Math.Min(ColorCanvas.ActualHeight, pos.Y));
            pos.X = left;
            pos.Y = top;
            //if (pos.X > 0 && pos.Y > 0 && pos.X < img.PixelWidth && pos.Y < img.PixelHeight)
            //{
            //SampleImageClick(SampleImage, pos);
            Console.WriteLine(left +","+top);
            
            HSlider.Slider.Value = left;
            if (top < 127.5)
            {
                SSlider.Slider.Value = 1.0 - Math.Abs(top - 127.5) / 127.5;
                VSlider.Slider.Value = 1.0;
            }
            else
            {
                VSlider.Slider.Value = 1.0 - (top - 127.5) / 127.5;
                SSlider.Slider.Value = 1.0;
            }
            Console.WriteLine(left + "," + top);

            var RGB = Util.FromHSV(HSlider.Slider.Value, SSlider.Slider.Value, VSlider.Slider.Value);

            var color = Color.FromArgb(Color.A, RGB.R, RGB.G, RGB.B);
            SetColorRGB(color);
            //SetColorHSV(color);
            Aim.Margin = new Thickness() { Left = left - Aim.ActualWidth / 2, Top = top - Aim.ActualHeight / 2 };
        }

        private void ColorPickerControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            this.MouseMove -= ColorPickerControl_MouseMove;
            this.MouseUp -= ColorPickerControl_MouseUp;
        }

        private void SampleImage2_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //var pos = e.GetPosition(SampleImage2);
            //var img = SampleImage2.Source as BitmapSource;
            //SampleImageClick(img, pos);
        }

        private void Swatch_OnOnPickColor(Color color)
        {
            SetColorRGB(Color.FromArgb((byte)ASlider.Slider.Value, color.R, color.G, color.B));
        }

        private void HSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var s = SSlider.Slider.Value;
                var v = VSlider.Slider.Value;
                var h = (float) value;
                //var a = (int) ASlider.Slider.Value;
                var c = Util.FromAhsv(255, h, s, v);
                c.A = Color.A;
                SetColorRGB(c);
                Aim.Margin = new Thickness() { Left = h - Aim.ActualWidth / 2, Top = (1.0 - v) * 255 - Aim.ActualHeight / 2 };
                //SetColorHSV(h, s, l);
            }
        }

        private void SSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var s = (float)value;
                var v = VSlider.Slider.Value;
                var h = HSlider.Slider.Value;
                var a = (int)ASlider.Slider.Value;
                var c = Util.FromAhsv(255, h, s, v);
                c.A = Color.A;
                SetColorRGB(c);
                Aim.Margin = new Thickness() { Left = h - Aim.ActualWidth / 2, Top = (1.0 - v) * 255 - Aim.ActualHeight / 2 };
                //SetColorHSV(h, s, l);
            }

        }

        private void VSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var s = SSlider.Slider.Value;
                var v = value;
                var h = HSlider.Slider.Value;
                var a = (int)ASlider.Slider.Value;
                var c = Util.FromAhsv(255, h, s, v);
                c.A = Color.A;
                SetColorRGB(c);
                Aim.Margin = new Thickness() { Left = h - Aim.ActualWidth / 2, Top = (1.0 - v) * 255 - Aim.ActualHeight / 2 };
                //SetColorHSV(h, s, l);
            }
        }

        private void RSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte) value;
                Color.R = val;
                SetColorHSV(Color);
                //Aim.Margin = new Thickness() { Left = HSlider.Slider.Value - Aim.ActualWidth / 2, Top = (1.0 - LSlider.Slider.Value) * 255 - Aim.ActualHeight / 2 };
            }
        }

        private void GSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte) value;
                Color.G = val;
                SetColorHSV(Color);
                //Aim.Margin = new Thickness() { Left = HSlider.Slider.Value - Aim.ActualWidth / 2, Top = (1.0 - LSlider.Slider.Value) * 255 - Aim.ActualHeight / 2 };
            }
        }

        private void BSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte) value;
                Color.B = val;
                SetColorHSV(Color);
                //Aim.Margin = new Thickness() { Left = HSlider.Slider.Value - Aim.ActualWidth / 2, Top = (1.0 - LSlider.Slider.Value) * 255 - Aim.ActualHeight / 2 };
            }
        }

        private void ASlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte)value;
                Color.A = val;
                SetColorHSV(Color);
            }
        }

        

        private void PickerHueSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateImageForHSV();
        }


        private void UpdateImageForHSV()
        {

            ////var hueChange = (int)((PickerHueSlider.Value / 360.0) * 240);
            //var sliderHue = (float) PickerHueSlider.Value;


            ////var colorPickerImage = System.IO.Path.Combine(Environment.CurrentDirectory, @"/Resources/colorpicker1.png");
            //var img =
            //    new BitmapImage(new Uri("pack://application:,,,/ColorPickerWPF;component/Resources/colorpicker2.png",
            //        UriKind.Absolute));

            //var writableImage = BitmapFactory.ConvertToPbgra32Format(img);

            //if (sliderHue <= 0f || sliderHue >= 360f)
            //{
            //    // No hue change just return
            //    SampleImage2.Source = img;
            //    return;
            //}

            //using (var context = writableImage.GetBitmapContext())
            //{
            //    long numPixels = img.PixelWidth*img.PixelHeight;

            //    for (int x = 0; x < img.PixelWidth; x++)
            //    {
            //        for (int y = 0; y < img.PixelHeight; y++)
            //        {
            //            var pixel = writableImage.GetPixel(x, y);

            //            var newHue = (float) (sliderHue + pixel.GetHue());
            //            if (newHue >= 360)
            //                newHue -= 360;

            //            var color = Util.FromAhsb((int) 255,
            //                newHue, pixel.GetSaturation(), pixel.GetBrightness());

            //            writableImage.SetPixel(x, y, color);
            //        }
            //    }
            //}



            //SampleImage2.Source = writableImage;
        }

        


        public void SaveCustomPalette(string filename)
        {
            var colors = CustomColorSwatch.GetColors();
            ColorPalette.CustomColors = colors;

            try
            {
                ColorPalette.SaveToXml(filename);
            }
            catch (Exception ex)
            {
                ex = ex;
            }
        }

        public void LoadCustomPalette(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    ColorPalette = ColorPalette.LoadFromXml(filename);

                    CustomColorSwatch.SwatchListBox.ItemsSource = ColorPalette.CustomColors.ToList();

                    // Do regular one too

                    ColorSwatch1.Clear();
                    ColorSwatch2.Clear();
                    ColorSwatch1.AddRange(ColorPalette.BuiltInColors.Take(NumColorsFirstSwatch).ToArray());
                    ColorSwatch2.AddRange(ColorPalette.BuiltInColors.Skip(NumColorsFirstSwatch).Take(NumColorsSecondSwatch).ToArray());
                    Swatch1.SwatchListBox.ItemsSource = ColorSwatch1;
                    Swatch2.SwatchListBox.ItemsSource = ColorSwatch2;

                }
                catch (Exception ex)
                {
                    ex = ex;
                }

            }
        }


        public void LoadDefaultCustomPalette()
        {
            LoadCustomPalette(Path.Combine(ColorPickerSettings.CustomColorsDirectory, ColorPickerSettings.CustomColorsFilename));
        }

    }
}
