using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Windows;
using System.IO;
namespace MetaLite_Viewer.Model
{
    public static class SnapShot
    {
        private enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static byte[] GetBitmap(Visual visual, System.Windows.Point PointXY, int width, int height)
        {
            IntPtr source;
            IntPtr destination;
            byte[] pixels = new byte[5000 * 5000 * 3];
            using (Bitmap bitmap = new Bitmap(width, height))
            {

                HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(visual);
                using (Graphics graphicsFromVisual = Graphics.FromHwnd(hwndSource.Handle))
                {
                    using (Graphics graphicsFromImage = Graphics.FromImage(bitmap))
                    {

                        source = graphicsFromVisual.GetHdc();
                        destination = graphicsFromImage.GetHdc();

                        BitBlt(destination, 0, 0, bitmap.Width, bitmap.Height, source, (int)0, (int)0, TernaryRasterOperations.SRCCOPY);

                        graphicsFromVisual.ReleaseHdc(source);
                        graphicsFromImage.ReleaseHdc(destination);
                    }
                }
                Console.WriteLine( bitmap.GetPixel(width / 2, width / 2));
                IntPtr hBitmap = bitmap.GetHbitmap();

                BitmapImage result = new BitmapImage();
                using (var fs = new FileStream(@"e:\snap.png", FileMode.Create, FileAccess.Write))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {

                        bitmap.Save(stream, ImageFormat.Png);



                        result.BeginInit();
                        // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                        // Force the bitmap to load right now so we can dispose the stream.
                        result.CacheOption = BitmapCacheOption.OnLoad;
                        result.StreamSource = stream;
                        result.DecodePixelWidth = width;
                        result.DecodePixelHeight = height;
                        result.EndInit();
                        result.Freeze();
                        var convertedBitmap = new FormatConvertedBitmap(result, PixelFormats.Bgr24, null, 0);
                        convertedBitmap.CopyPixels(pixels, 5000 * 3, 0);
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(convertedBitmap));
                        encoder.Save(stream);
                        stream.Position = 0;
                        stream.CopyTo(fs);
                    }
                }
                
                DeleteObject(hBitmap);
            }
            
            return pixels;
        }
    }
}
