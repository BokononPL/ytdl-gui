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
using System.Text.RegularExpressions;

namespace WpfApp1
{
	public partial class MainWindow : Window
	{
		public static readonly string CMD_EXE = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe";

		public ConsoleOutputCapture ConsoleOutputCapture;

		public MainWindow()
		{
			InitializeComponent();
			PathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			ConsoleOutputCapture = new ConsoleOutputCapture(ConsoleOutput, ConsoleScrollContainer, this.Dispatcher);
			
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
			string programArg = "/c youtube-dl.exe ";
			programArg += link + outputFolderOption;
			ConsoleOutputCapture.Clear();
			StartProcessHidden(programArg, (time, output) => ConsoleOutputCapture.Write($"[ytdl-gui] Finished downloading in {time.TotalSeconds} s"));
		}


		private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			DialogResult result = dialog.ShowDialog();
			if (result.ToString() == "OK")
			{
				SetFilenameAndDuration(dialog.FileName);
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
				fadeoutoffset = - fadeoutoffset + FFProbeUtils.FromFormattedString(EndTimeBox.Text) - 1;
				if (!(FadeInBox.Text == "")) fadecommand += $",afade=type=in:start_time={FFProbeUtils.FromFormattedString(StartTimeBox.Text)}:duration={FadeInBox.Text}";
				if (!(FadeOutBox.Text == "")) fadecommand += $",afade=type=out:start_time=\"{fadeoutoffset.ToString().Replace(',', '.')}\":duration={FadeOutBox.Text}";
				outputFolderOption = $" -i \"{FileBox.Text}\" -f mp3 -q:a {AudioQualitySlider.Value} -filter_complex \"[0:a]volume={vol}{fadecommand}[a]\" -ss {StartTimeBox.Text} -to {EndTimeBox.Text} -map \"[a]\" \"{PathBox.Text}\\{fileName}.mp3\"";
			}
			string programArg = "/c ffmpeg.exe ";
			programArg += outputFolderOption;
			StartProcessHidden(programArg, (time, output) => { 
				ConsoleOutputCapture.Write($"[ytdl-gui] Finished converting in {time.TotalSeconds} s"); 
				if(DeleteOriginalFile.IsChecked == true)
				{
					File.Delete(FileBox.Text);
					ConsoleOutputCapture.Write($"[ytdl-gui] Deleted the original file at {FileBox.Text}");
				}
			});
		}

		private void DisableButtonFor(System.Windows.Controls.Button button, int ms) 
		{
			button.IsEnabled = false;
			Task ButonDisabledTask = Task.Delay(ms);
			ButonDisabledTask.ContinueWith(t =>
			{
				this.Dispatcher.Invoke(() =>
				{
					button.IsEnabled = true;
				});
			});
		}

		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{
			string programArg = "/c youtube-dl.exe -U";
			var process = Process.Start(CMD_EXE, programArg);
		}

		private void RecentFileButton_Click(object sender, RoutedEventArgs e)
		{
			if (!String.IsNullOrEmpty(ConsoleOutputCapture.StoredOutput))
			{
				SetFilenameAndDuration(GetConvertedFilename(ConsoleOutputCapture.StoredOutput));
			}
		}

		private void StartProcessHidden(string args, Action<TimeSpan, string> onFinishedCallback)
		{
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = CMD_EXE,
					Arguments = args,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};
			process.OutputDataReceived += (a, b) =>
			{
				if (!String.IsNullOrWhiteSpace(b.Data)) ConsoleOutputCapture.Write(b.Data);
			};
			process.ErrorDataReceived += (a, b) =>
			{
				if (!String.IsNullOrWhiteSpace(b.Data)) ConsoleOutputCapture.Write(b.Data);
			};
			process.Start();
			process.EnableRaisingEvents = true;
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.Exited += (obj, a) => this.Dispatcher.Invoke(() => {
				onFinishedCallback.Invoke(process.ExitTime - process.StartTime, ConsoleOutputCapture.StoredOutput);
			});
		}

		private void StartProcessHidden(string args)
		{
			StartProcessHidden(args, (_, __) => { });
		}

		private string GetConvertedFilename(string ytdlOutput)
		{
			string pattern = "Merging formats into \"(.*)\"\\n";
			var extractedFilename = Regex.Match(ytdlOutput, pattern);
			return extractedFilename.Groups[1].Value;
		}

		private void SetFilenameAndDuration(string filename)
		{
			FileBox.Text = filename;
			StartTimeBox.Text = FFProbeUtils.FormatDuration(0);
			EndTimeBox.Text = FFProbeUtils.GetFormattedDuration(filename);
		}
	}
}
