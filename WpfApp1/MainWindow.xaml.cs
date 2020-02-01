using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
        Process process;
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
			//SizeChanged = "ScrollViewer_SizeChanged"
			string strCmdText;
			strCmdText = link + outputFolderOption;
            Console.WriteLine(strCmdText);
            ProcessStartInfo cmdStartInfo = new ProcessStartInfo("youtube-dl.exe");
            cmdStartInfo.CreateNoWindow = true;
            cmdStartInfo.RedirectStandardOutput = true;
            cmdStartInfo.RedirectStandardError = true;
            cmdStartInfo.RedirectStandardInput = true;
            cmdStartInfo.UseShellExecute = false;
            cmdStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmdStartInfo.Arguments = strCmdText;
            
            process = new Process();
            process.StartInfo = cmdStartInfo;

            if (process.Start() == true)
            {
                process.OutputDataReceived += new DataReceivedEventHandler(_cmd_OutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(_cmd_ErrorDataReceived);
                process.Exited += new EventHandler(_cmd_Exited);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            else
            {
                process = null;
            }
            process.Start();
		}

        private void Window_Closed(object sender, EventArgs e)
        {
            if ((process != null) &&
                (process.HasExited != true))
            {
                process.CancelErrorRead();
                process.CancelOutputRead();
                process.Close();
                process.WaitForExit();
            }
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

        void _cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole(e.Data);
        }

        void _cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole(e.Data, Brushes.Red);
        }

        void _cmd_Exited(object sender, EventArgs e)
        {
            process.OutputDataReceived -= new DataReceivedEventHandler(_cmd_OutputDataReceived);
            process.Exited -= new EventHandler(_cmd_Exited);
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            outputViewer.ScrollToBottom();
        }

        private void UpdateConsole(string text)
        {
            UpdateConsole(text, null);
        }

        private void UpdateConsole(string text, Brush color)
        {
            if (!output.Dispatcher.CheckAccess())
            {
                output.Dispatcher.Invoke(
                        new Action(
                                () =>
                                {
                                    WriteLine(text, color);
                                }
                            )
                    );
            }
            else
            {
                WriteLine(text, color);
            }
        }

        private void WriteLine(string text, Brush color)
        {
            if (text != null)
            {
                Span line = new Span();
                if (color != null)
                {
                    line.Foreground = color;
                }
                foreach (string textLine in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    line.Inlines.Add(new Run(textLine));
                }
                line.Inlines.Add(new LineBreak());
                output.Inlines.Add(line);
                outputViewer.ScrollToBottom();
            }
        }
    }
}
