using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MetaLite_Viewer.Helper
{
    class ImageHelper
    {
        public static BitmapSource ToBitmapSource(DrawingImage source)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, new Rect(new Point(0, 0), new Size(source.Width, source.Height)));
            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)source.Width, (int)source.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }

        public static BitmapSource UIElementToBitmapSource(UIElement element)
        {
            var target = new RenderTargetBitmap((int)(element.RenderSize.Width), (int)(element.RenderSize.Height), 96, 96, PixelFormats.Pbgra32);
            var brush = new VisualBrush(element);

            var visual = new DrawingVisual();
            var drawingContext = visual.RenderOpen();


            drawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0),
                new Point(element.RenderSize.Width, element.RenderSize.Height)));

            drawingContext.Close();

            target.Render(visual);

            return target;
        }

        public static BitmapImage ConvertRenderTargetBitmapToBitmapImage(RenderTargetBitmap wbm)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.StreamSource = new MemoryStream(stream.ToArray()); //stream;
                bmp.EndInit();
                bmp.Freeze();
            }
            return bmp;
        }

        public static GeometryDrawing DrawRectangle(double x, double y, double width, double height, Brush brush, Pen pen)
        {
            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.FillRule = FillRule.EvenOdd;
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 275, 275)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(x, y, width, height)));
            GeometryDrawing geometryDrawing = new GeometryDrawing(brush, pen, geometryGroup);
            return geometryDrawing;
        }

        public static WriteableBitmap ResizeImage(WriteableBitmap img, double scale)
        {
            BitmapSource source = img;

            var s = new ScaleTransform(scale, scale);

            var res = new TransformedBitmap(img, s);

            return ConvertBitmapSourcetoWriteableBitmap(res);
        }

        public static WriteableBitmap ConvertBitmapSourcetoWriteableBitmap(BitmapSource source)
        {
            // Calculate stride of source
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);

            // Create data array to hold source pixel data
            byte[] data = new byte[stride * source.PixelHeight];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Create WriteableBitmap to copy the pixel data to.      
            WriteableBitmap target = new WriteableBitmap(source.PixelWidth
                , source.PixelHeight, source.DpiX, source.DpiY
                , source.Format, null);

            // Write the pixel data to the WriteableBitmap.
            target.WritePixels(new Int32Rect(0, 0
                , source.PixelWidth, source.PixelHeight)
                , data, stride, 0);

            return target;
        }
    }
}
