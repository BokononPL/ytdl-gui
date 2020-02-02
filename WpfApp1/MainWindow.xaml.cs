using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WpfApp1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			PathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}

		private void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			DisableButtonFor(DownloadButton, 1000);
			string link = AddressBox.Text;
			string outputFolderOption = "";
			if (PathBox.Text != "")
			{
				outputFolderOption = $" -o \"{PathBox.Text}\\%(title)s.%(ext)s\"";
			}
			string programArg = $"{ ((KeepCmdOpen.IsChecked == true) ? "/k" : "/c") } youtube-dl.exe ";
			programArg += link + outputFolderOption;
			var process = System.Diagnostics.Process.Start(Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe", programArg);
		}


		private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			DialogResult result = dialog.ShowDialog();
			if (result.ToString() == "OK")
				FileBox.Text = dialog.FileName;
			var duration = GetDuration(dialog.FileName);
			LengthSlider.Maximum = duration;
			LengthSlider.HigherValue = duration;
		}

		private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			DialogResult result = folderDialog.ShowDialog();
			if (result.ToString() == "OK")
				PathBox.Text = folderDialog.SelectedPath;
		}

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			DisableButtonFor(ConvertButton, 1000);
			string outputFolderOption = "";
			if (PathBox.Text != "")
			{
				string fileName = System.IO.Path.GetFileNameWithoutExtension(FileBox.Text);
				var vol = VolumeSlider.Value.ToString().Replace(',', '.');
				TimeSpan startTime = TimeSpan.FromSeconds(LengthSlider.LowerValue);
				string startTimeStr = startTime.ToString(@"hh\:mm\:ss\.fff");
				TimeSpan endTime = TimeSpan.FromSeconds(LengthSlider.HigherValue);
				string endTimeStr = endTime.ToString(@"hh\:mm\:ss\.fff");
				outputFolderOption = $" -i \"{FileBox.Text}\" -f mp3 -q:a {AudioQualitySlider.Value} -filter:a \"volume={vol}\"  -ss {startTimeStr} -to {endTimeStr} \"{PathBox.Text}\\{fileName}.mp3\"";
			}
			string programArg = $"{ ((KeepCmdOpen.IsChecked == true) ? "/k" : "/c") } ffmpeg.exe ";
			programArg += outputFolderOption;
			System.Diagnostics.Process.Start(Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe", programArg);
		}

		private void DisableButtonFor(System.Windows.Controls.Button button, int ms) 
		{
			button.IsEnabled = false;
			Task ButonDisabledTask = Task.Delay(ms);
			ButonDisabledTask.ContinueWith(t =>
			{
				this.Dispatcher.Invoke(() =>
				{
					ConvertButton.IsEnabled = true;
				});
			});
		}

		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{
			string programArg = $"{ ((KeepCmdOpen.IsChecked == true) ? "/k" : "/c") } youtube-dl.exe -U";
			var process = System.Diagnostics.Process.Start(Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe", programArg);
		}

		private double GetDuration(string fileName)
		{
			string ffprobeFindDur = $"/c ffprobe.exe -i \"{fileName}\" -show_format | find \"duration\" ";
			Process process = new Process();
			process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe";
			process.StartInfo.Arguments = ffprobeFindDur; // Note the /c command (*)
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			string[] split = output.Trim().Split('=');
			double result = 0;
			Double.TryParse(split[1], out result);
			process.WaitForExit();
			return result;
		}
	}
}
