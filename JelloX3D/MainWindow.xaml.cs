using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using wpfpslib;
using OpenCvSharp;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
namespace JelloX3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private DispatcherTimer _frameTimer;

        private static int w = 3840; // 16的倍數
        private static int h = 2160;

        private static Queue<byte[][]> pixels_Q = new Queue<byte[][]>();
        private bool baloaded = false;
        private bool riloaded = false;
        public MainWindow()
        {

            InitializeComponent();
            Content();
        }
        int functionButton = 0;
        int FunctionButton {
            get { return functionButton; }
            set
            {
                functionButton = value;
                if (functionButton == 0)
                {

                }else if(functionButton == 1)
                {

                }
                else
                {

                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Content()
        {
            BackgroundWorker back = new BackgroundWorker();
            back.DoWork += (s, e) =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                }
            };
            back.RunWorkerAsync();
        }
    }
}
