using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace MetaLite_Viewer.Model
{
	class FileProcess
	{
        public static RenderTargetBitmap ConvertToBitmap(UIElement uiElement, double resolution)
        {
            var scale = resolution / 96d;

            uiElement.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var sz = uiElement.DesiredSize;
            var rect = new Rect(sz);
            uiElement.Arrange(rect);

            var bmp = new RenderTargetBitmap((int)(scale * (rect.Width)), (int)(scale * (rect.Height)), scale * 96, scale * 96, PixelFormats.Default);
            bmp.Render(uiElement);

            return bmp;
        }
    }
    public class SnapshotHelper
    {
        public static BitmapSource Capture(Rect absoluteControlRect)
        {
            using (var screenBmp = new System.Drawing.Bitmap(
                (int)absoluteControlRect.Width,
                (int)absoluteControlRect.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen((int)absoluteControlRect.Left, (int)absoluteControlRect.Top, 0, 0, screenBmp.Size);
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public static Rect GetAbsoltutePlacement(FrameworkElement visual)
        {
            Point absolutePos = visual.PointToScreen(new Point(0, 0));
            return new Rect(absolutePos.X, absolutePos.Y, visual.ActualWidth, visual.ActualHeight);
        }

    }
}
