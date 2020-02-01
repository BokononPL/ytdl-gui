using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
		}

		private void DownloadButton_Click(object sender, RoutedEventArgs e)
		{

			string link = AddressBox.Text;
			string outputFolderOption = "";
			if (PathBox.Text != "")
			{
				outputFolderOption = $" -o \"{PathBox.Text}\\%(title)s.%(ext)s\"";
			}
			string strCmdText = "/k youtube-dl.exe ";
			strCmdText += link + outputFolderOption;
			var process = System.Diagnostics.Process.Start("cmd.exe", strCmdText);
			process.WaitForExit();
		}


		private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = "C:\\";

			DialogResult result = dialog.ShowDialog();
			if (result.ToString() == "OK")
				FileBox.Text = dialog.FileName;
		}

		private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			folderDialog.SelectedPath = "C:\\";

			DialogResult result = folderDialog.ShowDialog();
			if (result.ToString() == "OK")
				PathBox.Text = folderDialog.SelectedPath;
		}

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			string link = AddressBox.Text;
			string outputFolderOption = "";
			if (PathBox.Text != "")
			{
				outputFolderOption = $" -i \"{PathBox.Text}\" -f mp3 -q:a 2 -filter:a \"volume=0.5\" \"{PathBox.Text}\\output.mp3\"";
			}
			string strCmdText;
			strCmdText = link + outputFolderOption;
			System.Diagnostics.Process.Start("ffmpeg.exe", strCmdText);
		}
	}
}
