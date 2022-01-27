using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using MetaLite_Viewer.Helper;

namespace MetaLite_Viewer
{
	/// <summary>
	/// SplashScreen.xaml 的互動邏輯
	/// </summary>
	public partial class SplashScreen : Window
	{
		private bool checkpass = true;
		public bool CheckPass 
		{ 
			get { return checkpass; }
			set { checkpass = value; }
		}
		static PerformanceCounter cpu = new PerformanceCounter(
			"Processor", "% Processor Time", "_Total");
		static PerformanceCounter memory = new PerformanceCounter(
			"Memory", "% Committed Bytes in Use");
		public SplashScreen()
		{
			InitializeComponent();
			
		}

		private string checkTitle = "System check failed.";
		public string CheckTitle
		{
			get { return checkTitle; }
			set
			{
				checkTitle = value;
			}
		}

		private string checkResult = "";
		public string CheckResult
		{
			get { return checkResult; }
			set
			{
				checkResult = value;
			}
		}

		private List<BackgroundWorker> backgroundWorkers = new List<BackgroundWorker>();

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			version.Content = "Version: v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

			BackgroundWorker ramCheck = new BackgroundWorker();
			backgroundWorkers.Add(ramCheck);
			ramCheck.DoWork += (s, e) => {
				double RAM = (new ComputerInfo().AvailablePhysicalMemory);
				RAM = RAM / (1024 * 1024 * 1024);
				this.Dispatcher.BeginInvoke(new Action(() => {
					RamCheck.Content = "Available RAM: " + RAM.ToString("N2") + " GB";
				}));
				if (RAM > Util.AppConfig("RESERVED_MEMORY", 1024))
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						RamCheck.Content += " Checked!";
						RamCheckState.Foreground = ColorHelper.XamlStringColor("#FF32B932");
						RamCheckState.Content = "●";
					}));
				}
				else
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						checkResult += "Insufficient memory:\n";
						checkResult += "\tThe computer does not have enough memory space to execute MetaLite.\n";
						checkResult += "\tPlease close some running programs, then try again.\n";

						RamCheck.Content += " Insufficient memory";
						RamCheckState.Foreground = ColorHelper.XamlStringColor("#FFB93232");
						RamCheckState.Content = "✖";
					}));
					CheckPass = false;
				}
			};
			ramCheck.RunWorkerCompleted += (s, e) => { backgroundWorkers.Remove(ramCheck); };
			ramCheck.RunWorkerAsync();
			
			BackgroundWorker serverCheck = new BackgroundWorker();
			backgroundWorkers.Add(serverCheck);
			serverCheck.DoWork += (s, e) =>
			{
				MainWindow.KillProcess();
				if (Util.AppConfig("IS_LAUNCH_SERVER", true))
				{
					Process.Start("metalite-server.exe");
					int retryTime = 20;
					int i = 0;
					int latency = -1;
					for (i=0; i < retryTime; i++)
					{

						var text = " " + (i+1).ToString() + "/" + retryTime.ToString();
						this.Dispatcher.BeginInvoke(new Action(() => { ServerCheckState.Content = text; }));
						Thread.Sleep(500);


						try
						{
							latency = MainWindow.ProbServer();
							Console.WriteLine(latency) ;
							if (latency >= 0)
							{
								this.Dispatcher.BeginInvoke(new Action(() => {
									ServerCheck.Content += " Checked!";
									ServerCheckState.Foreground = ColorHelper.XamlStringColor("#FF32B932");
									ServerCheckState.Content = "●";
								}));
								Thread.Sleep(100);

								return;
							}
							
						}
						catch
						{
						}
						
					}
					this.Dispatcher.BeginInvoke(new Action(() => {
						checkResult += "Server launch failed:\n";
						checkResult += "\tThe metalite-server.exe is launch failed.\n";
						checkResult += "\tIf you need the function that depends on metalite-server, \n";
						checkResult += "\tplease restart the program manually.\n";
						
						ServerCheckState.Foreground = ColorHelper.XamlStringColor("#FFB93232");
						ServerCheckState.Content = "✖";
					}));					
					CheckPass = false;

				}
				else
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						ServerCheckState.Foreground = ColorHelper.XamlStringColor("#FFFFDB38");
						ServerCheckState.Content = "skip";
					}));
					Thread.Sleep(100);
				}
			};
			serverCheck.RunWorkerCompleted += (s, e) => { backgroundWorkers.Remove(serverCheck); };
			serverCheck.RunWorkerAsync();

			BackgroundWorker versionCheck = new BackgroundWorker();
			backgroundWorkers.Add(versionCheck);
			versionCheck.DoWork += (s, e) =>
			{
				var versionCheckResult = MainWindow.CheckVersion();
				if (string.IsNullOrEmpty(versionCheckResult))
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						VersionCheck.Content += " Checked!";
						VersionCheckState.Foreground = ColorHelper.XamlStringColor("#FF32B932");
						VersionCheckState.Content = "●";
					}));
				}
				else
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						checkResult += "Version check failed:\n";
						checkResult += "\t" + versionCheckResult + "\n";

						VersionCheckState.Foreground = ColorHelper.XamlStringColor("#FFB93232");
						VersionCheckState.Content = "✖";
					}));
					CheckPass = false;
				}
				
			};
			versionCheck.RunWorkerCompleted += (s, e) => { backgroundWorkers.Remove(versionCheck); };
			versionCheck.RunWorkerAsync();

			Thread.Sleep(50);
			BackgroundWorker wait = new BackgroundWorker();
			wait.DoWork += (s, e) =>
			{
				while (backgroundWorkers.Count != 0)
				{
					Thread.Sleep(100);
				}
			};
			wait.RunWorkerCompleted += (s, e) => { this.Hide(); };
			wait.RunWorkerAsync();
		}
	}
}
