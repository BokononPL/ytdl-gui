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
using System.Windows.Media.TextFormatting;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WpfApp1
{
	public partial class MainWindow : Window
	{
		public string DownloadOut = "";
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
			string programArg = "/C yt-dlp ";
			programArg += link + outputFolderOption;
			StartProcessHidden_Async(programArg);
		}

		private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};
			DialogResult result = dialog.ShowDialog();
			if (result.ToString() == "OK")
			{
				SetFileData(dialog.FileName);
			}
			
		}

		private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderDialog = new FolderBrowserDialog
			{
				SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
			};
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
				string MetadataOptions = "";
				Double.TryParse(FadeOutBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out double fadeoutoffset);
				fadeoutoffset = - fadeoutoffset + FFProbeUtils.FromFormattedString(EndTimeBox.Text) - 1;
				if (!(FadeInBox.Text == "")) fadecommand += $",afade=type=in:start_time={FFProbeUtils.FromFormattedString(StartTimeBox.Text)}:duration={FadeInBox.Text}";
				if (!(FadeOutBox.Text == "")) fadecommand += $",afade=type=out:start_time=\"{fadeoutoffset.ToString().Replace(',', '.')}\":duration={FadeOutBox.Text}";
				MetadataOptions += $" -metadata title=\"{TagTitle.Text}\"";
				MetadataOptions += $" -metadata artist=\"{TagArtist.Text}\"";
				MetadataOptions += $" -metadata album=\"{TagAlbum.Text}\"";
				MetadataOptions += $" -metadata track=\"{TagTrackNumber.Text}\"";
				if (!(OutFileName.Text == "")) fileName = OutFileName.Text;
				outputFolderOption = $" -i \"{FileBox.Text}\" -f mp3 -q:a {AudioQualitySlider.Value} -filter_complex \"[0:a]volume={vol}{fadecommand}[a]\" -ss {StartTimeBox.Text} -to {EndTimeBox.Text} -map \"[a]\" {MetadataOptions} \"{PathBox.Text}\\{fileName}.mp3\"";
			}
			string programArg = "/c ffmpeg.exe ";
			programArg += outputFolderOption;
			StartProcessHidden(programArg);
			if(DeleteOriginalFile.IsChecked == true)
			{
				File.Delete(FileBox.Text);
			}
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
			string programArg = "/C yt-dlp.exe -U";
			StartProcessHidden(programArg);
		}

		private void RecentFileButton_Click(object sender, RoutedEventArgs e)
		{
			if (!String.IsNullOrEmpty(DownloadOut))
			{
				SetFileData(GetConvertedFilename(DownloadOut));
			}
		}

		private async void StartProcessHidden_Async(string args)
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
			string stdout;
			process.Start();
			while ((stdout = await process.StandardOutput.ReadLineAsync()) != null) 
			{
				if (!String.IsNullOrWhiteSpace(stdout))
				{
					DownloadOut += stdout + "\n";
				}
			}
			process.WaitForExit();
			return;
		}

		private void StartProcessHidden(string args)
		{
			Process process = new Process();
			{
				process.StartInfo = new ProcessStartInfo()
				{
					FileName = CMD_EXE,
					Arguments = args,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};
			}
			process.Start();
			process.WaitForExit();
			return;
		}

		private string GetConvertedFilename(string ytdlOutput)
		{
			string pattern = "Merging formats into \"(.*)\"\\n";
			var extractedFilename = Regex.Match(ytdlOutput, pattern);
			return extractedFilename.Groups[1].Value.Replace(@"\", "/");
		}

		private bool ContainsKeyNested(JObject obj, string[] keys)
		{
			foreach (string key in keys)
			{ 
				if (!obj.ContainsKey(key))
				{
					return false;
				}
				try
				{
					obj = JObject.Parse(obj[key].ToString());
				}
				catch (JsonReaderException)
				{
					continue;
				}
				
			}
			return true;
		}

		private void SetFileData(string filename)
		{
			JObject FileInfo = FFProbeUtils.GetFileInfo(filename);
			FileBox.Text = filename;
			StartTimeBox.Text = TimeSpan.FromSeconds(0).ToString(@"hh\:mm\:ss\.fff");
			EndTimeBox.Text = TimeSpan.FromSeconds(Convert.ToDouble(FileInfo["format"]["duration"])).ToString(@"hh\:mm\:ss\.fff");
			OutFileName.Text = "";
			if (ContainsKeyNested(FileInfo, new string[] { "format", "tags", "title" })) TagTitle.Text = FileInfo["format"]["tags"]["title"].ToString();
			if (ContainsKeyNested(FileInfo, new string[] { "format", "tags", "artist" })) TagArtist.Text = FileInfo["format"]["tags"]["artist"].ToString();
			if (ContainsKeyNested(FileInfo, new string[] { "format", "tags", "album" })) TagAlbum.Text = FileInfo["format"]["tags"]["album"].ToString();
			if (ContainsKeyNested(FileInfo, new string[] { "format", "tags", "track" })) TagTrackNumber.Text = FileInfo["format"]["tags"]["track"].ToString();
		}
	}
}
