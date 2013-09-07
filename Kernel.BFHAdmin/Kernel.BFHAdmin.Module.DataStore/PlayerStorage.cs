using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Module.DataStore.Models;

namespace Kernel.BFHAdmin.Client.DataStore
{
    public class PlayerStorage:DbContext
    {
        public PlayerStorage()
            : base("BFHStorage")
        {
            
        }
        public DbSet<PlayerCache> PlayerCaches { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<PlayerHistoryItem> PlayerHistoryItems { get; set; }
    }
}
