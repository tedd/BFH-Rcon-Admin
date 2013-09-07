using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Client.DataStore;
using Kernel.BFHAdmin.Module.DataStore;
using Kernel.BFHAdmin.Module.DataStore.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerEvents : IAmAModule
    {
        private RconClient _rconClient;
        //private PlayerStorage _playerStorage;
        public static PlayerEvents Current { get; private set; }
        private Dictionary<Player, PlayerCache> _players = new Dictionary<Player, PlayerCache>();

        public delegate void PlayerUpdatedDelegate(object sender, PlayerCache playerCache);
        public event PlayerUpdatedDelegate PlayerUpdated;
        protected virtual void OnPlayerUpdated(PlayerCache playerCache)
        {
            PlayerUpdatedDelegate handler = PlayerUpdated;
            if (handler != null) handler(this, playerCache);
        }

        public delegate void RegisteredDelegate(PlayerEvents playerEvents);
        public static event RegisteredDelegate Registered;

        protected virtual void OnRegistered(PlayerEvents playerevents)
        {
            RegisteredDelegate handler = Registered;
            if (handler != null) handler(playerevents);
        }

        public PlayerEvents()
        {
            Current = this;
        }

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.PlayerListCommand.PlayerUpdated += PlayerListCommandOnPlayerUpdated;
            _rconClient.PlayerListCommand.PlayerLeft += PlayerListCommandOnPlayerLeft;
            _rconClient.PlayerListCommand.PlayerJoined += PlayerListCommandOnPlayerJoined;

            //DatabaseController.Initialize();
            //_playerStorage = new PlayerStorage();

            OnRegistered(this);
        }

        public void ModuleLoadComplete()
        {

        }

        #region Player tracking
        private void PlayerListCommandOnPlayerJoined(object sender, Player player)
        {
            lock (_players)
            {
                // Add player to our cache

                //var pcs = (from pc in _playerStorage.PlayerCaches
                //          where pc.Player.Name == player.Name
                //          select pc).ToList<PlayerCache>();
                //PlayerCache dPlayer;
                //if (pcs.Count != 0)
                //{
                //    dPlayer = pcs.First();
                //    dPlayer.Player = player;
                //}
                //else
                //{
                //    // Create PlayerCache in DB and save
                    var dPlayer = new PlayerCache();
                    dPlayer.Player = player;
                    //_playerStorage.PlayerCaches.Add(dPlayer);
                    //try
                    //{
                    //    _playerStorage.SaveChanges();
                    //}
                    //catch (Exception exception)
                    //{
                    //    Debug.WriteLine(exception.ToString());
                    //}
                //}

                _players.Add(player, dPlayer);
            }
        }

        private void PlayerListCommandOnPlayerLeft(object sender, Player player)
        {
            lock (_players)
            {
                // Remove player from our cache
                _players.Remove(player);
            }
        }

        private void PlayerListCommandOnPlayerUpdated(object sender, Player player)
        {
            lock (_players)
            {
                // Take a snapshot and fire off event
                _players[player].TakeHistorySnapshot();
                
                //// Save change do DB
                //try
                //{
                //    _playerStorage.SaveChanges();
                //}
                //catch (Exception exception)
                //{
                //    Debug.WriteLine(exception.ToString());
                //}

                OnPlayerUpdated(_players[player]);
            }
        }
        #endregion

  
    }
}
