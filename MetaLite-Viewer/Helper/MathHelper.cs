using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLite_Viewer.Helper
{
	class MathHelper
	{
		public static int Clip(int input, int min, int max)
		{
			return Math.Max(min, Math.Min(max, input));
		}

		public static double Clip(double input, double min, double max)
		{
			return Math.Max(min, Math.Min(max, input));
		}
	}
}
