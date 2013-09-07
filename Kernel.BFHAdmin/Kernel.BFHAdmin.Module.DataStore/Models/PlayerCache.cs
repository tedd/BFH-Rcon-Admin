using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;

namespace Kernel.BFHAdmin.Module.DataStore.Models
{
    public class PlayerCache : NotifyPropertyBase
    {
        [Key]
        public long Id { get; set; }

       

        private Player _player;
        private DateTime _lastScoreReportedTime = DateTime.Now;

        public Player Player
        {
            get { return _player; }
            set
            {
                if (Equals(value, _player)) return;
                _player = value;
                OnPropertyChanged();
            }
        }

        [NotMapped]
        public int LastScoreReported { get; set; }
        [NotMapped]
        public DateTime LastScoreReportedTime
        {
            get { return _lastScoreReportedTime; }
            set { _lastScoreReportedTime = value; }
        }

        public Player LastPlayerDelta { get; set; }
        public Player LastPlayer { get; set; }
        private List<PlayerHistoryItem> _history { get; set; }
        //[ForeignKey("Id")]
        public IEnumerable<PlayerHistoryItem> History
        {
            get
            {
                List<PlayerHistoryItem> history;
                lock (_history)
                {
                    history = new List<PlayerHistoryItem>(_history);
                }
                foreach (var historyItem in history)
                {
                    yield return historyItem;
                }
            }
        }

        public List<PlayerHistoryItem> CopyHistory()
        {
            lock (_history)
            {
                return new List<PlayerHistoryItem>(_history);
            }
        }

        private PlayerHistoryItem LastInHistory
        {
            get
            {
                lock (_history)
                {
                    var count = _history.Count;
                    if (count > 0)
                        return _history[count - 1];
                    return null;
                }
            }
        }

        public PlayerCache()
        {
            _history = new List<PlayerHistoryItem>();
        }

        public void TakeHistorySnapshot()
        {
            lock (_history)
            {
                var historyItem = new PlayerHistoryItem();
                historyItem.Time = Player.LastUpdate;

                historyItem.Player = Player.Clone();
                var last = LastInHistory;
                if (last == null)
                    historyItem.PlayerDelta = new Player();
                else
                {
                    historyItem.PlayerDelta = Player - last.Player;
                }
                LastPlayer = historyItem.Player;
                LastPlayerDelta = historyItem.PlayerDelta;

                _history.Add(historyItem);
            }
        }
    }
}
