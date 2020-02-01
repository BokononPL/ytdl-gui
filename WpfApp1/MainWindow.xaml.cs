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

			String link = AddressBox.Text;

			string strCmdText;
			strCmdText = link;
			System.Diagnostics.Process.Start("youtube-dl.exe", strCmdText);
		}

		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
/*			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = "C:\\";

			DialogResult result = dialog.ShowDialog();
			if (result.ToString() == "OK")
				PathBox.Text = dialog.FileName;*/

			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			folderDialog.SelectedPath = "C:\\";

			DialogResult result = folderDialog.ShowDialog();
			if (result.ToString() == "OK")
				PathBox.Text = folderDialog.SelectedPath;
		}
	}
}
