using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MetaLite_Viewer.Model;
using LiveCharts;
using LiveCharts.Wpf;
using report;
using System.Diagnostics;

namespace MetaLite_Viewer.Helper
{
	class Util
	{
		
		public static T AppConfig<T>(string strKey, T defaultValue)
		{
			var result = defaultValue;
			try
			{
				if (ConfigurationManager.AppSettings[strKey] != null)
					result = (T)Convert.ChangeType(ConfigurationManager.AppSettings[strKey], typeof(T));
			}
			catch (Exception ex)
			{
				Console.WriteLine("AppConfig get ConfigurationManager.AppSettings exception");
			}

			return result;
		}
	}
}
