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
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.Effects;
using System.Windows.Interop;
using Dicom;
using Dicom.Imaging;
namespace JelloX3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Object3DView : UserControl
    {
        private float Opacity = 0.5f;
        public double interval = 0.7;
        int picscale = 5;
        float WHratio;
        float ratio;
        public double mpp = 0.5;
        ModelVisual3D FrontModel;
        Model3DGroup BackModel;
        private List<MeshGeometry3D> FrontViews = new List<MeshGeometry3D>();
        private List<MeshGeometry3D> BackViews = new List<MeshGeometry3D>();
        public Object3DView(string[] filenames, int[] tiffindexs, int width, int height)
        {
            InitializeComponent();


            int layers = filenames.Length;
            float DataWidth = 0;
            float DataHeight = 0;
            List<ImageBrush> MaterialDataSource = new List<ImageBrush>();

            DataWidth = width;
            DataHeight = height;
            ratio = 1000 / DataWidth;
            DataWidth = ratio * DataWidth;
            DataHeight = ratio * DataHeight;
            WHratio = DataHeight / DataWidth;
            double zinterval = (picscale * 2) * (1.0 / 1000) * (interval / mpp) * ratio;
            for (int i = 0; i < layers; i++)
            {
                var rTf = new RotateTransform3D();
                var AAR = new AxisAngleRotation3D();
                AAR.Axis = new Vector3D(0, 1, 0);
                rTf.Rotation = AAR;
                var MG3D = new MeshGeometry3D();
                FrontViews.Add(MG3D);
                MG3D.Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
                MG3D.TextureCoordinates = new PointCollection() {
                    new System.Windows.Point(0,0),
                    new System.Windows.Point(0,1),
                    new System.Windows.Point(1,1),
                    new System.Windows.Point(1,0),
                };
                MG3D.TriangleIndices = new Int32Collection()
                {
                    0,
                    1,
                    2,
                    0,
                    2,
                    3,
                };

                var M = new DiffuseMaterial();
                if (System.IO.Path.GetExtension(filenames[i]).ToLower() == ".dcm")
                {
                    var dcmFile = DicomFile.Open(filenames[i]);
                    int dicomFrames = -1;
                    dcmFile.Dataset.TryGetValue<int>(DicomTag.NumberOfFrames, 0, out dicomFrames);

                    if (dicomFrames <= 0) dicomFrames = 1;
                    var frame = new DicomImage(dcmFile.Dataset, 0);
                    var DICOMBitmap = frame.RenderImage(0).AsClonedBitmap();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        DICOMBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        var DICOMImage = new BitmapImage();
                        DICOMImage.BeginInit();
                        DICOMImage.StreamSource = ms;
                        DICOMImage.CacheOption = BitmapCacheOption.OnLoad;
                        DICOMImage.EndInit();
                        DICOMImage.Freeze();
                        var mat = DICOMImage.ToMat();
                        Cv2.Resize(mat, mat, new OpenCvSharp.Size(DataWidth, DataHeight));
                        var gray = new Mat(mat.Size(), MatType.CV_8UC1);
                        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGRA2GRAY);
                        Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2BGRA);
                        var channels = mat.Split();
                        channels[3] = gray;
                        Cv2.Merge(channels, mat);
                        var bitmap = mat.ToBitmapSource();


                        MaterialDataSource.Add(new ImageBrush(bitmap));
                        M.Brush = MaterialDataSource.Last();
                    }
                }
                else
                {

                    using (FileStream fullImageStream = new FileStream(filenames[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var originalImage = new BitmapImage();

                        originalImage.BeginInit();

                        originalImage.StreamSource = fullImageStream;

                        originalImage.CacheOption = BitmapCacheOption.OnLoad;

                        originalImage.DecodePixelWidth = 1000;

                        originalImage.EndInit();
                        originalImage.Freeze();

                        var mat = originalImage.ToMat();
                        Cv2.Resize(mat, mat, new OpenCvSharp.Size(DataWidth, DataHeight));
                        var gray = new Mat(mat.Size(), MatType.CV_8UC1);
                        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGRA2GRAY);
                        Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2BGRA);
                        var channels = mat.Split();
                        channels[3] = gray;
                        Cv2.Merge(channels, mat);
                        var bitmap = mat.ToBitmapSource();


                        MaterialDataSource.Add(new ImageBrush(bitmap));
                        M.Brush = MaterialDataSource.Last();

                    }

                }
                M.Brush.Opacity = Opacity;
                var GM3 = new GeometryModel3D();
                GM3.Geometry = MG3D;
                GM3.Material = M;
                GM3.Transform = rTf;
                WorldModels.Children.Add(GM3);
            }

            BackModel = new Model3DGroup();
            BackModel.Transform = new TranslateTransform3D(0, 0, (layers - 1) * zinterval);
            WorldModels.Children.Add(BackModel);
            for (int i = 0; i < layers; i++)
            {

                var rTf_B = new RotateTransform3D();
                var AAR_B = new AxisAngleRotation3D();
                AAR_B.Angle = 180;
                AAR_B.Axis = new Vector3D(0, 1, 0);
                rTf_B.Rotation = AAR_B;
                var MG3D_B = new MeshGeometry3D();
                BackViews.Add(MG3D_B);
                MG3D_B.Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
                MG3D_B.TextureCoordinates = new PointCollection() {
                    new System.Windows.Point(1,0),
                    new System.Windows.Point(1,1),
                    new System.Windows.Point(0,1),
                    new System.Windows.Point(0,0),
                };
                MG3D_B.TriangleIndices = new Int32Collection()
                {
                    0,
                    1,
                    2,
                    0,
                    2,
                    3,
                };
                //var M_B = new EmissiveMaterial();
                var M_B = new DiffuseMaterial();
                M_B.Brush = MaterialDataSource[layers - i - 1];
                M_B.Brush.Opacity = Opacity;

                var GM3 = new GeometryModel3D();
                GM3.Geometry = MG3D_B;
                GM3.Material = M_B;
                GM3.Transform = rTf_B;
                BackModel.Children.Add(GM3);
            }

            KeyDown += Window_KeyDown;

        }
        
        public Object3DView(string _3DTifName)
        {
            InitializeComponent();
            FileStream tif3DStream = new FileStream(_3DTifName, FileMode.Open, FileAccess.Read, FileShare.Read);
            TiffBitmapDecoder decoder = new TiffBitmapDecoder(tif3DStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
            
            int layers = decoder.Frames.Count;
            float DataWidth = 0;
            float DataHeight = 0;
            List<ImageBrush> MaterialDataSource = new List<ImageBrush>();

            DataWidth = decoder.Frames[0].PixelWidth;
            DataHeight = decoder.Frames[0].PixelHeight;
            ratio = 1000 / DataWidth;
            DataWidth = ratio * DataWidth;
            DataHeight = ratio * DataHeight;
            WHratio = DataHeight / DataWidth;
            double zinterval = (picscale * 2) * (1.0 / 1000) * (interval / mpp);// *(648.0/3000);
            for (int i = 0; i < layers; i++)
            {
                var rTf = new RotateTransform3D();
                var AAR = new AxisAngleRotation3D();
                AAR.Axis = new Vector3D(0, 1, 0);
                rTf.Rotation = AAR;
                var MG3D = new MeshGeometry3D();
                FrontViews.Add(MG3D);
                MG3D.Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
                MG3D.TextureCoordinates = new PointCollection() {
                    new System.Windows.Point(0,0),
                    new System.Windows.Point(0,1),
                    new System.Windows.Point(1,1),
                    new System.Windows.Point(1,0),
                };
                MG3D.TriangleIndices = new Int32Collection()
                {
                    0,
                    1,
                    2,
                    0,
                    2,
                    3,
                };

                var M = new DiffuseMaterial();

                decoder.Frames[layers-1-i].Freeze();
                var mat = decoder.Frames[layers - 1 - i].ToMat();
                Cv2.Resize(mat, mat, new OpenCvSharp.Size(DataWidth, DataHeight));
                var gray = new Mat(mat.Size(), MatType.CV_8UC1);
                Cv2.CvtColor(mat, gray, ColorConversionCodes.BGRA2GRAY);
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2BGRA);
                var channels = mat.Split();
                channels[3] = gray;
                Cv2.Merge(channels, mat);
                var bitmap = mat.ToBitmapSource();
                MaterialDataSource.Add(new ImageBrush(bitmap));
                M.Brush = MaterialDataSource.Last();

                M.Brush.Opacity = Opacity;
                var GM3 = new GeometryModel3D();
                GM3.Geometry = MG3D;
                GM3.Material = M;
                GM3.Transform = rTf;
                WorldModels.Children.Add(GM3);
            }

            BackModel = new Model3DGroup();
            BackModel.Transform = new TranslateTransform3D(0, 0, (layers - 1) * zinterval);
            WorldModels.Children.Add(BackModel);
            for (int i = 0; i < layers; i++)
            {

                var rTf_B = new RotateTransform3D();
                var AAR_B = new AxisAngleRotation3D();
                AAR_B.Angle = 180;
                AAR_B.Axis = new Vector3D(0, 1, 0);
                rTf_B.Rotation = AAR_B;
                var MG3D_B = new MeshGeometry3D();
                BackViews.Add(MG3D_B);
                MG3D_B.Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
                MG3D_B.TextureCoordinates = new PointCollection() {
                    new System.Windows.Point(1,0),
                    new System.Windows.Point(1,1),
                    new System.Windows.Point(0,1),
                    new System.Windows.Point(0,0),
                };
                MG3D_B.TriangleIndices = new Int32Collection()
                {
                    0,
                    1,
                    2,
                    0,
                    2,
                    3,
                };
                var M_B = new DiffuseMaterial();
                M_B.Brush = MaterialDataSource[layers - i - 1];
                M_B.Brush.Opacity = Opacity;

                var GM3 = new GeometryModel3D();
                GM3.Geometry = MG3D_B;
                GM3.Material = M_B;
                GM3.Transform = rTf_B;
                BackModel.Children.Add(GM3);
            }

            KeyDown += Window_KeyDown;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
                
        }
        private void OnFrame(object sender, EventArgs e)
        {
            
        }
        bool isXAxis = true;
        System.Windows.Vector vec= new System.Windows.Vector(0, 0);
        private void World_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                vec = e.GetPosition((IInputElement)sender) - down;
                
                ccamera.Position = new Point3D(oldPosX - vec.X/200, oldPosY + vec.Y / 200, z_distance);

            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                vec = e.GetPosition((IInputElement)sender) - Angledown;
                var Xro = new RotateTransform3D();
                var Yro = new RotateTransform3D();
                Xro.Rotation = new AxisAngleRotation3D(new Vector3D(0,1,0), (oldAngleX + vec.X) % 360);
                Yro.Rotation = new AxisAngleRotation3D(new Vector3D(1,0,0), (oldAngleY + vec.Y) % 360);
                
                LTransGroup.Children.RemoveAt(LTransGroup.Children.Count-1);
                LTransGroup.Children.RemoveAt(LTransGroup.Children.Count-1);
                LTransGroup.Children.Add(Xro);
                LTransGroup.Children.Add(Yro);

            }
        }
        System.Windows.Point down = new System.Windows.Point();
        TranslateTransform3D oldPos = new TranslateTransform3D();
        double oldPosX = 0;
        double oldPosY = 0;
        private void World_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ppp = e.GetPosition((IInputElement)sender);
            down = new System.Windows.Point(ppp.X, ppp.Y);
        }

        private void World_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                z_distance = ccamera.Position.Z - 1;
            }
            else
            {
                z_distance = ccamera.Position.Z + 1;
            }
            ccamera.Position = new Point3D(ccamera.Position.X, ccamera.Position.Y, z_distance);
        }

        double oldAngleX = 0;
        double oldAngleY = 0;
        double oldAngleZ = 0;
        System.Windows.Point Angledown = new System.Windows.Point();
        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ppp = e.GetPosition((IInputElement)sender);
            Angledown = ppp;
        }
            
        private double z_distance = 15;
        private void World_Loaded(object sender, RoutedEventArgs e)
        {
            ccamera.Position = new Point3D(z_distance * Math.Sin(Math.PI * ((0 * 0.45) / 360)), 0, z_distance * Math.Cos(Math.PI * ((0 * 0.45) / 360)));
            double total = Math.Abs(ccamera.Position.X) + Math.Abs(ccamera.Position.Z);
            ccamera.LookDirection = new Vector3D(-1.0f * ccamera.Position.X / total, 0, -1.0f * ccamera.Position.Z / total);

        }
            
        public void ChangeInterval(double newInterval)
        {
            interval = newInterval;
            Console.WriteLine(interval);

            double zinterval = (picscale * 2) * (1.0 / 1000) * (interval / mpp) * ratio;// *(648.0/3000);
            BackModel.Transform = new TranslateTransform3D(0, 0, (FrontViews.Count - 1) * zinterval);
            for (int i = 0; i < FrontViews.Count; i++)
            {

                FrontViews[i].Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
                BackViews[i].Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
            }
        }
        public void ChangeMPP(double newmpp)
        {
            mpp = newmpp;
            double zinterval = (picscale*2)*(1.0 / 1000) * (interval / mpp) * ratio;
            BackModel.Transform = new TranslateTransform3D(0, 0, (FrontViews.Count - 1) * zinterval);
            for (int i = 0; i < FrontViews.Count; i++)
            {

                FrontViews[i].Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale ,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
                BackViews[i].Positions = new Point3DCollection(new Point3DCollection() {
                    new Point3D(-1*picscale,1*picscale*WHratio,i*zinterval),
                    new Point3D(-1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,-1*picscale*WHratio,i*zinterval),
                    new Point3D(1*picscale,1*picscale*WHratio,i*zinterval),
                });
            }
        }
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            oldPosX = ccamera.Position.X;
            oldPosY = ccamera.Position.Y;
        }
        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            oldAngleX = (oldAngleX + vec.X + 360) % 360;
            oldAngleY = (oldAngleY + vec.Y + 360) % 360;
        }
    }
}

