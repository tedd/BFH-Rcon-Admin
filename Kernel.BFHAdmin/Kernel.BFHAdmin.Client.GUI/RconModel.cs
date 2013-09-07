using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Module.DefaultScript;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.GUI
{

    public class RconModel : NotifyPropertyBase
    {
        public RconClient RconClient { get; set; }
        public ObservableCollection<Player>  Players { get; set; }
        private Dispatcher Dispatcher;

        private WriteableBitmap _mapImage;
        public WriteableBitmap MapImage
        {
            get { return _mapImage; }
            set
            {
                if (Equals(value, _mapImage)) return;
                _mapImage = value;
                OnPropertyChanged();
            }
        }

        public RconModel(Dispatcher dispatcher)
        {
            
            Players = new ObservableCollection<Player>();
            Dispatcher = dispatcher;
            RconClient = new RconClient();

            RconClient.PlayerListCommand.PlayerJoined += RconClientOnPlayerJoined;
            RconClient.PlayerListCommand.PlayerLeft += RconClientOnPlayerLeft;
            RconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
            RconClient.Connected += RconClientOnConnected;
        }

        private void RconClientOnConnected(object sender)
        {
            RconClient.SendMessageAll("Kernel.BHFAdmin: Alpha 1 Online.");
            //RconClient.SendMessageAll("Kernel.BHFAdmin: Developer: Weathroz, tedd@konge.net.");
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            //MapImage = new WriteableBitmap();
            
        }

        public async void Start()
        {
            await RconClient.Connect(Config.RconServerAddress, Config.RconServerPort, Config.RconServerPassword);
        }

        private void RconClientOnPlayerLeft(object sender, Player player)
        {
            Dispatcher.Invoke(() => Players.Remove(player));
        }

        private void RconClientOnPlayerJoined(object sender, Player player)
        {
            Dispatcher.Invoke(() => Players.Add(player));
        }

    }
}
