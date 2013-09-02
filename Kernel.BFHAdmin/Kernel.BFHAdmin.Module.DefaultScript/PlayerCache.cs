using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PlayerCache
    {
        public int RoundFlagHolds { get; set; }
        public int TotalFlagHolds { get; set; }
        public Player Player { get; set; }

        public PlayerCache()
        {
            //Player = new Player();
        }
    }
}
