using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class GreetFlagCarrier : IAmAModule
    {

        private RconClient _rconClient;
        private Dictionary<Player, PlayerCache> _players = new Dictionary<Player, PlayerCache>();

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.PlayerListCommand.PlayerUpdated += PlayerListCommandOnPlayerUpdated;
            _rconClient.PlayerListCommand.PlayerLeft += PlayerListCommandOnPlayerLeft;
            _rconClient.PlayerListCommand.PlayerJoined += PlayerListCommandOnPlayerJoined;
            _rconClient.ServerInfoCommand.RoundEnd+= ServerInfoCommandOnNewRound;
        }

        private void ServerInfoCommandOnNewRound(object sender, ServerInfo lastRoundServerInfo)
        {
            // Need to reset status
            foreach (var player in _players.Values)
            {
                _rconClient.SendMessagePlayer(player.Player.Name, "Your flag holds this round: " + player.RoundFlagHolds);
                _rconClient.SendMessagePlayer(player.Player.Name, "Your flag holds this session: " + player.TotalFlagHolds);

                player.Player.IsFlagholder = false;
                player.RoundFlagHolds = 0;
            }
        }

        private void PlayerListCommandOnPlayerJoined(object sender, Player player)
        {
            lock (_players)
            {
                _players.Add(player, new PlayerCache() { Player = (Player)player.Clone() });
            }
        }

        private void PlayerListCommandOnPlayerLeft(object sender, Player player)
        {
            lock (_players)
            {
                _players.Remove(player);
            }
        }

        private void PlayerListCommandOnPlayerUpdated(object sender, Player player)
        {
            lock (_players)
            {
                var wasFlagHolder = _players[player].Player.IsFlagholder;
                if (wasFlagHolder != player.IsFlagholder)
                {
                    if (wasFlagHolder)
                    {
                        _players[player].RoundFlagHolds++;
                        _players[player].TotalFlagHolds++;
                        _rconClient.SendMessageAll("Aww... " + player.Name + " lost the flag. What a looser.");
                    }
                    else
                    {
                        _rconClient.SendMessageAll(player.Name + " got the flag!");
                    }
                }
                _players[player].Player.IsFlagholder = player.IsFlagholder;
            }
        }
    }
}
