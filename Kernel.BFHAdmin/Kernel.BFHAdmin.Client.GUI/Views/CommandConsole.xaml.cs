using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Client.GUI.Views
{
    /// <summary>
    /// Interaction logic for CommandConsole.xaml
    /// </summary>
    public partial class CommandConsole : UserControl
    {
        private RconModel _rconModel;
        private Paragraph paragraph = null;
        private int _maxLogLineChunks = 500;

        public CommandConsole()
        {
            InitializeComponent();
        }

        public void Register(RconModel rconModel)
        {
            _rconModel = rconModel;
            _rconModel.NewClient +=
                (sender, client) =>
                { client.ReceivedUnhandledLine += RconClientOnReceivedUnhandledLine; };
            
        }

        private void RconClientOnReceivedUnhandledLine(object sender, string line)
        {
            WriteLine(line, Brushes.Red);
        }

        private Paragraph WriteLine(string line, Brush foreground)
        {
            Dispatcher.Invoke(
                () =>
                {
                    if (paragraph == null)
                    {
                        paragraph = new Paragraph();
                        paragraph.Margin = new Thickness(0, 0, 0, 0);
                        txtOut.Document.Blocks.Add(paragraph);
                    }

                    var run = new Run(line + "\r\n");
                    run.Foreground = foreground;
                    paragraph.Inlines.Add(run);

                    // If we have too much text we'll chop away from the beginning
                    if (paragraph.Inlines.Count > _maxLogLineChunks)
                        paragraph.Inlines.Remove(paragraph.Inlines.FirstInline);
                    txtOut.ScrollToEnd();
                });
            return paragraph;
        }

        private void txtIn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                //_rconModel.RconClient.SendRaw(txtIn.Text);
                var command = txtIn.Text;
                ProcessCommand(command);
            }
        }

        private async void ProcessCommand(string command)
        {

            var paragraph = WriteLine(command, Brushes.Purple);
            var lines = await _rconModel.RconClient.Command.Exec(txtIn.Text);
            foreach (var l in lines)
            {
                var run = new Run(l + "\r\n");
                run.Foreground = Brushes.Blue;
                paragraph.Inlines.Add(run);
            }
        }

    }
}
