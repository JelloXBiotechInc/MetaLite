using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// FormatConvert.xaml 的互動邏輯
	/// </summary>
	public partial class FormatConvert : Window
	{
		public FormatConvert()
		{
			InitializeComponent();
		}
		private Grid RunningEffect;
		private void SelectFileButton_Click(object sender, RoutedEventArgs e)
		{
			using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
			{

				// Set filter for file extension and default file extension 
				openFileDialog.Filter = "Philips iSyntax|*.isyntax;";
				openFileDialog.RestoreDirectory = true;
				openFileDialog.DereferenceLinks = false;
				openFileDialog.Multiselect = false;

				// Display OpenFileDialog by calling ShowDialog method 
				if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					SelectFileTextBlock.Text = openFileDialog.FileName;
					SelectOutputButton.IsEnabled = true;
				}
			}
		}
		private void SelectOutputButton_Click(object sender, RoutedEventArgs e)
		{
			using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
			{

				// Set filter for file extension and default file extension 
				saveFileDialog.Filter = "Tile Tiff|*.tiff;";
				saveFileDialog.RestoreDirectory = true;
				saveFileDialog.DereferenceLinks = false;
				saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(SelectFileTextBlock.Text);

				// Display OpenFileDialog by calling ShowDialog method 
				if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					SelectOutputTextBlock.Text = saveFileDialog.FileName;
					ConvertButton.IsEnabled = true;
				}
			}
		}

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			RunningEffect = new Grid();
			RunningEffect.Background = Helper.ColorHelper.XamlStringColor("#99000000");
			var sp = new StackPanel();
			sp.Orientation = Orientation.Vertical;
			sp.Margin = new Thickness() { Bottom = 10, Top = 80 };
			var progressText = new TextBlock();
			progressText.Text = "Data converting ...";
			progressText.Foreground = Helper.ColorHelper.XamlStringColor("#ffffff");
			progressText.FontSize = 15;
			progressText.HorizontalAlignment = HorizontalAlignment.Center;
			progressText.VerticalAlignment = VerticalAlignment.Center;
			progressText.Margin = new Thickness() { Bottom = 10, Top = 10 };
			sp.Children.Add(progressText);
			var progressBar = new ProgressBar();
			progressBar.IsIndeterminate = true;
			progressBar.HorizontalAlignment = HorizontalAlignment.Center;
			progressBar.VerticalAlignment = VerticalAlignment.Center;
			progressBar.Height = 5;
			progressBar.Margin = new Thickness() { Bottom = 10, Top = 10 };
			progressBar.Width = this.Width * 0.7;
			sp.Children.Add(progressBar);
			RunningEffect.Children.Add(sp);
			body.Children.Add(RunningEffect);
			if (System.IO.Path.GetExtension(SelectOutputTextBlock.Text).ToLower() == ".tiff") 
			{
				cmd = @".\isyntax_to_ometiff.py --start-level 0 --Q " + ((int)Quality.Value).ToString() + " " + SelectFileTextBlock.Text + " " + System.IO.Path.GetDirectoryName(SelectOutputTextBlock.Text);
				BackgroundWorker i2tWorker = new BackgroundWorker();
				i2tWorker.DoWork += isyntax2tiff;
				i2tWorker.RunWorkerCompleted += ConvertComplete;
				i2tWorker.RunWorkerAsync();

			}
		}
		private void ConvertComplete(object sender, RunWorkerCompletedEventArgs e) 
		{
			body.Children.Remove(RunningEffect);
		}
		private string cmd = string.Empty;
		private void isyntax2tiff(object sender, DoWorkEventArgs e)
		{
			string err = string.Empty;
			string outResult = string.Empty;
			try
			{
				using (Process process = new Process())
				{
					process.StartInfo = new ProcessStartInfo(@".\python.exe")
					{
						Arguments = (cmd),
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						CreateNoWindow = true
					};
					process.Start();
					outResult = process.StandardOutput.ReadToEnd();
					outResult = outResult.Replace(Environment.NewLine, string.Empty);
					err = process.StandardError.ReadToEnd();
					process.WaitForExit();
					if (err != string.Empty)
						messagebox.Show(err, "Convert script error.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString() + outResult.ToString() + err);
			}
		}
	}
}
