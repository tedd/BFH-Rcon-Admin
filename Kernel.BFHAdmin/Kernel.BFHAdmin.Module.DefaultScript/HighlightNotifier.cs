using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common.Utils;

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
            var mostDeaths = new List<string>();
            var mostSuicides = new List<string>();
            int highestKill = 0;
            int highestDeath = 0;
            int highestSuicide = 0;
            foreach (var player in players)
            {
                // Kills
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

                // Deaths
                if (player.Score.Deaths > highestDeath)
                {
                    highestDeath = player.Score.Deaths;
                    mostDeaths.Clear();
                }
                if (player.Score.Deaths == highestDeath)
                {
                    if (!mostDeaths.Contains(player.Name))
                        mostDeaths.Add(player.Name);
                } 
                
                // Suicides
                if (player.Score.Suicides > highestSuicide)
                {
                    highestSuicide = player.Score.Suicides;
                    mostSuicides.Clear();
                }
                if (player.Score.Suicides == highestSuicide)
                {
                    if (!mostSuicides.Contains(player.Name))
                        mostSuicides.Add(player.Name);
                }
            }

            //_rconClient.SendMessageAll("KD: Round end.");
            //var mostKillsStr = (from m in mostKills
            //                    select m.Name).ToList<string>();
            if (mostKills.Count > 0 && highestKill > 0)
            {
                mostKills.Sort();
                string names = StringUtils.ListConcatWithAnd(mostKills);
                Task.Delay(1200).ContinueWith(
                    (t) =>
                    _rconClient.SendMessageAll("KD: Most kills this round was " + highestKill + " by " + names));
            }

            if (mostDeaths.Count > 0 && highestDeath > 0)
            {
                mostDeaths.Sort();
                string names = StringUtils.ListConcatWithAnd(mostDeaths);
                Task.Delay(1000).ContinueWith(
                    (t) =>
                    _rconClient.SendMessageAll("KD: Most deaths this round was " + highestDeath + " by " + names));
            }  
            
            if (mostSuicides.Count > 0 && highestSuicide > 0)
            {
                mostSuicides.Sort();
                string names = StringUtils.ListConcatWithAnd(mostSuicides);
                Task.Delay(1000).ContinueWith(
                    (t) =>
                    _rconClient.SendMessageAll("KD: Most suicides this round was " + highestSuicide + " by " + names));
            }

            //" after " + lastRoundServerInfo.ElapsedRoundTime + " seconds: " + lastRoundServerInfo.Team1.Name + " " + lastRoundServerInfo.Team1.);
        }

    }
}
