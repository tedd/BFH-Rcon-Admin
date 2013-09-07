using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;

namespace Kernel.BFHAdmin.Module.DefaultScript.Models
{
    public class PlayerHistoryItem: NotifyPropertyBase
    {
        private DateTime _time = DateTime.Now;

        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (value.Equals(_time)) return;
                _time = value;
                OnPropertyChanged();
            }
        }

        public Player Player { get; set; }
        public Player PlayerDelta { get; set; }
    }
}
