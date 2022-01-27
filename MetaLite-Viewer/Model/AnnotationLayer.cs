using MetaLite_Viewer.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MetaLite_Viewer.Model
{
    class AnnotationLayer : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public WriteableBitmap Layer {get;set;}
        public string title;
        public string Title {
            get
            {
                return title;
            }
            set
            {
                if (title == value)
                    return;
                title = value;
                OnPropertyChanged(nameof(Title));
                
            }
        }
        public string Uid { get; set; }
        public int Transparency 
        {
            get
            {
                return (int)((float)_annotationColor.A * 100 / 255);
            }
            set
            {
                if (Transparency != value)
                {
                    Transparency = value;
                    OnPropertyChanged(nameof(Transparency));
                }
            }
        }

        private bool _isChecked = true;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (IsChecked!= value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                    OnPropertyChanged(nameof(Visibility));
                }                
            }
        }

        public Visibility Visibility
        {
            get
            {
                return _isChecked ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private Color _annotationColor = ColorHelper.GetColorByProbability();
        public Color AnnotationColor
        {
            get { return _annotationColor; }
            set
            {
                _annotationColor = value;
                
                
                OnPropertyChanged(nameof(AnnotationColor));
                OnPropertyChanged(nameof(AnnotationColorBrush));
                OnPropertyChanged(nameof(Transparency));
            }
        }

        public Brush ColorButtonShow
        {
            get {
                var tmpColor = _annotationColor;
                tmpColor.A = 255;
                return new SolidColorBrush(tmpColor);
            }
            set
            {
                OnPropertyChanged(nameof(AnnotationColor));
                OnPropertyChanged(nameof(AnnotationColorBrush));
                OnPropertyChanged(nameof(Transparency));
            }
        }
        
        private int _intColor = ColorHelper.IntHIGHLIGHTER;
        public int IntColor
        {
            get
            {
                return _intColor;
            }
            set
            {
                _intColor = value;
            }
        }

        public Brush AnnotationColorBrush
        {
            get { return new SolidColorBrush(AnnotationColor); }
            set { AnnotationColor = (value as SolidColorBrush).Color; }
        }

        public AnnotationLayer(WriteableBitmap layer, string title, string uid)
        {
            // Cross thread UI update aquire bitmap freeze, the unfreeze are execute in AnnotationSelector.xaml.cs AnnotationLayerBox_SelectionChanged
            Layer = layer;
            Layer.Freeze();
            Title = title;
            Uid = uid;
        }

        public AnnotationLayer(WriteableBitmap layer, string title, string uid, bool acrossThread)
        {
            // To create single thread bitmap without freeze
            Layer = layer;
            Title = title;
            Uid = uid;
        }
        private void SetAnnotationColor(byte B,byte G, byte R, byte A)
        {
            int stride = Layer.PixelWidth * Layer.Format.BitsPerPixel / 8;

            byte[] annotationPixels = new byte[Layer.PixelHeight * stride];

            Layer.CopyPixels(annotationPixels, stride, 0);

            Parallel.For(0, Layer.PixelHeight, y =>
            {
                for (int x = 0; x < stride; x += 4)
                {
                    if (annotationPixels[x + 3 + y * stride] != 0x00)
                    {
                        annotationPixels[x + y * stride] = B;
                        annotationPixels[x + 1 + y * stride] = G;
                        annotationPixels[x + 2 + y * stride] = R;
                        annotationPixels[x + 3 + y * stride] = A;
                    }
                }
            });
            Layer.WritePixels(new Int32Rect(0, 0, Layer.PixelWidth, Layer.PixelHeight), annotationPixels, stride, 0);
        }

        private void SetPngAnnotation()
        {
            int stride = Layer.PixelWidth * Layer.Format.BitsPerPixel / 8;

            byte[] annotationPixels = new byte[Layer.PixelHeight * stride];

            Layer.CopyPixels(annotationPixels, stride, 0);

            Parallel.For(0, Layer.PixelHeight, y =>
            {
                for (int x = 0; x < stride; x += 4)
                {
                    if (annotationPixels[x + 0 + y * stride] != 0x00 || annotationPixels[x + 1 + y * stride] != 0x00 || annotationPixels[x + 2 + y * stride] != 0x00)
                    {
                        annotationPixels[x + y * stride] = ColorHelper.HIGHLIGHTER.B;
                        annotationPixels[x + 1 + y * stride] = ColorHelper.HIGHLIGHTER.G;
                        annotationPixels[x + 2 + y * stride] = ColorHelper.HIGHLIGHTER.R;
                        annotationPixels[x + 3 + y * stride] = ColorHelper.HIGHLIGHTER.A;
                    }
                    else
                    {
                        annotationPixels[x + y * stride] = 0;
                        annotationPixels[x + 1 + y * stride] = 0;
                        annotationPixels[x + 2 + y * stride] = 0;
                        annotationPixels[x + 3 + y * stride] = 0;
                    }
                }
            });
            Layer.WritePixels(new Int32Rect(0, 0, Layer.PixelWidth, Layer.PixelHeight), annotationPixels, stride, 0);
        }

        public AnnotationLayer(WriteableBitmap layer, string title, string uid, Color? annotationColor, bool isJellox=true)
        {
            // The asynchronous fileopen must color annotation before Freeze
            Layer = layer;
            AnnotationColor = (Color)annotationColor;

            if (isJellox)
                SetAnnotationColor(0, 0, 0, ColorHelper.HIGHLIGHTER.A);
            else
                SetPngAnnotation();
            Layer.Freeze();
            Title = title;
            Uid = uid;
        }
    }
}
