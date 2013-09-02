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
using Kernel.BFHAdmin.Client.BFHRconProtocol;

namespace Kernel.BFHAdmin.Client.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RconModel rconModel;
        public MainWindow()
        {
            InitializeComponent();

            Test();
        }

        private async void Test()
        {
            rconModel = new RconModel(Dispatcher);
            rconModel.RconClient.ReceivedUnhandledLine += ClientOnReceivedLine;
            rconModel.Start();

            this.DataContext = rconModel;
        }

        private void ClientOnReceivedLine(object sender, string line)
        {
            Dispatcher.Invoke(() => txtOut.AppendText(line + "\r\n"));
        }

        private void txtIn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                rconModel.RconClient.SendRaw(txtIn.Text);
            }
        }
    }
}
