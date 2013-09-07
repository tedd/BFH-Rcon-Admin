using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;

namespace Kernel.BFHAdmin.Module.DefaultScript.Models
{
    public class PlayerCache : NotifyPropertyBase
    {
        public Player Player { get; set; }
        public Player LastPlayerDelta { get; set; }
        public Player LastPlayer { get; set; }
        public List<PlayerHistoryItem> History { get; private set; }
        
        private PlayerHistoryItem LastInHistory
        {
            get
            {
                var count = History.Count;
                if (count > 0)
                    return History[count - 1];
                return null;
            }
        }

        public PlayerCache()
        {
            History = new List<PlayerHistoryItem>();
        }

        public void TakeHistorySnapshot()
        {
            lock (History)
            {
                var historyItem=new PlayerHistoryItem();

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
                
                History.Add(historyItem);
            }
        }
    }
}
