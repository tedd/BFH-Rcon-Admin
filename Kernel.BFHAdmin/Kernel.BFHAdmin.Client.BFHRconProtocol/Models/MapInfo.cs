using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class MapInfo
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int MaxPlayers { get; set; }
        public List<ServerInfo.GameType> GameTypes { get; set; }

        public MapInfo()
        {
            GameTypes= new List<ServerInfo.GameType>();
        }
    }
}
