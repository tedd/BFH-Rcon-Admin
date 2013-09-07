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
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : UserControl
    {
        private RconModel _rconModel;
        private Paragraph paragraph = null;
        private int _maxLogLineChunks = 500;
        public ChatWindow()
        {
            InitializeComponent();
        }

        public void Register(RconModel rconModel)
        {
            _rconModel = rconModel;
            _rconModel.NewClient +=
                (sender, client) =>
                    { client.ClientChatBufferCommand.ChatLineReceived += ClientChatBufferCommandOnChatLineReceived; };
        }

        private void ClientChatBufferCommandOnChatLineReceived(object sender, ChatHistoryItem chatHistoryItem)
        {
            WriteLine(string.Format("{0} {1} {2} <{3} -> {4}> {5}", 
                chatHistoryItem.TimeStamp,
                chatHistoryItem.Type,
                chatHistoryItem.Number,
                chatHistoryItem.From, 
                chatHistoryItem.What, 
                chatHistoryItem.Message), Brushes.Black);
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
                var line = txtIn.Text;
                _rconModel.RconClient.SendMessageAll(line);
                WriteLine("--> " + line, Brushes.Blue);
            }
        }

    }
}
