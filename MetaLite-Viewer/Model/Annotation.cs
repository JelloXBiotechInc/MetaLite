using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLite_Viewer
{
    class Annotation
    {
        public Annotation(int x, int y, int width, int height, double softmax)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Softmax = softmax;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Softmax { get; set; }
    }
}
