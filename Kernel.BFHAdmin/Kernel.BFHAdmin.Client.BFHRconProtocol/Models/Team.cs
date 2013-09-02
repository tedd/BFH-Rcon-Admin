using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    [ImplementPropertyChanging]
    public class Team
    {
        public string Name { get; set; }
        public int TicketState { get; set; }
        public int StartTickets { get; set; }
        public int Tickets { get; set; }
        public int Count { get; set; }

        public Team Clone()
        {
            return JsonConvert.DeserializeObject<Team>(JsonConvert.SerializeObject(this));
        }
    }
   
}
