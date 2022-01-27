using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaLite_Viewer.SubUnit;
using System.Windows;
using wpfpslib;
using System.ComponentModel;
using System.Threading;
using MetaLite_Viewer.Helper;

namespace MetaLite_Viewer
{
    class Resource
    {
        public static List<ImageData> ImageDatas { get; } = new List<ImageData>();

        public static MainWindow MainWindow { get; set; }

        public static AnnotationSelector AnnotationSelector { get; set; } = new AnnotationSelector();

        public static ChannelMaskEffect RGBEffect { get; set; } = new ChannelMaskEffect() { R = 1.0, G = 1.0, B = 1.0, RIndex = 0, GIndex = 1, BIndex = 2 };

        public static IHC IHCEffect { get; set; } = new IHC() { HematoxylinIndex = 2, DabIndex = 1, BackgroundIndex = 0 };

        public static HE HEEffect { get; set; } = new HE() { HematoxylinIndex = 2, EosinIndex = 0, BackgroundIndex = 1 };

        public static PseudoStaining PseudoStainingEffect { get; set; } = new PseudoStaining()
        {
            HematoxylinIndex = 0,
            EosinIndex = 2,
            DabIndex = 1,
            EosinIntensity = 1,
            HematoxylinIntensity = 1,
            DabIntensity = 1,
            HematoxylinColor = new Color() { R = 85, G = 0, B = 136, A = 255 },
            EosinColor = new Color() { R = 255, G = 136, B = 196, A = 255 },
            DabColor = new Color() { R = 98, G = 40, B = 30, A = 255 },
        };

        public static ChannelPanel ChannelSelector { get; set; }

        public static ImageData nowImageData
        {
            get
            {
                if (SelectedImageIndex > ImageDatas.Count - 1 || SelectedImageIndex < 0)
                    return null;
                return ImageDatas[SelectedImageIndex];
            }
        }

        public volatile static bool UpdateScreenLock = false;

        public static Model.AnnotationLayer nowAnnotationLayer
        {
            get
            {
                return nowImageData.AnnotationLayers[nowImageData.SelectedLayerIndex];
            }
        }


        public static WriteableBitmap WriteableBitmapCursor { get; set; }
        public static WriteableBitmap WriteableBitmapContourCursor { get; set; }
        public static WriteableBitmap WriteableBitmapContour { get; set; }
        public static WriteableBitmap WriteableBitmapView { get; set; }
        public static byte[] LoadedView { get; set; }
        public static byte[] View = new byte[0];
        public static Stack<ViewBuffer> DelayViewBuffer = new Stack<ViewBuffer>();
        public static Dictionary<long, SvsTile> SvsCacheTile = new Dictionary<long, SvsTile>(200);
        public static SvsTile[,] SvsCacheTile_;
        public static int ViewWidth = -1;
        public static int ViewHeight = -1;

        private static int selectedImageIndex;
        public static int SelectedImageIndex
        {
            get { return selectedImageIndex; }
            set
            {
                if (value >= 0)
                    selectedImageIndex = value;
                else
                    selectedImageIndex = 0;
            }
        }

        public class ViewBuffer
        {
            public long TimeStamp = 0;
            public byte[] Data = new byte[0];
            public ViewBuffer(long timeStamp, byte[] data)
            {
                TimeStamp = timeStamp;
                Data = data;
            }
        }

        public class DelayViewerWorker
        {
            public long TimeStamp = 0;
            public BackgroundWorker Worker = new BackgroundWorker();
            public DelayViewerWorker(long timeStamp, BackgroundWorker worker)
            {
                TimeStamp = timeStamp;
                Worker = worker;
            }
        }
        public static void FromArray()
        {
            try
            {
                if (View.Length == ViewWidth* ViewHeight*4)
                    WriteableBitmapView.WritePixels(new Int32Rect(0, 0, ViewWidth, ViewHeight), View, 4 * ViewWidth, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static bool isLayerExist
        {
            get
            {
                if (ImageDatas.Count > 0)
                {
                    if (nowImageData.AnnotationLayers.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;

            }
        }

        public static bool isDataReaded
        {
            get
            {
                if (ImageDatas.Count > 0)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        public static double progressBarMax { get; set; }
        /*****
         * Free 
         * Lock
         */
        public enum ImageType
        {
            Normal,
            HugeTif,
            Tif3D,
            WSI,
            DICOM,
            DICOM3D,
            ISyntax,
        }
    }
}
