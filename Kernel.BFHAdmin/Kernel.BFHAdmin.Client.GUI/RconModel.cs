using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Module.DefaultScript;
using NLog;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.GUI
{

    public class RconModel : NotifyPropertyBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private int _reconnectDelayMs = 10000;

        public delegate void NewClientDelegate(object sender, RconClient rconClient);
        public event NewClientDelegate NewClient;
        protected virtual void OnNewClient(RconClient rconclient)
        {
            NewClientDelegate handler = NewClient;
            if (handler != null) handler(this, rconclient);
        }

        public RconClient RconClient
        {
            get { return _rconClient; }
            set
            {
                if (Equals(value, _rconClient)) return;
                _rconClient = value;
                OnNewClient(_rconClient);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Player> Players
        {
            get { return _players; }
            set
            {
                if (Equals(value, _players)) return;
                _players = value;
                OnPropertyChanged();
            }
        }

        private Dispatcher Dispatcher;

        private WriteableBitmap _mapImage;
        private RconClient _rconClient;
        private ObservableCollection<Player> _players;
        private bool _isConnecting;
        private object _isConnectingLock = new object();

        public bool IsConnecting
        {
            get { return _isConnecting; }
            private set
            {
                if (value.Equals(_isConnecting)) return;
                _isConnecting = value;
                OnPropertyChanged();
            }
        }

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
        }

        private async void DoConnect()
        {
            if (RconClient != null)
                RconClient.Dispose();

            Dispatcher.Invoke(() => Players.Clear());
            RconClient = new RconClient();

            RconClient.Disconnected += RconClientOnDisconnected;

            RconClient.PlayerListCommand.PlayerJoined += RconClientOnPlayerJoined;
            RconClient.PlayerListCommand.PlayerLeft += RconClientOnPlayerLeft;
            RconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
            RconClient.Connected += RconClientOnConnected;
            await RconClient.Connect(Config.RconServerAddress, Config.RconServerPort, Config.RconServerPassword);
    }

        private void RconClientOnDisconnected(object sender)
        {
            Connect();
        }

        public void Connect()
        {
            lock (_isConnectingLock)
            {
                if (!IsConnecting)
                {
                    IsConnecting = true;
                    Task.Delay(_reconnectDelayMs).ContinueWith(
                        (o) =>
                        {
                            lock (_isConnectingLock)
                            {
                                try
                                {
                                    DoConnect();
                                }
                                catch (Exception exception)
                                {
                                    log.ErrorException("RconModel.Connect(): Exception connecting: ", exception);
                                    Connect();
                                }
                                IsConnecting = false;
                            }
                        });
                }
            }
        }

        private void RconClientOnConnected(object sender)
        {
            //  RconClient.SendMessageAll("Kernel.BHFAdmin: Alpha 1 Online.");
            //RconClient.SendMessageAll("Kernel.BHFAdmin: Alpha 1 Online.");
            //RconClient.SendMessageAll("'KD' messages are in active development and will be buggy at times.");
            //RconClient.SendMessageAll("Kernel.BHFAdmin: Developer: Weathroz, tedd@konge.net.");
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            //MapImage = new WriteableBitmap();

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
