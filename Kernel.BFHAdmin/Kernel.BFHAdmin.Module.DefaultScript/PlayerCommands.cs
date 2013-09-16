using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Module.DataStore.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerCommands : IAmAModule
    {
        private RconClient _rconClient;

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.ClientChatBufferCommand.CommandReceived += ClientChatBufferCommandOnCommandReceived;
        }

        private void ClientChatBufferCommandOnCommandReceived(object sender, ChatHistoryItem chatHistoryItem, string @from, string command, string[] param, string fullParam)
        {

            var player = _rconClient.PlayerListCommand.GetPlayer(chatHistoryItem.From);

            if (command == "c" || command == "clear")
            {
                Task.Delay(2000)
                    .ContinueWith(task => _rconClient.SendMessagePlayer(player, "Tip: To stop spam type /stopspam"));
            }
            if (command == "stopspam")
            {
                PlayerCacheAndHistory.Current.GetPlayerCache(player).Settings.StopSpam = true;
                _rconClient.SendMessagePlayer(player, "KD: Lowering amount of messages sent to you. /startspam to undo.");
            }
            if (command == "startspam")
            {
                PlayerCacheAndHistory.Current.GetPlayerCache(player).Settings.StopSpam = true;
                _rconClient.SendMessagePlayer(player, "KD: Increasing amount of messages sent to you. /stopspam to undo.");
            }

            if (command == "deaths")
            {
                Player thisPlayer = player;
                var playerName = "Your";
                if (param.Length > 0)
                {
                    playerName = param[0];
                    thisPlayer = _rconClient.PlayerListCommand.SearchForPlayer(playerName);
                }
                if (thisPlayer == null)
                {
                    _rconClient.SendMessagePlayer(player, "KD: Could not find match for player name " + playerName + ".");
                    return;
                }
                playerName = thisPlayer.Name;
                    _rconClient.SendMessagePlayer(player,
                                                  "KD: " + playerName + " kills/deaths this round: " +
                                                  thisPlayer.Score.Kills + "/" + thisPlayer.Score.Deaths);
            }

          if (command == "ping")
            {
                Player pingPlayer = player;
                var playerName = "Your";
                if (param.Length > 0)
                {
                    playerName = param[0];
                    pingPlayer = _rconClient.PlayerListCommand.SearchForPlayer(playerName);
                }
                if (pingPlayer == null)
                {
                    _rconClient.SendMessagePlayer(player, "KD: Could not find match for player name " + playerName + ".");
                    return;
                }
                playerName = pingPlayer.Name;
                PlayerCache playerCache = PlayerCacheAndHistory.Current.GetPlayerCache(pingPlayer);
                
                var total = 0;
                var num = 0;
                var highest = 0;
                var lowest = 10000;
                DateTime oldest = DateTime.Now;
                foreach (var h in playerCache.CopyHistory())
                {
                    if (h.Player.Ping != 0)
                    {
                        num++;
                        total += h.Player.Ping;
                        lowest = Math.Min(lowest, h.Player.Ping);
                        highest = Math.Max(highest, h.Player.Ping);
                        if (h.Time < oldest)
                            oldest = h.Time;
                    }
                }
                _rconClient.SendMessagePlayer(player, "KD: " + playerName + " current ping: " + player.Ping + " ms.");
                if (num > 0)
                {
                    var ping = (decimal)total / (decimal)num;
                    var pingStr = string.Format("{0:0.00}", ping);
                    var since = DateTime.Now.Subtract(oldest);
                    _rconClient.SendMessagePlayer(player, "KD: " + playerName + " average ping the last " + (int)since.TotalSeconds + " seconds is " + pingStr + " ms.");
                    _rconClient.SendMessagePlayer(player, "KD: " + playerName + " highest: " + highest + "ms. Lowest: " + lowest + " ms.");
                }

            }
        }

        public void ModuleLoadComplete()
        {

        }

    }
}
