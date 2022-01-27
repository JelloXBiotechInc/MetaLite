using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MetaLite_Viewer.Helper
{
    class ColorHelper
    {
        public static Color HIGHLIGHTER { get; set; } = Color.FromArgb(byte.Parse(ConfigurationManager.AppSettings["DEFAULT_ALPHA"], NumberFormatInfo.InvariantInfo), 180, 255, 160);
        public static Color ERASER { get; set; } = Colors.Transparent;

        public static Color GetColorByProbability()
        {
            Random rnd = new Random();
            byte red = (byte)(MathHelper.Clip( rnd.Next() % 256, 0, 255));
            byte green = (byte)(MathHelper.Clip((rnd.Next()+ red)%256, 0, 255-red));
            byte blue = (byte)(MathHelper.Clip((rnd.Next() + red + green) % 256, 0, 255-(red+green)));
            return Color.FromArgb(
                byte.Parse(ConfigurationManager.AppSettings["DEFAULT_ALPHA"], NumberFormatInfo.InvariantInfo),
                red,
                green,
                blue);
        }
        public static int IntHIGHLIGHTER
        {
            get
            {
                int colorcode = 0;
                colorcode = (colorcode | byte.Parse(ConfigurationManager.AppSettings["DEFAULT_ALPHA"], NumberFormatInfo.InvariantInfo)) << 8;
                colorcode = (colorcode | 255) << 8;
                colorcode = (colorcode | 255) << 8;
                colorcode = (colorcode | 0);
                return colorcode;
            }
        }

        public static SolidColorBrush XamlStringColor(string colorString)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(colorString);                        
        }

        public static int IntColorFromARGB(byte alpha, byte red, byte green, byte blue)
        {
            int outInt = 0;
            outInt = (outInt | alpha) << 8;
            outInt = (outInt | red) << 8;
            outInt = (outInt | green) << 8;
            outInt = (outInt | blue);
            return outInt;
        }
    }
}
