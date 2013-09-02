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
    public class ChatHistoryItem 
    {
        public int Number { get; set; }
        public string From { get; set; }
        public string What { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }


        public ChatHistoryItem Clone()
        {
            return JsonConvert.DeserializeObject<ChatHistoryItem>(JsonConvert.SerializeObject(this));
        }
    }
}
