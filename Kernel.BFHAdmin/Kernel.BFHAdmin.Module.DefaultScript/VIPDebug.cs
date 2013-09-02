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
    public class VIPDebug : IAmAModule
    {

        private RconClient _rconClient;
        private Dictionary<Player, PlayerCache> _players = new Dictionary<Player, PlayerCache>();

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.PlayerListCommand.PlayerUpdated += PlayerListCommandOnPlayerUpdated;
            _rconClient.PlayerListCommand.PlayerLeft += PlayerListCommandOnPlayerLeft;
            _rconClient.PlayerListCommand.PlayerJoined += PlayerListCommandOnPlayerJoined;
            _rconClient.ServerInfoCommand.RoundEnd += ServerInfoCommandOnRoundEnd;
            _rconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            _rconClient.SendMessageAll("KernelDebug: Round start.");
        }

        private void ServerInfoCommandOnRoundEnd(object sender, ServerInfo lastRoundServerInfo)
        {
            _rconClient.SendMessageAll("KernelDebug: Round end.");            
        }

        private void PlayerListCommandOnPlayerJoined(object sender, Player player)
        {
            lock (_players)
            {
                _players.Add(player, new PlayerCache() { Player = player.Clone() });
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
                //if (!player.Vip)
                //    return;

                var score = _players[player].Player.Score.Score;
                if (score != player.Score.Score)
                {
                    var scoreDiff = player.Score.Score - score;
                    string scoreDiffStr = scoreDiff.ToString();
                    if (scoreDiff > 0)
                        scoreDiffStr = "+" + scoreDiffStr;
                        _rconClient.SendMessagePlayer(player.Name, "KD: Score: " + scoreDiffStr);
                }
                _players[player].Player.Score.Score = player.Score.Score;
            }
        }
    }
}
