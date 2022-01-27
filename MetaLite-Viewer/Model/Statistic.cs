using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLite_Viewer.Model
{
	public enum ExpressionOpt { High, Low };
	public class ki67
	{
		public long Index { get; set; }
		public string FileName { get; set; }
		public double Percentage { get; set; }
		public ExpressionOpt Expression { get; set;}
	}
	
	public class cell
	{
		public long Index { get; set; }
		public string FileName { get; set; }
		public double Counting { get; set; }
	}
}
