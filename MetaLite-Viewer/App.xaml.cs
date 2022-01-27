using MetaLite_Viewer.Subwindow;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MetaLite_Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            MainWindow main = new MainWindow();
            var splash = new SplashScreen();
            this.MainWindow = splash;
            splash.Show();
            // For iRIS Xe
            // RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            Task.Factory.StartNew(() =>
            {
                //simulate some work being done
                while (splash.Visibility == Visibility.Visible)
                {
                    Thread.Sleep(10);
                }

                //since we're not on the UI thread
                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    this.MainWindow = main;
                    if (!splash.CheckPass)
                    {
                        if (messagebox.Show(splash.CheckResult + "The system check is failed, some functionality of MetaLite might not be correct.\nDo you still want launch?", splash.CheckTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            this.Shutdown();
                    }
                    main.Show();
                    splash.Close();
                });
            });
        }
    }
}
