using System;
using System.Windows;
using JelloX3D;
using MetaLite_Viewer.Helper;
namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// _3DViewer.xaml 的互動邏輯
	/// </summary>
	public partial class _3DViewer : Window
	{
		private Object3DView viewer;
		public _3DViewer()
		{
			InitializeComponent();
		}
		public _3DViewer(string[] filenames, int[] tiffIndexs, int width, int height)
		{
			InitializeComponent();
			viewer = new Object3DView(filenames, tiffIndexs, width, height);
			body.Children.Add(viewer);
		}
		public _3DViewer(string _3DTifName)
		{
			InitializeComponent();
			viewer = new Object3DView(_3DTifName);
			body.Children.Add(viewer);
		}

		
		private void Window_Closed(object sender, EventArgs e)
		{
			Resource.MainWindow.manual3DViewer.IsChecked = false;
		}

		private void VoxelSetting_Click(object sender, RoutedEventArgs e)
		{
			var voxelsetting = new VoxelSetting(viewer.interval, viewer.mpp);
			voxelsetting.ShowDialog();
			if (voxelsetting.DialogResult == true)
			{
				viewer.ChangeInterval(MathHelper.Clip(voxelsetting.ZInterval, 0, double.PositiveInfinity));
				viewer.ChangeMPP(MathHelper.Clip(voxelsetting.MPP, 0, double.PositiveInfinity));
			}
		}
	}
}
