using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Module.DataStore.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerCommands_Track : IAmAModule
    {
        private RconClient _rconClient;
        private Dictionary<Player, List<Player>> _trackTargets = new Dictionary<Player, List<Player>>();
        private TimeSpan _autoStatInterval = new TimeSpan(0, 0, 2, 0);
        private DateTime _lastAutoStat = DateTime.Now;

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.ClientChatBufferCommand.CommandReceived += ClientChatBufferCommandOnCommandReceived;
            _rconClient.PlayerListCommand.PlayerLeft += PlayerListCommandOnPlayerLeft;
            _rconClient.PlayerListCommand.PlayerUpdateDone += PlayerListCommandOnPlayerUpdateDone;
        }

        private void PlayerListCommandOnPlayerUpdateDone(object sender)
        {
            if (DateTime.Now.Subtract(_lastAutoStat) > _autoStatInterval)
            {
                _lastAutoStat = DateTime.Now;
                lock (_trackTargets)
                {
                    foreach (var target in _trackTargets.Keys)
                    {
                     
                        SendToTrackers(target, string.Format("KD: {0}: KD-ratio of {1} (K{2}-D{3}).", target.Name, target.Score.KillDeathRatio, target.Score.Kills, target.Score.Deaths));
                    }
                }
            }
        }

        public void ModuleLoadComplete()
        {
            PlayerCacheAndHistory.Current.PlayerUpdated += PlayerEventsOnPlayerUpdated;
        }

        private void PlayerEventsOnPlayerUpdated(object sender, PlayerCache playercache)
        {
            var name = playercache.Player.Name;
            if (playercache.LastPlayerDelta.Score.Kills > 0)
                SendToTrackers(playercache.Player, string.Format("KD: {0} kills +{1}. Total: {2}", name, playercache.LastPlayerDelta.Score.Kills, playercache.Player.Score.Kills));
            if (playercache.LastPlayerDelta.Score.Deaths > 0)
                SendToTrackers(playercache.Player, string.Format("KD: {0} deaths +{1}. Total: {2}", name, playercache.LastPlayerDelta.Score.Deaths, playercache.Player.Score.Deaths));
            if (playercache.LastPlayerDelta.Score.Suicides > 0)
                SendToTrackers(playercache.Player, string.Format("KD: {0} suicides +{1}. Total: {2}", name, playercache.LastPlayerDelta.Score.Suicides, playercache.Player.Score.Suicides));
        }

        private void PlayerListCommandOnPlayerLeft(object sender, Player player)
        {
            CleanUpTracker(player);
        }

        private void ClientChatBufferCommandOnCommandReceived(object sender, ChatHistoryItem chatHistoryItem,
                                                              string @from, string command, string[] param,
                                                              string fullParam)
        {
            var player = _rconClient.PlayerListCommand.GetPlayer(chatHistoryItem.From);

            if (command == "track")
            {

                if (param.Length == 0)
                {
                    _rconClient.SendMessagePlayer(player, "KD: Player name required for tracking.");
                }
                else
                {
                    var targetPlayerName = param[0];
                    var targetPlayer = _rconClient.PlayerListCommand.SearchForPlayer(targetPlayerName);

                    if (targetPlayer == null)
                    {
                        _rconClient.SendMessagePlayer(player, "KD: Could not find match for player name " + targetPlayerName + ".");
                        return;
                    }
                    targetPlayerName = targetPlayer.Name;

                    AddTrackingTarget(player, targetPlayer);

                    _rconClient.SendMessagePlayer(player,
                                                  "KD: Tracking " + targetPlayerName + ". Will report kills/deaths to you.");
                }
            }
            if (command == "untrack")
            {

                if (param.Length == 0)
                {
                    _rconClient.SendMessagePlayer(player, "KD: Player name required for untracking.");
                }
                else
                {
                    var targetPlayerName = param[0];
                    var targetPlayer = _rconClient.PlayerListCommand.SearchForPlayer(targetPlayerName);

                    if (targetPlayer == null)
                    {
                        _rconClient.SendMessagePlayer(player, "KD: Could not find match for player name " + targetPlayerName + ".");
                        return;
                    }
                    targetPlayerName = targetPlayer.Name;

                    bool result = RemoveTrackingTarget(player, targetPlayer);

                    if (result)
                        _rconClient.SendMessagePlayer(player, "KD: Tracking of " + targetPlayerName + " stopped.");
                    else
                        _rconClient.SendMessagePlayer(player, "KD: You were not tracking " + targetPlayerName + ".");
                }
            }
        }

        private void AddTrackingTarget(Player player, Player targetPlayer)
        {
            lock (_trackTargets)
            {
                if (!_trackTargets.ContainsKey(targetPlayer))
                    _trackTargets.Add(targetPlayer, new List<Player>());
                if (!_trackTargets[targetPlayer].Contains(player))
                    _trackTargets[targetPlayer].Add(player);
            }
        }

        private bool RemoveTrackingTarget(Player player, Player targetPlayer)
        {
            lock (_trackTargets)
            {
                var res = false;
                if (!_trackTargets.ContainsKey(targetPlayer))
                    return false;
                if (_trackTargets[targetPlayer].Contains(player))
                {
                    _trackTargets[targetPlayer].Remove(player);
                    res = true;
                }
                if (_trackTargets[targetPlayer].Count == 0)
                    _trackTargets.Remove(targetPlayer);
                return res;
            }
        }

        private void CleanUpTracker(Player player)
        {
            lock (_trackTargets)
            {
                if (!_trackTargets.ContainsKey(player))
                {
                    SendToTrackers(player, "KD: " + player.Name + " left the game.");
                    _trackTargets.Remove(player);
                }

                foreach (var kvp in _trackTargets)
                {
                    if (kvp.Value.Contains(player))
                        kvp.Value.Remove(player);
                }
            }
        }

        private void SendToTrackers(Player player, string msg)
        {
            lock (_trackTargets)
            {
                if (!_trackTargets.ContainsKey(player))
                    return;

                foreach (var tracker in _trackTargets[player])
                {
                    _rconClient.SendMessagePlayer(tracker, msg);
                }
            }
        }
    }
}
