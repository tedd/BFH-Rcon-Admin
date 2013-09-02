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
    public class AdminListItem
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public AdminListItem Clone()
        {
            return JsonConvert.DeserializeObject<AdminListItem>(JsonConvert.SerializeObject(this));
        }
    }
}
