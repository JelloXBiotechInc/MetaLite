using MetaLite_Viewer.Helper;
using MetaLite_Viewer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Point = System.Windows.Point;
using ColorConverter = System.Windows.Media.ColorConverter;
using Color = System.Windows.Media.Color;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell;
using MetaLite_Viewer.Subwindow;
using System.Security.Permissions;
using Windows.UI.StartScreen;
using System.Windows.Data;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using System.Windows.Media.TextFormatting;
using System.Runtime.InteropServices;
using OpenSlideNET;
using System.Web.UI.WebControls;
using System.Windows.Media.Animation;
using System.Drawing.Imaging;
using System.Security.AccessControl;
using OpenCvSharp.WpfExtensions;
using Dicom;
using Dicom.Imaging;

namespace MetaLite_Viewer
{
	class ImageData : IDisposable, INotifyPropertyChanged
	{
		public FileStream OriginalImageFileStream { get; private set; }
		private const int OPENSLIDEWSI = -2;
		private const int DICOMWSI = -3;
		public WriteableBitmap resizedImage { get; set; }
		public Resource.ImageType ImageType;
		public BitmapSource ResizedImage
		{
			get
			{

				if (ThumbnailReady == 1)
					return resizedImage as BitmapSource;
				else
				{
					var tmp = FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4);
					tmp.Freeze();
					return tmp as BitmapSource;
				}


			}
			
		}
		public byte[] resizedImageBytes { get; set; }
		private volatile double[] TimeStamp = { (DateTime.Now.AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds };
		public BitmapSource ToShowImage
		{
			get
			{
				if (OriginImage == null)
				{
					return ResizedImage;
				}
				else
					return OriginImage;
			}
		}

		private BitmapSource originImage;
		public BitmapSource? OriginImage
		{
			get
			{
				return originImage;
			}
			set
			{
				originImage = value;
				OnPropertyChanged(nameof(OriginImage));
				OnPropertyChanged(nameof(ToShowImage));
			}
		}
		private Mat oriMat;
		public Mat OriMat
		{
			get
			{
				return oriMat;
			}
			private set
			{
				oriMat = value;
				OnPropertyChanged(nameof(OriMat));
			}
		}

		public string recolorType { get; private set; }
		public ObservableCollection<AnnotationLayer> AnnotationLayers { get; private set; } = new ObservableCollection<AnnotationLayer>();
		public double LINE_WIDTH { get { return System.Math.Max(1, DataWidth / 800.0); } }
		public double POINT_WIDTH { get { return LINE_WIDTH * 2; } }

		public double ObjectCounting { get; set; } = double.NegativeInfinity;

		public int DataWidth;
		public int DataHeight;
		public static int TileWidth = 1;
		public static int TileHeight = 1;

		public IEnumerable<(long Width, long Height)> LevelDimensions;

		private int selectedLayerIndex { get; set; }
		public int SelectedLayerIndex
		{
			get
			{
				
				if (AnnotationLayers.Count == 0)
					selectedLayerIndex = -1;
				else
					selectedLayerIndex = MathHelper.Clip(selectedLayerIndex, 0, AnnotationLayers.Count - 1);
				return selectedLayerIndex;
			}
			set
			{
				if (value >= 0)
				{
					selectedLayerIndex = value;
				}

				if (AnnotationLayers.Count <= 0)
				{
					Resource.WriteableBitmapContour.Clear(Colors.Transparent);
					selectedLayerIndex = -1;
				}
			}
		}

		public int AnnotationWidth { get { return (int)(DataWidth / AnnotationScale); } }
		public int AnnotationHeight { get { return (int)(DataHeight / AnnotationScale); } }
		public double AnnotationScale { get; set; }
		public double AnnotationRatio { get; set; }
		public AIModule AIModule { get; private set; }
		public string Filename { get; set; }
		public string FilenameWithAnnotation
		{
			get
			{
				string[] paths = Dir.Split('\\');
				// The first underline will be eat by showing the name of file on the title board.
				return (IsAnnotationDirty ? "*" : "") + "_" + paths[paths.Length - 1] + " (" + System.IO.Path.GetFileName(AnnotationFilePrefix) + ")";
			}
		}


		public int ThumbnailReady = 0; // 0: not ready, 1: is ready and default ready, -1: is huge reading
		private string _annotationFilePrefix;
		public string AnnotationFilePrefix
		{
			get
			{
				return _annotationFilePrefix;
			}
			private set
			{
				_annotationFilePrefix = value;
				OnPropertyChanged(nameof(AnnotationFilePrefix));
				OnPropertyChanged(nameof(FilenameWithAnnotation));
			}
		}

		private bool isAnnotationDirty = false;
		public bool IsAnnotationDirty
		{
			get
			{
				return isAnnotationDirty;
			}
			set
			{
				isAnnotationDirty = value;
				OnPropertyChanged(nameof(FilenameWithAnnotation));
			}
		}
		private static readonly object locker = new object();
		
		public bool isHugeImage = false;
		public int hugeTileSize = 4096;
		public int ThumbnailWidth = 1024;
		public int ThumbnailHeight = 1024;
		public byte[,][] hugeData = new byte[TileWidth, TileHeight][];
		public byte[] TN;
		public string Dir { get; set; }
		private byte BitDepth;
		public int ImageIndex = -1;
		public ThreadPriority LoadPriority = ThreadPriority.Lowest;
		// normal image
		public ImageData(string dir, int imageIndex, Resource.ImageType imageType)
		{
			ImageType = imageType;
			switch (ImageType)
			{
				case Resource.ImageType.Normal:
					NormalImage(dir, imageIndex);
					break;
				case Resource.ImageType.WSI:
					WSIImage(dir, OPENSLIDEWSI);
					break;
				default:
					break;
			}			
				
		}
		public ImageData(BitmapSource bitmapFrame, string dir, int imageIndex, Resource.ImageType imageType)
		{
			ImageType = imageType;
			switch (ImageType)
			{
				case Resource.ImageType.Tif3D:
					Tif3D(bitmapFrame, dir, imageIndex);
					break;
				case Resource.ImageType.DICOM3D:
					Tif3D(bitmapFrame, dir, imageIndex);
					break;
				default:
					break;
			}

		}
		private void NormalImage(string dir, int imageIndex) 
		{ 
			Dir = dir;
			ImageIndex = imageIndex;

			using (ShellObject picture = ShellObject.FromParsingName(Dir))
			{
				if (picture != null)
				{
					var raw_w = picture.Properties.GetProperty(SystemProperties.System.Image.HorizontalSize);
					var raw_h = picture.Properties.GetProperty(SystemProperties.System.Image.VerticalSize);
					var raw_bitdepth = picture.Properties.GetProperty(SystemProperties.System.Image.BitDepth);

					long getSize_Width = long.Parse(raw_w.ValueAsObject.ToString());
					long getSize_Height = long.Parse(raw_h.ValueAsObject.ToString());
					BitDepth = byte.Parse(raw_bitdepth.ValueAsObject.ToString());
					if (23170 * 23170 < getSize_Width * getSize_Height)
					{
						isHugeImage = true;
						ImageType = Resource.ImageType.HugeTif;
						AnnotationRatio = Math.Sqrt((double)(10000 * 10000) / (getSize_Width * getSize_Height));
						AnnotationScale = 1.0 / AnnotationRatio;
						if (imageIndex == 0)
						{
							Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
							{
								messagebox.Show("The image size is " + getSize_Width + "x" + getSize_Height + " is too large,\nand exceeds the limit of 536,870,912 pixels.\nRescale the annotation to ratio " + AnnotationRatio,
									"Warning", MessageBoxButton.OK, MessageBoxImage.Information);
							}));
						}
					}
					else
					{
						AnnotationScale = 1;
						AnnotationRatio = 1.0 / AnnotationScale;

					}
					DataWidth = (int)getSize_Width;
					DataHeight = (int)getSize_Height;
				}
			}

			TileWidth = (int)Math.Ceiling((double)DataWidth / hugeTileSize);
			TileHeight = (int)Math.Ceiling((double)DataHeight / hugeTileSize);
			hugeData = new byte[TileWidth, TileHeight][];
			if (DataWidth < ThumbnailWidth)
				ThumbnailWidth = DataWidth;
			ThumbnailHeight = (int)((double)ThumbnailWidth * DataHeight / DataWidth);
			TN = new byte[ThumbnailWidth * ThumbnailHeight * 4];

