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
using System.Globalization;

namespace WpfApp1
{
	public partial class MainWindow : Window
	{
		public static readonly string CMD_EXE = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe";

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
			Process.Start(CMD_EXE, programArg);
		}


		private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			DialogResult result = dialog.ShowDialog();
			if (result.ToString() == "OK")
			{
				FileBox.Text = dialog.FileName;
				var durationStr = FFProbeUtils.GetFormattedDuration(dialog.FileName);
				StartTimeBox.Text = FFProbeUtils.FormatDuration(0);
				EndTimeBox.Text = durationStr;
			}
			
		}

		private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			DialogResult result = folderDialog.ShowDialog();
			if (result.ToString() == "OK")
			{
				PathBox.Text = folderDialog.SelectedPath;
			}
		}

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			DisableButtonFor(ConvertButton, 1000);
			string outputFolderOption = "";
			if (PathBox.Text != "")
			{
				string fileName = System.IO.Path.GetFileNameWithoutExtension(FileBox.Text);
				var vol = VolumeSlider.Value.ToString().Replace(',', '.');
				string fadecommand = "";
				Double.TryParse(FadeOutBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out double fadeoutoffset);
				fadeoutoffset = - fadeoutoffset + FFProbeUtils.FromFormattedString(EndTimeBox.Text);
				if (!(FadeInBox.Text == "")) fadecommand += $",afade=type=in:start_time={FFProbeUtils.FromFormattedString(StartTimeBox.Text)}:duration={FadeInBox.Text}";
				if (!(FadeOutBox.Text == "")) fadecommand += $",afade=type=out:start_time={fadeoutoffset.ToString()}:duration={FadeOutBox.Text}";
				outputFolderOption = $" -i \"{FileBox.Text}\" -f mp3 -q:a {AudioQualitySlider.Value} -filter_complex \"[0:a]volume={vol}{fadecommand}[a]\" -ss {StartTimeBox.Text} -to {EndTimeBox.Text} -map \"[a]\" \"{PathBox.Text}\\{fileName}.mp3\"";
			}
			string programArg = $"{ ((KeepCmdOpen.IsChecked == true) ? "/k" : "/c") } ffmpeg.exe ";
			programArg += outputFolderOption;
			Process.Start(CMD_EXE, programArg);
		}

		private void DisableButtonFor(System.Windows.Controls.Button button, int ms) 
		{
			button.IsEnabled = false;
			Task ButonDisabledTask = Task.Delay(1000);
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
			var process = Process.Start(CMD_EXE, programArg);
		}


	}
}
