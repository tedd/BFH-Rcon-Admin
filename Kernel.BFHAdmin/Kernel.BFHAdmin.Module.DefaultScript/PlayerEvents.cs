using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Module.DefaultScript.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerEvents : IAmAModule
    {
        private RconClient _rconClient;
        public static PlayerEvents Current { get; set; }
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

            PlayerUpdated += OnPlayerUpdated;

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
                _players.Add(player, new PlayerCache() { Player = player });
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
                OnPlayerUpdated(_players[player]);
            }
        }
        #endregion

        private void OnPlayerUpdated(object sender, PlayerCache playerCache)
        {
            var last = playerCache.LastPlayerDelta;
            if (last == null)
                return;
        }
    }
}
