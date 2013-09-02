using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Module.DefaultScript;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.GUI
{
    [ImplementPropertyChanging]
    public class RconModel
    {
        public RconClient RconClient { get; set; }
        public ObservableCollection<Player>  Players { get; set; }
        private Dispatcher Dispatcher;

        public RconModel(Dispatcher dispatcher)
        {
            Players = new ObservableCollection<Player>();
            Dispatcher = dispatcher;
            RconClient = new RconClient();

            RconClient.PlayerListCommand.PlayerJoined += RconClientOnPlayerJoined;
            RconClient.PlayerListCommand.PlayerLeft += RconClientOnPlayerLeft;
        }

        public async void Start()
        {
            await RconClient.Connect("31.204.131.11", 8567, "SECRETPASSWORD");
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