			if (isHugeImage && ImageIndex == 0)
			{
				resizedImage = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));

				ParallelReadData(true);
			}
			else
			{
				using (FileStream fullImageStream = new FileStream(Dir, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var originalImage = new BitmapImage();
					originalImage.BeginInit();

					originalImage.StreamSource = fullImageStream;

					originalImage.CacheOption = BitmapCacheOption.OnDemand;
					if (DataWidth > ThumbnailWidth)
						originalImage.DecodePixelWidth = ThumbnailWidth;
					originalImage.EndInit();
					originalImage.Freeze();
					var resizeMat = (originalImage).ToMat();

					if (DataWidth > ThumbnailWidth)
						resizeMat = resizeMat.Resize(new OpenCvSharp.Size(ThumbnailWidth, ThumbnailHeight));

					var resizeBitmap = resizeMat.ToBitmapSource();
					resizedImage = new WriteableBitmap(resizeBitmap);
					resizedImage.Freeze();
					ThumbnailReady = 1;
				}

			}
			while (running.Count != 0)
			{
				var tmp = running.Dequeue();
				if (tmp.IsBusy)
					running.Enqueue(tmp);
			}
			AIModule = new HHAIModule(AnnotationWidth, AnnotationHeight, this);
			var filePrefix = new FileInfo(Dir).Directory.FullName + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Dir);
			AnnotationFilePrefix = filePrefix + ".jellox";

			if (File.Exists(AnnotationFilePrefix))
			{
				LoadAnnotation(AnnotationFilePrefix);
			}
			else
			{
				IsAnnotationDirty = false;
			}

		}
		// OpenSlide WSI
		private OpenSlideImage Slide;
		private int nowSvsLevel = -1;
		private double nowZoom = 1;
		private double nowImageActualWidth = -1;
		private double nowImageActualHeight = -1;
		private double nowScrollViewer_Width = -1;
		private double nowScrollViewer_Height = -1;
		private double nowScrollViewer_HorizontalOffset = -1;
		private double nowScrollViewer_VerticalOffset = -1;
		private string svsTempDir;
		int wsiTileSize = 512;
		DeepZoomGenerator? initDz = null;
		public void WSIImage(string dir, int imageIndex)
		{
			Dir = dir;
			ImageIndex = imageIndex;
			tifIndex = OPENSLIDEWSI; // Svs code for PreLoad() function
			ThumbnailWidth = ThumbnailWidth * 4;
			svsTempDir = Dir;
			
			Slide = OpenSlideImage.Open(svsTempDir);
			initDz = new DeepZoomGenerator(Slide, tileSize: wsiTileSize, overlap: 1);
			{
				var dz = new DeepZoomGenerator(Slide, tileSize: 256, overlap: 0, limitBounds: true);
				LevelDimensions = dz.LevelDimensions;
				int itr = 0;
				
				

				foreach (var wh in LevelDimensions)
				{
					var tileInfo = new DeepZoomGenerator.TileInfo();
					dz.GetTile(level: itr, 0, 0, out tileInfo);
					
					itr++;
				}
				isHugeImage = true;
				if (Slide != null)
				{
					
					long getSize_Width = Slide.Width;
					long getSize_Height = Slide.Height;
					if (23170 * 23170 < getSize_Width * getSize_Height)
					{
						

						AnnotationRatio = Math.Sqrt((double)(10000 * 10000) / (getSize_Width * getSize_Height));
						AnnotationScale = 1.0 / AnnotationRatio;
						
						Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
						{
							messagebox.Show("The image size is " + getSize_Width + "x" + getSize_Height + " is too large,\nand exceeds the limit of 536,870,912 pixels.\nRescale the annotation to ratio " + AnnotationRatio,
								"Warning", MessageBoxButton.OK, MessageBoxImage.Information);
						}));
						

					}
					else
					{
						AnnotationScale = 1;
						AnnotationRatio = 1.0 / AnnotationScale;

					}
					DataWidth = (int)getSize_Width;
					DataHeight = (int)getSize_Height;

				}

				int level = 0;
				double svsWidth = DataWidth;
				while (svsWidth > ThumbnailWidth)
				{
					svsWidth = svsWidth / 2;
					level++;
				}
				Stream s = new MemoryStream();
				Slide.GetThumbnailAsJpegToStream(ThumbnailWidth, s);
				
				resizedImage = new WriteableBitmap((new Bitmap(s)).ToBitmapSource());
				resizedImageBytes = resizedImage.ToByteArray();
				resizedImage.Freeze();
				Console.WriteLine("ch"+resizedImageBytes.Length / resizedImage.PixelWidth / resizedImage.PixelHeight);
				ThumbnailReady = 1;
				
			}
			AIModule = new HHAIModule(AnnotationWidth, AnnotationHeight, this);
			
			var filePrefix = new FileInfo(Dir).Directory.FullName + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Dir);
			AnnotationFilePrefix = filePrefix + ".jellox";
			
			if (File.Exists(AnnotationFilePrefix))
			{
				LoadAnnotation(AnnotationFilePrefix);
			}
			else
			{
				IsAnnotationDirty = false;
			}
		}
		public int tifIndex = -1;
		public int TifIndex
		{
			get
			{
				return tifIndex;
			}
		}
		// 3D Tiff
		public void Tif3D(BitmapSource source, string dir, int imageIndex)
		{
			tifIndex = imageIndex;

			Dir = dir;

			DataWidth = (int)source.Width;
			DataHeight = (int)source.Height;
			TileWidth = (int)Math.Ceiling((double)DataWidth / hugeTileSize);
			TileHeight = (int)Math.Ceiling((double)DataHeight / hugeTileSize);
			hugeData = new byte[TileWidth, TileHeight][];

			if (DataWidth < ThumbnailWidth)
				ThumbnailWidth = DataWidth;
			ThumbnailHeight = (int)((double)ThumbnailWidth * DataHeight / DataWidth);
			AnnotationScale = 1;
			AnnotationRatio = 1.0 / AnnotationScale;
			
			TN = new byte[ThumbnailWidth * ThumbnailHeight * 4];
			if (isHugeImage && imageIndex == 0)
			{
				ThumbnailReady = -1;
				ParallelReadData3D(imageIndex, true);

			}
			else
			{
				var convertedBitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
				var resizeMat = convertedBitmap.ToMat();

				if (DataWidth > ThumbnailWidth)
					resizeMat = resizeMat.Resize(new OpenCvSharp.Size(0, 0), (double)ThumbnailWidth / DataWidth, (double)ThumbnailWidth / DataWidth);

				var resizeBitmap = resizeMat.ToBitmapSource();
				resizedImage = new WriteableBitmap(resizeBitmap);
				resizedImage.Freeze();
				ThumbnailReady = 1;
			}
			
			while (running.Count != 0)
			{
				var tmp = running.Dequeue();
				if (tmp.IsBusy)
					running.Enqueue(tmp);
			}

			AIModule = new HHAIModule(AnnotationWidth, AnnotationHeight, this);

			Filename = System.IO.Path.GetFileName(dir);
			var filePrefix = new FileInfo(Filename).Directory.FullName + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Filename);
			AnnotationFilePrefix = filePrefix + "_" + (tifIndex + 1).ToString() + ".jellox";

			string filePath = System.IO.Path.GetDirectoryName(Dir) + System.IO.Path.DirectorySeparatorChar +
					System.IO.Path.GetFileNameWithoutExtension(Dir) + System.IO.Path.DirectorySeparatorChar +
					System.IO.Path.GetFileName(AnnotationFilePrefix);

			if (File.Exists(filePath))
			{
				LoadAnnotation(filePath);
			}
			else
			{
				IsAnnotationDirty = false;
			}
		}

		public static BitmapSource FromArray(byte[] data, int w, int h, int ch)
		{
			System.Windows.Media.PixelFormat format = PixelFormats.Default;

			if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
			if (ch == 3) format = PixelFormats.Bgr24; //RGB
			if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


			WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
			wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

			return wbm;
		}

		public void PreLoad()
		{
			if (ImageType == Resource.ImageType.Tif3D)  // If TIF index exist means the file type is 3D TIF.
			{
				if (OriginImage == null)
				{
					using (var cacheFileStream = new FileStream(Dir, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						try
						{
							TiffBitmapDecoder decoder = new TiffBitmapDecoder(cacheFileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);

							var convertedBitmap = new FormatConvertedBitmap(decoder.Frames[tifIndex], PixelFormats.Bgra32, null, 0);
							var oriImageMat = (convertedBitmap).ToMat();
							var oriImageBitmapSource = oriImageMat.ToBitmapSource();
							oriImageBitmapSource.Freeze();
							OriginImage = oriImageBitmapSource;
							cacheFileStream.Close();
							cacheFileStream.Dispose();
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}
						finally
						{
							cacheFileStream.Close();
							cacheFileStream.Dispose();
						}
					}
				}
			}else if (ImageType == Resource.ImageType.DICOM3D)
			{
				if (OriginImage == null)
				{
					var dcmFile = DicomFile.Open(Dir);
					var dicomframe = new DicomImage(dcmFile.Dataset, 0);
					var bitmap = dicomframe.RenderImage(0).AsClonedBitmap();
					using (MemoryStream ms = new MemoryStream())
					{
						bitmap.Save(ms, ImageFormat.Png);
						var frame = new BitmapImage();
						frame.BeginInit();
						frame.StreamSource = ms;
						frame.CacheOption = BitmapCacheOption.OnLoad;
						frame.EndInit();
						frame.Freeze();
						OriginImage = frame;
					}					
				}
			}
			else if (ImageType == Resource.ImageType.Normal) // If TIF index exist means the file type is other image type or one frame TIF.
			{
				if (OriginImage == null && !isHugeImage)
				{

					using (var cacheFileStream = new FileStream(Dir, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var originalImage = new BitmapImage();

						originalImage.BeginInit();

						originalImage.StreamSource = cacheFileStream;
						originalImage.CacheOption = BitmapCacheOption.OnLoad;
						originalImage.EndInit();
						originalImage.Freeze();


						OriginImage = (BitmapSource)originalImage;
					}
				}
				else if (isHugeImage && CachedTiles.Count != hugeData.Length)
				{
					ParallelReadData(false);
				}
			}else if (ImageType == Resource.ImageType.DICOM)
			{

			}
		}
		private Queue<int> CachedTiles = new Queue<int>();
		public Queue<BackgroundWorker> running = new Queue<BackgroundWorker>();
		public List<BackgroundWorker> owo = new List<BackgroundWorker>();
		private void ReadData()
		{
			BackgroundWorker bk = new BackgroundWorker();
			bk.DoWork += (s, e) =>
			{

				Thread.CurrentThread.Priority = ThreadPriority.Lowest;
				Cached = true;
				for (int i = 0; i < TileWidth; i++)
				{
					int localI = i;
					if (!Cached) { Console.WriteLine("can"); return; };

					for (int j = 0; j < TileHeight; j++)
					{
						if (!Cached) { Console.WriteLine("can"); return; };
						int localJ = j;


						var tileImage = new BitmapImage();
						lock (hugeData)
						{
							if (hugeData[localI, localJ] != null) continue;
						}
						using (FileStream fsLocal = new FileStream(Dir, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							tileImage.BeginInit();
							tileImage.StreamSource = fsLocal;
							tileImage.CacheOption = BitmapCacheOption.OnLoad;
							var rec = new Int32Rect(
								localI * hugeTileSize,
								localJ * hugeTileSize,
								hugeTileSize - Math.Max((localI + 1) * hugeTileSize - DataWidth, 0),
								hugeTileSize - Math.Max((localJ + 1) * hugeTileSize - DataHeight, 0));
							tileImage.SourceRect = rec;
							tileImage.EndInit();

							tileImage.Freeze();
							if (!Cached) { Console.WriteLine("can"); return; };
							int stride = hugeTileSize * (tileImage.Format.BitsPerPixel / 8); ;
							try
							{
								lock (hugeData)
								{
									var tmpTile = new byte[hugeTileSize * hugeTileSize * 4];

									tileImage.CopyPixels(tmpTile, stride, 0);
									if (!Cached) { Console.WriteLine("can"); return; };
									hugeData[localI, localJ] = tmpTile;
									CachedTiles.Enqueue(1);
								}
							}
							catch (Exception opep) { Console.WriteLine(opep); Console.WriteLine(hugeData[localI, localJ].Length + "hugeData[localI, localJ].Length "); }

						}
					}
				}
			};
			bk.RunWorkerAsync();
		}
		public double HugeImageLoadingProgress = 0;
		private bool PreLoadRunning = false;
		private void ParallelReadData(bool isBlockToFinish = true)
		{
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Cached = true;
			if (!PreLoadRunning)
			{
				PreLoadRunning = true;
			}
			else { return; }

			double progress = 0;
			for (int i = 0; i < TileWidth; i++)
			{
				int localI = i;
				if (ImageIndex != Resource.SelectedImageIndex)
				{
					PreLoadRunning = false;
					return;
				};

				BackgroundWorker bk = new BackgroundWorker() { WorkerSupportsCancellation = true };
				if (isBlockToFinish)
				{
					running.Enqueue(bk);
				}
				bk.DoWork += (s, e) =>
				{


					for (int j = 0; j < TileHeight; j++)
					{
						int localJ = j;

						lock (hugeData)
						{
							if (hugeData[localI, localJ] != null) continue;
							else hugeData[localI, localJ] = new byte[hugeTileSize * hugeTileSize * 4];
						}
				
						using (FileStream fsLocal = new FileStream(Dir, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							var tileImage = new BitmapImage();
							tileImage.BeginInit();
							tileImage.StreamSource = fsLocal;
							tileImage.CacheOption = BitmapCacheOption.OnLoad;
							var rec = new Int32Rect(
								localI * hugeTileSize,
								localJ * hugeTileSize,
								hugeTileSize - Math.Max((localI + 1) * hugeTileSize - DataWidth, 0),
								hugeTileSize - Math.Max((localJ + 1) * hugeTileSize - DataHeight, 0));
							Console.WriteLine(rec);
							tileImage.SourceRect = rec;
							tileImage.EndInit();

							tileImage.Freeze();
							
							int stride = hugeTileSize * (tileImage.Format.BitsPerPixel / 8); ;
							lock (hugeData)
							{
								tileImage.CopyPixels(hugeData[localI, localJ], stride, 0);
								CachedTiles.Enqueue(1);
							}

							try
							{
								lock (hugeData)
								{
									HugeImageLoadingProgress = 100.0 * CachedTiles.Count / (TileWidth * TileHeight);
									if (LastViewPara != null)
									{
										GetHugeView(LastViewPara[0], LastViewPara[1], LastViewPara[2], LastViewPara[3], LastViewPara[4], LastViewPara[5], LastViewPara[6], DateTime.Now.Ticks);
										Resource.MainWindow.HugeImageShower.Dispatcher.BeginInvoke(new Action(() => {
											Resource.FromArray();
										}));
									}

									if (ImageIndex != Resource.SelectedImageIndex)
									{
										Console.WriteLine("can");
										PreLoadRunning = false;
										e.Cancel = true;
										return;
									};
									if (isBlockToFinish)
									{
										int iFrom = (int)Math.Floor(((double)localI * hugeTileSize) * ThumbnailWidth / DataWidth);
										int iTo = (int)Math.Floor(((double)(localI + 1) * hugeTileSize - Math.Max((localI + 1) * hugeTileSize - DataWidth, 0)) * ThumbnailWidth / DataWidth);
										int jFrom = (int)Math.Floor(((double)localJ * hugeTileSize) * ThumbnailHeight / DataHeight);
										int jTo = (int)Math.Floor(((double)(localJ + 1) * hugeTileSize - Math.Max((localJ + 1) * hugeTileSize - DataHeight, 0)) * ThumbnailHeight / DataHeight);
										try
										{
											for (int I = iFrom; I < iTo; I++)
											{
												for (int J = jFrom; J < jTo; J++)
												{
													int thumbnailIndex = (int)((double)J + (double)localJ * hugeTileSize / DataHeight) * ThumbnailWidth * 4 + (int)((double)I + (double)localI * hugeTileSize / DataWidth) * 4;
													if (TN[Math.Min(thumbnailIndex, ThumbnailHeight * ThumbnailWidth * 4 - 1)] == 0)
													{
														int bufferIndex = (int)(((double)J - jFrom) * DataHeight / ThumbnailHeight) * hugeTileSize * 4 + (int)(((double)I - iFrom) * DataWidth / ThumbnailWidth) * 4;
														TN[Math.Min(thumbnailIndex, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex, hugeTileSize * hugeTileSize * 4 - 1)];
														TN[Math.Min(thumbnailIndex + 1, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex + 1, hugeTileSize * hugeTileSize * 4 - 1)];
														TN[Math.Min(thumbnailIndex + 2, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex + 2, hugeTileSize * hugeTileSize * 4 - 1)];
														TN[Math.Min(thumbnailIndex + 3, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex + 3, hugeTileSize * hugeTileSize * 4 - 1)];
														//fill++;
														progress = 100.0 * ((double)ThumbnailWidth * J + I) / ((double)ThumbnailWidth * ((double)ThumbnailWidth * DataHeight / DataWidth));
													}
												}
											}
										}
										catch (Exception eeee) { Console.WriteLine(localI + "," + localJ + eeee); }

									}
								}
							}
							catch (Exception opep) { Console.WriteLine(opep); Console.WriteLine(hugeData[localI, localJ].Length + "hugeData[localI, localJ].Length "); }
							
						}

						if (isBlockToFinish && ImageIndex == 0)
						{

							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								Resource.MainWindow.mainViewer.Source = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));
							if (HugeImageLoadingProgress > Resource.MainWindow.statusProgressBar.Value)
									Resource.MainWindow.statusProgressBar.Value = HugeImageLoadingProgress;
							}));
						}
						else
						{
							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								if (HugeImageLoadingProgress > Resource.MainWindow.statusProgressBar.Value)
									Resource.MainWindow.statusProgressBar.Value = HugeImageLoadingProgress;
							}));
							HugeImageLoadingProgress = 100.0 * CachedTiles.Count / (TileWidth * TileHeight);
							Console.WriteLine("Now progress" + HugeImageLoadingProgress);
						}
					}
					
				};

				bk.RunWorkerCompleted += (s, e) =>
				{
					

					if (isBlockToFinish && ImageIndex == 0)
					{
						if (HugeImageLoadingProgress >= 100)
						{
							resizedImage = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));
							resizedImage.Freeze();
							ThumbnailReady = -1;
						}

					}

					(s as BackgroundWorker).Dispose();

				};

				bk.RunWorkerAsync();
			}

		}

		private void ParallelReadData(int frames, bool isBlockToFinish = true)
		{
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			var dcmFile = DicomFile.Open(Dir, FileReadOption.ReadLargeOnDemand);
			DicomDataset ds = Dicom.Imaging.Codec.DicomCodecExtensions.ChangeTransferSyntax(dcmFile.Dataset, DicomTransferSyntax.DeflatedExplicitVRLittleEndian);
			int[,] dcmIndex = new int[frames, 2];
			for (int i = 0; i< frames; i++)
			{
				var PerFrameFunctionalGroupsSequence = dcmFile.Dataset.Get<DicomSequence>(DicomTag.PerFrameFunctionalGroupsSequence, null);
				var PlanePositionSlideSequence = PerFrameFunctionalGroupsSequence.Items[i].Get<DicomSequence>(DicomTag.PlanePositionSlideSequence);
				dcmIndex[i, 0] = (PlanePositionSlideSequence.Items[0].Get<int>(DicomTag.RowPositionInTotalImagePixelMatrix) - 1) / hugeTileSize;
				dcmIndex[i, 1] = (PlanePositionSlideSequence.Items[0].Get<int>(DicomTag.ColumnPositionInTotalImagePixelMatrix) - 1) / hugeTileSize;
			}
			Cached = true;
			if (!PreLoadRunning)
			{
				PreLoadRunning = true;
			}
			else { return; }

			double progress = 0;
			
			int para = (int)Math.Ceiling((double)frames/10);
			
			for (int i = 0; i < 10; i++)
			{
				int localI = i;
				
				if (ImageIndex != Resource.SelectedImageIndex)
				{
					PreLoadRunning = false;
					return;
				};

				BackgroundWorker bk = new BackgroundWorker() { WorkerSupportsCancellation = true };
				if (isBlockToFinish)
				{
					running.Enqueue(bk);
				}
				bk.DoWork += (s, e) =>
				{

					
					for (int j = 0; j < para; j++)
					{
						int localJ = j;

						
						
						int index = localI * para + localJ;
						if (index >= frames)
							continue;
						if (j%4 == 0)
							Thread.Sleep(2000);
						byte[] buffer = Dicom.Imaging.DicomPixelData.Create(ds).GetFrame(index).Data;
						{
							Console.WriteLine("index"+ index);
							
							int _J = dcmIndex[index, 0];
							int _I = dcmIndex[index, 1];
							
							lock (hugeData)
							{
								if (hugeData[_I, _J] == null)
									hugeData[_I, _J] = new byte[hugeTileSize * hugeTileSize * 4];
								buffer.CopyTo(hugeData[_I, _J], 0);
								CachedTiles.Enqueue(1);
							}

							try
							{
								lock (hugeData)
								{
									HugeImageLoadingProgress = 100.0 * CachedTiles.Count / (TileWidth * TileHeight);
									if (LastViewPara != null)
									{
										GetHugeView(LastViewPara[0], LastViewPara[1], LastViewPara[2], LastViewPara[3], LastViewPara[4], LastViewPara[5], LastViewPara[6], DateTime.Now.Ticks);
										Resource.MainWindow.HugeImageShower.Dispatcher.BeginInvoke(new Action(() =>
										{
											Resource.FromArray();
										}));
									}

									if (ImageIndex != Resource.SelectedImageIndex)
									{
										PreLoadRunning = false;
										e.Cancel = true;
										return;
									};
									if (isBlockToFinish)
									{
										int iFrom = (int)Math.Floor(((double)_I * hugeTileSize) * ThumbnailWidth / DataWidth);
										int iTo = (int)Math.Floor(((double)(_I + 1) * hugeTileSize - Math.Max((_I + 1) * hugeTileSize - DataWidth, 0)) * ThumbnailWidth / DataWidth);
										int jFrom = (int)Math.Floor(((double)_J * hugeTileSize) * ThumbnailHeight / DataHeight);
										int jTo = (int)Math.Floor(((double)(_J + 1) * hugeTileSize - Math.Max((_J + 1) * hugeTileSize - DataHeight, 0)) * ThumbnailHeight / DataHeight);
										try
										{
											for (int I = iFrom; I < iTo; I++)
											{
												for (int J = jFrom; J < jTo; J++)
												{
													int thumbnailIndex = (int)((double)J + (double)_J * hugeTileSize / DataHeight) * ThumbnailWidth * 4 + (int)((double)I + (double)_I * hugeTileSize / DataWidth) * 4;
													if (TN[Math.Min(thumbnailIndex, ThumbnailHeight * ThumbnailWidth * 4 - 1)] == 0)
													{
														int bufferIndex = (int)(((double)J - jFrom) * DataHeight / ThumbnailHeight) * hugeTileSize * 3 + (int)(((double)I - iFrom) * DataWidth / ThumbnailWidth) * 3;
														TN[Math.Min(thumbnailIndex, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[_I, _J][Math.Min(bufferIndex, hugeTileSize * hugeTileSize * 3 - 1)];
														TN[Math.Min(thumbnailIndex + 1, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[_I, _J][Math.Min(bufferIndex + 1, hugeTileSize * hugeTileSize * 3 - 1)];
														TN[Math.Min(thumbnailIndex + 2, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[_I, _J][Math.Min(bufferIndex + 2, hugeTileSize * hugeTileSize * 3 - 1)];
														TN[Math.Min(thumbnailIndex + 3, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = 255;
														progress = 100.0 * ((double)ThumbnailWidth * J + I) / ((double)ThumbnailWidth * ((double)ThumbnailWidth * DataHeight / DataWidth));
													}
												}
											}
										}
										catch (Exception eeee) { Console.WriteLine(localI + "," + localJ + eeee); }

									}
								}
							}
							catch (Exception opep) { Console.WriteLine(opep); Console.WriteLine(hugeData[localI, localJ].Length + "hugeData[localI, localJ].Length "); }

						}
						
						if (isBlockToFinish && ImageIndex == 0)
						{

							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								Resource.MainWindow.mainViewer.Source = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));
								if (HugeImageLoadingProgress > Resource.MainWindow.statusProgressBar.Value)
									Resource.MainWindow.statusProgressBar.Value = HugeImageLoadingProgress;
							}));
						}
						else
						{
							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								if (HugeImageLoadingProgress > Resource.MainWindow.statusProgressBar.Value)
									Resource.MainWindow.statusProgressBar.Value = HugeImageLoadingProgress;
							}));
							HugeImageLoadingProgress = 100.0 * CachedTiles.Count / (TileWidth * TileHeight);
							Console.WriteLine("Now progress" + HugeImageLoadingProgress);
						}
					}

				};

				bk.RunWorkerCompleted += (s, e) =>
				{
					for (int i = 0; i < TileWidth; i++)
					{
						for (int j = 0; j < TileHeight; j++)
						{
							if (hugeData[i, j] == null) hugeData[i, j] = new byte[hugeTileSize * hugeTileSize * 4];
						}
					}

					if (isBlockToFinish && ImageIndex == 0)
					{
						if (HugeImageLoadingProgress >= 100)
						{
							resizedImage = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));
							resizedImage.Freeze();
							ThumbnailReady = -1;
						}

					}

					(s as BackgroundWorker).Dispose();

				};

				bk.RunWorkerAsync();
			}

		}
		private void ParallelReadSvs(bool isBlockToFinish = true)
		{
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Cached = true;
			if (!PreLoadRunning)
			{
				PreLoadRunning = true;
			}
			else { return; }

			
			double progress = 0;
			

				
			for (int i = 0; i < TileWidth; i++)
			{
				int localI = i;

				BackgroundWorker bk = new BackgroundWorker() { WorkerSupportsCancellation = true };
				if (isBlockToFinish)
				{
					running.Enqueue(bk);
				}
				bk.DoWork += (s, e) =>
				{
					for (int j = 0; j < TileHeight; j++)
					{
						int localJ = j;
						

						lock (hugeData)
						{
							if (hugeData[localI, localJ] != null) continue;
							else hugeData[localI, localJ] = new byte[hugeTileSize * hugeTileSize * 4];
						}
						using (OpenSlideImage image = OpenSlideImage.Open(Dir))
						{
							var dz = new DeepZoomGenerator(image, tileSize: hugeTileSize, overlap: 0);
							// Generate dzi file.
							string dziFileContent = dz.GetDzi(format: "png");
							DeepZoomGenerator.TileInfo tileInfo;
							byte[] rawTileData = dz.GetTile(level: dz.LevelCount-1, locationX: localI, locationY: localJ, out tileInfo);

							lock (hugeData)
							{
								if (rawTileData.Length == hugeTileSize * hugeTileSize * 4)
									hugeData[localI, localJ] = rawTileData;
								else
								{
									for (int restWidth = 0; restWidth < tileInfo.Width; restWidth++)
									{
										for (int restHeight = 0; restHeight < tileInfo.Height; restHeight++)
										{
											hugeData[localI, localJ][(hugeTileSize*4)*restHeight + restWidth*4+0] = rawTileData[(tileInfo.Width * 4) * restHeight + restWidth * 4 + 0];
											hugeData[localI, localJ][(hugeTileSize*4)*restHeight + restWidth*4+1] = rawTileData[(tileInfo.Width * 4) * restHeight + restWidth * 4 + 1];
											hugeData[localI, localJ][(hugeTileSize*4)*restHeight + restWidth*4+2] = rawTileData[(tileInfo.Width * 4) * restHeight + restWidth * 4 + 2];
											hugeData[localI, localJ][(hugeTileSize*4)*restHeight + restWidth*4+3] = rawTileData[(tileInfo.Width * 4) * restHeight + restWidth * 4 + 3];
										}
									}
								}
								CachedTiles.Enqueue(1);
							}
							try
							{
								lock (hugeData)
								{
									HugeImageLoadingProgress = 100.0 * CachedTiles.Count / (TileWidth * TileHeight);
									if (LastViewPara != null)
									{
										GetHugeView(LastViewPara[0], LastViewPara[1], LastViewPara[2], LastViewPara[3], LastViewPara[4], LastViewPara[5], LastViewPara[6], DateTime.Now.Ticks);
										Resource.MainWindow.HugeImageShower.Dispatcher.BeginInvoke(new Action(() => {
											Resource.FromArray();
										}));
									}

									
									if (isBlockToFinish)
									{
										int iFrom = (int)Math.Floor(((double)localI * hugeTileSize) * ThumbnailWidth / DataWidth);
										int iTo = (int)Math.Floor(((double)(localI + 1) * hugeTileSize - Math.Max((localI + 1) * hugeTileSize - DataWidth, 0)) * ThumbnailWidth / DataWidth);
										int jFrom = (int)Math.Floor(((double)localJ * hugeTileSize) * ThumbnailHeight / DataHeight);
										int jTo = (int)Math.Floor(((double)(localJ + 1) * hugeTileSize - Math.Max((localJ + 1) * hugeTileSize - DataHeight, 0)) * ThumbnailHeight / DataHeight);
										try
										{
											for (int I = iFrom; I < iTo; I++)
											{
												for (int J = jFrom; J < jTo; J++)
												{
													int thumbnailIndex = (int)((double)J + (double)localJ * hugeTileSize / DataHeight) * ThumbnailWidth * 4 + (int)((double)I + (double)localI * hugeTileSize / DataWidth) * 4;
													if (TN[Math.Min(thumbnailIndex, ThumbnailHeight * ThumbnailWidth * 4 - 1)] == 0)
													{
														int bufferIndex = (int)(((double)J - jFrom) * DataHeight / ThumbnailHeight) * hugeTileSize * 4 + (int)(((double)I - iFrom) * DataWidth / ThumbnailWidth) * 4;
														TN[Math.Min(thumbnailIndex, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex, hugeTileSize * hugeTileSize * 4 - 1)];
														TN[Math.Min(thumbnailIndex + 1, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex + 1, hugeTileSize * hugeTileSize * 4 - 1)];
														TN[Math.Min(thumbnailIndex + 2, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex + 2, hugeTileSize * hugeTileSize * 4 - 1)];
														TN[Math.Min(thumbnailIndex + 3, ThumbnailHeight * ThumbnailWidth * 4 - 1)] = hugeData[localI, localJ][Math.Min(bufferIndex + 3, hugeTileSize * hugeTileSize * 4 - 1)];
														//fill++;
														progress = 100.0 * ((double)ThumbnailWidth * J + I) / ((double)ThumbnailWidth * ((double)ThumbnailWidth * DataHeight / DataWidth));
													}
												}
											}
										}
										catch (Exception eeee) { Console.WriteLine(localI + "," + localJ + eeee); }

									}
								}
							}
							catch (Exception opep) { Console.WriteLine(opep); Console.WriteLine(hugeData[localI, localJ].Length + "hugeData[localI, localJ].Length "); }
						}
						if (isBlockToFinish && ImageIndex == 0)
						{

							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								Resource.MainWindow.mainViewer.Source = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));
								if (HugeImageLoadingProgress > Resource.MainWindow.statusProgressBar.Value)
									Resource.MainWindow.statusProgressBar.Value = HugeImageLoadingProgress;
							}));
						}
						else
						{
							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								if (HugeImageLoadingProgress > Resource.MainWindow.statusProgressBar.Value)
									Resource.MainWindow.statusProgressBar.Value = HugeImageLoadingProgress;
							}));
							HugeImageLoadingProgress = 100.0 * CachedTiles.Count / (TileWidth * TileHeight);
						}
						
					}

				};

				bk.RunWorkerCompleted += (s, e) =>
				{


					if (isBlockToFinish && ImageIndex == 0)
					{
						if (HugeImageLoadingProgress >= 100)
						{
							resizedImage = new WriteableBitmap(FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4));
							resizedImage.Freeze();
							ThumbnailReady = -1;
						}

					}
					(s as BackgroundWorker).Dispose();

				};

				bk.RunWorkerAsync();
			}

		}
		public volatile bool Cached = false;
		public void ClearCache()
		{
			try
			{
				Cached = false;

				lock (hugeData)
				{
					hugeData = new byte[TileWidth, TileHeight][];
				}
				
				CachedTiles.Clear();
				HugeImageLoadingProgress = 0;
				OriginImage = null;
				GC.Collect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}


		}
		private void ParallelReadData3D(int imageIndex, bool isBlockToFinish = true)
		{
			lock (hugeData)
			{
				if (hugeData == null)
					hugeData = new byte[TileWidth, TileHeight][];
				for (int i = 0; i < TileWidth; i++)
				{
					for (int j = 0; j < TileHeight; j++)
					{
						hugeData[i, j] = new byte[hugeTileSize * hugeTileSize * 4];
					}
				}
			}
			Cached = true;
			for (int i = 0; i < TileWidth; i++)
			{
				int localI = i;

				BackgroundWorker bk = new BackgroundWorker();
				if (isBlockToFinish)
				{
					running.Enqueue(bk);
				}
				bk.DoWork += (s, e) =>
				{
					Thread.CurrentThread.Priority = ThreadPriority.Highest;
					FileStream fsLocal = new FileStream(Dir, FileMode.Open, FileAccess.Read, FileShare.Read);

					for (int j = 0; j < TileHeight; j++)
					{
						int localJ = j;

						TiffBitmapDecoder decoder = new TiffBitmapDecoder(fsLocal, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);


						var rec = new Int32Rect(
							localI * hugeTileSize,
							localJ * hugeTileSize,
							hugeTileSize - Math.Max((localI + 1) * hugeTileSize - DataWidth, 0),
							hugeTileSize - Math.Max((localJ + 1) * hugeTileSize - DataHeight, 0));

						var stride = hugeTileSize * decoder.Frames[tifIndex].Format.BitsPerPixel / 8;

						try
						{
							lock (hugeData)
							{
								var tmpTile = new byte[hugeTileSize * hugeTileSize * 4];
								if (ImageIndex != Resource.SelectedImageIndex) Thread.Sleep(50);

								decoder.Frames[tifIndex].CopyPixels(rec, tmpTile, stride, 0);

								hugeData[localI, localJ] = tmpTile;
								CachedTiles.Enqueue(1);
								if (ImageIndex != Resource.SelectedImageIndex) Thread.Sleep(50);
							}
						}
						catch (Exception opep) { Console.WriteLine(opep); Console.WriteLine(hugeData[localI, localJ].Length + "hugeData[localI, localJ].Length "); }
						double progress = 0;

						if (isBlockToFinish && imageIndex == 0)
						{
							Resource.MainWindow.mainViewer.Dispatcher.BeginInvoke(new Action(() =>
							{
								Resource.MainWindow.mainViewer.Source = FromArray(TN, ThumbnailWidth, (int)((double)ThumbnailWidth * DataHeight / DataWidth), 4);
							}));
						}
					}
					fsLocal.Close();
				};
				bk.RunWorkerCompleted += (s, e) =>
				{
					
				};
				

				bk.RunWorkerAsync();
			}
		}

		public void LayerTransparency(int LayerNumber, int Value)
		{
			Color TempColor = AnnotationLayers[LayerNumber].AnnotationColor;
			int alpha = System.Math.Max(System.Math.Min((int)AnnotationLayers[LayerNumber].AnnotationColor.A + Value, 255), 1);
			TempColor.A = (Byte)alpha;
			AnnotationLayers[LayerNumber].AnnotationColor = TempColor;
		}

		public static string UidGenerator()
		{
			return ((DateTime.Now.AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
		}
		public static string UidGenerator(int parameter)
		{
			return ((DateTime.Now.AddHours(-8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds + parameter).ToString();
		}
		public void AddMatAnnotationLayer()
		{

		}
		int ViewWidth = -1;
		int ViewHeight = -1;
		ulong totalPixel = 0;
		byte[] buffer = new byte[0];
		public double[]? LastViewPara = null;
		private Dictionary<long, bool> runningSvsCacheing = new Dictionary<long, bool>();
		public void GetHugeView(
			double imageActualWidth,
			double imageActualHeight,
			double scrollViewer_Width,
			double scrollViewer_Height,
			double scrollViewer_HorizontalOffset,
			double scrollViewer_VerticalOffset,
			double zoom,
			long timestamp
			)
		{
			// double cache, Resource and Deamon
			if (imageActualWidth <= 0 || imageActualHeight <= 0) return;
			if (HugeImageLoadingProgress != 100)
			{

				LastViewPara = new double[]{
					imageActualWidth,
					imageActualHeight,
					scrollViewer_Width,
					scrollViewer_Height,
					scrollViewer_HorizontalOffset,
					scrollViewer_VerticalOffset,
					zoom,
				};
			}
			ulong[] loadedPixels = { 0 };
			
			double xoffset = Math.Max(0, (Resource.ViewWidth - (imageActualWidth)) / 2);// If there is blank space in scrollview the contour viewer start with 0
			double yoffset = Math.Max(0, (Resource.ViewHeight - (imageActualHeight)) / 2);
			byte[,][] localCache;
			int[,] localWidthCache;
			int hit_tile = 0;
			if (ImageType == Resource.ImageType.WSI)
			{
				int level = 0;
				while (LevelDimensions.ElementAt(level).Width < imageActualWidth && level < LevelDimensions.Count()-1)
				{
					level++;
				}
				long leveledDataWidth = LevelDimensions.ElementAt(level).Width;
				long leveledDataHeight = LevelDimensions.ElementAt(level).Height;
				double widthRatio = (double)leveledDataWidth / imageActualWidth;
				double heightRatio = (double)leveledDataHeight / imageActualHeight;
				TimeStamp[0] = timestamp;
				int Resource_ViewWidth = Resource.ViewWidth;
				int Resource_ViewHeight = Resource.ViewHeight;
				if (totalPixel != (ulong)Resource_ViewWidth * (ulong)Resource_ViewHeight)
				{
					totalPixel = (ulong)Resource_ViewWidth * (ulong)Resource_ViewHeight;
					buffer = new byte[totalPixel * 4];
				}
				
				var initTileInfo = new DeepZoomGenerator.TileInfo();
				int hugeTileWidth = (int)Math.Ceiling((double)leveledDataWidth / wsiTileSize);
				int hugeTileHeight = (int)Math.Ceiling((double)leveledDataHeight / wsiTileSize);
				localCache = new byte[hugeTileWidth, hugeTileHeight][];
				localWidthCache = new int[hugeTileWidth, hugeTileHeight];

				
				
				initDz.GetTile(level: level, 0, 0, out initTileInfo);

				if (initTileInfo.Width != initTileInfo.Height)
				{
					initTileInfo.Height= initTileInfo.Width;
				}

				if (nowImageActualWidth != imageActualWidth ||
					nowImageActualHeight != imageActualHeight ||
					nowScrollViewer_Width != scrollViewer_Width ||
					nowScrollViewer_Height != scrollViewer_Height ||
					nowScrollViewer_HorizontalOffset != scrollViewer_HorizontalOffset ||
					nowScrollViewer_VerticalOffset != scrollViewer_VerticalOffset ||
					nowZoom != zoom
					)
				{
					int paradivier = 540;
					nowImageActualWidth = imageActualWidth;
					nowImageActualHeight = imageActualHeight;
					nowScrollViewer_Width = scrollViewer_Width;
					nowScrollViewer_Height = scrollViewer_Height;
					nowScrollViewer_HorizontalOffset = scrollViewer_HorizontalOffset;
					nowScrollViewer_VerticalOffset = scrollViewer_VerticalOffset;
					nowZoom = zoom;
					Parallel.For(0, Resource_ViewHeight / paradivier + 1, parallelTileIterator =>
					{

						for (int viewY = parallelTileIterator * paradivier; viewY < Resource_ViewHeight-1 && viewY < (parallelTileIterator + 1) * paradivier; viewY+=2)
						{
							int annx_bias;
							int tilex;

							int yBias = viewY * Resource_ViewWidth * 4;
							int anny = (int)MathHelper.Clip(((double)viewY - yoffset + scrollViewer_VerticalOffset + 0.5) * leveledDataHeight / imageActualHeight, 0, DataHeight - 1);
							var thumbnaily = (int)MathHelper.Clip(((double)viewY - yoffset + scrollViewer_VerticalOffset + 0.5) * resizedImage.PixelHeight / imageActualHeight, 0, resizedImage.PixelHeight - 1);
							int anny_bias = Math.Min(anny / wsiTileSize, hugeTileHeight - 1);
							int tiley = anny % wsiTileSize;

							for (int viewX = 0; viewX < Resource_ViewWidth-1; viewX+=2)
							{
								int xBias = viewX * 4;
								int annx = (int)MathHelper.Clip(((double)viewX - xoffset + scrollViewer_HorizontalOffset + 0.5) * leveledDataWidth / imageActualWidth, 0, DataWidth - 1);
								var thumbnailx = (int)MathHelper.Clip(((double)viewX - xoffset + scrollViewer_HorizontalOffset + 0.5) * resizedImage.PixelWidth / imageActualWidth, 0, resizedImage.PixelWidth - 1);
								annx_bias = Math.Min(annx / wsiTileSize, hugeTileWidth - 1);
								tilex = annx % wsiTileSize;

								if (localCache[annx_bias, anny_bias] == null)
								{
									long key = (annx_bias << 10);
									key = (key << 10) + anny_bias;
									key = (key << 8) + level;
									if (!runningSvsCacheing.ContainsKey(key))
									{
										lock (Resource.SvsCacheTile)
										{
											if (Resource.SvsCacheTile.ContainsKey(key))
											{
												localCache[annx_bias, anny_bias] = Resource.SvsCacheTile[key].Data;
												localWidthCache[annx_bias, anny_bias] = Resource.SvsCacheTile[key].ByteWidth;
											}
											else
											{
												lock (runningSvsCacheing)
												{
													if (!runningSvsCacheing.ContainsKey(key))
														runningSvsCacheing.Add(key, true);
												}
												long localkey = key;
												int localannx_bias = annx_bias;
												int localanny_bias = anny_bias;
												BackgroundWorker bkkkk = new BackgroundWorker();
												bkkkk.DoWork += (s, e) => {
													{
														var tileInfo = new DeepZoomGenerator.TileInfo();
														var tmpTile = localCache[localannx_bias, localanny_bias] = initDz.GetTile(level: level, localannx_bias, localanny_bias, out tileInfo);
														var tmpWidth = localWidthCache[localannx_bias, localanny_bias] = (int)tileInfo.Width;

														Resource.SvsCacheTile[localkey] =
															new SvsTile(
																tmpTile,
																tmpWidth
															)
														;
													}
												};
												bkkkk.RunWorkerCompleted += (s, e) =>
												{
													lock (runningSvsCacheing)
													{
														runningSvsCacheing.Remove(localkey);
													}
												};
												bkkkk.RunWorkerAsync();
											}
										}
									}
									else { continue; }
								}
								int tmp_tiley = (int)((double)tiley * (initTileInfo.Height + 1 - 2) / wsiTileSize);
								tilex = (int)((double)tilex * (initTileInfo.Width + 1 - 2) / wsiTileSize);

								
								try
								{
									int bytePosition = tmp_tiley * localWidthCache[annx_bias, anny_bias] * 4 + tilex * 4;
									if (viewY - yoffset < 0 || viewY + yoffset >= Resource_ViewHeight || viewX - xoffset < 0 || viewX + xoffset >= Resource_ViewWidth)
									{
										buffer[yBias + xBias] = 0;
										buffer[yBias + xBias + 1] = 0;
										buffer[yBias + xBias + 2] = 0;
										buffer[yBias + xBias + 3] = 0;
									}
									else if (localCache[annx_bias, anny_bias] == null)
									{
										buffer[yBias + xBias + 0] = resizedImageBytes[thumbnaily * resizedImage.PixelWidth * 3 + thumbnailx * 3 + 0];
										buffer[yBias + xBias + 1] = resizedImageBytes[thumbnaily * resizedImage.PixelWidth * 3 + thumbnailx * 3 + 1];
										buffer[yBias + xBias + 2] = resizedImageBytes[thumbnaily * resizedImage.PixelWidth * 3 + thumbnailx * 3 + 2];
										buffer[yBias + xBias + 3] = 255;
									}
									else if (bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 7 < localCache[annx_bias, anny_bias].Length)
									{
										buffer[yBias + xBias + 0] = localCache[annx_bias, anny_bias][bytePosition + 0];
										buffer[yBias + xBias + 1] = localCache[annx_bias, anny_bias][bytePosition + 1];
										buffer[yBias + xBias + 2] = localCache[annx_bias, anny_bias][bytePosition + 2];
										buffer[yBias + xBias + 3] = 255;
										
										buffer[yBias + xBias + 4] = localCache[annx_bias, anny_bias][bytePosition + 4];
										buffer[yBias + xBias + 5] = localCache[annx_bias, anny_bias][bytePosition + 5];
										buffer[yBias + xBias + 6] = localCache[annx_bias, anny_bias][bytePosition + 6];
										buffer[yBias + xBias + 7] = 255;
										
										buffer[yBias + Resource_ViewWidth * 4+xBias + 0] = localCache[annx_bias, anny_bias][bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 0];
										buffer[yBias + Resource_ViewWidth * 4+xBias + 1] = localCache[annx_bias, anny_bias][bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 1];
										buffer[yBias + Resource_ViewWidth * 4+xBias + 2] = localCache[annx_bias, anny_bias][bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 2];
										buffer[yBias + Resource_ViewWidth * 4+xBias + 3] = 255;
										buffer[yBias + Resource_ViewWidth * 4+xBias + 4] = localCache[annx_bias, anny_bias][bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 4];
										buffer[yBias + Resource_ViewWidth * 4+xBias + 5] = localCache[annx_bias, anny_bias][bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 5];
										buffer[yBias + Resource_ViewWidth * 4+xBias + 6] = localCache[annx_bias, anny_bias][bytePosition + localWidthCache[annx_bias, anny_bias] * 4 + 6];
										buffer[yBias + Resource_ViewWidth * 4+xBias + 7] = 255;



									}
								}
								catch (Exception ex)
								{
									Console.WriteLine("here" + ex + "\n" + buffer.Length + "," + (yBias + xBias + 3));

								}
								finally
								{
								}
							}
						}
					});
					Resource.DelayViewBuffer.Push(new Resource.ViewBuffer(DateTime.Now.Ticks, buffer));
				}
				else
				{
					int paradivier = 1080;
					Parallel.For(0, Resource_ViewHeight / paradivier + 1, parallelTileIterator =>
					{

						for (int viewY = parallelTileIterator * paradivier; viewY < Resource_ViewHeight && viewY < (parallelTileIterator + 1) * paradivier; viewY++)
						{
							int annx_bias;
							int tilex;

							int yBias = viewY * Resource_ViewWidth * 4;
							int anny = (int)MathHelper.Clip(((double)viewY - yoffset + scrollViewer_VerticalOffset + 0.5) * leveledDataHeight / imageActualHeight, 0, DataHeight - 1);
							var thumbnaily = (int)MathHelper.Clip(((double)viewY - yoffset + scrollViewer_VerticalOffset + 0.5) * resizedImage.PixelHeight / imageActualHeight, 0, resizedImage.PixelHeight - 1);
							int anny_bias = Math.Min(anny / wsiTileSize, hugeTileHeight - 1);
							int tiley = anny % wsiTileSize;

							for (int viewX = 0; viewX < Resource_ViewWidth; viewX++)
							{
								int xBias = viewX * 4;
								int annx = (int)MathHelper.Clip(((double)viewX - xoffset + scrollViewer_HorizontalOffset + 0.5) * leveledDataWidth / imageActualWidth, 0, DataWidth - 1);
								var thumbnailx = (int)MathHelper.Clip(((double)viewX - xoffset + scrollViewer_HorizontalOffset + 0.5) * resizedImage.PixelWidth / imageActualWidth, 0, resizedImage.PixelWidth - 1);
								annx_bias = Math.Min(annx / wsiTileSize, hugeTileWidth - 1);
								tilex = annx % wsiTileSize;

								if (localCache[annx_bias, anny_bias] == null)
								{
									long key = (annx_bias << 10);
									key = (key << 10) + anny_bias;
									key = (key << 8) + level;
									if (!runningSvsCacheing.ContainsKey(key))
									{
										lock (Resource.SvsCacheTile)
										{
											if (Resource.SvsCacheTile.ContainsKey(key))
											{
												localCache[annx_bias, anny_bias] = Resource.SvsCacheTile[key].Data;
												localWidthCache[annx_bias, anny_bias] = Resource.SvsCacheTile[key].ByteWidth;
											}
											else
											{
												lock (runningSvsCacheing)
												{
													if (!runningSvsCacheing.ContainsKey(key))
														runningSvsCacheing.Add(key, true);
												}
												long localkey = key;
												int localannx_bias = annx_bias;
												int localanny_bias = anny_bias;
												BackgroundWorker bkkkk = new BackgroundWorker();
												bkkkk.DoWork += (s, e) => {
													{
														var tileInfo = new DeepZoomGenerator.TileInfo();
														var tmpTile = localCache[localannx_bias, localanny_bias] = initDz.GetTile(level: level, localannx_bias, localanny_bias, out tileInfo);
														var tmpWidth = localWidthCache[localannx_bias, localanny_bias] = (int)tileInfo.Width;

														Resource.SvsCacheTile[localkey] = 
															new SvsTile(
																tmpTile,
																tmpWidth
															)
														;
													}
												};
												bkkkk.RunWorkerCompleted += (s, e) =>
												{
													lock (runningSvsCacheing)
													{
														runningSvsCacheing.Remove(localkey);
														if (runningSvsCacheing.Count == 0)
															GetHugeView(LastViewPara[0], LastViewPara[1], LastViewPara[2], LastViewPara[3], LastViewPara[4], LastViewPara[5], LastViewPara[6], DateTime.Now.Ticks);
													}
												};
												bkkkk.RunWorkerAsync();
											}
										}
									}
									else { continue; }
								}

								int tmp_tiley = (int)((double)tiley * (initTileInfo.Height + 1 - 2) / wsiTileSize);
								tilex = (int)((double)tilex * (initTileInfo.Width + 1 - 2) / wsiTileSize);


								try
								{
									int bytePosition = tmp_tiley * localWidthCache[annx_bias, anny_bias] * 4 + tilex * 4;
									if (viewY - yoffset < 0 || viewY + yoffset >= Resource_ViewHeight || viewX - xoffset < 0 || viewX + xoffset >= Resource_ViewWidth)
									{
										buffer[yBias + xBias] = 0;
										buffer[yBias + xBias + 1] = 0;
										buffer[yBias + xBias + 2] = 0;
										buffer[yBias + xBias + 3] = 0;
									}
									else if (localCache[annx_bias, anny_bias] == null)
									{
										buffer[yBias + xBias + 0] = resizedImageBytes[thumbnaily * resizedImage.PixelWidth * 3 + thumbnailx * 3 + 0];
										buffer[yBias + xBias + 1] = resizedImageBytes[thumbnaily * resizedImage.PixelWidth * 3 + thumbnailx * 3 + 1];
										buffer[yBias + xBias + 2] = resizedImageBytes[thumbnaily * resizedImage.PixelWidth * 3 + thumbnailx * 3 + 2];
										buffer[yBias + xBias + 3] = 255;
									}
									else if (bytePosition + 3 < localCache[annx_bias, anny_bias].Length)
									{
										buffer[yBias + xBias + 0] = localCache[annx_bias, anny_bias][bytePosition + 0];
										buffer[yBias + xBias + 1] = localCache[annx_bias, anny_bias][bytePosition + 1];
										buffer[yBias + xBias + 2] = localCache[annx_bias, anny_bias][bytePosition + 2];
										buffer[yBias + xBias + 3] = 255;
									}
								}
								catch (Exception ex)
								{
									Console.WriteLine("here" + ex + "\n" + buffer.Length + "," + (yBias + xBias + 3));

								}
								finally
								{
									lock (loadedPixels)
									{
										loadedPixels[0] += 1;

									}
									if (loadedPixels[0] == totalPixel) Resource.DelayViewBuffer.Push(new Resource.ViewBuffer(timestamp, buffer));
								}
							}
						}
					});
				}
			}
			else if (ImageType == Resource.ImageType.HugeTif)
			{
				int paradivier = 1080;
				ulong totalPixel = (ulong)Resource.ViewWidth * (ulong)Resource.ViewHeight;
				var buffer = new byte[totalPixel * 4];
				double widthRatio = DataWidth / imageActualWidth;
				double heightRatio = DataHeight / imageActualHeight;
				Parallel.For(0, Resource.ViewHeight / paradivier + 1, parallelTileIterator =>
				{
					for (int viewY = parallelTileIterator * paradivier; viewY < Resource.ViewHeight && viewY < (parallelTileIterator + 1) * paradivier; viewY++)
					{
						int yBias = viewY * Resource.ViewWidth * 4;
						int anny = (int)MathHelper.Clip((viewY - yoffset + scrollViewer_VerticalOffset + 0.5) * heightRatio, 0, DataHeight - 1);

						int annx_bias;
						int anny_bias = anny / wsiTileSize;

						int tilex;
						int tiley = anny % wsiTileSize;

						for (int viewX = 0; viewX < Resource.ViewWidth; viewX++)
						{
							int xBias = viewX * 4;
							int annx = (int)MathHelper.Clip((viewX - xoffset + scrollViewer_HorizontalOffset + 0.5) * widthRatio, 0, DataWidth - 1);
							annx_bias = annx / wsiTileSize;
							tilex = annx % wsiTileSize;
							try
							{
								int data = 0;
								if (hugeData[annx_bias, anny_bias] != null)
								{
									if (viewY - yoffset < 0 || viewY + yoffset >= Resource.ViewHeight || viewX - xoffset < 0 || viewX + xoffset >= Resource.ViewWidth)
									{
										buffer[yBias + xBias] = 0;
										buffer[yBias + xBias + 1] = 0;
										buffer[yBias + xBias + 2] = 0;
										buffer[yBias + xBias + 3] = 0;
										continue;
									}
									else
									{
										buffer[yBias + xBias] = hugeData[annx_bias, anny_bias][tilex * 4 + tiley * wsiTileSize * 4];
										buffer[yBias + xBias + 1] = hugeData[annx_bias, anny_bias][tilex * 4 + 1 + tiley * wsiTileSize * 4];
										buffer[yBias + xBias + 2] = hugeData[annx_bias, anny_bias][tilex * 4 + 2 + tiley * wsiTileSize * 4];
										buffer[yBias + xBias + 3] = 255;
									}

									if (yBias + xBias + 3 >= buffer.Length) { Console.WriteLine("execed"); }

								}
								else
								{
									buffer[yBias + xBias] = 0;
									buffer[yBias + xBias + 1] = 0;
									buffer[yBias + xBias + 2] = 0;
									buffer[yBias + xBias + 3] = 0;
								}


							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}
							finally
							{
								lock (loadedPixels)
								{
									loadedPixels[0] += 1;

								}
								if (loadedPixels[0] == totalPixel) Resource.DelayViewBuffer.Push(new Resource.ViewBuffer(timestamp, buffer));
							}


						}
					}

				});
			}
			
			return;

		}

		public void AddAnnotationLayer()
		{
			int LayersCount = AnnotationLayers.Count;
			string Uid = UidGenerator();
			WriteableBitmap Layer = new WriteableBitmap(AnnotationWidth, AnnotationHeight, 96, 96, PixelFormats.Pbgra32, null);
			Layer.Clear(Colors.Transparent);
			AnnotationLayers.Add(new AnnotationLayer(Layer, "New Layer " + LayersCount.ToString(), Uid));
			SelectedLayerIndex = AnnotationLayers.Count - 1;
			IsAnnotationDirty = true;
		}

		public void AddAnnotationLayer(string name)
		{
			string Uid = UidGenerator();
			WriteableBitmap Layer = new WriteableBitmap(AnnotationWidth, AnnotationHeight, 96, 96, PixelFormats.Pbgra32, null);
			Layer.Clear(Colors.Transparent);
			AnnotationLayers.Add(new AnnotationLayer(Layer, name, Uid, false));
			AnnotationLayers.Last().Layer.Clear(Colors.Transparent);
			Resource.AnnotationSelector.SetFocus(AnnotationLayers.Last().Uid);
			SelectedLayerIndex = AnnotationLayers.Count - 1;
			IsAnnotationDirty = true;
		}

		public void AddAnnotationLayer(string name, Color color)
		{
			string Uid = UidGenerator();
			WriteableBitmap Layer = new WriteableBitmap(AnnotationWidth, AnnotationHeight, 96, 96, PixelFormats.Pbgra32, null);
			Layer.Clear(Colors.Transparent);
			AnnotationLayers.Add(new AnnotationLayer(Layer, name, Uid, false));
			AnnotationLayers.Last().Layer.Clear(Colors.Transparent);
			AnnotationLayers.Last().AnnotationColor = color;
			Resource.AnnotationSelector.SetFocus(AnnotationLayers.Last().Uid);
			SelectedLayerIndex = AnnotationLayers.Count - 1;
			IsAnnotationDirty = true;
		}

		public void AddAnnotationLayer(WriteableBitmap bitmap, string name)
		{
			string Uid = UidGenerator();
			bitmap.Clear(Colors.Transparent);
			AnnotationLayers.Add(new AnnotationLayer(bitmap, name, Uid));
			SelectedLayerIndex = AnnotationLayers.Count - 1;
			IsAnnotationDirty = true;
		}

		public void RemoveAnnotationLayer(int LayerIndex)
		{
			AnnotationLayers.RemoveAt(LayerIndex);
			if (AnnotationLayers.Count == 0)
				IsAnnotationDirty = false;
			else
				IsAnnotationDirty = true;
		}

		public void LoadAnnotation(string filePath)
		{
			try
			{
				if (AnnotationLayers.Count == 0)
					AnnotationFilePrefix = filePath;

				if (System.IO.Path.GetExtension(filePath).ToLower(CultureInfo.CurrentCulture) == ".png")
				{
					byte bitDepth = 0;

					using (ShellObject picture = ShellObject.FromParsingName(filePath))
					{
						if (picture != null)
						{
							var raw_w = picture.Properties.GetProperty(SystemProperties.System.Image.HorizontalSize);
							var raw_h = picture.Properties.GetProperty(SystemProperties.System.Image.VerticalSize);
							var raw_bitdepth = picture.Properties.GetProperty(SystemProperties.System.Image.BitDepth);

							long getSize_Width = long.Parse(raw_w.ValueAsObject.ToString());
							long getSize_Height = long.Parse(raw_h.ValueAsObject.ToString());
							bitDepth = byte.Parse(raw_bitdepth.ValueAsObject.ToString());
							Console.WriteLine(getSize_Width + "n " + getSize_Height + bitDepth);
							if (23170 * 23170 < getSize_Width * getSize_Height)
							{


								messagebox.Show("The annotation file size is " + getSize_Width + "x" + getSize_Height + " is too large,\nand exceeds the limit of 536,870,912 pixels.\nRescale the annotation to ratio " + AnnotationRatio,
									"Warning", MessageBoxButton.OK, MessageBoxImage.Information);

							}
						}
					}
					using (FileStream fullImageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var annotationImage = new BitmapImage();
						annotationImage.BeginInit();

						annotationImage.StreamSource = fullImageStream;

						annotationImage.CacheOption = BitmapCacheOption.OnDemand;

						annotationImage.DecodePixelWidth = AnnotationWidth;
						annotationImage.DecodePixelHeight = AnnotationHeight;

						annotationImage.EndInit();
						annotationImage.Freeze();

						var resizeMat = (annotationImage).ToMat();
						if (resizeMat.Channels() == 1)
						{
							resizeMat = resizeMat.CvtColor(ColorConversionCodes.GRAY2BGRA);
							var splitMat = resizeMat.Split();
							splitMat[3] = splitMat[0];
							Cv2.Merge(splitMat, resizeMat);
						}
						else if (resizeMat.Channels() == 3)
						{
							resizeMat = resizeMat.CvtColor(ColorConversionCodes.BGR2BGRA);
							var splitMat = resizeMat.Split();
							splitMat[3] = splitMat[0];
							Cv2.Merge(splitMat, resizeMat);
						}
						var resizeBitmap = resizeMat.ToBitmapSource();
						AnnotationLayers.Add(new AnnotationLayer(new WriteableBitmap(resizeBitmap), System.IO.Path.GetFileNameWithoutExtension(filePath), UidGenerator(), ColorHelper.HIGHLIGHTER, false));


					}
				}
				else
				{
					TiffBitmapDecoder decoder = new TiffBitmapDecoder(new Uri(filePath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
					foreach (var frame in decoder.Frames)
					{
						BitmapSource bitmap = frame;
						if (bitmap.Format != PixelFormats.Pbgra32)
							bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Pbgra32, null, 0);

						if (bitmap.Height != AnnotationHeight || bitmap.Width != AnnotationWidth)
						{

							bitmap = new TransformedBitmap(bitmap, new ScaleTransform(AnnotationWidth / bitmap.Width, AnnotationHeight / bitmap.Height));

						}
						// Check the anntation Uid
						var Uid = (frame.Metadata as BitmapMetadata).Copyright;
						if (string.IsNullOrEmpty(Uid))
						{
							Uid = UidGenerator();
						}
						Color color;
						try
						{
							color = (Color)ColorConverter.ConvertFromString((frame.Metadata as BitmapMetadata).Subject);
						}
						catch (Exception e)
						{
							color = ColorHelper.HIGHLIGHTER;
							Console.WriteLine("LoadAnnotation color catch" + e);
						}
						string annotationName;
						try
						{
							annotationName = (frame.Metadata as BitmapMetadata).Title;
						}
						catch (Exception e)
						{
							annotationName = "Default Name";
							Console.WriteLine("LoadAnnotation annotationName catch" + e);
						}
						try
						{
							AnnotationLayers.Add(new AnnotationLayer(new WriteableBitmap(bitmap), annotationName, Uid, color));
						}
						catch (Exception e)
						{
							Console.WriteLine("LoadAnnotation catch"+e);
						}
					}

				}
				Resource.AnnotationSelector.SetFocus(AnnotationLayers.Last().Uid);
			}
			catch (FileNotFoundException)
			{
				throw;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public async Task GetAIAnnotations(string TempFile,
			string redtext, string greentext, string bluetext, string ApparentMagnification,
			bool endOfTask)
		{
			await AIModule.GetAIAnnotations(AnnotationWidth, AnnotationHeight, tifIndex, TempFile, Dir,
			redtext, greentext, bluetext, ApparentMagnification,
			endOfTask).ConfigureAwait(true);
		}

		public async Task GetAIAnnotations(string TempFile, string ApparentMagnification,
			bool endOfTask)
		{
			if (ImageType == Resource.ImageType.WSI)
			{
				await AIModule.GetAIAnnotations(AnnotationWidth, AnnotationHeight, tifIndex, TempFile, svsTempDir,
				"", "", "", ApparentMagnification,
				endOfTask).ConfigureAwait(true);
			}
		}

		public void CleanAnnotations()
		{
			if (selectedLayerIndex == 0)
			{
				AIModule.CleanAnnotations();
			}
			AnnotationLayers[selectedLayerIndex].Layer.Clear(Colors.Transparent);
			IsAnnotationDirty = true;
		}
		public void FillColor()
		{
			AnnotationLayers[selectedLayerIndex].Layer.Clear(AnnotationLayers[selectedLayerIndex].AnnotationColor);
			IsAnnotationDirty = true;
		}

		public Stream OriginalSizePngStream
		{
			get
			{
				var ms = new MemoryStream();
				PngBitmapEncoder encoder = new PngBitmapEncoder();
				BitmapSource pngSource;
				if (tifIndex >= 0)
				{

					var cacheFileStream = new FileStream(Dir, FileMode.Open, FileAccess.Read);

					TiffBitmapDecoder decoder = new TiffBitmapDecoder(cacheFileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
					var convertedBitmap = new FormatConvertedBitmap(decoder.Frames[tifIndex], PixelFormats.Bgra32, null, 0);
					var oriImageMat = (convertedBitmap).ToMat();
					var oriImageBitmapSource = oriImageMat.ToBitmapSource();
					oriImageBitmapSource.Freeze();
					pngSource = oriImageBitmapSource;
					cacheFileStream.Close();
					cacheFileStream.Dispose();

					GC.Collect();

				}
				else
				{
					var originalImage = new BitmapImage();
					originalImage.BeginInit();
					originalImage.UriSource = new Uri(Dir, UriKind.RelativeOrAbsolute);
					originalImage.CacheOption = BitmapCacheOption.OnDemand;
					originalImage.EndInit();
					pngSource = originalImage;
					pngSource.Freeze();
				}
				encoder.Frames.Add(BitmapFrame.Create(pngSource));
				encoder.Save(ms);
				ms.Position = 0;
				return ms;
			}
		}

		public Stream AnnotationMapPngStream
		{
			get
			{
				var scaleUpAnnotationLayer = AnnotationLayers[SelectedLayerIndex].Layer;


				var AnnotationRectsStream = new MemoryStream();
				PngBitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(scaleUpAnnotationLayer));
				encoder.Save(AnnotationRectsStream);
				AnnotationRectsStream.Position = 0;
				return AnnotationRectsStream;

			}
		}

		public WriteableBitmap GrayScaleAnnotationMaskImage
		{
			get
			{
				int stride = AnnotationWidth * 4;

				byte[] grayScaleAnnotationMaskPixels = new byte[AnnotationHeight * stride];
				var grayScaleAnnotationMaskImage = new WriteableBitmap(AnnotationWidth, AnnotationHeight, 96, 96, PixelFormats.Bgr32, null);
				Color background = Colors.Black;
				background.A = 255;
				grayScaleAnnotationMaskImage.Clear(background);
				foreach (var AnnotationLayer in AnnotationLayers)
				{
					Console.WriteLine("foreach");
					Console.WriteLine(AnnotationHeight * stride);
					byte[] annotationPixels = new byte[AnnotationHeight * stride];
					Console.WriteLine(AnnotationHeight * stride + "-");
					AnnotationLayer.Layer.CopyPixels(annotationPixels, stride, 0);

					Color color = Colors.White;
					Parallel.For(0, AnnotationHeight, y =>
					{
						for (int x = 0; x < stride; x += 4)
						{
							if (annotationPixels[x + 3 + y * stride] != 0x00)
							{
								grayScaleAnnotationMaskPixels[x + y * stride] = color.B;
								grayScaleAnnotationMaskPixels[x + 1 + y * stride] = color.G;
								grayScaleAnnotationMaskPixels[x + 2 + y * stride] = color.R;
								grayScaleAnnotationMaskPixels[x + 3 + y * stride] = color.A;
							}
						}
					});
				}

				grayScaleAnnotationMaskImage.WritePixels(new Int32Rect(0, 0, AnnotationWidth, AnnotationHeight), grayScaleAnnotationMaskPixels, stride, 0);
				return grayScaleAnnotationMaskImage;

			}
		}
		public WriteableBitmap PngAnnotationImage
		{
			get
			{
				int stride = AnnotationWidth * 4;

				byte[] pngAnnotationPixels = new byte[AnnotationHeight * stride];
				var pngAnnotationImage = new WriteableBitmap(AnnotationWidth, AnnotationHeight, 96, 96, PixelFormats.Bgra32, null);
				Color background = Colors.Transparent;
				pngAnnotationImage.Clear(background);
				var AnnotationLayer = AnnotationLayers[selectedLayerIndex];
				byte[] annotationPixels = new byte[AnnotationHeight * stride];
				AnnotationLayer.Layer.CopyPixels(annotationPixels, stride, 0);

				Color color = AnnotationLayer.AnnotationColor;
				Parallel.For(0, AnnotationHeight, y =>
				{
					for (int x = 0; x < stride; x += 4)
					{
							
						if (annotationPixels[x + 3 + y * stride] != 0x00)
						{
							pngAnnotationPixels[x + y * stride] = color.B;
							pngAnnotationPixels[x + 1 + y * stride] = color.G;
							pngAnnotationPixels[x + 2 + y * stride] = color.R;
							pngAnnotationPixels[x + 3 + y * stride] = color.A;
						}
					}
				});

				pngAnnotationImage.WritePixels(new Int32Rect(0, 0, AnnotationWidth, AnnotationHeight), pngAnnotationPixels, stride, 0);
				return pngAnnotationImage;
			}
		}

		public async Task UpdateAntibodyPercentage(FunctionData functionData)
		{
			if (AnnotationLayers.Count == 0)
			{
				AddAnnotationLayer();
				SelectedLayerIndex = AnnotationLayers.Count - 1;
				AnnotationLayers[SelectedLayerIndex].Layer = AnnotationLayers[SelectedLayerIndex].Layer.Clone();
				AnnotationLayers[SelectedLayerIndex].Layer.Clear(Colors.Transparent);
			}
			else
			{
				bool isdirty = false;
			}
			await AIModule.PluginFunctionPost(functionData).ConfigureAwait(true);
		}

		public async Task UpdateAntibodyPercentage(
			string redtext,
			string greentext,
			string bluetext,
			string ApparentMagnification,
			string RangeMin,
			string RangeMax,
			bool Threshold,
			double ThresholdValue,
			bool Parallel
			)
		{
			if (AnnotationLayers.Count == 0)
			{
				AddAnnotationLayer();
				SelectedLayerIndex = AnnotationLayers.Count - 1;
				AnnotationLayers[SelectedLayerIndex].Layer = AnnotationLayers[SelectedLayerIndex].Layer.Clone();
				AnnotationLayers[SelectedLayerIndex].Layer.Clear(Colors.Transparent);


			}
			else
			{
				bool isdirty = false;
			}
			await AIModule.AntibodyPercentageAsync(AnnotationWidth, AnnotationHeight, tifIndex, redtext, greentext, bluetext, Dir,
				ApparentMagnification,
				RangeMin,
				RangeMax,
				Threshold ? -1 : ThresholdValue,
				Parallel
				).ConfigureAwait(true);
		}

#nullable enable
		public async Task SaveAnnotations(string? filePrefix)
		{
			if (AnnotationLayers.Count > 0)
			{
				if (tifIndex >= 0)
				{

					string path = System.IO.Path.GetDirectoryName(Dir) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Dir);

					try
					{
						// Determine whether the directory exists.
						if (!Directory.Exists(path))
						{
							// Try to create the directory.
							DirectoryInfo di = Directory.CreateDirectory(path);
							Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
						}

						if (filePrefix == null)
						{
							filePrefix = path + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(AnnotationFilePrefix);
							Console.WriteLine(filePrefix);
						}
						else
						{
							AnnotationFilePrefix = filePrefix;
						}
						using (var fs = new FileStream(filePrefix, FileMode.Create, FileAccess.Write))
						using (var ms = new MemoryStream())
						{
							TiffBitmapEncoder encoder = new TiffBitmapEncoder();
							foreach (var AnnotationLayer in AnnotationLayers)
							{
								/* **************
								 * Title : the annotation layer's name
								 * Subject : the layer's color infomation
								 * Copyright : the Uid of the layer for UI operation (Generate rule : UidGenerator())
								 * **************/
								BitmapMetadata bitmapMetadata = new BitmapMetadata("tiff");
								bitmapMetadata.Title = AnnotationLayer.Title;
								bitmapMetadata.Subject = AnnotationLayer.AnnotationColor.ToString(NumberFormatInfo.InvariantInfo);
								bitmapMetadata.Copyright = AnnotationLayer.Uid;
								encoder.Frames.Add(BitmapFrame.Create(AnnotationLayer.Layer.Clone(), null, bitmapMetadata, null));
							}
							encoder.Compression = TiffCompressOption.Zip;
							encoder.Save(ms);
							ms.Position = 0;
							await ms.CopyToAsync(fs).ConfigureAwait(true);
						}
						if (Util.AppConfig("IS_SAVE_PNG", false))
						{
							using (var fs = new FileStream(filePrefix + ".png", FileMode.Create, FileAccess.Write))
							using (var ms = new MemoryStream())
							{
								PngBitmapEncoder encoder = new PngBitmapEncoder();
								encoder.Frames.Add(BitmapFrame.Create(GrayScaleAnnotationMaskImage));
								encoder.Save(ms);
								ms.Position = 0;
								await ms.CopyToAsync(fs).ConfigureAwait(true);
							}
						}
						IsAnnotationDirty = false;

					}
					catch (Exception e)
					{
						MessageBox.Show("The create '" + path + "' folder failed: " + e.ToString());
					}
					finally { }
				}
				else
				{
					if (System.IO.Path.GetExtension(AnnotationFilePrefix).ToLower(CultureInfo.CurrentCulture) == ".png")
					{
						filePrefix = System.IO.Path.GetDirectoryName(Dir) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Dir) + ".jellox";
					}
					if (filePrefix == null)
					{
						filePrefix = AnnotationFilePrefix;
					}
					else
					{
						AnnotationFilePrefix = filePrefix;
					}
					try
					{
						Console.WriteLine(filePrefix);
						using (var fs = new FileStream(filePrefix, FileMode.Create, FileAccess.Write))
						using (var ms = new MemoryStream())
						{
							TiffBitmapEncoder encoder = new TiffBitmapEncoder();
							foreach (var AnnotationLayer in AnnotationLayers)
							{
								/* **************
								 * Title : the annotation layer's name
								 * Subject : the layer's color infomation
								 * Copyright : the Uid of the layer for UI operation (Generate rule : UidGenerator())
								 * **************/
								BitmapMetadata bitmapMetadata = new BitmapMetadata("tiff");
								bitmapMetadata.Title = AnnotationLayer.Title;
								bitmapMetadata.Subject = AnnotationLayer.AnnotationColor.ToString(NumberFormatInfo.InvariantInfo);
								bitmapMetadata.Copyright = AnnotationLayer.Uid;
								encoder.Frames.Add(BitmapFrame.Create(AnnotationLayer.Layer.Clone(), null, bitmapMetadata, null));
							}


							encoder.Compression = TiffCompressOption.Zip;
							encoder.Save(ms);
							ms.Position = 0;
							await ms.CopyToAsync(fs).ConfigureAwait(true);
						}
						if (Util.AppConfig("IS_SAVE_PNG", false))
						{
							using (var fs = new FileStream(filePrefix + ".png", FileMode.Create, FileAccess.Write))
							using (var ms = new MemoryStream())
							{
								PngBitmapEncoder encoder = new PngBitmapEncoder();
								encoder.Frames.Add(BitmapFrame.Create(GrayScaleAnnotationMaskImage));
								encoder.Save(ms);
								ms.Position = 0;
								await ms.CopyToAsync(fs).ConfigureAwait(true);
							}
						}
						IsAnnotationDirty = false;
					}
					catch (UnauthorizedAccessException)
					{
						messagebox.Show("File save access denied, you have no authorize to modify the file path. \nFile not save.", "UnauthorizedAccessException", MessageBoxButton.OK, MessageBoxImage.Warning);
					}
				}
			}
		}
		public async Task ExportAnnotation(string filePrefix)
		{
			if (AnnotationLayers.Count > 0)
			{
					try
					{
						
						AnnotationFilePrefix = filePrefix;
						
						
							using (var fs = new FileStream(filePrefix, FileMode.Create, FileAccess.Write))
							using (var ms = new MemoryStream())
							{
								PngBitmapEncoder encoder = new PngBitmapEncoder();
								encoder.Frames.Add(BitmapFrame.Create(PngAnnotationImage));
								encoder.Save(ms);
								ms.Position = 0;
								await ms.CopyToAsync(fs).ConfigureAwait(true);
							}
						
						IsAnnotationDirty = false;

					}
					catch (Exception e)
					{
						MessageBox.Show("The create '" + filePrefix + "' file failed: " + e.ToString());
					}
					finally { }
				
			}
		}
#nullable disable

		public void DrawPolygon(int layerIndex, List<Point> points)
		{
			List<int> intPoints = new List<int>();
			foreach (var p in points)
			{
				intPoints.Add((int)p.X);
				intPoints.Add((int)p.Y);
			}
			intPoints.Add((int)points[0].X);
			intPoints.Add((int)points[0].Y);
			using (AnnotationLayers[layerIndex].Layer.GetBitmapContext())
			{
				AnnotationLayers[layerIndex].Layer.FillPolygon(intPoints.ToArray(), Color.FromArgb(20, 0, 0, 0));
			}
			IsAnnotationDirty = true;
		}

		public void DrawLine(int layerIndex, Point start, Point end, Point contourStart, Point contourEnd)
		{
			var vec = end - start;
			var contourVec = contourEnd - contourStart;

			var unionVec = vec / vec.Length;
			var unionContourVec = contourVec / vec.Length;
			for (int i = 0; i < vec.Length; i += 5)
			{
				Draw(layerIndex, start + i * unionVec, contourStart + i * unionContourVec);

			}

			IsAnnotationDirty = true;
		}
		private WriteableBitmap cursor;

		public void CursorSize(int radius)
		{
			cursor = Resource.WriteableBitmapCursor;
		}

		public void Draw(int layerIndex, Point center, Point contourCenter)
		{
			var X = MathHelper.Clip(center.X + 0.5, 0, AnnotationLayers[layerIndex].Layer.Width);
			var Y = MathHelper.Clip(center.Y + 0.5, 0, AnnotationLayers[layerIndex].Layer.Height);

			try
			{
				// Reserve the back buffer for updates.
				int cropLeft = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Width / 2 - X, 0));
				int cropRight = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Width / 2 - (AnnotationLayers[layerIndex].Layer.Width - X), 0));
				int cropTop = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Height / 2 - Y, 0));
				int cropButton = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Height / 2 - (AnnotationLayers[layerIndex].Layer.Height - Y), 0));
				AnnotationLayers[layerIndex].Layer.Lock();


				unsafe
				{
					IntPtr pBackBuffer = AnnotationLayers[layerIndex].Layer.BackBuffer;
					IntPtr CursorBackBuffer = Resource.WriteableBitmapContour.BackBuffer;

					IntPtr cBackBuffer = Resource.WriteableBitmapCursor.BackBuffer;

					pBackBuffer += (int)(Y - Resource.WriteableBitmapCursor.Height / 2) * AnnotationLayers[layerIndex].Layer.BackBufferStride;
					pBackBuffer += (int)(X - Resource.WriteableBitmapCursor.Width / 2) * 4;
					CursorBackBuffer += (int)(Y - Resource.WriteableBitmapCursor.Height / 2) * AnnotationLayers[layerIndex].Layer.BackBufferStride;
					CursorBackBuffer += (int)(X - Resource.WriteableBitmapCursor.Width / 2) * 4;
					// Get a pointer to the back buffer.
					// Find the address of the pixel to draw.
					// Compute the pixel's color.


					int BackBufferStride = AnnotationLayers[layerIndex].Layer.BackBufferStride;
					int AnnotationColorInt = ColorHelper.IntColorFromARGB(20, 0, 0, 0);
					Parallel.For(cropTop, (int)Resource.WriteableBitmapCursor.Height - cropButton, y =>
					{
						int y_bias = y * BackBufferStride;
						int x_bias;
						IntPtr AnnotationDataPtr;
						IntPtr _AnnotationDataPtr;

						int CursorData;
						for (int x = cropLeft; x < Resource.WriteableBitmapCursor.Width - cropRight; x++)
						{
							x_bias = x << 2;

							CursorData = *((int*)(cBackBuffer + y * Resource.WriteableBitmapCursor.BackBufferStride + x_bias));
							if (CursorData == 0)
								continue;

							// Find the address of the pixel to draw.

							AnnotationDataPtr = pBackBuffer + y_bias + x_bias;
							if (CursorData == -65536)
							{
								*((int*)(AnnotationDataPtr)) = AnnotationColorInt;
							}
							
						}
					});
				}
				// Specify the area of the bitmap that changed.				
				AnnotationLayers[layerIndex].Layer.AddDirtyRect(
					new Int32Rect(
						(int)(X - Resource.WriteableBitmapCursor.Width / 2) + cropLeft,
						(int)(Y - Resource.WriteableBitmapCursor.Height / 2) + cropTop,
						(int)Resource.WriteableBitmapCursor.Width - (cropRight),
						(int)Resource.WriteableBitmapCursor.Height - (cropButton)));

				// Annotation shower draw
				int yoffset = 0;
				if (Resource.MainWindow.mainViewer.ActualHeight * Resource.MainWindow.scaleTransform.ScaleY <= Resource.MainWindow.scrollViewer.ActualHeight)
				{
					yoffset = (int)(Resource.MainWindow.scrollViewer.ActualHeight - (Resource.MainWindow.mainViewer.ActualHeight * Resource.MainWindow.scaleTransform.ScaleY)) / 2;
				}

				int xoffset = 0;// If there is blank space in scrollview the contour viewer start with 0
				if (Resource.MainWindow.mainViewer.ActualWidth * Resource.MainWindow.scaleTransform.ScaleX <= Resource.MainWindow.scrollViewer.ActualWidth)
				{
					xoffset = (int)(Resource.MainWindow.scrollViewer.ActualWidth - (Resource.MainWindow.mainViewer.ActualWidth * Resource.MainWindow.scaleTransform.ScaleX)) / 2;
				}

				int contourX = (int)contourCenter.X;
				if (X >= AnnotationLayers[layerIndex].Layer.Width || X <= 0)
					contourX = MathHelper.Clip((int)contourCenter.X, xoffset, (int)Resource.WriteableBitmapContour.Width - xoffset);

				int contourY = (int)contourCenter.Y;
				if (Y >= AnnotationLayers[layerIndex].Layer.Height || Y <= 0)
					contourY = MathHelper.Clip((int)contourCenter.Y, yoffset, (int)Resource.WriteableBitmapContour.Height - yoffset);

				var radius = Resource.WriteableBitmapContourCursor.Height / 2;
				var OutlineColor = ColorHelper.IntColorFromARGB(255,
					AnnotationLayers[layerIndex].AnnotationColor.R,
					AnnotationLayers[layerIndex].AnnotationColor.G,
					AnnotationLayers[layerIndex].AnnotationColor.B);

				cropLeft = (int)(Math.Max(radius - contourX, 0));
				cropRight = (int)(Math.Max(radius - (Resource.WriteableBitmapContour.Width - xoffset - contourX), 0));
				cropTop = (int)(Math.Max(radius - contourY, 0));
				cropButton = (int)(Math.Max(radius - (Resource.WriteableBitmapContour.Height - yoffset - contourY), 0));
				Resource.WriteableBitmapContour.Lock();
				unsafe
				{
					IntPtr pBackBuffer = Resource.WriteableBitmapContour.BackBuffer;
					IntPtr cBackBuffer = Resource.WriteableBitmapContourCursor.BackBuffer;

					pBackBuffer += (int)(contourY - radius) * Resource.WriteableBitmapContour.BackBufferStride;
					pBackBuffer += (int)(contourX - radius) * 4;
					int BackBufferStride = Resource.WriteableBitmapContour.BackBufferStride;
					int AnnotationColorInt = ColorHelper.IntColorFromARGB(20, 0, 0, 0);
					Parallel.For(cropTop, (int)Resource.WriteableBitmapContourCursor.Height - cropButton, y =>
					{

						int y_bias = y * BackBufferStride;
						int x_bias;
						IntPtr AnnotationDataPtr;
						int CursorData;
						for (int x = cropLeft; x < Resource.WriteableBitmapContourCursor.Width - cropRight; x++)
						{

							x_bias = x << 2;
							CursorData = *((int*)(cBackBuffer + y * Resource.WriteableBitmapContourCursor.BackBufferStride + x_bias));
							if (CursorData == 0)
								continue;

							AnnotationDataPtr = pBackBuffer + y_bias + x_bias;
							// Assign the color data to the pixel.
							if (CursorData == -65536)
							{
								*((int*)(AnnotationDataPtr)) = AnnotationColorInt;
							}
							byte R = (byte)(CursorData >> 16 & 255);
							byte G = (byte)(CursorData >> 8 & 255);
							if (*((int*)(AnnotationDataPtr)) != AnnotationColorInt && (R != 0))
							{
								*((int*)(AnnotationDataPtr)) = OutlineColor;
							}
						}
					});
				}

				Resource.WriteableBitmapContour.AddDirtyRect(
					new Int32Rect(
						MathHelper.Clip((int)(contourX - Resource.WriteableBitmapContourCursor.Width / 2) + cropLeft, xoffset, (int)Resource.WriteableBitmapContour.Width),
						MathHelper.Clip((int)(contourY - Resource.WriteableBitmapContourCursor.Height / 2) + cropTop, yoffset, (int)Resource.WriteableBitmapContour.Height),
						MathHelper.Clip((int)Resource.WriteableBitmapContourCursor.Width - (cropRight), 0, (int)Math.Min(Resource.WriteableBitmapContourCursor.Width, Resource.WriteableBitmapContour.Width)),
						MathHelper.Clip((int)Resource.WriteableBitmapContourCursor.Height - (cropButton), 0, (int)Math.Min(Resource.WriteableBitmapContourCursor.Height, Resource.WriteableBitmapContour.Height))
						));
			}
			finally
			{
				// Release the back buffer and make it available for display.
				AnnotationLayers[layerIndex].Layer.Unlock();
				Resource.WriteableBitmapContour.Unlock();
			}

			IsAnnotationDirty = true;
		}




		public void Erase(int layerIndex, Point center, Point contourCenter)
		{
			var X = MathHelper.Clip(center.X + 0.5, 0, AnnotationLayers[layerIndex].Layer.Width);
			var Y = MathHelper.Clip(center.Y + 0.5, 0, AnnotationLayers[layerIndex].Layer.Height);
			try
			{
				// Reserve the back buffer for updates.
				int cropLeft = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Width / 2 - X, 0));
				int cropRight = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Width / 2 - (AnnotationLayers[layerIndex].Layer.Width - X), 0));
				int cropTop = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Height / 2 - Y, 0));
				int cropButton = (int)(System.Math.Max(Resource.WriteableBitmapCursor.Height / 2 - (AnnotationLayers[layerIndex].Layer.Height - Y), 0));
				AnnotationLayers[layerIndex].Layer.Lock();


				unsafe
				{
					IntPtr pBackBuffer = AnnotationLayers[layerIndex].Layer.BackBuffer;
					IntPtr CursorBackBuffer = Resource.WriteableBitmapContour.BackBuffer;

					IntPtr cBackBuffer = Resource.WriteableBitmapCursor.BackBuffer;

					pBackBuffer += (int)(Y - Resource.WriteableBitmapCursor.Height / 2) * AnnotationLayers[layerIndex].Layer.BackBufferStride;
					pBackBuffer += (int)(X - Resource.WriteableBitmapCursor.Width / 2) * 4;
					CursorBackBuffer += (int)(Y - Resource.WriteableBitmapCursor.Height / 2) * AnnotationLayers[layerIndex].Layer.BackBufferStride;
					CursorBackBuffer += (int)(X - Resource.WriteableBitmapCursor.Width / 2) * 4;
					// Get a pointer to the back buffer.
					// Find the address of the pixel to draw.
					// Compute the pixel's color.

					int BackBufferStride = AnnotationLayers[layerIndex].Layer.BackBufferStride;
					int AnnotationColorInt = ColorHelper.IntColorFromARGB(20, 0, 0, 0);
					Parallel.For(cropTop, (int)Resource.WriteableBitmapCursor.Height - cropButton, y =>
					{
						int y_bias = y * BackBufferStride;
						int x_bias;
						IntPtr AnnotationDataPtr;
						IntPtr _AnnotationDataPtr;

						int CursorData;
						for (int x = cropLeft; x < Resource.WriteableBitmapCursor.Width - cropRight; x++)
						{
							x_bias = x << 2;

							CursorData = *((int*)(cBackBuffer + y * Resource.WriteableBitmapCursor.BackBufferStride + x_bias));
							if (CursorData == 0)
								continue;

							// Find the address of the pixel to draw.

							AnnotationDataPtr = pBackBuffer + y_bias + x_bias;
							// Assign the color data to the pixel.
							if (CursorData != 0)
							{
								*((int*)(AnnotationDataPtr)) = 0;
							}
						}
					});
				}
				// Specify the area of the bitmap that changed.				
				AnnotationLayers[layerIndex].Layer.AddDirtyRect(
					new Int32Rect(
						(int)(X - Resource.WriteableBitmapCursor.Width / 2) + cropLeft,
						(int)(Y - Resource.WriteableBitmapCursor.Height / 2) + cropTop,
						(int)Resource.WriteableBitmapCursor.Width - (cropRight),
						(int)Resource.WriteableBitmapCursor.Height - (cropButton)));
				// Annotation shower draw
				int yoffset = 0;
				if (Resource.MainWindow.mainViewer.ActualHeight * Resource.MainWindow.scaleTransform.ScaleY <= Resource.MainWindow.scrollViewer.ActualHeight)
				{
					yoffset = (int)(Resource.MainWindow.scrollViewer.ActualHeight - (Resource.MainWindow.mainViewer.ActualHeight * Resource.MainWindow.scaleTransform.ScaleY)) / 2;
				}

				int xoffset = 0;// If there is blank space in scrollview the contour viewer start with 0
				if (Resource.MainWindow.mainViewer.ActualWidth * Resource.MainWindow.scaleTransform.ScaleX <= Resource.MainWindow.scrollViewer.ActualWidth)
				{
					xoffset = (int)(Resource.MainWindow.scrollViewer.ActualWidth - (Resource.MainWindow.mainViewer.ActualWidth * Resource.MainWindow.scaleTransform.ScaleX)) / 2;
				}

				int contourX = (int)contourCenter.X;
				if (X >= AnnotationLayers[layerIndex].Layer.Width || X <= 0)
					contourX = MathHelper.Clip((int)contourCenter.X, xoffset, (int)Resource.WriteableBitmapContour.Width - xoffset);

				int contourY = (int)contourCenter.Y;
				if (Y >= AnnotationLayers[layerIndex].Layer.Height || Y <= 0)
					contourY = MathHelper.Clip((int)contourCenter.Y, yoffset, (int)Resource.WriteableBitmapContour.Height - yoffset);
				var OutlineColor = ColorHelper.IntColorFromARGB(255,
					AnnotationLayers[layerIndex].AnnotationColor.R,
					AnnotationLayers[layerIndex].AnnotationColor.G,
					AnnotationLayers[layerIndex].AnnotationColor.B);
				var radius = Resource.WriteableBitmapContourCursor.Height / 2;
				cropLeft = (int)(Math.Max(radius - contourX, 0));
				cropRight = (int)(Math.Max(radius - (Resource.WriteableBitmapContour.Width - xoffset - contourX), 0));
				cropTop = (int)(Math.Max(radius - contourY, 0));
				cropButton = (int)(Math.Max(radius - (Resource.WriteableBitmapContour.Height - yoffset - contourY), 0));
				Resource.WriteableBitmapContour.Lock();
				unsafe
				{
					IntPtr pBackBuffer = Resource.WriteableBitmapContour.BackBuffer;
					IntPtr cBackBuffer = Resource.WriteableBitmapContourCursor.BackBuffer;

					pBackBuffer += (int)(contourY - radius) * Resource.WriteableBitmapContour.BackBufferStride;
					pBackBuffer += (int)(contourX - radius) * 4;
					int BackBufferStride = Resource.WriteableBitmapContour.BackBufferStride;
					int AnnotationColorInt = ColorHelper.IntColorFromARGB(20, 0, 0, 0);
					Parallel.For(cropTop, (int)Resource.WriteableBitmapContourCursor.Height - cropButton, y =>
					{

						int y_bias = y * BackBufferStride;
						int x_bias;
						IntPtr AnnotationDataPtr;
						int CursorData;
						for (int x = cropLeft; x < Resource.WriteableBitmapContourCursor.Width - cropRight; x++)
						{

							x_bias = x << 2;
							CursorData = *((int*)(cBackBuffer + y * Resource.WriteableBitmapContourCursor.BackBufferStride + x_bias));
							if (CursorData == 0)
								continue;

							AnnotationDataPtr = pBackBuffer + y_bias + x_bias;
							// Assign the color data to the pixel.
							byte R = (byte)(CursorData >> 16 & 255);
							byte G = (byte)(CursorData >> 8 & 255);
							if (G != 0 && *((int*)(AnnotationDataPtr)) != 0)
							{
								*((int*)(AnnotationDataPtr)) = OutlineColor;
							}
							if (R != 0)
							{
								*((int*)(AnnotationDataPtr)) = 0;
							}


						}
					});
				}

				Resource.WriteableBitmapContour.AddDirtyRect(
					new Int32Rect(
						MathHelper.Clip((int)(contourX - Resource.WriteableBitmapContourCursor.Width / 2) + cropLeft, xoffset, (int)Resource.WriteableBitmapContour.Width),
						MathHelper.Clip((int)(contourY - Resource.WriteableBitmapContourCursor.Height / 2) + cropTop, yoffset, (int)Resource.WriteableBitmapContour.Height),
						MathHelper.Clip((int)Resource.WriteableBitmapContourCursor.Width - (cropRight), 0, (int)Resource.WriteableBitmapContourCursor.Width),
						MathHelper.Clip((int)Resource.WriteableBitmapContourCursor.Height - (cropButton), 0, (int)Resource.WriteableBitmapContourCursor.Height)
						));
			}
			finally
			{
				// Release the back buffer and make it available for display.
				AnnotationLayers[layerIndex].Layer.Unlock();
				Resource.WriteableBitmapContour.Unlock();
			}
			IsAnnotationDirty = true;
		}

		public void EraseLine(int layerIndex, Point start, Point end, Point contourStart, Point contourEnd)
		{
			var vec = end - start;
			var contourVec = contourEnd - contourStart;
			var unionVec = vec / vec.Length;
			var unionContourVec = contourVec / vec.Length;
			for (int i = 0; i < vec.Length; i += 5)
			{
				Erase(layerIndex, start + i * unionVec, contourStart + i * unionContourVec);
			}

			IsAnnotationDirty = true;
		}

		Stack<int[]> FillStack = new Stack<int[]>();
		public void Fill(int layerIndex, Point center)
		{
			int X = (int)MathHelper.Clip(center.X, 0, AnnotationLayers[layerIndex].Layer.Width);
			int Y = (int)MathHelper.Clip(center.Y, 0, AnnotationLayers[layerIndex].Layer.Height);
			int AnnotationColorInt = ColorHelper.IntColorFromARGB(20, 0, 0, 0);

			try
			{
				// Reserve the back buffer for updates.
				AnnotationLayers[layerIndex].Layer.Lock();


				unsafe
				{
					int BackBufferStride = AnnotationLayers[layerIndex].Layer.BackBufferStride;
					IntPtr pBackBuffer = AnnotationLayers[layerIndex].Layer.BackBuffer;
					Stack<int> road = new Stack<int>();
					int y_bias = Y * BackBufferStride;
					int x_bias = X << 2;
					*((int*)(pBackBuffer + y_bias + x_bias)) = AnnotationColorInt;
					int[] dirction = new int[8];

					dirction[0] = -1 * BackBufferStride - 4;
					dirction[1] = -1 * BackBufferStride;
					dirction[2] = -1 * BackBufferStride + 4;
					dirction[3] = +4;
					dirction[4] = BackBufferStride + 4;
					dirction[5] = BackBufferStride;
					dirction[6] = BackBufferStride - 4;
					dirction[7] = -4;

					road.Push(y_bias + x_bias);
					int nowdirection = 0;
					while (road.Count > 0)
					{
						int tmp = road.Pop();
						IntPtr tmpPtr;
						for (int i = 0; i < 8; i++)
						{
							tmpPtr = pBackBuffer + MathHelper.Clip(tmp + dirction[(nowdirection + i) % 8], 0, (AnnotationLayers[layerIndex].Layer.PixelHeight - 1) * BackBufferStride + (AnnotationLayers[layerIndex].Layer.PixelWidth - 1) * 4);
							if (*((int*)(tmpPtr)) == 0)
							{
								*((int*)(tmpPtr)) = AnnotationColorInt;
								road.Push(tmp);
								road.Push(tmp + dirction[(nowdirection + i) % 8]);
								nowdirection = (nowdirection + i) % 8;
								break;
							}
						}
					}
				}
				// Specify the area of the bitmap that changed.				
				AnnotationLayers[layerIndex].Layer.AddDirtyRect(
					new Int32Rect(
						0,
						0,
						(int)AnnotationLayers[layerIndex].Layer.Width,
						(int)AnnotationLayers[layerIndex].Layer.Height));

			}
			finally
			{
				// Release the back buffer and make it available for display.
				AnnotationLayers[layerIndex].Layer.Unlock();
			}
			IsAnnotationDirty = true;
		}
		public void _Fill(int layerIndex, Point center)
		{
			var L = AnnotationLayers[layerIndex].Layer;
			var X = (int)MathHelper.Clip(center.X + 0.5, 0, L.Width);
			var Y = (int)MathHelper.Clip(center.Y + 0.5, 0, L.Height);

			Color replacementColor = AnnotationLayers[selectedLayerIndex].AnnotationColor;
			Color targetColor = Colors.Transparent;

			var bmp = AnnotationLayers[selectedLayerIndex].Layer;

			Queue<Point> q = new Queue<Point>();
			q.Enqueue(new Point(X, Y));
			while (q.Count > 0)
			{
				Point n = q.Dequeue();
				if (bmp.GetPixel((int)n.X, (int)n.Y).A != 0)
					continue;
				Point w = n, e = new Point(n.X + 1, n.Y);
				while ((w.X >= 0) && bmp.GetPixel((int)w.X, (int)w.Y).A == 0)
				{
					bmp.SetPixel((int)w.X, (int)w.Y, replacementColor);
					if ((w.Y > 0) && bmp.GetPixel((int)w.X, (int)w.Y - 1).A == 0)
						q.Enqueue(new Point(w.X, w.Y - 1));
					if ((w.Y < bmp.Height - 1) && bmp.GetPixel((int)w.X, (int)w.Y + 1).A == 0)
						q.Enqueue(new Point(w.X, w.Y + 1));
					w.X--;
				}
				while ((e.X <= bmp.Width - 1) && bmp.GetPixel((int)e.X, (int)e.Y).A == 0)
				{
					bmp.SetPixel((int)e.X, (int)e.Y, replacementColor);
					if ((e.Y > 0) && bmp.GetPixel((int)e.X, (int)e.Y - 1).A == 0)
						q.Enqueue(new Point(e.X, e.Y - 1));
					if ((e.Y < bmp.Height - 1) && bmp.GetPixel((int)e.X, (int)e.Y + 1).A == 0)
						q.Enqueue(new Point(e.X, e.Y + 1));
					e.X++;
				}
			}
		}

		public void SetToHHAIModule()
		{
			AIModule = new HHAIModule(AnnotationWidth, AnnotationHeight, this);
		}

		public void SetToJelloXAIModule(string moduleName)
		{
			AIModule = new JelloxAIModule(AnnotationWidth, AnnotationHeight, this, moduleName);
		}


		public void Dispose()
		{
			OriginalImageFileStream.Dispose();
			Slide.Dispose();
		}
	}
}
