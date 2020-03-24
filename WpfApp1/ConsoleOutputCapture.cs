using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp1
{
    public class ConsoleOutputCapture : TextWriter
    {
        private TextBlock output;
        private readonly ScrollViewer container;
        private readonly Dispatcher guiDispatcher;

        public string StoredOutput { get; private set; }
        
        public ConsoleOutputCapture(TextBlock output, ScrollViewer container, Dispatcher guiDispatcher)
        {
            this.output = output;
            this.container = container;
            this.guiDispatcher = guiDispatcher;
            this.StoredOutput = "";
        }

        public override void Write(char value)
        {
            guiDispatcher.Invoke(() =>
            {
                output.Text += value;
                StoredOutput = output.Text;
            });
        }

        public override void Write(string value)
        {
            guiDispatcher.Invoke(() =>
            {
                output.Text += value + "\n";
                StoredOutput = output.Text;
                container.ScrollToBottom();
            });
        }

        public void Clear()
        {
            guiDispatcher.Invoke(() =>
            {
                output.Text = "";
                container.ScrollToTop();
                StoredOutput = output.Text;
            });
        }       

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
