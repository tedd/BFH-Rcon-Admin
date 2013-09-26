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
using NLog;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerHighlightNotifier : IAmAModule
    {

        private static Logger log = LogManager.GetCurrentClassLogger();
        private RconClient _rconClient;
        private PlayerCacheAndHistory playerEvents;
        private int _reportKillsToAllAt = 8;
        private int _reportKillsToPersonAt = 4;
        private int _minimumScoreToReport = 700;
        private int _warnCheatAtKDRatio = 20;
        private int _minimumScoreToReportTopScorer = 1500;
        private TimeSpan _killPeriodLookbehind = new TimeSpan(0, 0, 0, 60);
        private TimeSpan _deathPeriodLookbehind = new TimeSpan(0, 0, 0, 60);
        private TimeSpan _scoreReportHistory = new TimeSpan(0, 0, 0, 60);
        private TimeSpan _scoreReportDelay = new TimeSpan(0, 0, 0, 10);
        private Queue<PlayerHistoryItem> _lastKills = new Queue<PlayerHistoryItem>();
        private Queue<PlayerHistoryItem> _lastDeaths = new Queue<PlayerHistoryItem>();
        private int _lastkillsLength = 20;
        private int _lastDeathsLength = 20;
        private bool _lookingForFirstDeath = false;
        private bool _lookingForFirstKill = false;
        private Player _lastTopKiller = null;
        private Player _lastTopScorer = null;
        private bool _canReportTopScoreAndKill = false;
        private int _roundJustEnded = 0;
        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            rconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
            rconClient.ServerInfoCommand.RoundEnd += ServerInfoCommandOnRoundEnd;
            rconClient.PlayerListCommand.PlayerUpdateDone += PlayerListCommandOnPlayerUpdateDone;
        }

        private void PlayerListCommandOnPlayerUpdateDone(object sender)
        {
            _canReportTopScoreAndKill = true;
            ProcessRoundEnd();
        }

        public void ModuleLoadComplete()
        {
            playerEvents = PlayerCacheAndHistory.Current;
            //playerEvents.PlayerUpdated += PlayerEventsOnPlayerUpdated;
            playerEvents.PlayerUpdateDone += PlayerEventsOnPlayerUpdateDone;
        }

        private void PlayerEventsOnPlayerUpdateDone(object sender)
        {
            foreach (var playerCache in ((PlayerCacheAndHistory)sender).GetPlayersCache())
            {
                PlayerEventsOnPlayerUpdated(sender, playerCache);
            }
        }

        private void ServerInfoCommandOnRoundEnd(object sender, ServerInfo lastRoundServerInfo)
        {
            _roundJustEnded = 3;
        }
        private void ProcessRoundEnd()
        {
            _roundJustEnded--;
            if (_roundJustEnded != 0)
                return;
            //log.Fatal("PrintKillLabel queue size: " + _lastKills.Count);

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
                    nameStr += " & ";
                nameStr += lastLast;
                _rconClient.SendMessageAll("KD: Last kill was by: " + nameStr);
            }
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            _lastTopKiller = null;
            _lastTopScorer = null;
            _lastKills.Clear();
            _lookingForFirstKill = true;
            _lookingForFirstDeath = true;
        }

        private void PlayerEventsOnPlayerUpdated(object sender, PlayerCache playerCache)
        {
            PrintScore(playerCache);
            PrintKillLabel(playerCache);
            PrintDeathLabel(playerCache);
            PrintSuicideLabel(playerCache);
            PrintTooHighKD(playerCache);

            PrintNewTopKiller(playerCache);
            PrintNewTopScorer(playerCache);
        }

        private void PrintTooHighKD(PlayerCache playerCache)
        {

            // Did player kill someone?
            if (playerCache.LastPlayerDelta.Score.Kills < 1)
                return;

            if (playerCache.Player.Score.KillDeathRatio >= _warnCheatAtKDRatio)
            {
                _rconClient.SendMessageAll("Warning: " + playerCache.Player.Name + " has very high kill-death ratio: " +
                                           playerCache.Player.Score.KillDeathRatio + "(" + playerCache.Player.Score.Kills + " kills / " + playerCache.Player.Score.Deaths + " deaths)");
                _rconClient.SendMessageAll("Warning: If " + playerCache.Player.Name +
                                           " is cheating consider a VIP kick vote. Type: /kick " + playerCache.Player.Name);
            }

        }

        private void PrintNewTopKiller(PlayerCache playerCache)
        {

            if (_lastTopKiller == null)
            {
                _lastTopKiller = playerCache.Player.Clone();
                //foreach (var player in _rconClient.PlayerListCommand.GetPlayers())
                //{
                //    if (_lastTopKiller == null || player.Score.Kills > _lastTopKiller.Score.Kills)
                //        _lastTopKiller = player;
                //}
            }

            if (_lastTopKiller.Name != playerCache.Player.Name && playerCache.Player.Score.Kills > _lastTopKiller.Score.Kills)
            {
                if (_canReportTopScoreAndKill)
                {
                    if (!playerCache.Settings.StopSpam)
                    {
                        _rconClient.SendMessagePlayer(playerCache.Player,
                                                      "KD: You just replaced " + _lastTopKiller.Name +
                                                      " as top killer this round.");
                    }
                    var other = playerEvents.GetPlayerCache(_lastTopKiller);
                    if (other != null && !other.Settings.StopSpam)
                    {
                        _rconClient.SendMessagePlayer(_lastTopKiller,
                                                      "KD: You just got replaced as top killer by " +
                                                      playerCache.Player +
                                                      ".");
                    }
                }
                _lastTopKiller = playerCache.Player.Clone();
            }
        }

        private void PrintNewTopScorer(PlayerCache playerCache)
        {
            if (_lastTopScorer == null)
            {
                _lastTopScorer = playerCache.Player.Clone();
                //foreach (var player in _rconClient.PlayerListCommand.GetPlayers())
                //{
                //    if (_lastTopScorer == null || player.Score.Score > _lastTopScorer.Score.Score)
                //        _lastTopScorer = player;
                //}
            }

            if (_lastTopScorer.Name != playerCache.Player.Name && playerCache.Player.Score.Score > _lastTopScorer.Score.Score)
            //if (playerCache.Player.Score.Score > _lastTopScorer.Score.Score)
            {
                if (_canReportTopScoreAndKill && playerCache.Player.Score.Score > _minimumScoreToReportTopScorer)
                {
                    if (!playerCache.Settings.StopSpam)
                    {

                        _rconClient.SendMessagePlayer(playerCache.Player,
                                                      "KD: You just replaced " + _lastTopScorer.Name +
                                                      " as top scorer this round.");
                    }
                    var other = playerEvents.GetPlayerCache(_lastTopScorer);
                    if (other != null && !other.Settings.StopSpam)
                    {
                        _rconClient.SendMessagePlayer(_lastTopScorer,
                                                      "KD: You just got replaced as top scorer by " +
                                                      playerCache.Player.Name +
                                                      ".");
                    }
                }
                _lastTopScorer = playerCache.Player.Clone();
            }
        }

        private void PrintSuicideLabel(PlayerCache playerCache)
        {
            // Print some simple kill stats to player and all when at certain threshold

            // Did player kill someone?
            if (playerCache.LastPlayerDelta.Score.Suicides < 1)
                return;

            _rconClient.SendMessageAll("KD: " + playerCache.Player.Name + " suicided.");
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

            if (_lookingForFirstKill)
            {
                _lookingForFirstKill = false;
                _rconClient.SendMessageAll("KD: First kill: " + playerCache.Player.Name + " in " + _rconClient.ServerInfoCommand.ServerInfo.ElapsedRoundTime + " seconds.");
            }

            int kills = 0;
            DateTime oldestUpdate = default(DateTime);
            PlayerHistoryItem lastWithKill = null;
            foreach (var historyItem in historyItems)
            {
                kills += historyItem.PlayerDelta.Score.Kills;
                if (historyItem.PlayerDelta.Score.Kills > 0)
                    lastWithKill = historyItem;
            }
            // Last kill watch
            if (lastWithKill != null)
            {
                oldestUpdate = lastWithKill.Player.LastUpdate;
                _lastKills.Enqueue(lastWithKill);
            }
            // Last kill queue length limit
            while (_lastKills.Count > _lastkillsLength)
            {
                _lastKills.Dequeue();
            }

            var timeSinceOldest = DateTime.Now.Subtract(oldestUpdate);

            // Message player
            if (kills >= _reportKillsToPersonAt && !playerCache.Settings.StopSpam)
                _rconClient.SendMessagePlayer(playerCache.Player, "KD: Last " + (int)timeSinceOldest.TotalSeconds + " seconds: " + kills + " kills.");
            if (kills >= _reportKillsToAllAt)
                _rconClient.SendMessageAll("KD: " + playerCache.Player.Name + " has " + kills + " kills in just " + (int)timeSinceOldest.TotalSeconds + " seconds.");
        }
        private void PrintDeathLabel(PlayerCache playerCache)
        {
            // Print some simple Death stats to player and all when at certain threshold

            // Did player Death someone?
            if (playerCache.LastPlayerDelta.Score.Deaths < 1)
                return;


            var historyItems = new List<PlayerHistoryItem>(GetHistory(playerCache, _deathPeriodLookbehind));
            if (historyItems.Count() == 0)
                return;

            if (_lookingForFirstDeath)
            {
                _lookingForFirstDeath = false;
                _rconClient.SendMessageAll("KD: First Death: " + playerCache.Player.Name);
            }

            int deaths = 0;
            DateTime oldestUpdate = default(DateTime);
            PlayerHistoryItem lastWithDeath = null;
            foreach (var historyItem in historyItems)
            {
                deaths += historyItem.PlayerDelta.Score.Deaths;
                if (historyItem.PlayerDelta.Score.Deaths > 0)
                    lastWithDeath = historyItem;
            }
            // Last Death watch
            if (lastWithDeath != null)
            {
                oldestUpdate = lastWithDeath.Player.LastUpdate;
                _lastDeaths.Enqueue(lastWithDeath);
            }
            // Last Death queue length limit
            while (_lastDeaths.Count > _lastDeathsLength)
            {
                _lastDeaths.Dequeue();
            }

            var timeSinceOldest = DateTime.Now.Subtract(oldestUpdate);

            // Message player
            if (deaths > 1 && !playerCache.Settings.StopSpam)
            {

                _rconClient.SendMessagePlayer(playerCache.Player,
                                              "KD: Last " + (int)timeSinceOldest.TotalSeconds + " seconds: " + deaths +
                                              " deaths.");
            }
            if (deaths > 3)
                _rconClient.SendMessageAll("KD: " + playerCache.Player.Name + " has " + deaths + " deaths in just " + (int)timeSinceOldest.TotalSeconds + " seconds.");
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
            if (!playerCache.Settings.StopSpam && score > _minimumScoreToReport && score > playerCache.LastScoreReported - lastReportedTime.TotalSeconds)
            {
                //var tmpScore = playerCache.LastScoreReported;
                playerCache.LastScoreReported = score;
                playerCache.LastScoreReportedTime = DateTime.Now;
                //score -= tmpScore;

                if ((int)timeSinceOldest.TotalSeconds != 0)
                    _rconClient.SendMessagePlayer(playerCache.Player,
                                              "KD: Score last " + (int)_scoreReportHistory.TotalSeconds + " seconds: " + score);
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
