using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Newtonsoft.Json;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{

    public class Team : NotifyPropertyBase
    {
        private int _count;
        private int _tickets;
        private int _startTickets;
        private int _ticketState;
        private string _name;
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
            return JsonConvert.DeserializeObject<Team>(JsonConvert.SerializeObject(this));
        }
    }
   
}
