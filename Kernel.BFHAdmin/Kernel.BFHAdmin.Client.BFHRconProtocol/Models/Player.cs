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
    public class Player
    {
        private PlayerScore _score = new PlayerScore();
        public string Kit { get; set; }
        public bool TkData_Punished { get; set; }
        public int TkData_TimesPunished { get; set; }
        public int TkData_TimesForgiven { get; set; }

        public PlayerScore Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public string Index { get; set; }
        public string Name { get; set; }
        public int Team { get; set; }
        public int Ping { get; set; }
        public bool IsConnected { get; set; }
        public bool IsValid { get; set; }
        public bool IsRemote { get; set; }
        public bool IsAIPlayer { get; set; }
        public bool IsAlive { get; set; }
        public bool IsManDown { get; set; }
        public int ProfileId { get; set; }
        public bool IsFlagholder { get; set; }
        public int Suicide { get; set; }
        public decimal TimeToSpawn { get; set; }
        public int SquadId { get; set; }
        public bool IsSquadLeader { get; set; }
        public bool IsCommander { get; set; }
        public int SpawnGroup { get; set; }
        public string Address { get; set; }
        public decimal ConnectedAt { get; set; }
        public string VehicleName { get; set; }
        public int IdleTime { get; set; }
        public string Cdkeyhash { get; set; }
        public object TkData { get; set; }
        public string Unknown0 { get; set; }
        public string Unknown1 { get; set; }
        public string Unknown2 { get; set; }
        public bool Vip { get; set; }
        public string NucleusId { get; set; }

        public int VehicleType { get; set; }
        public string Position{ get; set; }



        public Player Clone()
        {
            return JsonConvert.DeserializeObject<Player>(JsonConvert.SerializeObject(this));
        }
    }
}
