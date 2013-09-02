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
    public class PlayerScore
    {
        public int TeamKills { get; set; }
        public int DamageAssists { get; set; }
        public int PassengerAssists { get; set; }
        public int TargetAssists { get; set; }
        public int Revives { get; set; }
        public int TeamDamages { get; set; }
        public int TeamVehicleDamages { get; set; }
        public int CpCaptures { get; set; }
        public int CpDefends { get; set; }
        public int CpAssists { get; set; }
        public int CpNeutralizes { get; set; }
        public int CpNeutralizeAssists { get; set; }
        public int Suicides { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }

        public PlayerScore Clone()
        {
            return JsonConvert.DeserializeObject<PlayerScore>(JsonConvert.SerializeObject(this));
        }
    }
}
