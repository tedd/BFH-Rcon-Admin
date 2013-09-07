using System;
using System.ComponentModel.DataAnnotations;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;

namespace Kernel.BFHAdmin.Module.DataStore.Models
{
    public class PlayerHistoryItem: NotifyPropertyBase
    {
        [Key]
        public int Id { get; set; }

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
