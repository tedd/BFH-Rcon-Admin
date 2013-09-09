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
    public class HighlightNotifier : IAmAModule
    {
        private RconClient _rconClient;
        private PlayerCacheAndHistory _playerEvents;

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.ServerInfoCommand.RoundEnd += ServerInfoCommandOnRoundEnd;
            _rconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
        }

        public void ModuleLoadComplete()
        {
            _playerEvents = PlayerCacheAndHistory.Current;
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            _rconClient.SendMessageAll(string.Format("KD: {0} round {1} of {2}.",
                _rconClient.ServerInfoCommand.ServerInfo.MapName,
                _rconClient.ServerInfoCommand.ServerInfo.CurrentRound,
                _rconClient.ServerInfoCommand.ServerInfo.TotalRounds));

            if (_rconClient.ServerInfoCommand.ServerInfo.CurrentRound == _rconClient.ServerInfoCommand.ServerInfo.TotalRounds)
            {
                _rconClient.SendMessageAll(string.Format("KD: Next up: {0}",
                                                         _rconClient.ServerInfoCommand.ServerInfo.NextMapName));
            }
        }

        private void ServerInfoCommandOnRoundEnd(object sender, ServerInfo lastRoundServerInfo)
        {
            var players = _rconClient.PlayerListCommand.GetPlayers();
            var mostKills = new List<string>();
            int highestKill = 0;
            foreach (var player in players)
            {
                if (player.Score.Kills > highestKill)
                {
                    highestKill = player.Score.Kills;
                    mostKills.Clear();
                }
                if (player.Score.Kills == highestKill)
                {
                    if (!mostKills.Contains(player.Name))
                        mostKills.Add(player.Name);
                }
            }

            //_rconClient.SendMessageAll("KD: Round end.");
            //var mostKillsStr = (from m in mostKills
            //                    select m.Name).ToList<string>();
            mostKills.Sort();
            if (mostKills.Count > 0)
            {
                string str = null;
                var last = mostKills.Last();
                mostKills.Remove(last);
                if (mostKills.Count > 0)
                    str = String.Join(", ", mostKills);

                if (str != null)
                    str += " & ";
                str += last;
                Task.Delay(1000).ContinueWith(
                    (t) =>
                     _rconClient.SendMessageAll("KD: Most kills this round was " + highestKill + " by " + str));
            }

            //" after " + lastRoundServerInfo.ElapsedRoundTime + " seconds: " + lastRoundServerInfo.Team1.Name + " " + lastRoundServerInfo.Team1.);
        }

    }
}
