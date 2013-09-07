using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.DataStore;

namespace Kernel.BFHAdmin.Module.DataStore
{
    public class DatabaseController
    {
        public static void Initialize()
        {
            //Database.SetInitializer<PlayerStorage>(new CreateDatabaseIfNotExists<PlayerStorage>());
            //Database.SetInitializer<PlayerStorage>(new DropCreateDatabaseIfModelChanges<PlayerStorage>());
            Database.SetInitializer<PlayerStorage>(new DropCreateDatabaseAlways<PlayerStorage>());
        
        }
    }
}
