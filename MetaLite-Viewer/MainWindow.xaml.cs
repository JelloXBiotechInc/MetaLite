using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Reactive.Linq;
using MetaLite_Viewer.Helper;
using MetaLite_Viewer.Model;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Window = System.Windows.Window;
using Polygon = System.Windows.Shapes.Polygon;
using MathHelper = MetaLite_Viewer.Helper.MathHelper;
using ColorHelper = MetaLite_Viewer.Helper.ColorHelper;
using LiveCharts;
using HtmlAgilityPack;
using MetaLite_Viewer.Subwindow;
using System.Windows.Media.Effects;
using MahApps.Metro.Controls;
using OpenCvSharp.WpfExtensions;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Dicom;
using Dicom.Imaging;
namespace MetaLite_Viewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const int OPENDICOMAS3D = 0;
		public const int OPENDICOMASWSI = 1;
		public const int OPENSLIDEWSI = -2;
		private static DropOutStack<UndoData> undoStack = new DropOutStack<UndoData>(4);
		private static DropOutStack<UndoData> redoStack = new DropOutStack<UndoData>(4);
		private ImageDrawing? originalImageDrawing;
		private DrawingGroup? originalImageDrawingGroup;

		GeometryDrawing zoomRect;
		double zoom = 1;
		private Queue<Resource.DelayViewerWorker> viewProb = new Queue<Resource.DelayViewerWorker>();
		private List<Resource.DelayViewerWorker> viewRunning = new List<Resource.DelayViewerWorker>();
		public long viewTimeTick = -1;
		private int garbage = 0;

		public MainWindow()
		{
			for (int i = 0; i < cacheRange; i++)
			{
				hashCached.Add(-1);
				cacheBitmapImage.Add(null);
			}

			InitializeComponent();
			VersionGenerator();
			CheckVersion();
			HugeViewConsumer();
			HugeViewProducer();

			ImportPlugin();
			Binding_Command();

			RecentFileList.MenuClick += (s, e) => OpenFilesFunction(new string[] { e.Filepath });

			Resource.progressBarMax = -1;

			EmbeddingAnnotationSelectboard();

			MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
			SliderGrid.Visibility = Visibility.Collapsed;
			statusProgressBar.Visibility = Visibility.Collapsed;
			BackgroundWorker serverProber = new BackgroundWorker();
			serverProber.DoWork += (s, e) => {
				Thread.CurrentThread.Priority = ThreadPriority.Lowest;
				while (true)
				{
					if (ProbServer() < 0)
					{
						this.Dispatcher.BeginInvoke(new Action(() => { logoGray.Visibility = Visibility.Visible; }));
					}
					else
					{
						this.Dispatcher.BeginInvoke(new Action(() => { logoGray.Visibility = Visibility.Hidden; }));
					}
					Thread.Sleep(1000);
				}
			};
			serverProber.RunWorkerAsync();

			scrollViewerRefresher.DoWork += (s, e) =>
			{
				while (true)
				{
					this.Dispatcher.BeginInvoke(new Action(() =>
					{
						if (scrollViewer.IsMouseCaptured || true)
						{
							DrawContour();
							
						}


					}));
					Thread.Sleep(10);
				}
			};
		}
		
		#region initialize
		private bool Connectable = false;
		public static long Latency { get; set; }
		private string metaLiteServerStatus { get; set; } = "init";
		public string MetaLiteServerStatus { get { return metaLiteServerStatus; } }

		private DispatcherTimer _frameTimer;
		private void OnSvsFrame(object sender, EventArgs e)
		{
			try
			{
				if (Resource.DelayViewBuffer.Count > 0)
				{
					var tmpBuffer = Resource.DelayViewBuffer.Pop();
					if (tmpBuffer != null)
					{
						if (viewTimeTick < tmpBuffer.TimeStamp)
						{
							viewTimeTick = tmpBuffer.TimeStamp;

							Resource.DelayViewBuffer.Clear();
							Resource.View = tmpBuffer.Data;

							garbage += 1;
							
								try
								{
									Resource.FromArray();
								}
								catch (Exception ex)
								{
									Console.WriteLine(ex);
								}

							
						}
						if (garbage > 100)
						{
							GC.Collect();
							garbage = 0;
						}
					}

				}

				if (viewProb.Count > 0 && viewRunning.Count < 8)
				{
					var tmpBK = viewProb.Dequeue();

					if (viewTimeTick < tmpBK.TimeStamp && tmpBK != null)
					{

						lock (viewRunning)
						{
							viewRunning.Add(tmpBK);
						}
						tmpBK.Worker.RunWorkerAsync();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private void OnSvsFrame2(object sender, EventArgs e)
		{
			if (Resource.isDataReaded)
			{
				if (Resource.nowImageData.isHugeImage)
				{
					var imageActualWidth = mainViewer.ActualWidth * zoom;
					var imageActualHeight = mainViewer.ActualHeight * zoom;
					var scrollViewer_Height = scrollViewer.ActualHeight;
					var scrollViewer_Width = scrollViewer.ActualWidth;
					var scrollViewer_VerticalOffset = scrollViewer.VerticalOffset;
					var scrollViewer_HorizontalOffset = scrollViewer.HorizontalOffset;
					lock (Resource.nowImageData)
					{
						if (Resource.nowImageData == null) return;
						Resource.nowImageData.GetHugeView(imageActualWidth, imageActualHeight, scrollViewer_Width, scrollViewer_Height, scrollViewer_HorizontalOffset, scrollViewer_VerticalOffset, zoom, DateTime.Now.Ticks);

					}
					Resource.FromArray();
				}
				else
				{
				}
			}
		}
		private void HugeViewProducer()
		{
			_frameTimer = new DispatcherTimer();
			_frameTimer.Tick += OnSvsFrame2;
			_frameTimer.Interval = TimeSpan.FromSeconds(1.0 / 24.0);
			_frameTimer.Start();
		}
		private void HugeViewConsumer()
		{
			_frameTimer = new DispatcherTimer(DispatcherPriority.Render);
			_frameTimer.Tick += OnSvsFrame;
			_frameTimer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
			_frameTimer.Start();
			return;
		}

		private void VersionGenerator()
		{
			var path = GetThisFilePath();
			var directory = System.IO.Path.GetDirectoryName(path);
			if (Directory.Exists(directory))
			{
				using (StreamWriter writer = new StreamWriter(directory + "\\version.txt", false))
				{
					writer.Write(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
				}
			}
		}

		private static string GetThisFilePath([CallerFilePath] string path = null)
		{
			return path;
		}

		public static string CheckVersion()
		{
			try
			{
				var html = "https://raw.githubusercontent.com/JelloXBiotechInc/MetaLite/main/MetaLite-Viewer/version.txt";
				HtmlWeb web = new HtmlWeb() { UseCookies = false };
				var htmlDoc = web.LoadFromWebAsync(html);
				if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version < new Version(htmlDoc.Result.ParsedText))
				{
					return "Your MetaLite is OLD version(v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + "). \nPlease update the software to the latest release version: v" + htmlDoc.Result.ParsedText + ".";
				}
				else
					return "";
			}
			catch
			{
				return "Cannot get software version information please check the internet setting, \n\nor check the update information on www.jellox.com/metalite-metax manually.";
			}
		}

		private void ImportPlugin()
		{
			try
			{
				string[] dirs = Directory.GetFiles(@"Plugin", "*.json");
				foreach (string dir in dirs)
				{
					var Plugin = new MenuItem() { Header = System.IO.Path.GetFileNameWithoutExtension(dir) };
					Plugin.Uid = System.IO.Path.GetFileName(dir);
					Plugin.Click += PluginFunctionButtonClick;
					PluginFunctions.Items.Add(Plugin);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("The process failed: {0}", e.ToString());
			}

		}

		private void EmbeddingAnnotationSelectboard()
		{
			object childContent = Resource.AnnotationSelector.Content;
			Resource.AnnotationSelector.Content = null;
			annotationSelectorGrid.Children.Add(childContent as UIElement);
		}

		public static int ProbServer()
		{
			int latency = -1;
			try
			{
				IPEndPoint tIPEndPoint = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["METALITE_SERVER"]), int.Parse(ConfigurationManager.AppSettings["PORT"]));
				TcpClient tClient = new TcpClient();
				tClient.Connect(tIPEndPoint);
				latency = tClient.LingerState.LingerTime;
				tClient.Close();
				return latency;
			}
			catch
			{
				return latency;
			}
		}
		public void ServerLaunchCheck()
		{
			metaLiteServerStatus = "init";
			int latency = -1;
			BackgroundWorker bgWorker = new BackgroundWorker();
			bgWorker.DoWork += (s, e) =>
			{
				for (int i = 0; i < 20; i++)
				{
					Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Connecting to MetaLite Analyzer..."; }));
					try
					{
						latency = ProbServer();
						if (latency >= 0)
						{

							break;
						}
					}
					catch
					{
						latency = -2;
					}
				}
			};
			bgWorker.RunWorkerCompleted += (s, e) =>
			{
				if (latency < 0)
				{
					statusTextBlock.Text = "MetaLite Analyzer is unavailable";
					messagebox.Show("MetaLite Analyzer is unavailable.\n If you need AI inference or statistic method,\n please reboot the whole program or restart MetaLite Analyzer Manually.");
					metaLiteServerStatus = "dead";
				}
				else
				{
					statusTextBlock.Text = "MetaLite Analyzer is ready";
					metaLiteServerStatus = "ready";
				}
			};
			bgWorker.RunWorkerAsync();
		}
		#endregion

		public static void KillProcess()
		{
			var DriverProcesses = Process.GetProcesses().
				 Where(pr => pr.ProcessName == "metalite-server");
			foreach (var process in DriverProcesses)
			{
				process.Kill();
			}

		}

		#region Window Event
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Resource.MainWindow = this;
			Resource.ChannelSelector = channelSelector;

			Resource.ChannelSelector.RedIntensity.SetBinding(
				ProgressBar.ValueProperty, new Binding()
				{
					Source = Resource.RGBEffect,
					Path = new PropertyPath("R"),
					Mode = BindingMode.TwoWay
				});
			Resource.ChannelSelector.GreenIntensity.SetBinding(
				ProgressBar.ValueProperty, new Binding()
				{
					Source = Resource.RGBEffect,
					Path = new PropertyPath("G"),
					Mode = BindingMode.TwoWay
				});
			Resource.ChannelSelector.BlueIntensity.SetBinding(
				ProgressBar.ValueProperty, new Binding()
				{
					Source = Resource.RGBEffect,
					Path = new PropertyPath("B"),
					Mode = BindingMode.TwoWay
				});

			
			Resource.ChannelSelector.PSHematoxylinIntensity.SetBinding(
				ProgressBar.ValueProperty, new Binding()
				{
					Source = Resource.PseudoStainingEffect,
					Path = new PropertyPath("HematoxylinIntensity"),
					Mode = BindingMode.TwoWay
				});
			Resource.ChannelSelector.PSEosinIntensity.SetBinding(
				ProgressBar.ValueProperty, new Binding()
				{
					Source = Resource.PseudoStainingEffect,
					Path = new PropertyPath("EosinIntensity"),
					Mode = BindingMode.TwoWay
				});
			Resource.ChannelSelector.PSDabIntensity.SetBinding(
				ProgressBar.ValueProperty, new Binding()
				{
					Source = Resource.PseudoStainingEffect,
					Path = new PropertyPath("DabIntensity"),
					Mode = BindingMode.TwoWay
				});
			Resource.ChannelSelector.PSHematoxylinIntensity.SetBinding(
				ProgressBar.ForegroundProperty, new Binding()
				{
					Source = Resource.PseudoStainingEffect,
					Path = new PropertyPath("HematoxylinColor"),
					Mode = BindingMode.TwoWay,
					Converter = new ColorToBrushConverter()
				});
			Resource.ChannelSelector.PSEosinIntensity.SetBinding(
				ProgressBar.ForegroundProperty, new Binding()
				{
					Source = Resource.PseudoStainingEffect,
					Path = new PropertyPath("EosinColor"),
					Mode = BindingMode.TwoWay,
					Converter = new ColorToBrushConverter()
				});
			Resource.ChannelSelector.PSDabIntensity.SetBinding(
				ProgressBar.ForegroundProperty, new Binding()
				{
					Source = Resource.PseudoStainingEffect,
					Path = new PropertyPath("DabColor"),
					Mode = BindingMode.TwoWay,
					Converter = new ColorToBrushConverter()
				});

		}

		private void Binding_Command()
		{
			// Save
			CommandBinding SaveCmdBinding = new CommandBinding(
				ApplicationCommands.Save,
				SaveAnnotationMenuItem_Click,
				(s, e) => { e.CanExecute = (Resource.isDataReaded) && (Resource.isLayerExist); });
			this.CommandBindings.Add(SaveCmdBinding);

			// Open
			CommandBinding OpenCmdBinding = new CommandBinding(
				ApplicationCommands.Open,
				OpenFileMenuItem_Click);
			this.CommandBindings.Add(OpenCmdBinding);

			// Exit
			CommandBinding ExitCmdBinding = new CommandBinding(
				ApplicationCommands.Close,
				ExitMenuItem_Click
				);
			this.CommandBindings.Add(ExitCmdBinding);

			// Undo
			CommandBinding UndoCmdBinding = new CommandBinding(
				ApplicationCommands.Undo,
				Undo_Click,
				(s, e) => { e.CanExecute = undoStack.notEmpty; });
			this.CommandBindings.Add(UndoCmdBinding);

			// Redo
			RoutedCommand RedoCmd = new RoutedCommand();
			RedoCmd.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control | ModifierKeys.Shift));

			CommandBinding RedoCmdBinding = new CommandBinding(
				RedoCmd,
				Redo_Click,
				(s, e) => { e.CanExecute = redoStack.notEmpty; });
			MenuRedo.Command = RedoCmd;
			this.CommandBindings.Add(RedoCmdBinding);

			// Fill
			RoutedCommand FillCmd = new RoutedCommand();
			FillCmd.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));

			CommandBinding FillCmdBinding = new CommandBinding(
				FillCmd,
				(s, e) => { manualFillButton.RaiseEvent(new RoutedEventArgs(RadioButton.ClickEvent)); },
				(s, e) => { e.CanExecute = Resource.isDataReaded; });
			this.CommandBindings.Add(FillCmdBinding);

			// Help
			RoutedCommand ManualCmd = new RoutedCommand();
			ManualCmd.InputGestures.Add(new KeyGesture(Key.F1));

			CommandBinding ManualCmdBinding = new CommandBinding(
				ManualCmd,
				userManual_Click,
				(s, e) => { e.CanExecute = true; });
			userManual.Command = ManualCmd;
			this.CommandBindings.Add(ManualCmdBinding);

			// Clean
			RoutedCommand CleanCmd = new RoutedCommand();
			CleanCmd.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));

			CommandBinding CleanCmdBinding = new CommandBinding(
				CleanCmd,
				CleanAnnotationButtonClick,
				(s, e) => { e.CanExecute = Resource.isLayerExist; });
			MenuClean.Command = CleanCmd;
			this.CommandBindings.Add(CleanCmdBinding);

			// Select all
			RoutedCommand SelectAllCmd = new RoutedCommand();
			SelectAllCmd.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));

			CommandBinding SelectAllCmdBinding = new CommandBinding(
				SelectAllCmd,
				SelectAll,
				(s, e) => { e.CanExecute = Resource.isLayerExist; });
			this.CommandBindings.Add(SelectAllCmdBinding);

			// Save as
			RoutedCommand SaveAsCmd = new RoutedCommand();
			SaveAsCmd.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));

			CommandBinding SaveAsCmdBinding = new CommandBinding(
				SaveAsCmd,
				SaveAnnotationAsMenuItem_Click,
				(s, e) => { e.CanExecute = (Resource.isDataReaded) && (Resource.isLayerExist); });
			MenuSaveAs.Command = SaveAsCmd;
			this.CommandBindings.Add(SaveAsCmdBinding);

			// Save all
			RoutedCommand SaveAllCmd = new RoutedCommand();
			SaveAllCmd.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt));

			CommandBinding SaveAllCmdBinding = new CommandBinding(
				SaveAllCmd,
				SaveAllAnnotationMenuItem_Click,
				(s, e) => { e.CanExecute = (Resource.isDataReaded); });
			MenuSaveAll.Command = SaveAllCmd;
			this.CommandBindings.Add(SaveAllCmdBinding);

			// Export Annotation
			RoutedCommand ExportAnnotationCmd = new RoutedCommand();
			ExportAnnotationCmd.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));

			CommandBinding ExportAnnotationCmdBinding = new CommandBinding(
				ExportAnnotationCmd,
				ExportAnnotationMenuItem_Click,
				(s, e) => { e.CanExecute = (Resource.isLayerExist); });
			MenuExportAnnotation.Command = ExportAnnotationCmd;
			this.CommandBindings.Add(ExportAnnotationCmdBinding);

			// Open Annotation
			RoutedCommand OpenAnnotationCmd = new RoutedCommand();

			CommandBinding OpenAnnotationBinding = new CommandBinding(
				OpenAnnotationCmd,
				OpenAnnotationMenuItem_Click,
				(s, e) => { e.CanExecute = (Resource.isDataReaded); });
			MenuOpenAnnotation.Command = OpenAnnotationCmd;
			this.CommandBindings.Add(OpenAnnotationBinding);

			// Creating a KeyBinding between the Open command and Ctrl-R
			var exitCmdKeyBinding = new KeyBinding(
				ApplicationCommands.Close,
				Key.F4,
				ModifierKeys.Alt);

			InputBindings.Add(exitCmdKeyBinding);
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
			TextBox textBox = Keyboard.FocusedElement as TextBox;
			if (textBox != null)
			{
				TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
				textBox.MoveFocus(tRequest);
			}
		}

		private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void ScaleMenuItem_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		private void MaximizeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
		}

		private void ScaleWindowMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ResizeWindow();
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (Resource.AnnotationSelector != null)
			{
				Resource.AnnotationSelector.Close();
			}
			Close();
		}

		private void fileNameTextBlock_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				ResizeWindow();
			}
		}

		private void ResizeWindow()
		{
			WindowState ^= WindowState.Maximized ^ WindowState.Normal;
			if (WindowState == WindowState.Normal)
				sizemodebuttom.Content = "";
			else
				sizemodebuttom.Content = "";
			Position = position;
			UpdateDrawingCursor();
		}
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// Adjust the Window Margin to avoid cut window's edge when Maximized MainWindow
			if (WindowState == WindowState.Normal)
			{
				mainGrid.Margin = new Thickness(0);
			}
			else
			{
				mainGrid.Margin = new Thickness(5, 5, 5, 10);
			}
		}
		#endregion

		#region Whole Image Viewer Mouse Function

		private void WholeImageViewer_RightButtonDown(object sender, MouseButtonEventArgs e)
		{
			wholeImageViewer.CaptureMouse();
			e.Handled = true;
			DrawZoomByMouseMove(e);
		}

		private void WholeImageViewer_RightButtonUp(object sender, MouseButtonEventArgs e)
		{
			wholeImageViewer.ReleaseMouseCapture();
			if (!Resource.isDataReaded)
				return;
			DrawZoomByMouseMove(e);
		}

		private void WholeImageViewer_MouseMove(object sender, MouseEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			DrawZoomByMouseMove(e);
		}

		private void RightPanelHidden_Click(object sender, RoutedEventArgs e)
		{
			Duration duration = new Duration(TimeSpan.FromSeconds(0.25));
			DoubleAnimation doubleanimation;

			if (rightPanelTransform.ScaleX == 0)
			{
				RightPanelHidden_Text.Text = "";
				doubleanimation = new DoubleAnimation(1, duration);
				doubleanimation.AccelerationRatio = 0.5;
				doubleanimation.DecelerationRatio = 0.1;

			}
			else
			{
				RightPanelHidden_Text.Text = "";
				doubleanimation = new DoubleAnimation(0, duration);
				doubleanimation.AccelerationRatio = 0.5;
				doubleanimation.DecelerationRatio = 0.1;

			}

			rightPanelTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleanimation);
		}

		private void wholeImageViewer_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Point viewerPosition = e.GetPosition(wholeImageViewer);
			if (scrollViewer.ExtentHeight <= scrollViewer.ExtentWidth)
			{
				Position = new Point(
				(viewerPosition.X) * Resource.nowImageData.DataWidth / wholeImageViewer.ActualWidth,
				(scrollViewer.ExtentWidth / scrollViewer.ExtentHeight) * (viewerPosition.Y - ((scrollViewer.ExtentWidth - scrollViewer.ExtentHeight) / 2) * wholeImageViewer.ActualHeight / scrollViewer.ExtentWidth) * Resource.nowImageData.DataHeight / wholeImageViewer.ActualHeight
				);
			}
			else
			{
				Position = new Point(
				(scrollViewer.ExtentHeight / scrollViewer.ExtentWidth) * (viewerPosition.X - ((scrollViewer.ExtentHeight - scrollViewer.ExtentWidth) / 2) * wholeImageViewer.ActualWidth / scrollViewer.ExtentHeight) * Resource.nowImageData.DataWidth / wholeImageViewer.ActualWidth,
				(viewerPosition.Y) * Resource.nowImageData.DataHeight / wholeImageViewer.ActualHeight
				);
			}

			if (e.Delta > 0)
			{
				if (this.zoom < 2 && this.zoom >= 1)
				{
					Zoom += 0.1;
				}
				else
				{
					Zoom += 0.5;
				}
			}
			else if (e.Delta < 0)
			{
				if (this.zoom < 2 && this.zoom >= 1)
				{
					Zoom -= 0.1;
				}
				else
				{
					Zoom -= 0.5;
				}
			}
			if (CheckDoingFunction())
			{
				UpdateDrawingCursor();
			}

			DrawZoomRect();
		}

		private void DrawZoomByMouseMove(MouseEventArgs e)
		{
			if (e.RightButton == MouseButtonState.Pressed && Resource.isDataReaded)
			{
				Point viewerPosition = e.GetPosition(wholeImageViewer);
				if (scrollViewer.ExtentHeight <= scrollViewer.ExtentWidth)
				{
					Position = new Point(
					(viewerPosition.X) * Resource.nowImageData.DataWidth / wholeImageViewer.ActualWidth,
					(scrollViewer.ExtentWidth / scrollViewer.ExtentHeight) * (viewerPosition.Y - ((scrollViewer.ExtentWidth - scrollViewer.ExtentHeight) / 2) * wholeImageViewer.ActualHeight / scrollViewer.ExtentWidth) * Resource.nowImageData.DataHeight / wholeImageViewer.ActualHeight
					);
				}
				else
				{
					Position = new Point(
					(scrollViewer.ExtentHeight / scrollViewer.ExtentWidth) * (viewerPosition.X - ((scrollViewer.ExtentHeight - scrollViewer.ExtentWidth) / 2) * wholeImageViewer.ActualWidth / scrollViewer.ExtentHeight) * Resource.nowImageData.DataWidth / wholeImageViewer.ActualWidth,
					(viewerPosition.Y) * Resource.nowImageData.DataHeight / wholeImageViewer.ActualHeight
					);
				}

				DrawZoomRect();
			}
		}

		private void WholeImageSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (e.NewValue != Zoom)
				Zoom = e.NewValue;
		}
		private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
		{
			WholeImageScalers.Visibility = Visibility.Visible;
		}
		private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			WholeImageScalers.Visibility = Visibility.Collapsed;
		}
		#endregion

		#region Open File
		private void Open3DTifFile_Click(object sender, RoutedEventArgs e)
		{
			using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
			{

				// Set filter for file extension and default file extension 
				openFileDialog.Filter = "Image Files|*.tiff;*.tif";
				openFileDialog.RestoreDirectory = true;
				openFileDialog.Multiselect = false;

				// Display OpenFileDialog by calling ShowDialog method 
				if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string[] files = (string[])openFileDialog.FileNames;

					Open3DFiles(files[0]);
				}
			}
		}
		private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckDirtyData()) return;
			using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
			{

				// Set filter for file extension and default file extension 
				openFileDialog.Filter = "Image Files|*.jpeg;*.jpg;*.png;*.tif;*.tiff;*.svs;*.ndpi;*.dcm";
				openFileDialog.RestoreDirectory = true;
				openFileDialog.DereferenceLinks = true;
				openFileDialog.Multiselect = true;

				// Display OpenFileDialog by calling ShowDialog method 
				if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string[] files = (string[])openFileDialog.FileNames;
					OpenFilesFunction(files);
				}
			}
		}

		private void MenuOpenWSI_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckDirtyData()) return;
			using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
			{

				// Set filter for file extension and default file extension 
				openFileDialog.Filter = "Image Files|*.svs;*.ndpi";
				openFileDialog.RestoreDirectory = true;
				openFileDialog.DereferenceLinks = true;
				openFileDialog.Multiselect = false;

				// Display OpenFileDialog by calling ShowDialog method 
				if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string[] files = (string[])openFileDialog.FileNames;
					if (files.Length == 1)
					{
						
						OpenFiles(files[0]);
						
					}					
				}
			}
		}

		private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
		{
			using (var folder = new System.Windows.Forms.FolderBrowserDialog())
			{
				System.Windows.Forms.DialogResult result = folder.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folder.SelectedPath))
				{
					var files = from file in Directory.GetFiles(folder.SelectedPath)
								where (System.IO.Path.GetExtension(file) == ".tiff" ||
									  System.IO.Path.GetExtension(file) == ".jpg" ||
									  System.IO.Path.GetExtension(file) == ".png" ||
									  System.IO.Path.GetExtension(file) == ".tif")
									  && !file.Contains(".jellox")
								select file;

					OpenFiles(files, Resource.ImageType.Normal);
					ImagePickerSlider.Value = 1;
				}

			}
		}
		private void OpenFilesFunction(string[] files)
		{
			if (files.Length == 1)
			{
				FileAttributes attr = File.GetAttributes(files[0]);

				if (System.IO.Path.GetExtension(files[0]).ToLower() == ".tif")
				{
					using (ShellObject picture = ShellObject.FromParsingName(files[0]))
					{
						if (picture != null)
						{
							var raw_w = picture.Properties.GetProperty(SystemProperties.System.Image.HorizontalSize);
							var philips_check = picture.Properties.GetProperty(SystemProperties.System.ApplicationName);

							if (raw_w.ValueAsObject == null)
							{
								messagebox.Show("The file are not support. \n", "File format issue", MessageBoxButton.OK, MessageBoxImage.Error);
								return;
							}
							else if (philips_check.ValueAsObject != null && philips_check.ValueAsObject.ToString().ToLower().Contains("philips"))
							{
								OpenFiles(files[0]);
							}
							else
							{
								try
								{
									FileStream tif3DStream = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read);
									TiffBitmapDecoder decoder = new TiffBitmapDecoder(tif3DStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
									int count = decoder.Frames.Count;
									if (count > 1)
										Open3DFiles(files[0]);
									else
										OpenFiles(files, Resource.ImageType.Normal);
								}
								catch
								{
									messagebox.Show("The file format are not support or damaged. ", "File format issue", MessageBoxButton.OK, MessageBoxImage.Error);
									return;
								}
							}
						}
					}
				}
				else if (System.IO.Path.GetExtension(files[0]).ToLower() == ".svs" ||
							System.IO.Path.GetExtension(files[0]).ToLower() == ".ndpi" ||
							System.IO.Path.GetExtension(files[0]).ToLower() == ".svslide" ||
							System.IO.Path.GetExtension(files[0]).ToLower() == ".scn" ||
							System.IO.Path.GetExtension(files[0]).ToLower() == ".mrxs" ||
							System.IO.Path.GetExtension(files[0]).ToLower() == ".bif" ||
							System.IO.Path.GetExtension(files[0]).ToLower() == ".tiff"
							)
				{
					OpenFiles(files[0]);
				}
				else if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				{
					OpenFiles(files, Resource.ImageType.DICOM);
				}
				else if (System.IO.Path.GetExtension(files[0]).ToLower() == ".dcm")
				{
					OpenDICOM3DFiles(files[0]);
				}
				else
				{
					OpenFiles(files, Resource.ImageType.Normal);
				}
			}
			else
			{

				foreach (var f in files)
				{
					if (System.IO.Path.GetExtension(f).ToLower() == ".svs" ||
							System.IO.Path.GetExtension(f).ToLower() == ".ndpi" ||
							System.IO.Path.GetExtension(f).ToLower() == ".svslide" ||
							System.IO.Path.GetExtension(f).ToLower() == ".scn" ||
							System.IO.Path.GetExtension(f).ToLower() == ".mrxs" ||
							System.IO.Path.GetExtension(f).ToLower() == ".bif" ||
							System.IO.Path.GetExtension(f).ToLower() == ".tiff"
							)
					{
						messagebox.Show(f + " can not open as 3D image. \nPlease open it one at a time", "File format issue", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}else if (System.IO.Path.GetExtension(f).ToLower() == ".dcm")
					{
						OpenDICOM3DFiles(files, Resource.ImageType.DICOM3D);
						return;
					}
					if (System.IO.Path.GetExtension(f).ToLower() == ".tif"
						)
					{
						using (ShellObject picture = ShellObject.FromParsingName(f))
						{
							if (picture != null)
							{
								var philips_check = picture.Properties.GetProperty(SystemProperties.System.ApplicationName);

								if (philips_check.ValueAsObject != null && philips_check.ValueAsObject.ToString().ToLower().Contains("philips"))
								{
									messagebox.Show(f + " can not open as 3D image. \nPlease open it one at a time", "File format issue", MessageBoxButton.OK, MessageBoxImage.Error);
									return;
								}
							}
						}
					}
					try
					{
						if (System.IO.Path.GetExtension(f).ToLower() == ".tif"
						)
						{
							FileStream tif3DStream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
							TiffBitmapDecoder decoder = new TiffBitmapDecoder(tif3DStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
						}
					}
					catch
					{
						messagebox.Show("The file " + f + " cannot open, the format is not supported or data damaged. \n", "File format issue", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
				}
				OpenFiles(files, Resource.ImageType.Normal);
			}
			RecentFileList.InsertFile(files[0]);
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				OpenFilesFunction(files);
			}
		}

		private void OffAllFunction()
		{
			//Drawing = false;
			//Erasing = false;
			//Polying = false;
			//Filling = false;
			//Rectangle = false;
			manualDrawingButton.IsChecked = false;
			manualErasingButton.IsChecked = false;
			manualPolygonButton.IsChecked = false;
			manualRectangle.IsChecked = false;
			manualFillButton.IsChecked = false;
		}

		private void Open3DFiles(string file)
		{
			InitProgressBar("Loading data...");
			CloseImage();
			for (int i = 0; i < hashCached.Count; i++)
			{
				hashCached[i] = -1;
			}

			BackgroundWorker bk = new BackgroundWorker();
			bk.WorkerReportsProgress = true;
			bk.DoWork += (s, e) =>
			{

				FileStream tif3DStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
				TiffBitmapDecoder decoder = new TiffBitmapDecoder(tif3DStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
				int count = decoder.Frames.Count;
				SliderGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					ImagePickerSlider.Maximum = 1;
					if (count > 1)
						SliderGrid.Visibility = Visibility.Visible;
					else
						SliderGrid.Visibility = Visibility.Collapsed;
				}));


				for (int i = 0; i < decoder.Frames.Count; i++)
				{
					decoder.Frames[i].Freeze();

					Resource.ImageDatas.Add(new ImageData(decoder.Frames[i], file, i, Resource.ImageType.Tif3D));

					if (i == 0)
					{
						Dispatcher.BeginInvoke(new Action(() => {
							Resource.SelectedImageIndex = 0;
							SetImage(Resource.SelectedImageIndex);
							ImagePickerSlider.Value = 1;

							int xoffset = Math.Max(0, (int)(scrollViewer.ActualWidth - (mainViewer.ActualWidth * scaleTransform.ScaleX)) / 2);// If there is blank space in scrollview the contour viewer start with 0
							int yoffset = Math.Max(0, (int)(scrollViewer.ActualHeight - (mainViewer.ActualHeight * scaleTransform.ScaleY)) / 2);

						}));
					}


					SliderGrid.Dispatcher.BeginInvoke(new Action(() => { ImagePickerSlider.Maximum = Resource.ImageDatas.Count; PicNumberLabel.Text = (Resource.ImageDatas.Count).ToString(); }));

					if (i % 10 == 0)
					{
						tif3DStream.Close();
						tif3DStream.Dispose();
						GC.Collect();
						tif3DStream = new FileStream(file, FileMode.Open, FileAccess.Read);
						decoder = new TiffBitmapDecoder(tif3DStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
					}
					bk.ReportProgress((i) * 100 / decoder.Frames.Count);
				}
				tif3DStream.Close();
				tif3DStream.Dispose();
				GC.Collect();

				bk.ReportProgress(100);
			};
			bk.ProgressChanged += (s, e) =>
			{
				statusProgressBar.Value = e.ProgressPercentage;
			};
			bk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(InitializedOpenFile_Completed);
			bk.RunWorkerAsync(statusProgressBar.Value);
		}

		private void OpenDICOM3DFiles(string file)
		{
			InitProgressBar("Loading data...");
			CloseImage();
			for (int i = 0; i < hashCached.Count; i++)
			{
				hashCached[i] = -1;
			}

			BackgroundWorker bk = new BackgroundWorker();
			bk.WorkerReportsProgress = true;
			bk.DoWork += (s, e) =>
			{

				//FileStream tif3DStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
				//TiffBitmapDecoder decoder = new TiffBitmapDecoder(tif3DStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
				//int count = decoder.Frames.Count;
				var dcmFile = DicomFile.Open(file);
				int dicomFrames = -1;
				dcmFile.Dataset.TryGetValue<int>(DicomTag.NumberOfFrames, 0, out dicomFrames);
				if (dicomFrames <= 0) dicomFrames = 1;


				SliderGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					ImagePickerSlider.Maximum = 1;
					if (dicomFrames > 1)
						SliderGrid.Visibility = Visibility.Visible;
					else
						SliderGrid.Visibility = Visibility.Collapsed;
				}));


				for (int i = 0; i < dicomFrames; i++)
				{
					var frame = new DicomImage(dcmFile.Dataset, i);
					var bitmap = frame.RenderImage(i).AsClonedBitmap();
					using (MemoryStream ms = new MemoryStream())
					{
						bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
						var tileImage = new BitmapImage();
						tileImage.BeginInit();
						tileImage.StreamSource = ms;
						tileImage.CacheOption = BitmapCacheOption.OnLoad;
						tileImage.EndInit();
						tileImage.Freeze();
						Resource.ImageDatas.Add(new ImageData(tileImage, file, i, Resource.ImageType.DICOM3D));
					}

					if (i == 0)
					{
						Dispatcher.BeginInvoke(new Action(() => {
							Resource.SelectedImageIndex = 0;
							SetImage(Resource.SelectedImageIndex);
							ImagePickerSlider.Value = 1;
							//int height = (int)Resource.WriteableBitmapHugeImage.Height;
							//int width = (int)Resource.WriteableBitmapHugeImage.Width;

							int xoffset = Math.Max(0, (int)(scrollViewer.ActualWidth - (mainViewer.ActualWidth * scaleTransform.ScaleX)) / 2);// If there is blank space in scrollview the contour viewer start with 0
							int yoffset = Math.Max(0, (int)(scrollViewer.ActualHeight - (mainViewer.ActualHeight * scaleTransform.ScaleY)) / 2);

						}));
					}


					SliderGrid.Dispatcher.BeginInvoke(new Action(() => { ImagePickerSlider.Maximum = Resource.ImageDatas.Count; PicNumberLabel.Text = (Resource.ImageDatas.Count).ToString(); }));

					if (i % 10 == 0)
					{
						GC.Collect();
					}
					bk.ReportProgress((i) * 100 / dicomFrames);
				}
				GC.Collect();

				bk.ReportProgress(100);
			};
			bk.ProgressChanged += (s, e) =>
			{
				statusProgressBar.Value = e.ProgressPercentage;
			};
			bk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(InitializedOpenFile_Completed);
			bk.RunWorkerAsync(statusProgressBar.Value);
		}
		private void OpenDICOM3DFiles(IEnumerable<string> files, Resource.ImageType imageType)
		{
			InitProgressBar("Loading data...");
			CloseImage();
			for (int i = 0; i < hashCached.Count; i++)
			{
				hashCached[i] = -1;
			}

			BackgroundWorker bk = new BackgroundWorker();
			bk.WorkerReportsProgress = true;
			bk.DoWork += (s, e) =>
			{

				int count = files.Count<string>();

				SliderGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					ImagePickerSlider.Maximum = 1;
					if (count > 1)
						SliderGrid.Visibility = Visibility.Visible;
					else
						SliderGrid.Visibility = Visibility.Collapsed;
				}));


				for (int i = 0; i < count; i++)
				{
					var dcmFile = DicomFile.Open(files.ElementAt<string>(i));
					int dicomFrames = -1;
					dcmFile.Dataset.TryGetValue<int>(DicomTag.NumberOfFrames, 0, out dicomFrames);

					if (dicomFrames <= 0) dicomFrames = 1;
					var frame = new DicomImage(dcmFile.Dataset, 0);
					var bitmap = frame.RenderImage(0).AsClonedBitmap();
					using (MemoryStream ms = new MemoryStream())
					{
						bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
						var tileImage = new BitmapImage();
						tileImage.BeginInit();
						tileImage.StreamSource = ms;
						tileImage.CacheOption = BitmapCacheOption.OnLoad;
						tileImage.EndInit();
						tileImage.Freeze();
						Resource.ImageDatas.Add(new ImageData(tileImage, files.ElementAt<string>(i), i, Resource.ImageType.DICOM3D));
					}

					if (i == 0)
					{
						Dispatcher.BeginInvoke(new Action(() => {
							Resource.SelectedImageIndex = 0;
							SetImage(Resource.SelectedImageIndex);
							ImagePickerSlider.Value = 1;
							//int height = (int)Resource.WriteableBitmapHugeImage.Height;
							//int width = (int)Resource.WriteableBitmapHugeImage.Width;

							int xoffset = Math.Max(0, (int)(scrollViewer.ActualWidth - (mainViewer.ActualWidth * scaleTransform.ScaleX)) / 2);// If there is blank space in scrollview the contour viewer start with 0
							int yoffset = Math.Max(0, (int)(scrollViewer.ActualHeight - (mainViewer.ActualHeight * scaleTransform.ScaleY)) / 2);

						}));
					}


					SliderGrid.Dispatcher.BeginInvoke(new Action(() => { ImagePickerSlider.Maximum = Resource.ImageDatas.Count; PicNumberLabel.Text = (Resource.ImageDatas.Count).ToString(); }));

					if (i % 10 == 0)
					{
						GC.Collect();
					}
					bk.ReportProgress((i) * 100 / dicomFrames);
				}
				GC.Collect();

				bk.ReportProgress(100);
			};
			bk.ProgressChanged += (s, e) =>
			{
				statusProgressBar.Value = e.ProgressPercentage;
			};
			bk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(InitializedOpenFile_Completed);
			bk.RunWorkerAsync(statusProgressBar.Value);
		}

		private void OpenFiles(IEnumerable<string> files, Resource.ImageType imageType)
		{
			InitProgressBar("Loading data...");
			CloseImage();
			for (int i = 0; i < hashCached.Count; i++)
			{
				hashCached[i] = -1;
			}

			BackgroundWorker bk = new BackgroundWorker();
			bk.WorkerReportsProgress = true;
			bk.DoWork += (s, e) =>
			{
				int count = files.Count<string>();

				SliderGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					ImagePickerSlider.Maximum = 1;
					if (count > 1)
						SliderGrid.Visibility = Visibility.Visible;
					else
						SliderGrid.Visibility = Visibility.Collapsed;
				}));

				for (int i = 0; i < count; i++)
				{

					Resource.ImageDatas.Add(new ImageData(files.ElementAt<string>(i), i, imageType));
					if (i == 0)
					{
						Dispatcher.BeginInvoke(new Action(() => {
							Resource.SelectedImageIndex = 0;
							SetImage(Resource.SelectedImageIndex);
							ImagePickerSlider.Value = 1;
							widthRatio = Resource.nowImageData.DataWidth / mainViewer.ActualWidth;
							heightRatio = Resource.nowImageData.DataHeight / mainViewer.ActualHeight;
							annotationWidthRatio = Resource.nowImageData.AnnotationWidth / mainViewer.ActualWidth;
							annotationHeightRatio = Resource.nowImageData.AnnotationHeight / mainViewer.ActualHeight;
						}));
					}

					SliderGrid.Dispatcher.BeginInvoke(new Action(() => { ImagePickerSlider.Maximum = Resource.ImageDatas.Count; PicNumberLabel.Text = (Resource.ImageDatas.Count).ToString(); }));

					bk.ReportProgress((i + 1) * 100 / count);
				}
				GC.Collect();

				bk.ReportProgress(100);
			};
			bk.ProgressChanged += (s, e) =>
			{
				statusProgressBar.Value = e.ProgressPercentage;
			};
			bk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(InitializedOpenFile_Completed);
			bk.RunWorkerAsync(statusProgressBar.Value);
		}

		private void OpenFiles(string file)
		{
			
			InitProgressBar("Loading data...");
			CloseImage();
			for (int i = 0; i < hashCached.Count; i++)
			{
				hashCached[i] = -1;
			}

			BackgroundWorker bk = new BackgroundWorker();
			bk.WorkerReportsProgress = true;
			bk.DoWork += (s, e) =>
			{
				
				SliderGrid.Dispatcher.BeginInvoke(new Action(() =>
				{
					ImagePickerSlider.Maximum = 1;
					SliderGrid.Visibility = Visibility.Collapsed;
				}));

				
				Resource.ImageDatas.Add(new ImageData(file, OPENSLIDEWSI, Resource.ImageType.WSI));
				Dispatcher.BeginInvoke(new Action(() => {
					Resource.SelectedImageIndex = 0;
					SetImage(Resource.SelectedImageIndex);
					ImagePickerSlider.Value = 1;
					widthRatio = Resource.nowImageData.DataWidth / mainViewer.ActualWidth;
					heightRatio = Resource.nowImageData.DataHeight / mainViewer.ActualHeight;
					annotationWidthRatio = Resource.nowImageData.AnnotationWidth / mainViewer.ActualWidth;
					annotationHeightRatio = Resource.nowImageData.AnnotationHeight / mainViewer.ActualHeight;
				}));

				
				SliderGrid.Dispatcher.BeginInvoke(new Action(() => { ImagePickerSlider.Maximum = 1; PicNumberLabel.Text = 1.ToString(); }));

				
				GC.Collect();

				bk.ReportProgress(100);
			};
			
			bk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(InitializedOpenFile_Completed);
			bk.RunWorkerAsync();
		}


		private void InitializedOpenFile_Completed(object sender, RunWorkerCompletedEventArgs e)
		{
			ResetProgressBar();
			if (Resource.isDataReaded)
			{
				ImagePickerSlider.Maximum = Resource.ImageDatas.Count;
				PicNumberLabel.Text = ImagePickerSlider.Maximum.ToString();
				ImagePickerSlider.Minimum = 1;

				SetImage(Resource.SelectedImageIndex);

				UpdateStatisticData();
			}
		}

		private void SetThumbnail(ImageData imageData)
		{
			this.originalImageDrawingGroup = new DrawingGroup();
			ImageDrawing background = new ImageDrawing();
			background.Rect = new Rect(0, 0, 275, 275);
			background.ImageSource = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Gray2, null);
			this.originalImageDrawingGroup.Children.Add(background);
			this.originalImageDrawing = new ImageDrawing();
			if (imageData.DataHeight > imageData.DataWidth)
			{
				this.originalImageDrawing.Rect = new Rect(275 * (1 - (float)imageData.DataWidth / (float)imageData.DataHeight) / 2, 0, 275 * imageData.DataWidth / imageData.DataHeight, 275);
			}
			else
			{
				this.originalImageDrawing.Rect = new Rect(0, 275 * (1 - (float)imageData.DataHeight / (float)imageData.DataWidth) / 2, 275, 275 * imageData.DataHeight / imageData.DataWidth);
			}

			this.originalImageDrawing.ImageSource = imageData.ResizedImage;
			this.originalImageDrawingGroup.Children.Add(originalImageDrawing);

			RectangleGeometry clipGeometry =
				new RectangleGeometry(new Rect(new Point(0, 0), new Point(275, 275)));

			originalImageDrawingGroup.ClipGeometry = clipGeometry;
			wholeImageViewer.Source = new DrawingImage(this.originalImageDrawingGroup);
		}

		private Stack<BackgroundWorker> preLoadimage = new Stack<BackgroundWorker>();
		private Stack<BackgroundWorker> primaryLoadImage = new Stack<BackgroundWorker>();
		private Queue<BackgroundWorker> processingPreLoadimage = new Queue<BackgroundWorker>();
		private List<int> hashCached = new List<int>();
		private void SetImage(int imageIndex)
		{
			ImageData imageData = Resource.ImageDatas[imageIndex];
			widthRatio = imageData.DataWidth / mainViewer.ActualWidth;
			heightRatio = imageData.DataHeight / mainViewer.ActualHeight;
			annotationWidthRatio = imageData.AnnotationWidth / mainViewer.ActualWidth;
			annotationHeightRatio = imageData.AnnotationHeight / mainViewer.ActualHeight;

			SetThumbnail(imageData);

			BackgroundWorker nowLoad = new BackgroundWorker();
			primaryLoadImage.Push(nowLoad);
			nowLoad.DoWork += (s, e) =>
			{

				lock (hashCached)
				{
					if (hashCached[imageIndex % cacheRange] != imageIndex && imageIndex == Resource.SelectedImageIndex)
					{
						int oldIndex = hashCached[imageIndex % cacheRange];
						if (oldIndex >= 0)
						{
							Resource.ImageDatas[oldIndex].ClearCache();
						}
						hashCached[imageIndex % cacheRange] = imageIndex;
					}
				}
				Resource.ImageDatas[hashCached[imageIndex % cacheRange]].PreLoad();
			};
			nowLoad.RunWorkerCompleted += (s, e) =>
			{
				if (imageIndex == Resource.SelectedImageIndex)
				{
					mainViewer.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding() { Source = Resource.ImageDatas[imageIndex], Path = new PropertyPath("ToShowImage") });
					HugeImageShower.SetBinding(System.Windows.Controls.Image.VisibilityProperty, new Binding() { Source = Resource.ImageDatas[imageIndex].isHugeImage, Converter = new BooleanToVisibilityConverter() });
					HugeImageShower.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding() { Source = Resource.WriteableBitmapView });
					double sliderMin = 0;
					double sliderMax = 1;
					
					if (imageIndex + cacheRange / 2 > ImagePickerSlider.Maximum)
					{
						sliderMax = ImagePickerSlider.Maximum - 1;
						sliderMin = Math.Max(ImagePickerSlider.Minimum - 1, ImagePickerSlider.Maximum - (cacheRange - 1) - 1);
					}
					else if (imageIndex - cacheRange / 2 < ImagePickerSlider.Minimum)
					{
						sliderMax = Math.Min(ImagePickerSlider.Maximum - 1, ImagePickerSlider.Minimum + (cacheRange - 1) - 1);
						sliderMin = ImagePickerSlider.Minimum - 1;
					}
					else
					{
						sliderMax = imageIndex + cacheRange / 2 - 1;
						sliderMin = imageIndex - cacheRange / 2 - 1;
					}

					BackgroundWorker preLoadWork = new BackgroundWorker();
					preLoadWork.WorkerSupportsCancellation = true;

					preLoadWork.DoWork += (s, e) =>
					{
						Thread.CurrentThread.Priority = ThreadPriority.Lowest;
						for (int i = (int)sliderMin; i <= (int)sliderMax; i++)
						{
							int workIndex = i;
							if (preLoadWork.CancellationPending)
							{
								e.Cancel = true;
								break;
							}
							lock (hashCached)
							{

								if (hashCached[workIndex % cacheRange] != workIndex)
								{

									int oldIndex = hashCached[workIndex % cacheRange];
									if (oldIndex >= 0)
									{
										Resource.ImageDatas[oldIndex].ClearCache();
									}
								}
							}
							Resource.ImageDatas[workIndex].PreLoad();
						}
					};
					preLoadimage.Push(preLoadWork);

				}
				if (preLoadimage.Count != 0)
				{
					var tmp = preLoadimage.Pop();
					tmp.RunWorkerAsync();
					processingPreLoadimage.Enqueue(tmp);
				}
				else
				{
					while (processingPreLoadimage.Count != 0)
					{
						var tmp = processingPreLoadimage.Dequeue();
						tmp.CancelAsync();
						tmp.Dispose();
					}


					primaryLoadImage.Clear();
				}
				preLoadimage.Clear();
				GC.Collect();

			};

			mainViewer.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding() { Source = Resource.nowImageData, Path = new PropertyPath("ToShowImage") });
			HugeImageShower.SetBinding(System.Windows.Controls.Image.VisibilityProperty, new Binding() { Source = Resource.nowImageData.isHugeImage, Converter = new BooleanToVisibilityConverter() });
			HugeImageShower.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding() { Source = Resource.WriteableBitmapView });


			if (primaryLoadImage.Count != 0)
				primaryLoadImage.Pop().RunWorkerAsync();

			preLoadimage.Clear();
			primaryLoadImage.Clear();


			fileNameTextBlock.SetBinding(Button.ContentProperty, new Binding() { Source = imageData, Path = new PropertyPath("FilenameWithAnnotation") });
			DrawContour();
			Resource.AnnotationSelector.ImageChanged();
		}
		
		#endregion

		#region Server Memu
		private void ReloadMetaLiteServer_Click(object sender, RoutedEventArgs e)
		{
			ReloadMetaLiteServer();
			ServerLaunchCheck();
		}

		public static void ReloadMetaLiteServer()
		{
			KillProcess();
			Process.Start("metalite-server.exe");
		}
		#endregion



		private void ZoomTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
			e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
		}
		private void ZoomTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!Double.TryParse(ZoomTextBox.Text, out double input) ||
				string.IsNullOrEmpty(ZoomTextBox.Text) ||
				input < 1)
			{
				ZoomTextBox.Text = "1";
				Zoom = 1;
			}
		}

		#region Zoom UI Events
		private IDisposable _subscription = null;
		private void ZoomDecreaseButtonDown(object sender, RoutedEventArgs e)
		{
			_subscription =
				Observable
					.Interval(TimeSpan.FromMilliseconds(50))
					.Select(x => String.Format("{0:00}", x))
					.ObserveOnDispatcher()
					.Subscribe(x => { Zoom = MathHelper.Clip(zoom - zoom * 0.05, 1, 65536); });
		}
		private void ZoomDecreaseButtonUp(object sender, RoutedEventArgs e)
		{
			_subscription.Dispose();
		}

		private void ZoomIncreaseButtonDown(object sender, RoutedEventArgs e)
		{

			_subscription =
				Observable
					.Interval(TimeSpan.FromMilliseconds(50))
					.Select(x => String.Format("{0:00}", x))
					.ObserveOnDispatcher()
					.Subscribe(x => Zoom += Zoom * 0.05);
		}
		private void ZoomIncreaseButtonUp(object sender, RoutedEventArgs e)
		{
			_subscription.Dispose();
		}

		private void ZoomX2ButtonClick(object sender, RoutedEventArgs e)
		{
			Zoom = 2;
		}
		private void ZoomX4ButtonClick(object sender, RoutedEventArgs e)
		{
			Zoom = 4;
		}
		private void ZoomX10ButtonClick(object sender, RoutedEventArgs e)
		{
			Zoom = 10;
		}
		private void ZoomX20ButtonClick(object sender, RoutedEventArgs e)
		{
			Zoom = 20;
		}
		private void ZoomX40ButtonClick(object sender, RoutedEventArgs e)
		{
			Zoom = 40;
		}

		#endregion

		public double Zoom
		{
			get { return zoom; }
			set
			{
				if (!Resource.isDataReaded)
					return;
				if (zoom < 1)
				{
					zoom = 1;
				}
				else
				{
					zoom = value;
				}

				ZoomTextBox.Text = String.Format("{0:F1}", zoom);
				WholeImageSlider.Value = zoom;

				if (scaleTransform != null)
				{
					scaleTransform.ScaleX = zoom;
					scaleTransform.ScaleY = zoom;
				}

				if (polygon.Points.Count != 0)
				{
					for (int i = 0; i < UIPolyPoints.Count; i++)
					{
						UIPolyPoints.ElementAt(i).Width = 8 / zoom;
						UIPolyPoints.ElementAt(i).Height = 8 / zoom;
						UIPolyPoints.ElementAt(i).Margin = new Thickness(left: polylinePoints.ElementAt(i).X - UIPolyPoints.ElementAt(i).Width / 2, top: polylinePoints.ElementAt(i).Y - UIPolyPoints.ElementAt(i).Height / 2, right: 0, bottom: 0);
						UIPolyPoints.ElementAt(i).StrokeThickness = 1.5 / zoom;
					}
					polygon.StrokeThickness = 1.5 / zoom;
					polygonBackground.StrokeThickness = 2.5 / zoom;
				}
				if (scrollViewer != null)
				{
					scrollViewer.ScrollToHorizontalOffset(Math.Max(0, (position.X - Resource.nowImageData.DataWidth / zoom / 2) * mainViewer.ActualWidth * zoom / Resource.nowImageData.DataWidth));
					scrollViewer.ScrollToVerticalOffset(Math.Max(0, (position.Y - Resource.nowImageData.DataHeight / zoom / 2) * mainViewer.ActualHeight * zoom / Resource.nowImageData.DataHeight));
					UIViewer.ScrollToHorizontalOffset(Math.Max(0, (position.X - Resource.nowImageData.DataWidth / zoom / 2) * mainViewer.ActualWidth * zoom / Resource.nowImageData.DataWidth));
					UIViewer.ScrollToVerticalOffset(Math.Max(0, (position.Y - Resource.nowImageData.DataHeight / zoom / 2) * mainViewer.ActualHeight * zoom / Resource.nowImageData.DataHeight));

				}
			}
		}

		private void DrawZoomRect()
		{
			if (!Resource.isDataReaded)
				return;

			if (originalImageDrawingGroup == null)
			{
				return;
			}

			originalImageDrawingGroup.Children.Remove(zoomRect);

			double emptyOffset_x = 0;
			double viewportOffset_x = 0;
			double emptyOffset_y = 0;
			double viewportOffset_y = 0;
			double MaxEdge;
			if (scrollViewer.ExtentWidth > scrollViewer.ExtentHeight)
			{
				emptyOffset_y = (scrollViewer.ExtentWidth - scrollViewer.ExtentHeight) / 2;
				MaxEdge = scrollViewer.ExtentWidth;
			}
			else
			{
				emptyOffset_x = (scrollViewer.ExtentHeight - scrollViewer.ExtentWidth) / 2;
				MaxEdge = scrollViewer.ExtentHeight;
			}
			if (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth <= -1)
			{
				viewportOffset_x = -Math.Max(0, (scrollViewer.ViewportWidth - scrollViewer.ExtentWidth) / 2);
			}
			else if (scrollViewer.ExtentHeight - scrollViewer.ViewportHeight <= -1)
			{
				viewportOffset_y = -Math.Max(0, (scrollViewer.ViewportHeight - scrollViewer.ExtentHeight) / 2);
			}

			this.zoomRect = ImageHelper.DrawRectangle(
				275.0 * (scrollViewer.HorizontalOffset + emptyOffset_x + viewportOffset_x) / MaxEdge,
				275.0 * (scrollViewer.VerticalOffset + emptyOffset_y + viewportOffset_y) / MaxEdge,
				275.0 * (scrollViewer.ViewportWidth / MaxEdge),
				275.0 * (scrollViewer.ActualHeight / MaxEdge),
				new SolidColorBrush(Color.FromArgb(90, 0, 0, 0)),
				new Pen(Brushes.White, 1)
				);

			originalImageDrawingGroup.Children.Add(this.zoomRect);

		}

		private Point position = new Point(0, 0);
		public Point Position
		{
			get { return position; }
			set
			{
				if (!Resource.isDataReaded)
				{
					return;
				}
				position.X = MathHelper.Clip(value.X, Resource.nowImageData.DataWidth / Zoom / 2, Resource.nowImageData.DataWidth - Resource.nowImageData.DataWidth / Zoom / 2);
				position.Y = MathHelper.Clip(value.Y, Resource.nowImageData.DataHeight / Zoom / 2, Resource.nowImageData.DataHeight - Resource.nowImageData.DataHeight / Zoom / 2);

				if (scrollViewer != null)
				{
					scrollViewer.ScrollToHorizontalOffset(Math.Max(0, (this.position.X - Resource.nowImageData.DataWidth / Zoom / 2) * mainViewer.ActualWidth * Zoom / Resource.nowImageData.DataWidth));
					scrollViewer.ScrollToVerticalOffset(Math.Max(0, (this.position.Y - Resource.nowImageData.DataHeight / Zoom / 2) * mainViewer.ActualHeight * Zoom / Resource.nowImageData.DataHeight));
					UIViewer.ScrollToHorizontalOffset(Math.Max(0, (this.position.X - Resource.nowImageData.DataWidth / Zoom / 2) * mainViewer.ActualWidth * Zoom / Resource.nowImageData.DataWidth));
					UIViewer.ScrollToVerticalOffset(Math.Max(0, (this.position.Y - Resource.nowImageData.DataHeight / Zoom / 2) * mainViewer.ActualHeight * Zoom / Resource.nowImageData.DataHeight));
				}
			}
		}


		#region Main Function
		private async void MultipleImageAIAnnotationButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded || Resource.nowImageData.ImageType == Resource.ImageType.WSI) return;
			if (Resource.nowImageData.ImageType == Resource.ImageType.WSI)
			{
				WSIAIInference();
				return;
			}

			var dlg = new Subwindow.AIDialogBox { Owner = this, };
			string[] channlTexts = new string[3];

			dlg.ShowDialog();

			if (dlg.DialogResult == true)
			{
				channlTexts[(int)Resource.RGBEffect.RIndex] = dlg.RedComboBox.Text;
				channlTexts[(int)Resource.RGBEffect.GIndex] = dlg.GreenComboBox.Text;
				channlTexts[(int)Resource.RGBEffect.BIndex] = dlg.BlueComboBox.Text;
				MainMenu.IsEnabled = false;
				if (dlg.SelectedModel == "HH_AI_ANNOTATING_SERVER_STRING")
				{
					InitProgressBar("Inferencing...");

					foreach (var imageData in Resource.ImageDatas)
					{
						imageData.SetToHHAIModule();
					}
				}
				else
				{
					InitProgressBar("Model loading...");

					foreach (var imageData in Resource.ImageDatas)
					{
						imageData.SetToJelloXAIModule(dlg.SelectedModel);
					}
				}

				int from = (int)dlg.RangeSelector.LowerValue;
				int to = (int)dlg.RangeSelector.UpperValue;
				string red_channel = channlTexts[0];
				string green_channel = channlTexts[1];
				string blue_channel = channlTexts[2];
				string apparent_magnification = dlg.ApparentMagnification.Text;
				statusTextBlock.Text = "AI Annotating...";
				try
				{
					double left = -1;
					BackgroundWorker timer = new BackgroundWorker();
					timer.DoWork += (s, e) => {
						statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Estimating time to finish"; }));
						while (left < 0)
						{
							Thread.Sleep(1000);
						}
						while (left > 1)
						{
							left--;
							statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Left about " + ((int)(left / 60)).ToString() + "m" + ((int)(left % 60)).ToString() + "s"; }));
							Thread.Sleep(1000);
						}
					};
					timer.RunWorkerAsync();
					BackgroundWorker bk = new BackgroundWorker();
					List<string> fileToDelete = new List<string>();
					bk.DoWork += async (s, e) =>
					{
						DateTime start = DateTime.Now;
						for (int i = from - 1; i <= to - 1; i++)
						{
							int index = i;

							if (dlg.SelectedModel == "HH_AI_ANNOTATING_SERVER_STRING")
							{
								SetProgressBar(30, 99);
								await Resource.ImageDatas[index].GetAIAnnotations("", "", "", "", "", index == to - 1).ConfigureAwait(true);
							}
							else
							{
								string TempFilePath = System.IO.Path.GetTempFileName();
								SetProgressBar(index - (from - 1), (to - from + 1), TempFilePath);
								await Resource.ImageDatas[index].GetAIAnnotations(TempFilePath,
									red_channel,
									green_channel,
									blue_channel,
									apparent_magnification,
									index == to - 1).ConfigureAwait(true);

								fileToDelete.Add(TempFilePath);
							}
							// time estimate
							if (to - from > 1)
							{
								DateTime now = DateTime.Now;
								double during = ((TimeSpan)(now - start)).TotalSeconds;
								double avg = during / (i - (from - 1) + 1);
								if (((to - 1) - i) * avg < left || left < 0)
									left = ((to - 1) - i) * avg;
							}
						}
						
						statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { ResetProgressBar(); statusTextBlock.Text = "Finish"; }));
						
					};
					bk.RunWorkerCompleted += (s, e) =>
					{
						foreach (var file in fileToDelete)
						{
							File.Delete(file);
						}
						
					};
					bk.RunWorkerAsync();

					//Zoom = zoom;
				}
				catch (Exception ex)
				{
					ResetProgressBar();
					statusTextBlock.Text = "Failed";
					messagebox.Show(ex.ToString(), "ERROR code 0002\n AI inference function failed.");
				}
			}
		}
		private async void WSIAIInference()
		{
			var dlg = new Subwindow.AIWSIDialogBox { Owner = this, };
			string[] channlTexts = new string[3];

			dlg.ShowDialog();

			if (dlg.DialogResult == true)
			{
				MainMenu.IsEnabled = false;
				
				InitProgressBar("Model loading...");

				foreach (var imageData in Resource.ImageDatas)
				{
					imageData.SetToJelloXAIModule(dlg.SelectedModel);
				}

				int from = (int)dlg.RangeSelector.LowerValue;
				int to = (int)dlg.RangeSelector.UpperValue;
				
				string apparent_magnification = dlg.ApparentMagnification.Text;
				statusTextBlock.Text = "AI Annotating...";
				try
				{
					double left = -1;
					BackgroundWorker timer = new BackgroundWorker();
					timer.DoWork += (s, e) => {
						statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Estimating time to finish"; }));
						while (left < 0)
						{
							Thread.Sleep(1000);
						}
						while (left > 1)
						{
							left--;
							statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Left about " + ((int)(left / 60)).ToString() + "m" + ((int)(left % 60)).ToString() + "s"; }));
							Thread.Sleep(1000);
						}
					};
					timer.RunWorkerAsync();
					BackgroundWorker bk = new BackgroundWorker();
					List<string> fileToDelete = new List<string>();
					bk.DoWork += async (s, e) =>
					{
						DateTime start = DateTime.Now;
						for (int i = from - 1; i <= to - 1; i++)
						{
							int index = i;

							
							string TempFilePath = System.IO.Path.GetTempFileName();
							SetProgressBar(index - (from - 1), (to - from + 1), TempFilePath);
							await Resource.ImageDatas[index].GetAIAnnotations(TempFilePath,
								
								apparent_magnification,
								index == to - 1).ConfigureAwait(true);
							fileToDelete.Add(TempFilePath);
							
							// time estimate
							if (to - from > 1)
							{
								DateTime now = DateTime.Now;
								double during = ((TimeSpan)(now - start)).TotalSeconds;
								double avg = during / (i - (from - 1) + 1);
								if (((to - 1) - i) * avg < left || left < 0)
									left = ((to - 1) - i) * avg;
							}
						}

						statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { ResetProgressBar(); statusTextBlock.Text = "Finish"; }));

					};
					bk.RunWorkerCompleted += (s, e) =>
					{
						foreach (var file in fileToDelete)
						{
							File.Delete(file);
						}

					};
					bk.RunWorkerAsync();

					//Zoom = zoom;
				}
				catch (Exception ex)
				{
					ResetProgressBar();
					statusTextBlock.Text = "Failed";
					messagebox.Show(ex.ToString(), "ERROR code 0002\n AI inference function failed.");
				}
			}
		}

		private int StatisticDataFrom = 0;
		public Dictionary<string, ChartValues<double>> StatisticDataDictionary = new Dictionary<string, ChartValues<double>>();
		private async void StatisticDataButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded || Resource.nowImageData.ImageType == Resource.ImageType.WSI || Resource.nowImageData.isHugeImage) return;
			var dlg = new Subwindow.StatisticDialogBox { Owner = this, };
			string[] channlTexts = new string[3];

			dlg.ShowDialog();
			if (dlg.DialogResult == true)
			{
				channlTexts[(int)Resource.RGBEffect.RIndex] = dlg.RedComboBox.Text;
				channlTexts[(int)Resource.RGBEffect.GIndex] = dlg.GreenComboBox.Text;
				channlTexts[(int)Resource.RGBEffect.BIndex] = dlg.BlueComboBox.Text;
				MainMenu.IsEnabled = false;
				InitProgressBar("Initialing...");
				statusTextBlock.Text = "Calculating antibody percentage...";
				int from = (int)dlg.RangeSelector.LowerValue;
				int to = (int)dlg.RangeSelector.UpperValue;
				try
				{
					statisticPanel.ClearData();
					StatisticDataDictionary = new Dictionary<string, ChartValues<double>>();
					ObservableCollection<long> indexLabels = new ObservableCollection<long>();
					ChartValues<double> ki67Percentage = new ChartValues<double>();
					StatisticDataDictionary.Add("Expression", ki67Percentage);
					statisticPanel.PlotData("Index", "Percentage", from, StatisticDataDictionary);
					StatisticDataFrom = from;

					BackgroundWorker timer = new BackgroundWorker();
					double left = -1;
					timer.DoWork += (s, e) => {
						statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Estimating time to finish"; }));
						while (left < 0)
						{
							Thread.Sleep(1000);
						}
						while (left > 1)
						{
							left--;
							statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Left about " + ((int)(left / 60)).ToString() + "m" + ((int)(left % 60)).ToString() + "s"; }));
							Thread.Sleep(1000);
						}
					};
					timer.RunWorkerAsync();
					Action action = async delegate
					{
						InitProgressBar("Inferencing...");
						statisticPanel.InitColumn();
						statisticPanel.AddColumn("Percentage");
						statisticPanel.AddColumn("Expression");

						DateTime start = DateTime.Now;
						for (int i = from - 1; i <= to - 1; i++)
						{
							string TempFilePath = System.IO.Path.GetTempFileName();
							SetProgressBar((i - (from - 1)) * 99 / (to - from + 1), (((i - (from - 1) + 1) * 99)) / (to - from + 1));

							await Resource.ImageDatas[i].UpdateAntibodyPercentage(
								channlTexts[0],
								channlTexts[1],
								channlTexts[2],
								dlg.ApparentMagnification.Text,
								dlg.RangeMin.Text,
								dlg.RangeMax.Text,
								dlg.Threshold.IsChecked.Value,
								dlg.ThresholdValue.Value,
								dlg.Parallel.IsChecked.Value
								).ConfigureAwait(true);
							List<string> gridData = new List<string>() { (i + 1).ToString(), System.IO.Path.GetFileName(Resource.ImageDatas[i].Dir) };

							double percentage = Resource.ImageDatas[i].AIModule.AntibodyPercentage;
							ExpressionOpt expression = percentage > 14 ? ExpressionOpt.High : ExpressionOpt.Low;
							gridData.Add(percentage.ToString("F2"));
							gridData.Add(expression.ToString());
							UpdateStatisticData();

							ki67Percentage.Add(Resource.ImageDatas[i].AIModule.AntibodyPercentage);
							statisticPanel.AddGirdData(gridData.ToArray());
							// time estimate
							if (to - from > 1)
							{
								DateTime now = DateTime.Now;
								double during = ((TimeSpan)(now - start)).TotalSeconds;
								double avg = during / (i - (from - 1) + 1);
								if (((to - 1) - i) * avg < left || left < 0)
									left = ((to - 1) - i) * avg;
							}
						}
						ResetProgressBar();
						statusTextBlock.Text = "Finish";
					};
					await Dispatcher.CurrentDispatcher.BeginInvoke(action);
				}
				catch (Exception ex)
				{
					ResetProgressBar();
					statusTextBlock.Text = "Failed";
					messagebox.Show(ex.ToString(), "Connection Failed!");
				}
			}
		}

		private async void PluginFunctionButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded || Resource.nowImageData.ImageType == Resource.ImageType.WSI || Resource.nowImageData.isHugeImage) return;
			var dlg = new Subwindow.PluginDialogBox((sender as UIElement).Uid) { Owner = this, };
			dlg.ShowDialog();
			if (dlg.DialogResult == true)
			{
				MainMenu.IsEnabled = false;
				InitProgressBar("Inferencing...");
				statusTextBlock.Text = "Calculating antibody percentage...";
				int from = (int)dlg.RangeSelector.LowerValue;
				int to = (int)dlg.RangeSelector.UpperValue;
				try
				{
					var cell = new CellCounting();
					cell.Show();
					var cellDataDictionary = new Dictionary<string, ChartValues<double>>();
					ChartValues<double> CellPercentage = new ChartValues<double>();
					cellDataDictionary.Add("value", CellPercentage);
					cell.statisticPanel.PlotData("Index", "Counting", from, cellDataDictionary);
					cell.statisticPanel.InitColumn();
					cell.statisticPanel.AddColumn("Counting");

					BackgroundWorker timer = new BackgroundWorker();
					double left = -1;
					timer.DoWork += (s, e) => {
						statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Estimating time to finish"; }));
						while (left < 0)
						{
							Thread.Sleep(1000);
						}
						while (left > 1)
						{
							left--;
							statusTextBlock.Dispatcher.BeginInvoke(new Action(() => { statusTextBlock.Text = "Left about " + ((int)(left / 60)).ToString() + "m" + ((int)(left % 60)).ToString() + "s"; }));
							Thread.Sleep(1000);
						}
					};
					timer.RunWorkerAsync();
					Action action = async delegate
					{
						InitProgressBar();
						DateTime start = DateTime.Now;
						for (int i = from - 1; i <= to - 1; i++)
						{
							string TempFilePath = System.IO.Path.GetTempFileName();
							SetProgressBar((i - (from - 1)) * 99 / (to - from + 1), (((i - (from - 1) + 1) * 99)) / (to - from + 1));

							await Resource.ImageDatas[i].UpdateAntibodyPercentage(dlg.functionData).ConfigureAwait(true);

							List<string> gridData = new List<string>() { (i + 1).ToString(), System.IO.Path.GetFileName(Resource.ImageDatas[i].Dir) };

							double percentage = Resource.ImageDatas[i].ObjectCounting;
							gridData.Add(percentage.ToString());

							CellPercentage.Add(Resource.ImageDatas[i].ObjectCounting);
							cell.statisticPanel.AddGirdData(gridData.ToArray());
							// time estimate
							if (to - from > 1)
							{
								DateTime now = DateTime.Now;
								double during = ((TimeSpan)(now - start)).TotalSeconds;
								double avg = during / (i - (from - 1) + 1);
								if (((to - 1) - i) * avg < left || left < 0)
									left = ((to - 1) - i) * avg;
							}

						}

						ResetProgressBar();
						statusTextBlock.Text = "Finish";
					};
					await statusProgressBar.Dispatcher.BeginInvoke(action);

				}
				catch (Exception ex)
				{
					statusTextBlock.Text = "Failed";
					ResetProgressBar();
					messagebox.Show(ex.ToString(), "Connection Failed!");
				}
			}
		}

		private void ExportReportButtonClick(object sender, RoutedEventArgs e)
		{
			var reportPreview = new Subwindow.ReportPreview(statisticPanel.dataGrid, "Index", "Percentage", StatisticDataFrom, StatisticDataDictionary);
			reportPreview.Show();
		}

		private void _3DViewerButtonClick()
		{
			if (_3DViewerWindow!=null && _3DViewerWindow.IsActive)
			{
				_3DViewerWindow.Close();
				return;
			}
			List<string> filenames = new List<string>();
			List<int> tiffIndex = new List<int>();
			if (Resource.ImageDatas[0].ImageType == Resource.ImageType.WSI || Resource.ImageDatas[0].ImageType == Resource.ImageType.DICOM)
			{
				return;
			}
			if (Resource.ImageDatas[0].ImageType == Resource.ImageType.DICOM3D)
			{
				int dataW = Resource.ImageDatas[0].DataWidth;
				int dataH = Resource.ImageDatas[0].DataHeight;
				foreach (var data in Resource.ImageDatas)
				{
					if (dataW != data.DataWidth || dataH != data.DataHeight)
					{
						messagebox.Show("The DICOM images size are inconsist, cannot build 3D viewer", "Image format error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
					filenames.Add(data.Dir);
					tiffIndex.Add(data.TifIndex);
				}
				_3DViewerWindow = new Subwindow._3DViewer(filenames.ToArray(), tiffIndex.ToArray(), dataW, dataH);

				_3DViewerWindow.body.SetBinding(Window.EffectProperty, new Binding()
				{
					Source = Resource.MainWindow.scrollViewer,
					Path = new PropertyPath("Effect"),
				});
				_3DViewerWindow.Show();
			}
			else if (Resource.ImageDatas[0].TifIndex >= 0)
			{
				_3DViewerWindow = new Subwindow._3DViewer(Resource.ImageDatas[0].Dir);
				_3DViewerWindow.SetBinding(Window.EffectProperty, new Binding()
				{
					Source = Resource.MainWindow.scrollViewer,
					Path = new PropertyPath("Effect"),
				});
				_3DViewerWindow.Show();

			}
			else
			{
				int dataW = Resource.ImageDatas[0].DataWidth;
				int dataH = Resource.ImageDatas[0].DataHeight;

				foreach (var data in Resource.ImageDatas)
				{
					if (dataW != data.DataWidth || dataH != data.DataHeight || data.TifIndex >= 0) 
					{ 
					messagebox.Show("The images size are inconsist, cannot build 3D viewer", "Image format error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
					filenames.Add(data.Dir); 
					tiffIndex.Add(data.TifIndex);
				}
				_3DViewerWindow = new Subwindow._3DViewer(filenames.ToArray(), tiffIndex.ToArray(), dataW, dataH);

				_3DViewerWindow.SetBinding(Window.EffectProperty, new Binding()
				{
					Source = Resource.MainWindow.scrollViewer,
					Path = new PropertyPath("Effect"),
				});
				_3DViewerWindow.Show();
				
			}
			
		}

		private void aboutUs_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new Subwindow.AboutUs { };
			dlg.ShowDialog();
		}

		#endregion


		private double lastProgressTime = 1;
		public void SetProgressBar(double StartNumber, double EndNumber)
		{
			statusProgressBar.Value = StartNumber;
			double now = statusProgressBar.Value;
			BackgroundWorker bgWorker = new BackgroundWorker();
			progressBarTasks.Enqueue(bgWorker);
			bgWorker.WorkerSupportsCancellation = true;
			bgWorker.DoWork += (s, e) =>
			{
				while (now < EndNumber && statusProgressBar.Visibility == Visibility.Visible)
				{
					if (bgWorker.CancellationPending)
					{
						break;
					}

					statusProgressBar.Dispatcher.BeginInvoke(new Action(() => {
						var value = EndNumber - (EndNumber - now) * 0.999;
						if (statusProgressBar.Value < value)
							now = statusProgressBar.Value = value;
					}));

					Thread.Sleep(25);
				}
			};
			bgWorker.RunWorkerAsync();
		}
		private string getProgressPercentage(string TempFilePath)
		{
			string lastLine = "";
			try
			{
				StreamReader r = new StreamReader(TempFilePath);

				while (!r.EndOfStream)
				{
					lastLine = r.ReadLine();
				}
				r.Close();
			}
			catch { Console.WriteLine("reading progressBar catch" + TempFilePath); }


			return lastLine;
		}
		public void SetProgressBar(int now, int total, string TempFilePath)
		{
			BackgroundWorker bgWorker = new BackgroundWorker();
			progressBarTasks.Enqueue(bgWorker);
			bgWorker.WorkerSupportsCancellation = true;
			bgWorker.DoWork += (s, e) =>
			{
				int timeout = 0;
				string progressPercentage = getProgressPercentage(TempFilePath);
				var stringList = progressPercentage.Split('\t');
				double percentage = 0;
				if (!string.IsNullOrEmpty(progressPercentage))
				{
					percentage = double.Parse(stringList[0]);
				}

				while (percentage < 95 && timeout < 60 && statusProgressBar.Visibility == Visibility.Visible)
				{
					if (bgWorker.CancellationPending)
					{
						break;
					}
					progressPercentage = getProgressPercentage(TempFilePath);
					if (!string.IsNullOrEmpty(progressPercentage))
					{
						stringList = progressPercentage.Split('\t');
						percentage = double.Parse(stringList[0]);
						statusProgressBar.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
							statusProgressBar.Value = 100 * (((double)now + percentage / 100) / total);
							progressBarText.Text = stringList[1];
						}));
					}
					else
					{
						timeout++;
						Thread.Sleep(1000);
					}
					Thread.Sleep(100);
				}
			};
			bgWorker.RunWorkerAsync();
		}
		private Queue<BackgroundWorker> progressBarTasks = new Queue<BackgroundWorker>();

		public void ResetProgressBar()
		{
			Resource.progressBarMax = 101;
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
			{
				while (progressBarTasks.Count > 0)
				{
					progressBarTasks.Dequeue().CancelAsync();
				}

				scrollViewer.IsEnabled = true;
				annotationSelectorGrid.IsHitTestVisible = true;
				scrollViewer.Opacity = 1;
				statusProgressBar.Visibility = Visibility.Collapsed;
				statusProgressBar.Value = 0;
				MainMenu.IsEnabled = true;
			}));
		}

		public void InitProgressBar()
		{
			scrollViewer.IsEnabled = false;
			annotationSelectorGrid.IsHitTestVisible = false;
			scrollViewer.Opacity = 0.65;
			Resource.progressBarMax = 0;
			statusProgressBar.Visibility = Visibility.Visible;
			statusProgressBar.Value = 0;
			progressBarText.Text = "Initializing...";
		}

		public void InitProgressBar(string displayMessage)
		{
			scrollViewer.IsEnabled = false;
			annotationSelectorGrid.IsHitTestVisible = false;
			scrollViewer.Opacity = 0.65;
			Resource.progressBarMax = 0;
			statusProgressBar.Visibility = Visibility.Visible;
			statusProgressBar.Value = 0;
			progressBarText.Text = displayMessage;
		}

		private void CleanAnnotationButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isLayerExist) return;
			MessageBoxResult result = messagebox.Show("You are clearning current annotation layer, are you sure?",
											  "Confirmation",
											  MessageBoxButton.YesNo,
											  MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				undoStack.Push(new UndoData(Resource.SelectedImageIndex, Resource.nowAnnotationLayer, Resource.nowAnnotationLayer.Layer.Clone()));
				redoStack.Clear();
				Resource.nowImageData.CleanAnnotations();
			}

			DrawContour();
		}

		private void SelectAll(object sender, RoutedEventArgs e)
		{
			if (!Resource.isLayerExist) return;
			undoStack.Push(new UndoData(Resource.SelectedImageIndex, Resource.nowAnnotationLayer, Resource.nowAnnotationLayer.Layer.Clone()));
			redoStack.Clear();
			Resource.nowImageData.FillColor();
			DrawContour();
		}

		private static int cacheRange = 11;
		private List<BitmapImage?> cacheBitmapImage = new List<BitmapImage?>(cacheRange);
		private void ImagePickerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (Resource.isDataReaded)
			{
				Resource.SelectedImageIndex = (int)e.NewValue - 1;
				SetImage(Resource.SelectedImageIndex);

				DrawZoomRect();
				UpdateStatisticData();
			}
		}

		private void DepthTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			DepthTextBox.Opacity = 0;
		}

		private void DepthTextBox_KeyDown(object sender, KeyEventArgs e)
		{

			switch (e.Key)
			{
				case Key.Escape:
					DepthTextBox.Text = ((int)ImagePickerSlider.Value).ToString();
					ImagePickerSlider.Focus();
					break;
				case Key.Enter:
					try { ImagePickerSlider.Value = int.Parse(DepthTextBox.Text); }
					catch
					{
						DepthTextBox.Text = ((int)ImagePickerSlider.Value).ToString();
					}
					ImagePickerSlider.Focus();
					break;
				case Key.Space:
					e.Handled = true; // The MS shit UI EventHandler cannot get keyboard space as TextInput
					break;
			}
		}
		private void DepthTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{

			DepthTextBox.Opacity = 1;
			DepthTextBox.Focus();
		}

		private void DepthTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex re = new Regex("[^0-9]+");
			if (string.IsNullOrEmpty(e.Text))
			{
				(sender as TextBox).Text = "1";
			}
			else
			{
				e.Handled = re.IsMatch(e.Text);
			}
		}

		private void ImagePickerSlider_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				ImagePickerSlider.Value += 1;
			}
			else if (e.Delta < 0)
			{
				ImagePickerSlider.Value -= 1;
			}
		}

		void UpdateStatisticData()
		{
			if (Resource.nowImageData.AIModule.AntibodyPercentage >= 0)
			{
				statisticDataTextBlock.Text =
					string.Format("Score: {0}({1:N2}%)",
					Resource.nowImageData.AIModule.AntibodyPercentage > 14 ? "High" : "Low",
					Resource.nowImageData.AIModule.AntibodyPercentage);
			}
			else
			{
				statisticDataTextBlock.Text = "";
			}
		}

		private void HideUnHideButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			AnnotationHidden ^= true;
			ContourShower.Visibility = AnnotationHidden ? Visibility.Hidden : Visibility.Visible;
		}

		#region ScrollViewer Event

		private Point _oldPosition = new Point(0, 0);

		private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
		{
			Resource.WriteableBitmapContour = new WriteableBitmap((int)scrollViewer.ActualWidth, (int)scrollViewer.ActualHeight, 96, 96, PixelFormats.Pbgra32, null);
			Resource.WriteableBitmapContour.Clear(Colors.Transparent);
			ContourShower.Background = new ImageBrush { ImageSource = Resource.WriteableBitmapContour };
			Resource.WriteableBitmapView = new WriteableBitmap((int)scrollViewer.ActualWidth, (int)scrollViewer.ActualHeight, 96, 96, PixelFormats.Pbgra32, null);
			Resource.WriteableBitmapView.Clear(Colors.Transparent);
			Resource.ViewHeight = (int)scrollViewer.ActualHeight;
			Resource.ViewWidth = (int)scrollViewer.ActualWidth;
			Resource.View = new byte[Resource.ViewWidth * Resource.ViewHeight * 4];

			scrollViewer.Effect = Resource.RGBEffect;
		}

		private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Resource.ViewHeight = (int)e.NewSize.Height;
			Resource.ViewWidth = (int)e.NewSize.Width;
			Resource.View = new byte[Resource.ViewWidth * Resource.ViewHeight * 4];
			Resource.WriteableBitmapContour = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Pbgra32, null);
			Resource.WriteableBitmapContour.Clear(Colors.Transparent);
			ContourShower.Background = new ImageBrush { ImageSource = Resource.WriteableBitmapContour };
			
			Resource.WriteableBitmapView = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Pbgra32, null);
			Resource.WriteableBitmapView.Clear(Colors.Transparent);
			HugeImageShower.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding() { Source = Resource.WriteableBitmapView });

			Position = position;
			UpdateDrawingCursor();
			DrawContour();
		}

		private BackgroundWorker scrollViewerRefresher = new BackgroundWorker() { WorkerSupportsCancellation = true };
		private void ScrollViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			_oldPosition = e.GetPosition(mainViewer);
			scrollViewer.CaptureMouse();
			Mouse.OverrideCursor = CursorsExtensions.Grabbing;
			

		}

		private void ScrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			scrollViewer.ReleaseMouseCapture();
			Mouse.OverrideCursor = null;
		}

		private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
		{
			if (scrollViewer.IsMouseCaptured && Resource.isDataReaded)
			{
				Point newPosition = e.GetPosition(mainViewer);
				var difference = newPosition - _oldPosition;
				Position = new Point(
					position.X - difference.X * Resource.nowImageData.DataWidth / mainViewer.ActualWidth,
					position.Y - difference.Y * Resource.nowImageData.DataHeight / mainViewer.ActualHeight
				);
				
			}
		}

		private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			DrawZoomRect();


			DrawContour();

		}
		#endregion

		#region mainViewer Event
		private int drawingRadius = 20;
		public int DrawingRadius
		{
			get
			{
				return drawingRadius;
			}
			set
			{
				drawingRadius = value >= 1 ? value : 1;
			}
		}

		private int erasingRadius = 20;
		public int ErasingRadius
		{
			get
			{
				return erasingRadius;
			}
			set
			{
				erasingRadius = value >= 1 ? value : 1;
			}
		}

		public int CursorRadius
		{
			get
			{
				if (Drawing)
					return DrawingRadius;
				if (Erasing)
					return ErasingRadius;
				return 0;
			}
		}

		private double[] PolyStrokeDashArray = { 2, 1 };

		private void RadioButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			e.Handled = true;
			if ((sender as RadioButton).IsChecked == true)
				(sender as RadioButton).IsChecked = false;
			else
				(sender as RadioButton).IsChecked = true;
		}

		private bool _drawing = false;
		public bool Drawing
		{
			get { return _drawing; }
			set
			{
				_drawing = value;
				UpdateDrawingCursor();
			}
		}

		private bool _erasing = false;
		public bool Erasing
		{
			get { return _erasing; }
			set
			{
				_erasing = value;
				UpdateDrawingCursor();
			}
		}
		private bool _polying = false;
		public bool Polying
		{
			get { return _polying; }
			set
			{
				if (value)
				{
					polylinePoints = new PointCollection();
				}
				else
				{
					mainCanvas.Children.Remove(polygon);
					mainCanvas.Children.Remove(polygonBackground);
					while (UIPolyPoints.Count != 0)
					{
						mainCanvas.Children.Remove(UIPolyPoints.Pop());
					}
				}
				_polying = value;
				UpdateDrawingCursor();
			}
		}
		private bool _rectangle = false;
		public bool Rectangle
		{
			get { return _rectangle; }
			set
			{
				if (value)
				{
					polylinePoints = new PointCollection();
				}
				else
				{
					mainCanvas.Children.Remove(polygon);
					mainCanvas.Children.Remove(polygonBackground);
					while (UIPolyPoints.Count != 0)
					{
						mainCanvas.Children.Remove(UIPolyPoints.Pop());
					}
				}
				_rectangle = value;
				UpdateDrawingCursor();
			}
		}
		private bool _filling = false;
		public bool Filling
		{
			get { return _filling; }
			set
			{
				_filling = value;
				UpdateDrawingCursor();
			}
		}

		private bool _3dviewering = false;
		public bool _3DViewering
		{
			get {
				if (_3DViewerWindow == null)
					return false;
				return _3DViewerWindow.IsActive; 
			}
			set
			{
				_3dviewering = value;

				if (!_3dviewering) _3DViewerWindow.Close();
				else _3DViewerButtonClick();
			}
		}
		public _3DViewer _3DViewerWindow;

		private bool _annotationHidden = false;
		bool AnnotationHidden
		{
			get
			{
				return _annotationHidden;
			}
			set
			{
				_annotationHidden = value;
			}
		}

		private Polygon polygon = new Polygon();
		private Polygon polygonBackground = new Polygon();
		private PointCollection polylinePoints = new PointCollection();
		private Stack<System.Windows.Shapes.Rectangle> UIPolyPoints = new Stack<System.Windows.Shapes.Rectangle>();

		private void Manual_DrawingButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			if (Resource.nowImageData.AnnotationLayers.Count == 0)
			{
				Resource.nowImageData.AddAnnotationLayer();
				Resource.nowImageData.SelectedLayerIndex = 0;
				Resource.AnnotationSelector.annotationSelector.SelectedIndex = 0;
				Resource.AnnotationSelector.ImageChanged();
			}
		}

		private void Manual_ErasingButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			if (Resource.nowImageData.AnnotationLayers.Count == 0)
			{
				Resource.nowImageData.AddAnnotationLayer();
				Resource.nowImageData.SelectedLayerIndex = 0;
				Resource.AnnotationSelector.annotationSelector.SelectedIndex = 0;
				Resource.AnnotationSelector.ImageChanged();
			}
		}

		private void Manual_FillButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			if (Resource.nowImageData.AnnotationLayers.Count == 0)
			{
				Resource.nowImageData.AddAnnotationLayer();
				Resource.nowImageData.SelectedLayerIndex = 0;
				Resource.AnnotationSelector.annotationSelector.SelectedIndex = 0;
				Resource.AnnotationSelector.ImageChanged();
			}
		}

		private void Manual_PolygonButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			if (Resource.nowImageData.AnnotationLayers.Count == 0)
			{
				Resource.nowImageData.AddAnnotationLayer();
				Resource.nowImageData.SelectedLayerIndex = 0;
				Resource.AnnotationSelector.annotationSelector.SelectedIndex = 0;
				Resource.AnnotationSelector.ImageChanged();
			}
		}

		private void Manual_RectangleButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
		}

		private void MainCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			mainViewer.CaptureMouse();
			if (CheckDoingFunction())
			{
				if (!Resource.isLayerExist)
				{
					mainCanvas.ReleaseMouseCapture();
					messagebox.Show("Add Annotation Layer before drawing");
					return;
				}

				undoStack.Push(new UndoData(Resource.SelectedImageIndex, Resource.nowAnnotationLayer, Resource.nowAnnotationLayer.Layer.Clone()));
				redoStack.Clear();

			}
			if (Polying)
			{
				if (Resource.nowImageData.SelectedLayerIndex > Resource.nowImageData.AnnotationLayers.Count - 1)
				{
					messagebox.Show("Add Annotation Layer before Polygon selecting");
					return;
				}
				UIViewer.IsHitTestVisible = true;
				if (mainCanvas.InputHitTest(e.GetPosition(mainViewer)) is Rectangle)
				{
					mainCanvas.Children.Remove(polygon);
					mainCanvas.Children.Remove(polygonBackground);
					var tranlatedPoints = (from point in polylinePoints
										   select new Point(point.X * annotationWidthRatio,
															point.Y * annotationHeightRatio)).ToList();


					Resource.nowImageData.DrawPolygon(Resource.nowImageData.SelectedLayerIndex, tranlatedPoints);
					polylinePoints.Clear();
					while (UIPolyPoints.Count != 0)
					{
						mainCanvas.Children.Remove(UIPolyPoints.Pop());
					}
				}
				else
				{
					if (UIPolyPoints.Count == 0)
					{
						polygonBackground.StrokeThickness = 2.5 / Zoom;
						polygonBackground.FillRule = FillRule.EvenOdd;
						polygonBackground.StrokeLineJoin = PenLineJoin.Bevel;
						polygonBackground.Stroke = ColorHelper.XamlStringColor("#99000000"); ;
						mainCanvas.Children.Add(polygonBackground);
						polygon.StrokeDashArray = new DoubleCollection(PolyStrokeDashArray);
						polygon.StrokeThickness = 1.5 / Zoom;
						polygon.FillRule = FillRule.EvenOdd;
						polygon.Stroke = Brushes.GhostWhite;
						polygon.Style = this.FindResource("AxisMarkerStyle") as Style;
						mainCanvas.Children.Add(polygon);

					}

					polylinePoints.Add(e.GetPosition(mainViewer));
					polygon.Points = polylinePoints.Clone();
					polygonBackground.Points = polylinePoints.Clone();

					Rectangle polyPoint = new Rectangle();
					polyPoint.Width = 8 / zoom;
					polyPoint.Height = 8 / zoom;
					polyPoint.Fill = new SolidColorBrush { Color = Colors.Transparent };
					polyPoint.Stroke = new SolidColorBrush { Color = Colors.White };
					polyPoint.StrokeThickness = 1 / Zoom;

					polyPoint.BitmapEffect = new DropShadowBitmapEffect() { Direction = 0, ShadowDepth = 0, Softness = 0.1, Opacity = 1.0 };
					polyPoint.Margin = new Thickness(left: e.GetPosition(mainViewer).X - polyPoint.Width / 2, top: e.GetPosition(mainViewer).Y - polyPoint.Height / 2, right: 0, bottom: 0);
					
					UIPolyPoints.Push(polyPoint);
					mainCanvas.Children.Add(polyPoint);
				}
				UIViewer.IsHitTestVisible = false;
			}
			else if (Drawing)
			{
				var tranlatedPoint = new Point(MousePos.MouseX * Resource.nowImageData.AnnotationRatio,
												MousePos.MouseY * Resource.nowImageData.AnnotationRatio);

				scrollViewer.Dispatcher.BeginInvoke(new Action(() =>
				{
					Resource.nowImageData.Draw(Resource.nowImageData.SelectedLayerIndex, tranlatedPoint, Mouse.GetPosition(scrollViewer));
				}));
			}
			else if (Erasing)
			{
				var tranlatedPoint = new Point(MousePos.MouseX * Resource.nowImageData.AnnotationRatio,
												MousePos.MouseY * Resource.nowImageData.AnnotationRatio);

				scrollViewer.Dispatcher.BeginInvoke(new Action(() => {
					Resource.nowImageData.Erase(Resource.nowImageData.SelectedLayerIndex, tranlatedPoint, Mouse.GetPosition(scrollViewer));
				}));

			}
			else if (Filling)
			{
				var tranlatedPoint = new Point(MousePos.MouseX * Resource.nowImageData.AnnotationRatio,
												MousePos.MouseY * Resource.nowImageData.AnnotationRatio);

				scrollViewer.Dispatcher.BeginInvoke(new Action(() => {
					Resource.nowImageData.Fill(Resource.nowImageData.SelectedLayerIndex, tranlatedPoint);
				}));
			}
			else if (Rectangle)
			{
				if (RectangleStart == null)
				{
					polygonBackground.StrokeDashArray = new DoubleCollection(PolyStrokeDashArray);
					polygonBackground.StrokeThickness = 2 / zoom;
					polygonBackground.FillRule = FillRule.EvenOdd;
					polygonBackground.StrokeLineJoin = PenLineJoin.Bevel;
					polygonBackground.Stroke = ColorHelper.XamlStringColor("#99000000");
					mainCanvas.Children.Add(polygonBackground);
					polygon.StrokeDashArray = new DoubleCollection(PolyStrokeDashArray);
					polygon.StrokeThickness = 1.5 / zoom;
					polygon.FillRule = FillRule.EvenOdd;
					polygon.Stroke = ColorHelper.XamlStringColor("#99ffffff");
					polygon.Style = this.FindResource("AxisMarkerStyle") as Style;
					mainCanvas.Children.Add(polygon);
				}

				RectangleStart = e.GetPosition(mainViewer);

			}

		}

		private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			if (Polying)
			{
				if (polygon.Points.Count != 0)
				{
					polygon.Points = polylinePoints.Clone();
					polygon.Points.Add(e.GetPosition(mainViewer));
					polygonBackground.Points = polylinePoints.Clone();
					polygonBackground.Points.Add(e.GetPosition(mainViewer));
				}
			}

			if (e.LeftButton == MouseButtonState.Pressed && Resource.isLayerExist)
			{
				var scrollViewerPoint = Mouse.GetPosition(scrollViewer);
				var tranlatedPoint = new Point(MousePos.MouseX * Resource.nowImageData.AnnotationRatio,
												MousePos.MouseY * Resource.nowImageData.AnnotationRatio);
				if (Rectangle)
				{

					if (RectangleStart != null)
					{
						double restrict = 600 * (Resource.MainWindow.mainViewer.ActualHeight / Resource.nowImageData.AnnotationHeight) * Resource.nowImageData.AnnotationRatio;
						Point scrollViewerMax = new Point(scrollViewer.ActualWidth, scrollViewer.ActualHeight);
						Point scrollViewerMin = new Point(0, 0);

						Point mainViewerMax = mainViewer.PointFromScreen(scrollViewer.PointToScreen(scrollViewerMax));
						Point mainViewerMin = mainViewer.PointFromScreen(scrollViewer.PointToScreen(scrollViewerMin));

						polygon.Points = new PointCollection() {
							RectangleStart.Value,
							new Point(
								MathHelper.Clip(e.GetPosition(mainViewer).X, mainViewerMin.X, mainViewerMax.X),
								RectangleStart.Value.Y),
							new Point(
								MathHelper.Clip(e.GetPosition(mainViewer).X, mainViewerMin.X, mainViewerMax.X),
								MathHelper.Clip(e.GetPosition(mainViewer).Y, mainViewerMin.Y, mainViewerMax.Y)
								),
							new Point(
								RectangleStart.Value.X,
								MathHelper.Clip(e.GetPosition(mainViewer).Y, mainViewerMin.Y, mainViewerMax.Y)
								)
						};
						polygonBackground.Points = polygon.Points.Clone();
					}
				}
				else if (Drawing)
				{
					mainViewer.Dispatcher.BeginInvoke(new Action(() =>
					{
						Resource.nowImageData.Draw(Resource.nowImageData.SelectedLayerIndex, tranlatedPoint, Mouse.GetPosition(scrollViewer));
					}));

					if (_oldDrawingPosition != null)
					{
						var oldTranlatedPoint = new Point(_oldDrawingPosition.Value.X,
														  _oldDrawingPosition.Value.Y);
						var oldScrollViewerPoint = new Point(_oldScrollViewerMousePosition.X, _oldScrollViewerMousePosition.Y); // If don't use a var to store the point the result will want wrong
						scrollViewer.Dispatcher.BeginInvoke(new Action(() =>
						{
							Resource.nowImageData.DrawLine(Resource.nowImageData.SelectedLayerIndex,
								oldTranlatedPoint,
								tranlatedPoint,
								oldScrollViewerPoint,
								scrollViewerPoint);
						}));
					}
					_oldDrawingPosition = tranlatedPoint;
					_oldScrollViewerMousePosition = scrollViewerPoint;
				}
				else if (Erasing)
				{
					scrollViewer.Dispatcher.BeginInvoke(new Action(() =>
					{
						Resource.nowImageData.Erase(Resource.nowImageData.SelectedLayerIndex, tranlatedPoint, Mouse.GetPosition(scrollViewer));
					}));

					if (_oldDrawingPosition != null)
					{
						var oldTranlatedPoint = new Point(_oldDrawingPosition.Value.X,
														  _oldDrawingPosition.Value.Y);
						var oldScrollViewerPoint = new Point(_oldScrollViewerMousePosition.X, _oldScrollViewerMousePosition.Y); // If don't use a var to store the point the result will want wrong
						scrollViewer.Dispatcher.BeginInvoke(new Action(() =>
						{
							Resource.nowImageData.EraseLine(Resource.nowImageData.SelectedLayerIndex,
								oldTranlatedPoint,
								tranlatedPoint,
								oldScrollViewerPoint,
								scrollViewerPoint);
						}));
					}
					_oldDrawingPosition = tranlatedPoint;
					_oldScrollViewerMousePosition = scrollViewerPoint;
				}
				else if (Filling)
				{
					scrollViewer.Dispatcher.BeginInvoke(new Action(() => {
						Resource.nowImageData.Fill(Resource.nowImageData.SelectedLayerIndex, tranlatedPoint);
					}));
				}
			}
		}

		private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{


			mainViewer.ReleaseMouseCapture();
			_oldDrawingPosition = null;
			if (Rectangle && RectangleStart != null && polygon.Points.Count() > 0)
			{
				var startPosition = mainViewer.PointToScreen(RectangleStart.Value);
				Point scrollViewerMax = new Point(scrollViewer.ActualWidth, scrollViewer.ActualHeight);
				Point scrollViewerMin = new Point(0, 0);


				var endPosition = mainViewer.PointToScreen(
					new Point(
						MathHelper.Clip(polygon.Points[2].X, scrollViewerMin.X, scrollViewerMax.X),
						MathHelper.Clip(polygon.Points[2].Y, scrollViewerMin.Y, scrollViewerMax.Y)
						)
					);
				polygon.Points.Clear();
				polygonBackground.Points.Clear();
				mainCanvas.Children.Remove(polygon);
				mainCanvas.Children.Remove(polygonBackground);
				
				// snip wanted area

				int snapshotWidth = (int)Math.Abs(startPosition.X - endPosition.X);
				int snapshotHeight = (int)Math.Abs(startPosition.Y - endPosition.Y);
				if (snapshotWidth > 0 && snapshotHeight > 0)
				{
					Thread.Sleep(200);
					System.Windows.Forms.DialogResult isSave = System.Windows.Forms.DialogResult.Cancel;
					string fileName = "";
					using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog
					{
						FileName = System.IO.Path.GetFileNameWithoutExtension(Resource.nowImageData.Dir) + "_snapshot",
						Filter = "png files (*.png)|*.png",
						RestoreDirectory = true
					})
					{
						isSave = saveFileDialog.ShowDialog();
						fileName = saveFileDialog.FileName;
					}
					Thread.Sleep(200);
					System.Drawing.Bitmap snapshot = new System.Drawing.Bitmap(snapshotWidth, snapshotHeight);

					System.Drawing.Graphics G = System.Drawing.Graphics.FromImage(snapshot);
					G.CopyFromScreen((int)Math.Min(startPosition.X, endPosition.X), (int)Math.Min(startPosition.Y, endPosition.Y), 0, 0, new System.Drawing.Size((int)Math.Abs(startPosition.X - endPosition.X), (int)Math.Abs(startPosition.Y - endPosition.Y)), System.Drawing.CopyPixelOperation.SourceCopy);
					Clipboard.Clear();

					Clipboard.SetImage(snapshot.ToBitmapSource());
					if (isSave == System.Windows.Forms.DialogResult.OK)
					{
						
						using (System.IO.FileStream fs = System.IO.File.Open(fileName, System.IO.FileMode.OpenOrCreate))
						{
							snapshot.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
						}
					}
				}


				RectangleStart = null;
			}
			DrawContour();
		}
		class CustomShaderEffect : ShaderEffect
		{
			public CustomShaderEffect(PixelShader shader)
			{
				PixelShader = shader;
				UpdateShaderValue(InputProperty);
				UpdateShaderValue(HematoxylinIndexProperty);
				UpdateShaderValue(EosinIndexProperty);
				UpdateShaderValue(BackgroundIndexProperty);
			}

			public Brush Input
			{
				get { return (Brush)GetValue(InputProperty); }
				set { SetValue(InputProperty, value); }
			}

			public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(CustomShaderEffect), 0);
			/// <summary>
			/// Identifies the <see cref="HematoxylinIndex"/> property
			/// </summary>
			public static readonly DependencyProperty HematoxylinIndexProperty = DependencyProperty.Register(nameof(HematoxylinIndex), typeof(double), typeof(CustomShaderEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));
			/// <summary>
			/// Identifies the <see cref="DabIndex"/> property
			/// </summary>
			public static readonly DependencyProperty EosinIndexProperty = DependencyProperty.Register(nameof(EosinIndex), typeof(double), typeof(CustomShaderEffect), new UIPropertyMetadata(1d, PixelShaderConstantCallback(1)));
			/// <summary>
			/// Identifies the <see cref="BackgroundIndex"/> property
			/// </summary>
			public static readonly DependencyProperty BackgroundIndexProperty = DependencyProperty.Register(nameof(BackgroundIndex), typeof(double), typeof(CustomShaderEffect), new UIPropertyMetadata(2d, PixelShaderConstantCallback(2)));


			public double HematoxylinIndex
			{
				get => (double)GetValue(HematoxylinIndexProperty);
				set => SetValue(HematoxylinIndexProperty, value);
			}

			public double EosinIndex
			{
				get => (double)GetValue(EosinIndexProperty);
				set => SetValue(EosinIndexProperty, value);
			}

			public double BackgroundIndex
			{
				get => (double)GetValue(BackgroundIndexProperty);
				set => SetValue(BackgroundIndexProperty, value);
			}
		}
		private Point mainCanvasMousePosition;
		private Point _oldScrollViewerMousePosition;
		private double widthRatio = 1;
		private double heightRatio = 1;
		public double annotationWidthRatio = 1;
		public double annotationHeightRatio = 1;
		private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			widthRatio = Resource.nowImageData.DataWidth / mainViewer.ActualWidth;
			heightRatio = Resource.nowImageData.DataHeight / mainViewer.ActualHeight;
			annotationWidthRatio = Resource.nowImageData.AnnotationWidth / mainViewer.ActualWidth;
			annotationHeightRatio = Resource.nowImageData.AnnotationHeight / mainViewer.ActualHeight;
		}

		private Point? RectangleStart = null;

		private Point? _oldDrawingPosition = null;

		public void DrawContour()
		{
			if (Resource.isDataReaded)
			{
				int xoffset = Math.Max(0, (int)(scrollViewer.ActualWidth - (mainViewer.ActualWidth * zoom)) / 2);// If there is blank space in scrollview the contour viewer start with 0
				int yoffset = Math.Max(0, (int)(scrollViewer.ActualHeight - (mainViewer.ActualHeight * zoom)) / 2);

				var scrollViewer_Height = scrollViewer.ActualHeight;
				var scrollViewer_Width = scrollViewer.ActualWidth;
				var scrollViewer_VerticalOffset = scrollViewer.VerticalOffset;
				var scrollViewer_HorizontalOffset = scrollViewer.HorizontalOffset;
				

				if (!Resource.isLayerExist)
				{
					Resource.WriteableBitmapContour.Clear();
					return;
				}
				else
				{
				}

				int height = (int)Resource.WriteableBitmapContour.Height;
				int width = (int)Resource.WriteableBitmapContour.Width;


				BackgroundWorker BW = new BackgroundWorker();
				if (Resource.nowAnnotationLayer.Visibility == Visibility.Visible)
				{
					BW.DoWork += (s, e) =>
					{
						Thread.CurrentThread.Priority = ThreadPriority.Highest;

						ContourShower.Dispatcher.BeginInvoke(new Action<AnnotationLayer, int, int, int, int, int, bool>(renderAnnotation),
									Resource.nowAnnotationLayer, yoffset, xoffset, height, width, ColorHelper.IntColorFromARGB(255,
																			   (byte)(Resource.nowAnnotationLayer.AnnotationColor.R),
																			   (byte)(Resource.nowAnnotationLayer.AnnotationColor.G),
																			   (byte)(Resource.nowAnnotationLayer.AnnotationColor.B)
																		   ),
									true
									);
					};
				}
				else
				{
					BW.DoWork += (s, e) =>
					{
						Thread.CurrentThread.Priority = ThreadPriority.Highest;

						ContourShower.Dispatcher.BeginInvoke(new Action(() => {
							Resource.WriteableBitmapContour.Clear();
						}));
					};
				}

				BW.DoWork += (s, e) =>
				{
					int i = 0;
					Thread.CurrentThread.Priority = ThreadPriority.Lowest;
					foreach (var L in Resource.nowImageData.AnnotationLayers)
					{
						if (i != Resource.nowImageData.SelectedLayerIndex && L.Visibility == Visibility.Visible)
						{
							ContourShower.Dispatcher.BeginInvoke(new Action<AnnotationLayer, int, int, int, int, int, bool>(renderAnnotation),
								L, yoffset, xoffset, height, width, ColorHelper.IntColorFromARGB(200,
																		   (byte)(L.AnnotationColor.R),
																		   (byte)(L.AnnotationColor.G),
																		   (byte)(L.AnnotationColor.B)
																	   ),
								false
								);
						}
						i += 1;
					}

				};
				BW.RunWorkerAsync();
			}
		}

		private void renderAnnotation(AnnotationLayer L, int yoffset, int xoffset, int height, int width, int OutlineColorInt, bool showAnnotation)
		{
			try
			{
				Resource.WriteableBitmapContour.Lock();
				unsafe
				{
					IntPtr pBackBuffer = L.Layer.BackBuffer;
					int BackBufferStride = L.Layer.BackBufferStride;

					IntPtr ContourBackBuffer = Resource.WriteableBitmapContour.BackBuffer;
					int contourBackBufferStride = Resource.WriteableBitmapContour.BackBufferStride;

					double union = annotationHeightRatio / Zoom;
					int annotationHeight = (int)L.Layer.Height;
					int annotationWidth = (int)L.Layer.Width;

					var nowColor = L.AnnotationColor;
					int AnnotationColorInt = ColorHelper.IntColorFromARGB(nowColor.A, (byte)((int)nowColor.R * nowColor.A / 255), (byte)((int)nowColor.G * nowColor.A / 255), (byte)((int)nowColor.B * nowColor.A / 255));// 
					var WriteableBitmapContour_PixelHeight = Resource.WriteableBitmapContour.PixelHeight;
					var WriteableBitmapContour_PixelWidth = Resource.WriteableBitmapContour.PixelWidth;
					Parallel.For(0, WriteableBitmapContour_PixelHeight, y =>
					{
						int anny = (int)MathHelper.Clip((y - yoffset + scrollViewer.VerticalOffset + 0.5) * (annotationHeightRatio) / Zoom, 0, annotationHeight - 1);
						int anny_bias = anny * BackBufferStride;
						int annx_bias;
						int up, down, left, right;

						for (int x = 0; x < WriteableBitmapContour_PixelWidth; x++)
						{
							if (y - yoffset < 0 || y + yoffset >= WriteableBitmapContour_PixelHeight || x - xoffset < 0 || x + xoffset >= WriteableBitmapContour_PixelWidth)
							{
								*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = 0;
								continue;
							}
							int annx = (int)MathHelper.Clip((x - xoffset + scrollViewer.HorizontalOffset + 0.5) * (annotationWidthRatio) / Zoom, 0, annotationWidth - 1); ;

							annx_bias = annx << 2;

							up = annx_bias + Math.Max(anny_bias - (int)Math.Ceiling(union) * BackBufferStride, 0);
							down = annx_bias + Math.Min(anny_bias + (int)Math.Ceiling(union) * BackBufferStride, (int)(annotationHeight - (int)Math.Ceiling(union)) * BackBufferStride);
							left = anny_bias + Math.Max(annx_bias - (int)Math.Ceiling(union) * 4, 0);
							right = anny_bias + Math.Min(annx_bias + (int)Math.Ceiling(union) * 4, (int)(annotationWidth - (int)Math.Ceiling(union)) * 4);
							if (showAnnotation)
							{
								if (*((int*)(pBackBuffer + anny_bias + annx_bias)) != 0)
								{
									*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = AnnotationColorInt;
								}
								else
								{
									*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = 0;
								}
							}
							else
							{
								if (*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) != 0)
								{
									continue;
								}
							}


							if (*((int*)(pBackBuffer + anny_bias + annx_bias)) == 0 && *((int*)(pBackBuffer + up)) != 0)
							{
								*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = OutlineColorInt;
							}
							if (*((int*)(pBackBuffer + anny_bias + annx_bias)) == 0 && *((int*)(pBackBuffer + down)) != 0)
							{
								*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = OutlineColorInt;
							}
							if (*((int*)(pBackBuffer + anny_bias + annx_bias)) == 0 && *((int*)(pBackBuffer + left)) != 0)
							{
								*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = OutlineColorInt;
							}
							if (*((int*)(pBackBuffer + anny_bias + annx_bias)) == 0 && *((int*)(pBackBuffer + right)) != 0)
							{
								*((int*)(ContourBackBuffer + x * 4 + y * contourBackBufferStride)) = OutlineColorInt;
							}
						}
					});
				}

				Resource.WriteableBitmapContour.AddDirtyRect(
				new Int32Rect(
					0,
					0,
					(int)Resource.WriteableBitmapContour.Width,
					(int)Resource.WriteableBitmapContour.Height));

			}
			finally
			{
				// Release the back buffer and make it available for display.
				Resource.WriteableBitmapContour.Unlock();
			}

		}
		
		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (!Resource.isDataReaded)
				return;
			e.Handled = true;
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				// Cursor size hotkey
				if (Drawing)
				{
					if (e.Delta > 0)
					{
						DrawingRadius = (int)((DrawingRadius + 1) * 1.1);
					}
					else if (e.Delta < 0)
					{
						DrawingRadius = (int)((DrawingRadius - 1) * 0.9);
					}
					UpdateDrawingCursor();
				}
				else if (Erasing)
				{
					if (e.Delta > 0)
					{
						ErasingRadius = (int)((ErasingRadius + 1) * 1.1);

					}
					else if (e.Delta < 0)
					{
						ErasingRadius = (int)((ErasingRadius - 1) * 0.9);

					}
					UpdateDrawingCursor();
				}
			}
			else if (Keyboard.Modifiers == ModifierKeys.None)
			{
				// Zoom in/out
				if (polylinePoints.Count != 0 || RectangleStart != null)
					return;
				var mousePosition = e.GetPosition(mainViewer);
				
				var tranlatedPoint = new Point(mousePosition.X * widthRatio,
												mousePosition.Y * heightRatio);
				
				var oldPosition = Position;
				var vector = tranlatedPoint - oldPosition;
				
				double ceil = Math.Pow(2, Math.Ceiling(Math.Log(this.zoom + 0.0001, 2)));
				
				double step = e.Delta / Math.Abs(e.Delta) * (ceil - ceil / 2) / 10;
				vector /= ((Zoom + step) / Zoom);
				Position = tranlatedPoint - vector;
				Zoom += step;

				if (CheckDoingFunction())
				{
					UpdateDrawingCursor();
				}
			}
			else if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				// Modify layer transparency
				if (e.Delta > 0)
				{
					Resource.nowImageData.LayerTransparency(Resource.nowImageData.SelectedLayerIndex, 10);
				}
				else if (e.Delta < 0)
				{
					Resource.nowImageData.LayerTransparency(Resource.nowImageData.SelectedLayerIndex, -10);
				}
				DrawContour();
			}
			else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
			{
				// Change layer hotkey
				if (e.Delta > 0)
				{
					ImagePickerSlider.Value += 1;
				}
				else if (e.Delta < 0)
				{
					ImagePickerSlider.Value -= 1;
				}
			}
		}

		private bool CheckDoingFunction()
		{
			if (Drawing || Erasing || Polying || Filling)
				return true;

			return false;
		}

		private void UpdateDrawingCursor()
		{
			if (Drawing || Erasing)
			{
				double _radius = (CursorRadius * mainViewer.ActualHeight / Resource.nowImageData.DataHeight * 2) * Zoom / 2;

				Resource.WriteableBitmapContourCursor = CursorsExtensions.WriteableBitmapCursor(
								_radius,
								_radius,
								Brushes.Red,
								new Pen(Brushes.Green, 1));

				Resource.WriteableBitmapCursor = CursorsExtensions.WriteableBitmapCursor(
								CursorRadius * Resource.nowImageData.AnnotationRatio,
								CursorRadius * Resource.nowImageData.AnnotationRatio,
								Brushes.Red,
								new Pen(Brushes.Green, (Resource.nowImageData.AnnotationWidth / scrollViewer.ViewportWidth / Zoom)));
				mainViewer.Cursor = CursorsExtensions.CreateCursor(_radius, _radius, Brushes.Transparent, new Pen(Brushes.White, 2));
			}
			else if (Polying || Filling || Rectangle)
			{
				mainViewer.Cursor = Cursors.Cross;
			}
			else
			{
				mainViewer.Cursor = null;
			}

		}

		#endregion

		private async void SaveAnnotationMenuItem_Click(object sender, RoutedEventArgs e)
		{
			InitProgressBar("Saving data...");
			Action action = async delegate {
				SetProgressBar(1, 99);
				await Resource.nowImageData.SaveAnnotations(null).ConfigureAwait(true);
				ResetProgressBar();
			};
			Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, action);
		}

		private async void SaveAllAnnotationMenuItem_Click(object sender, RoutedEventArgs e)
		{
			InitProgressBar("Saving data...");
			Action action = async delegate {
				for (int progress = 0; progress < Resource.ImageDatas.Count; progress++)
				{
					Resource.progressBarMax = (progress + 1) * 99 / Resource.ImageDatas.Count;
					SetProgressBar(progress * 99 / Resource.ImageDatas.Count, (progress + 1) * 99 / Resource.ImageDatas.Count);
					await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
					{
						Resource.ImageDatas[progress].SaveAnnotations(null).ConfigureAwait(true);
					}));

				}
				ResetProgressBar();
			};
			Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, action);

		}
		private async void ExportAnnotationMenuItem_Click(object sender, RoutedEventArgs e)
		{
			using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog
			{
				FileName = System.IO.Path.GetFileNameWithoutExtension(Resource.nowImageData.Dir)+"_"+Resource.nowAnnotationLayer.Title,
				Filter = "png files (*.png)|*.png",
				RestoreDirectory = true
			})
			{

				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					InitProgressBar("Exporting data...");
					SetProgressBar(1, 99);
					await Resource.nowImageData.ExportAnnotation(saveFileDialog.FileName).ConfigureAwait(true);
					ResetProgressBar();
				}
			}
		}

		private void ConfirmOperation()
		{
			if (Polying && polylinePoints.Count > 2)
			{
				mainCanvas.Children.Remove(polygon);
				mainCanvas.Children.Remove(polygonBackground);
				var tranlatedPoints = (from point in polylinePoints
									   select new Point(point.X * Resource.nowImageData.DataWidth / mainViewer.ActualWidth,
														point.Y * Resource.nowImageData.DataHeight / mainViewer.ActualHeight)).ToList();

				Resource.nowImageData.DrawPolygon(Resource.nowImageData.SelectedLayerIndex, tranlatedPoints);
				polylinePoints.Clear();
				while (UIPolyPoints.Count != 0)
				{
					mainCanvas.Children.Remove(UIPolyPoints.Pop());
				}
				DrawContour();
			}
		}

		private void DropOperation()
		{
			if (Polying && polylinePoints.Count > 0)
			{
				mainCanvas.Children.Remove(polygon);
				mainCanvas.Children.Remove(polygonBackground);
				polylinePoints.Clear();
				while (UIPolyPoints.Count != 0)
				{
					mainCanvas.Children.Remove(UIPolyPoints.Pop());
				}
			}
			else if (Rectangle && RectangleStart != null)
			{
				polygon.Points.Clear();
				polygonBackground.Points.Clear();
				mainCanvas.Children.Remove(polygon);
				mainCanvas.Children.Remove(polygonBackground);

				RectangleStart = null;
			}
		}
		private void OpenUserManual()
		{
			if (InternetAvailability.IsInternetAvailable() || true)
			{
				Process.Start(ConfigurationManager.AppSettings["USER_MANUAL"]);
			}
			else
			{
				string curDir = Directory.GetCurrentDirectory();
				Process.Start(String.Format("file:///{0}/MetaLite User’s Guide.html", curDir));
				
			}
		}

		private void Undo()
		{
			if (undoStack.Count <= 0)
			{
				return;
			}
			var undo = undoStack.Pop();
			int idx = Resource.ImageDatas[undo.ImageIndex].AnnotationLayers.IndexOf(undo.LayerUid as AnnotationLayer);
			if (idx >= 0)
			{
				redoStack.Push(new UndoData(undo.ImageIndex, undo.LayerUid, Resource.ImageDatas[undo.ImageIndex].AnnotationLayers[idx].Layer));
				Resource.ImageDatas[undo.ImageIndex].AnnotationLayers[idx].Layer = undo.Data;
			}



			DrawContour();
		}
		private void Redo()
		{
			if (redoStack.Count <= 0)
			{
				return;
			}

			var redo = redoStack.Pop();
			int idx = Resource.ImageDatas[redo.ImageIndex].AnnotationLayers.IndexOf(redo.LayerUid as AnnotationLayer);
			if (idx >= 0)
			{
				undoStack.Push(new UndoData(redo.ImageIndex, redo.LayerUid, Resource.ImageDatas[redo.ImageIndex].AnnotationLayers[idx].Layer));
				Resource.ImageDatas[redo.ImageIndex].AnnotationLayers[idx].Layer = redo.Data;
			}

			DrawContour();
		}

		private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Z:

					break;
				case Key.H:
					var tmp = annotationSelectorGrid.FindChild<ToggleButton>("HideAllAnnotation");
					tmp.RaiseEvent(new RoutedEventArgs(ToggleButton.ClickEvent));
					break;
				case Key.P:
					manualPolygonButton.RaiseEvent(new RoutedEventArgs(RadioButton.ClickEvent));
					break;
				case Key.D:
					manualDrawingButton.RaiseEvent(new RoutedEventArgs(RadioButton.ClickEvent));
					break;
				case Key.E:
					manualErasingButton.RaiseEvent(new RoutedEventArgs(RadioButton.ClickEvent));
					break;
				case Key.Enter:
					ConfirmOperation();
					break;
				case Key.Escape:
					DropOperation();
					break;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!CheckDirtyData()) e.Cancel = true;
			KillProcess();
		}

		private void CloseImageButton_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckDirtyData()) return;
			CloseImage();
		}
		private void CloseImage()
		{
			if (NewsContent != null)
				NewsContent.Dispose();
				NewsContent = null;
			Position = new Point(0, 0);
			scrollViewer.ScrollToHorizontalOffset(0);
			scrollViewer.ScrollToVerticalOffset(0);
			UIViewer.ScrollToHorizontalOffset(0);
			UIViewer.ScrollToVerticalOffset(0);
			Resource.SvsCacheTile.Clear();
			SliderGrid.Visibility = Visibility.Collapsed;
			fileNameTextBlock.Content = "";
			Zoom = 1;
			OffAllFunction();
			mainViewer.Source = null;
			mainCanvas.Children.Clear();
			wholeImageViewer.Source = null;
			HugeImageShower.Source = null;
			Resource.WriteableBitmapContour.Clear(Colors.Transparent);
			widthRatio = 1;
			heightRatio = 1;
			annotationWidthRatio = 1;
			annotationHeightRatio = 1;
			undoStack.Clear();
			redoStack.Clear();
			foreach (var img in Resource.ImageDatas)
			{
				img.AnnotationLayers.Clear();
			}
			Resource.ImageDatas.Clear();
			Resource.SelectedImageIndex = 0;
			Resource.WriteableBitmapContour.Clear(Colors.Transparent);
			Resource.WriteableBitmapView.Clear(Colors.Transparent);
			scrollViewer.IsEnabled = false;
			scrollViewer.Opacity = 0.65;

			preLoadimage.Clear();
			primaryLoadImage.Clear();
			processingPreLoadimage.Clear();
			GC.Collect();
		}

		private bool CheckDirtyData()
		{
			if (Resource.isDataReaded)
			{
				var checkAllSave = (from imageData in Resource.ImageDatas
									where imageData.IsAnnotationDirty
									select imageData).Any();
				if (checkAllSave)
				{
					MessageBoxResult result = messagebox.Show("There are some unsaved annotations. Do you want to discard all changes?",
											  "Confirmation",
											  MessageBoxButton.YesNo,
											  MessageBoxImage.Question);
					if (result == MessageBoxResult.No)
					{
						return false;
					}
				}
			}
			return true;
		}


		private async void SaveAnnotationAsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog
			{
				FileName = System.IO.Path.GetFileNameWithoutExtension(Resource.nowImageData.Dir),
				Filter = "jellox files (*.jellox)|*.jellox",
				RestoreDirectory = true
			})
			{
				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					InitProgressBar("Saving data...");
					SetProgressBar(1, 99);
					await Resource.nowImageData.SaveAnnotations(saveFileDialog.FileName).ConfigureAwait(true);
					ResetProgressBar();
				}
			}
		}

		private void OpenAnnotationMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!CheckDirtyData()) return;
			MessageBoxResult? result = null;
			if (Resource.isLayerExist)
			{

				result = messagebox.Show("Do you want open a annotation file and discard exist annotation,\nor append annotation layers after the layers already exist?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question,
										  "Open new",
										  "Append");
				
			}
			var openFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				Filter = "annotation files (*.jellox)|*.jellox;*.png",
				RestoreDirectory = true,
				Multiselect = true
			};

			// Display OpenFileDialog by calling ShowDialog method 
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (openFileDialog.FileNames.Length == Resource.ImageDatas.Count)
				{
					if (openFileDialog.FileNames.Length > 1)
					{
						MessageBoxResult batchResult = messagebox.Show("Are you tend to open each annotation to opened images?",
										  "Confirmation",
										  MessageBoxButton.YesNo,
										  MessageBoxImage.Question,
										  "Yes",
										  "No");
						if (batchResult == MessageBoxResult.Yes)
						{
							int itr = 0;
							foreach (ImageData imageData in Resource.ImageDatas)
							{
								if (result != null && result == MessageBoxResult.Yes) imageData.AnnotationLayers.Clear();
								imageData.LoadAnnotation(openFileDialog.FileNames[itr++]);
							}
						}
						else
						{
							foreach (string fileName in openFileDialog.FileNames)
							{
								if (result != null && result == MessageBoxResult.Yes) Resource.nowImageData.AnnotationLayers.Clear();
								Resource.nowImageData.LoadAnnotation(fileName);
							}
						}
						return;
					}

					
				}
				if (result != null && result == MessageBoxResult.Yes) Resource.nowImageData.AnnotationLayers.Clear();
				foreach (string fileName in openFileDialog.FileNames)
				{
				
					Resource.nowImageData.LoadAnnotation(fileName);
				}
			}
		}

		DispatcherTimer autoSaveTimer;

		private void SetAutoSaveTimer(object sender, RoutedEventArgs e)
		{
			autoSaveTimer = new DispatcherTimer();
			autoSaveTimer.Interval = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["AUTOSAVE_TIMER"], NumberFormatInfo.InvariantInfo));
			autoSaveTimer.Tick += (s, e) => { Resource.nowImageData.SaveAnnotations(null).ConfigureAwait(true); };
			if (Resource.isDataReaded)
				autoSaveTimer.Start();
		}

		private void UnsetAutoSaveTimer(object sender, RoutedEventArgs e)
		{
			autoSaveTimer.Stop();
		}

		private void ShowAnnotationSelector(object sender, RoutedEventArgs e)
		{
			if (!Resource.AnnotationSelector.IsActive)
				Resource.AnnotationSelector.Activate();

		}
		private void MinimizeWholeImageButton_Click(object sender, RoutedEventArgs e)
		{
			Duration duration = new Duration(TimeSpan.FromSeconds(0.1));
			DoubleAnimation doubleanimation;
			ThicknessAnimation thicknessanimation;
			if ((sender as Button).Content.ToString() == "")
			{
				doubleanimation = new DoubleAnimation(0, duration);
				thicknessanimation = new ThicknessAnimation((sender as Button).Margin, new Thickness(1, 1, 1, 1), duration);

				(sender as Button).Content = "";
			}
			else
			{
				doubleanimation = new DoubleAnimation(1, duration);
				thicknessanimation = new ThicknessAnimation((sender as Button).Margin, new Thickness(224, 0, 0, 224), duration);

				(sender as Button).Content = "";
			}
			rightPanelTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleanimation);
			rightPanelTransform.BeginAnimation(ScaleTransform.ScaleYProperty, doubleanimation);
			(sender as Button).BeginAnimation(Button.MarginProperty, thicknessanimation);
		}




		private void userManual_Click(object sender, RoutedEventArgs e)
		{
			OpenUserManual();
		}

		private void Undo_Click(object sender, RoutedEventArgs e)
		{
			Undo();
		}
		private void Redo_Click(object sender, RoutedEventArgs e)
		{
			Redo();
		}

		private void MenuItemZoomIn_Click(object sender, RoutedEventArgs e)
		{
			Zoom = Zoom * 1.1;
		}
		private void MenuItemZoomOut_Click(object sender, RoutedEventArgs e)
		{
			Zoom = Zoom * 0.9;
		}
		private void MenuItemFitWindos_Click(object sender, RoutedEventArgs e)
		{
			Zoom = 1;
		}

		private void MenuItemImagePanel_Click(object sender, RoutedEventArgs e)
		{
			rightPanel.SelectedIndex = 0;
		}
		private void MenuItemStatisticPanel_Click(object sender, RoutedEventArgs e)
		{
			rightPanel.SelectedIndex = 1;
		}

		private void MenuItemRenderMode_Click(object sender, RoutedEventArgs e)
		{
			channelSelector.ColorChannels.SelectedIndex = int.Parse((sender as MenuItem).Uid);
		}

		private void NewsContent_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				NewsContent.Navigate(new Uri(ConfigurationManager.AppSettings["NEWS"]));
			}
			catch
			{
				Console.WriteLine("Catch News Page Exception");
			}
		}

		
		private void CustomizeAIModel_Click(object sender, RoutedEventArgs e)
		{
			if (InternetAvailability.IsInternetAvailable())
			{
				Process.Start(ConfigurationManager.AppSettings["CUSTOMIZE_AI_MODEL"]);
			}
			else
			{
				messagebox.Show("Your device are not connect to internet, \nplease check the internet connection.", "Disconnection error");
			}
		}

		private void FormatConvertMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var formatConvert = new FormatConvert();
			formatConvert.ShowDialog();
		}
	}
	
	public class UndoData
	{
		private int _ImageIndex;
		private object _LayerUid;
		private WriteableBitmap _Data;
		public int ImageIndex
		{
			get
			{
				return _ImageIndex;
			}
			set
			{
				_ImageIndex = value;
			}
		}
		public object LayerUid
		{
			get
			{
				return _LayerUid;
			}
			set
			{
				_LayerUid = value;
			}
		}
		public WriteableBitmap Data
		{
			get
			{
				return _Data;
			}
			set
			{
				_Data = value;
			}
		}


		public UndoData(int selectedImageIndex, object selectedLayerUid, WriteableBitmap data)
		{
			Data = data;
			ImageIndex = selectedImageIndex;
			LayerUid = selectedLayerUid;
		}


		~UndoData()
		{
		}
	}
}
