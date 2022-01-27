using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetaLite_Viewer.Subwindow
{
	/// <summary>
	/// Voxel.xaml 的互動邏輯
	/// </summary>
	public partial class VoxelSetting : Window
	{
		public double ZInterval = 0;
		public double MPP = 0.5;
		public VoxelSetting(double zinterval, double mpp)
		{
			InitializeComponent();
			ZInterval = zinterval;
			MPP = mpp;
		}
		private void DecimalTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			bool approvedDecimalPoint = false;
			
			if (e.Text == ".")
			{
				if (!((TextBox)sender).Text.Contains("."))
					approvedDecimalPoint = true;
			}

			if (!(char.IsDigit(e.Text, e.Text.Length - 1) || approvedDecimalPoint || char.IsControl(e.Text, e.Text.Length - 1)))
			{
				e.Handled = true;

			}	
		}

		
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			if (!IsValid(this)) return;
			DialogResult = true;
		}
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
		private bool IsValid(DependencyObject node)
		{
			// Check if dependency object was passed
			if (node != null)
			{
				// Check if dependency object is valid.
				// NOTE: Validation.GetHasError works for controls that have validation rules attached 
				var isValid = !Validation.GetHasError(node);
				if (!isValid)
				{
					// If the dependency object is invalid, and it can receive the focus,
					// set the focus
					if (node is IInputElement) Keyboard.Focus((IInputElement)node);
					return false;
				}
			}

			// If this dependency object is valid, check all child dependency objects
			return LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().All(IsValid);

			// All dependency objects are valid
		}

		private void TextBox_Loaded(object sender, RoutedEventArgs e)
		{
			ZAxisInterval.Text = ZInterval.ToString();
		}
		private void MPP_Loaded(object sender, RoutedEventArgs e)
		{
			MPPTextBox.Text = MPP.ToString();
		}

		private void ZAxisInterval_TextChanged(object sender, TextChangedEventArgs e)
		{
			
			Double.TryParse(ZAxisInterval.Text, out ZInterval);
		}
		private void MPP_TextChanged(object sender, TextChangedEventArgs e)
		{

			Double.TryParse(MPPTextBox.Text, out MPP);
		}
	}
}
