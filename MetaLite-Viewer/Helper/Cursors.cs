using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace MetaLite_Viewer.Helper
{
    public static class CursorsExtensions
    {
        private static Cursor _grab;
        public static Cursor Grab
        {
            get
            {
                if (_grab == null)
                {
                    StreamResourceInfo sri = Application.GetResourceStream(new Uri("Image/Cursor/openhand.cur", UriKind.Relative));
                    if (sri != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        sri.Stream.CopyTo(ms);
                        ms.Position = 0;
                        _grab = new Cursor(ms, true);
                    }
                }
                return _grab;
            }
        }

        private static Cursor _grabbing;
        public static Cursor Grabbing
        {
            get
            {
                if (_grabbing == null)
                {
                    StreamResourceInfo sri = Application.GetResourceStream(new Uri("Image/Cursor/closehand.cur", UriKind.Relative));
                    if (sri != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        sri.Stream.CopyTo(ms);
                        ms.Position = 0;
                        _grabbing = new Cursor(ms, true);
                    }
                }
                return _grabbing;
            }
        }
        private static RenderTargetBitmap RenderCursor(double rx, double ry, SolidColorBrush brush, Pen pen)
        {
            var vis = new DrawingVisual();
            double bias = (pen.Thickness+1)/2;
            using (var dc = vis.RenderOpen())
            {
                dc.DrawEllipse(brush, new Pen(ColorHelper.XamlStringColor("#88000000"), 2), new Point(rx + pen.Thickness + 1, ry + pen.Thickness + 1), System.Math.Max(rx + bias, 1), System.Math.Max(ry + bias, 1));
                DashStyle dashStyle = new DashStyle();
                dashStyle.Offset = 0;
                dashStyle.Dashes = new DoubleCollection();
                dashStyle.Dashes.Add(1);
                dashStyle.Dashes.Add(3);
                pen.DashStyle = dashStyle;
                dc.DrawEllipse(brush, pen, new Point(rx + pen.Thickness + 1, ry + pen.Thickness + 1), System.Math.Max(rx + bias, 1), System.Math.Max(ry + bias, 1));
                dc.Close();
            }
            var rtb = new RenderTargetBitmap((int)((rx + pen.Thickness+1) *2+1), (int)((ry + pen.Thickness+1) * 2 +1), 96, 96, PixelFormats.Pbgra32);

            rtb.Render(vis);
            return rtb;
        }

        public static WriteableBitmap WriteableBitmapCursor(double rx, double ry, SolidColorBrush brush, Pen pen)
        {
            var rtb = RenderCursor(rx, ry, brush, pen);
            var CreateBitmapFrame = new WriteableBitmap(BitmapFrame.Create(rtb));
            CreateBitmapFrame.Freeze();
            return CreateBitmapFrame;
        }        

        public static Cursor CreateCursor(double rx, double ry, SolidColorBrush brush, Pen pen)
        {
            var rtb = RenderCursor(rx, ry, brush, pen);
            using (var ms1 = new MemoryStream())
            {
                var penc = new PngBitmapEncoder();
                var CreateBitmapFrame = BitmapFrame.Create(rtb);
                penc.Frames.Add(CreateBitmapFrame);
                penc.Save(ms1);

                var pngBytes = ms1.ToArray();
                var size = pngBytes.GetLength(0);
                

                //.cur format spec http://en.wikipedia.org/wiki/ICO_(file_format)
                using (var ms = new MemoryStream())
                {
                    {//ICONDIR Structure
                        ms.Write(BitConverter.GetBytes((Int16)0), 0, 2);//Reserved must be zero; 2 bytes
                        ms.Write(BitConverter.GetBytes((Int16)2), 0, 2);//image type 1 = ico 2 = cur; 2 bytes
                        ms.Write(BitConverter.GetBytes((Int16)1), 0, 2);//number of images; 2 bytes
                    }

                    {//ICONDIRENTRY structure
                        ms.WriteByte(32); //image width in pixels
                        ms.WriteByte(32); //image height in pixels

                        ms.WriteByte(0); //Number of Colors in the color palette. Should be 0 if the image doesn't use a color palette
                        ms.WriteByte(0); //reserved must be 0
                        ms.Write(BitConverter.GetBytes((Int16)(rx+ pen.Thickness+2)), 0, 2);//2 bytes. In CUR format: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
                        ms.Write(BitConverter.GetBytes((Int16)(ry+ pen.Thickness+2)), 0, 2);//2 bytes. In CUR format: Specifies the vertical coordinates of the hotspot in number of pixels from the top.
                        
                        ms.Write(BitConverter.GetBytes(size), 0, 4);//Specifies the size of the image's data in bytes
                        ms.Write(BitConverter.GetBytes((Int32)22), 0, 4);//Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                    }

                    ms.Write(pngBytes, 0, size);//write the png data.
                    ms.Seek(0, SeekOrigin.Begin);

                    return new Cursor(ms);
                }
            }
        }
    }

    public static class CursorsBuilder
    {
        
    }
}
