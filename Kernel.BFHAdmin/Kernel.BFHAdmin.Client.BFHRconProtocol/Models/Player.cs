using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Annotations;
using Newtonsoft.Json;
using PropertyChanging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class Player : NotifyPropertyBase
    {
        private PlayerScore _score = new PlayerScore();
        private string _position;
        private int _vehicleType;
        private string _nucleusId;
        private bool _vip;
        private string _unknown2;
        private string _unknown1;
        private string _unknown0;
        private string _cdkeyhash;
        private int _idleTime;
        private string _vehicleName;
        private decimal _connectedAt;
        private string _address;
        private int _spawnGroup;
        private bool _isCommander;
        private bool _isSquadLeader;
        private int _squadId;
        private decimal _timeToSpawn;
        private int _suicide;
        private bool _isFlagholder;
        private int _profileId;
        private bool _isManDown;
        private bool _isAlive;
        private bool _isAIPlayer;
        private bool _isRemote;
        private bool _isValid;
        private bool _isConnected;
        private int _ping;
        private int _team;
        private string _name;
        private string _index;
        private int _tkDataTimesForgiven;
        private int _tkDataTimesPunished;
        private bool _tkDataPunished;
        private string _kit;
        public string Kit
        {
            get { return _kit; }
            set
            {
                if (value == _kit) return;
                _kit = value;
                OnPropertyChanged();
            }
        }

        public bool TkData_Punished
        {
            get { return _tkDataPunished; }
            set
            {
                if (value.Equals(_tkDataPunished)) return;
                _tkDataPunished = value;
                OnPropertyChanged();
            }
        }

        public int TkData_TimesPunished
        {
            get { return _tkDataTimesPunished; }
            set
            {
                if (value == _tkDataTimesPunished) return;
                _tkDataTimesPunished = value;
                OnPropertyChanged();
            }
        }

        public int TkData_TimesForgiven
        {
            get { return _tkDataTimesForgiven; }
            set
            {
                if (value == _tkDataTimesForgiven) return;
                _tkDataTimesForgiven = value;
                OnPropertyChanged();
            }
        }
        [ExpandableObject()]
        public PlayerScore Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public string Index
        {
            get { return _index; }
            set
            {
                if (value == _index) return;
                _index = value;
                OnPropertyChanged();
            }
        }

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

        public int Team
        {
            get { return _team; }
            set
            {
                if (value == _team) return;
                _team = value;
                OnPropertyChanged();
            }
        }

        public int Ping
        {
            get { return _ping; }
            set
            {
                if (value == _ping) return;
                _ping = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (value.Equals(_isConnected)) return;
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (value.Equals(_isValid)) return;
                _isValid = value;
                OnPropertyChanged();
            }
        }

        public bool IsRemote
        {
            get { return _isRemote; }
            set
            {
                if (value.Equals(_isRemote)) return;
                _isRemote = value;
                OnPropertyChanged();
            }
        }

        public bool IsAIPlayer
        {
            get { return _isAIPlayer; }
            set
            {
                if (value.Equals(_isAIPlayer)) return;
                _isAIPlayer = value;
                OnPropertyChanged();
            }
        }

        public bool IsAlive
        {
            get { return _isAlive; }
            set
            {
                if (value.Equals(_isAlive)) return;
                _isAlive = value;
                OnPropertyChanged();
            }
        }

        public bool IsManDown
        {
            get { return _isManDown; }
            set
            {
                if (value.Equals(_isManDown)) return;
                _isManDown = value;
                OnPropertyChanged();
            }
        }

        public int ProfileId
        {
            get { return _profileId; }
            set
            {
                if (value == _profileId) return;
                _profileId = value;
                OnPropertyChanged();
            }
        }

        public bool IsFlagholder
        {
            get { return _isFlagholder; }
            set
            {
                if (value.Equals(_isFlagholder)) return;
                _isFlagholder = value;
                OnPropertyChanged();
            }
        }

        public int Suicide
        {
            get { return _suicide; }
            set
            {
                if (value == _suicide) return;
                _suicide = value;
                OnPropertyChanged();
            }
        }

        public decimal TimeToSpawn
        {
            get { return _timeToSpawn; }
            set
            {
                if (value == _timeToSpawn) return;
                _timeToSpawn = value;
                OnPropertyChanged();
            }
        }

        public int SquadId
        {
            get { return _squadId; }
            set
            {
                if (value == _squadId) return;
                _squadId = value;
                OnPropertyChanged();
            }
        }

        public bool IsSquadLeader
        {
            get { return _isSquadLeader; }
            set
            {
                if (value.Equals(_isSquadLeader)) return;
                _isSquadLeader = value;
                OnPropertyChanged();
            }
        }

        public bool IsCommander
        {
            get { return _isCommander; }
            set
            {
                if (value.Equals(_isCommander)) return;
                _isCommander = value;
                OnPropertyChanged();
            }
        }

        public int SpawnGroup
        {
            get { return _spawnGroup; }
            set
            {
                if (value == _spawnGroup) return;
                _spawnGroup = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                if (value == _address) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        public decimal ConnectedAt
        {
            get { return _connectedAt; }
            set
            {
                if (value == _connectedAt) return;
                _connectedAt = value;
                OnPropertyChanged();
            }
        }

        public string VehicleName
        {
            get { return _vehicleName; }
            set
            {
                if (value == _vehicleName) return;
                _vehicleName = value;
                OnPropertyChanged();
            }
        }

        public int IdleTime
        {
            get { return _idleTime; }
            set
            {
                if (value == _idleTime) return;
                _idleTime = value;
                OnPropertyChanged();
            }
        }

        public string Cdkeyhash
        {
            get { return _cdkeyhash; }
            set
            {
                if (value == _cdkeyhash) return;
                _cdkeyhash = value;
                OnPropertyChanged();
            }
        }

        //public object TkData { get; set; }
        public string Unknown0
        {
            get { return _unknown0; }
            set
            {
                if (value == _unknown0) return;
                _unknown0 = value;
                OnPropertyChanged();
            }
        }

        public string Unknown1
        {
            get { return _unknown1; }
            set
            {
                if (value == _unknown1) return;
                _unknown1 = value;
                OnPropertyChanged();
            }
        }

        public string Unknown2
        {
            get { return _unknown2; }
            set
            {
                if (value == _unknown2) return;
                _unknown2 = value;
                OnPropertyChanged();
            }
        }

        public bool Vip
        {
            get { return _vip; }
            set
            {
                if (value.Equals(_vip)) return;
                _vip = value;
                OnPropertyChanged();
            }
        }

        public string NucleusId
        {
            get { return _nucleusId; }
            set
            {
                if (value == _nucleusId) return;
                _nucleusId = value;
                OnPropertyChanged();
            }
        }

        public int VehicleType
        {
            get { return _vehicleType; }
            set
            {
                if (value == _vehicleType) return;
                _vehicleType = value;
                OnPropertyChanged();
            }
        }

        public string Position
        {
            get { return _position; }
            set
            {
                if (value == _position) return;
                _position = value;
                OnPropertyChanged();
            }
        }


        public Player Clone()
        {
            return JsonConvert.DeserializeObject<Player>(JsonConvert.SerializeObject(this));
        }
    }
}
