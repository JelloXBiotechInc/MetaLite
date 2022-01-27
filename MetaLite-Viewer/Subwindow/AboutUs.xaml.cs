using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Configuration;
using System.Reflection;

namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// Page1.xaml 的互動邏輯
	/// </summary>
	public partial class AboutUs : Window
	{

        private ObservableCollection<IconInfo> iconInfoList;
        public AboutUs()
        {
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            InitializeComponent();

            WarningInfo.Text += ConfigurationManager.AppSettings["WARNINGINFO"];
            FDAWarning.Text += ConfigurationManager.AppSettings["FDAWARNING"];

            pbc_version.Content = "Version:     " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); //利用反射的方式获取assembly的version信息

            Assembly asm = Assembly.GetExecutingAssembly();
            Copyright.Content = ((AssemblyCopyrightAttribute)asm.GetCustomAttribute(typeof(AssemblyCopyrightAttribute))).Copyright;
            Copyright.Content += " " + ((AssemblyCompanyAttribute)asm.GetCustomAttribute(typeof(AssemblyCompanyAttribute))).Company;
            Copyright.Content += " All Rights Reserved.";
            iconInfoList = new ObservableCollection<IconInfo>();

            iconInfoList.Add(new IconInfo("/Images/user.png", "https://www.flaticon.com/authors/chanut-is-industries", "Chanut is Industries"));
            iconInfoList.Add(new IconInfo("/Images/password.png", "https://www.flaticon.com/authors/freepik", "Freepik"));
            iconInfoList.Add(new IconInfo("/Images/user.png", "https://www.flaticon.com/authors/chanut-is-industries", "Chanut is Industries"));
            iconInfoList.Add(new IconInfo("/Images/password.png", "https://www.flaticon.com/authors/freepik", "Freepik"));
            iconInfoList.Add(new IconInfo("/Images/user.png", "https://www.flaticon.com/authors/chanut-is-industries", "Chanut is Industries"));
            iconInfoList.Add(new IconInfo("/Images/password.png", "https://www.flaticon.com/authors/freepik", "Freepik"));
            iconInfoList.Add(new IconInfo("/Images/user.png", "https://www.flaticon.com/authors/chanut-is-industries", "Chanut is Industries"));
            iconInfoList.Add(new IconInfo("/Images/password.png", "https://www.flaticon.com/authors/freepik", "Freepik"));

            this.lvIconInfo.ItemsSource = iconInfoList;
        }

        private void linkHelp_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() != typeof(Hyperlink))
                return;
            string link = ((Hyperlink)sender).NavigateUri.ToString();
            Process.Start(link);
        }

        private void linkHelp_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class IconInfo
    {
        private string icon;
        private string url;
        private string author;

        public IconInfo(string icon, string url, string author)
        {
            this.icon = icon;
            this.url = url;
            this.author = author;
        }

        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        public string Author
        {
            get
            {
                return this.author;
            }
            set
            {
                this.author = value;
            }
        }
    }
}
