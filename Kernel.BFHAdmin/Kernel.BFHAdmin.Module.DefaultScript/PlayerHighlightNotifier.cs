using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Module.DataStore.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerHighlightNotifier : IAmAModule
    {

        private RconClient _rconClient;
        private PlayerEvents playerEvents;
        private int _minimumScoreToReport = 400;
        private TimeSpan _killPeriodLookbehind = new TimeSpan(0, 0, 0, 30);
        private TimeSpan _scoreReportHistory = new TimeSpan(0, 0, 0, 60);
        private TimeSpan _scoreReportDelay = new TimeSpan(0, 0, 0, 10);
        private Queue<PlayerHistoryItem> _lastKills = new Queue<PlayerHistoryItem>();
        private int _lastkillsLength = 20;
        private bool _lookingForFirstKill = false;
        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            rconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
            rconClient.ServerInfoCommand.RoundEnd += ServerInfoCommandOnRoundEnd;
        }

        public void ModuleLoadComplete()
        {
            playerEvents = PlayerEvents.Current;
            playerEvents.PlayerUpdated += PlayerEventsOnPlayerUpdated;
        }

        private void ServerInfoCommandOnRoundEnd(object sender, ServerInfo lastRoundServerInfo)
        {
            DateTime lastKillTime = DateTime.MinValue;
            var lastKillers = new List<PlayerHistoryItem>();
            while (_lastKills.Count > 0)
            {
                var historyItem = _lastKills.Dequeue();
                if (historyItem.Time >= lastKillTime)
                {
                    lastKillTime = historyItem.Time;
                    lastKillers.Add(historyItem);
                }
            }
            var lastKillersUnique = new List<string>();
            foreach (var lk in new List<PlayerHistoryItem>(lastKillers))
            {
                if (lk.Time < lastKillTime)
                    lastKillers.Remove(lk);
                else
                {
                    if (!lastKillersUnique.Contains(lk.Player.Name))
                        lastKillersUnique.Add(lk.Player.Name);
                }
            }
            lastKillersUnique.Sort();
            string lastLast;
            string nameStr = null;
            if (lastKillersUnique.Count > 0)
            {
                lastLast = lastKillersUnique.Last();
                lastKillersUnique.Remove(lastLast);
                if (lastKillersUnique.Count > 0)
                {
                    nameStr = string.Join(", ", lastKillersUnique);
                }
                if (nameStr != null)
                    nameStr += " and ";
                nameStr += lastLast;
                _rconClient.SendMessageAll("KD: Last kill was by: " + nameStr);
            }
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            _lastKills.Clear();
            _lookingForFirstKill = true;
        }

        private void PlayerEventsOnPlayerUpdated(object sender, PlayerCache playerCache)
        {
            PrintScore(playerCache);
            PrintKillLabel(playerCache);
        }

        private void PrintKillLabel(PlayerCache playerCache)
        {
            // Print some simple kill stats to player and all when at certain threshold

            // Did player kill someone?
            if (playerCache.LastPlayerDelta.Score.Kills < 1)
                return;

            var historyItems = new List<PlayerHistoryItem>(GetHistory(playerCache, _killPeriodLookbehind));
            if (historyItems.Count() == 0)
                return;
            DateTime oldestUpdate = historyItems[0].Player.LastUpdate;

            if (_lookingForFirstKill)
            {
                _lookingForFirstKill = false;
                _rconClient.SendMessageAll("KD: First kill: " + playerCache.Player.Name);
            }
            
            // Last kill watch
            _lastKills.Enqueue(historyItems[0]);
            while (_lastKills.Count > _lastkillsLength)
            {
                _lastKills.Dequeue();
            }
            var timeSinceOldest = DateTime.Now.Subtract(oldestUpdate);

            int kills = 0;
            foreach (var historyItem in historyItems)
            {
                kills += historyItem.PlayerDelta.Score.Kills;
            }

            // Message player
            if (kills > 2)
                _rconClient.SendMessagePlayer(playerCache.Player, "Last " + (int)timeSinceOldest.TotalSeconds + " seconds: " + kills + " kills.");
            if (kills > 5)
                _rconClient.SendMessageAll(playerCache.Player.Name + " killed " + kills + " in just " + (int)timeSinceOldest.TotalSeconds + " seconds.");
        }

        private void PrintScore(PlayerCache playerCache)
        {

            // Print some simple kill stats to player and all when at certain threshold
            if (playerCache.LastPlayerDelta.Score.Score < 1)
                return;
            var lastReportedTime = DateTime.Now.Subtract(playerCache.LastScoreReportedTime);
            // Waiting since last report?
            if (lastReportedTime < _scoreReportDelay)
                return;

            var historyItems = new List<PlayerHistoryItem>(GetHistory(playerCache, _scoreReportHistory));
            if (historyItems.Count() == 0)
                return;
            DateTime oldestUpdate = historyItems.Last().Player.LastUpdate;
            //DateTime newestUpdate = historyItems.Last().Player.LastUpdate;
            var timeSinceOldest = DateTime.Now.Subtract(oldestUpdate);
            //var timeSinceNewest = DateTime.Now.Subtract(newestUpdate);

            int score = 0;
            foreach (var historyItem in historyItems)
            {
                score += historyItem.PlayerDelta.Score.Score;
                //if (score > _minimumScoreToReport)
                //    return;
            }
            var oldestSec = (int)timeSinceOldest.TotalSeconds;
            // Message player
            if (score > _minimumScoreToReport && score > playerCache.LastScoreReported - lastReportedTime.TotalSeconds)
            {
                //var tmpScore = playerCache.LastScoreReported;
                playerCache.LastScoreReported = score;
                playerCache.LastScoreReportedTime = DateTime.Now;
                //score -= tmpScore;

                if ((int)timeSinceOldest.TotalSeconds != 0)
                    _rconClient.SendMessagePlayer(playerCache.Player,
                                              "Score last " + (int)_scoreReportHistory.TotalSeconds + " seconds: " + score);
            }


        }

        private IEnumerable<PlayerHistoryItem> GetHistory(PlayerCache playerCache, TimeSpan scoreReportDelay)
        {
            // Get player history
            var history = playerCache.CopyHistory();
            for (int i = history.Count() - 1; i > -1; i--)
            {
                var historyItem = history[i];
                var since = DateTime.Now.Subtract(historyItem.Time);
                if (since > scoreReportDelay)
                    yield break;
                yield return historyItem;
            }
        }
    }
}
