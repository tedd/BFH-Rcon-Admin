using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Interfaces;
using Newtonsoft.Json;
using PropertyChanging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{

    public class Team : NotifyPropertyBase, ITypeCloneable<Team>
    {
        [Key]
        public int Id { get; set; }

        private int _count;
        private int _tickets;
        private int _startTickets;
        private int _ticketState;
        private string _name;
        private List<Player> _players = new List<Player>();
        [NotMapped]
        [ExpandableObject()]
        public IEnumerable<Player> Players
        {
            get
            {
                List<Player> players;
                lock (_players)
                {
                    players = new List<Player>(_players);
                }
                foreach (var player in players)
                {
                    yield return player;
                }
            }
        }
        
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public int TicketState
        {
            get { return _ticketState; }
            set
            {
                if (value == _ticketState) return;
                _ticketState = value;
                OnPropertyChanged();
            }
        }

        public int StartTickets
        {
            get { return _startTickets; }
            set
            {
                if (value == _startTickets) return;
                _startTickets = value;
                OnPropertyChanged();
            }
        }

        public int Tickets
        {
            get { return _tickets; }
            set
            {
                if (value == _tickets) return;
                _tickets = value;
                OnPropertyChanged();
            }
        }

        public int Count
        {
            get { return _count; }
            set
            {
                if (value == _count) return;
                _count = value;
                OnPropertyChanged();
            }
        }

        public Team Clone()
        {
            return (Team)this.MemberwiseClone();
        }

        public static Team operator -(Team a, Team b)
        {
            // Produce a delta object containing differences
            var ret = new Team();
            ret.Count = a.Count - b.Count;
            ret.StartTickets = a.StartTickets - b.StartTickets;
            ret.Name = a.Name;
            //ret.TicketState = (a.TicketState == b.TicketState ? 0 : b.TicketState);
            return ret;
        }

        internal void AddPlayer(Player player)
        {
            lock (_players)
            {
                if (!_players.Contains(player))
                {
                    _players.Add(player);
                    OnPropertyChanged("Players");
                }
            }
        }

        internal void RemovePlayer(Player player)
        {
            lock (_players)
            {
                if (_players.Contains(player))
                {
                    _players.Remove(player);
                    OnPropertyChanged("Players");
                }
            }
        }
    }

}
