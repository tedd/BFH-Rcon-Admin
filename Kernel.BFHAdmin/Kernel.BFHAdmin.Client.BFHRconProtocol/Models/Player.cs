using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Annotations;
using Kernel.BFHAdmin.Common.Interfaces;
using Newtonsoft.Json;
using PropertyChanging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class Player : NotifyPropertyBase, ITypeCloneable<Player>
    {
        // Note: When adding/removing fields you also need to update fields in both Clone() and -() methods.
        [Key]
        public int Id { get; set; }

        private PlayerScore _score = new PlayerScore();
        private PlayerPosition _position = new PlayerPosition();
        private int _vehicleType;
        private long _nucleusId;
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
        private int _teamId;
        private string _name;
        private string _index;
        private int _tkDataTimesForgiven;
        private int _tkDataTimesPunished;
        private bool _tkDataPunished;
        private string _kit;
        private string _fullName;
        private Team _team;
        private string _positionString;
        private DateTime _lastUpdate = new DateTime(1970, 01, 01);

        public static Player operator -(Player a, Player b)
        {
            // Produce a delta object containing differences
            var ret = new Player();

            // Integers
            ret.IdleTime = a.IdleTime - b.IdleTime;
            ret.Ping = a.Ping - b.Ping;
            ret.TimeToSpawn = a.TimeToSpawn - b.TimeToSpawn;
            ret.Suicide = a.Suicide - b.Suicide;
            ret.TkData_TimesForgiven = a.TkData_TimesForgiven - b.TkData_TimesForgiven;
            ret.TkData_TimesPunished = a.TkData_TimesPunished - b.TkData_TimesPunished;

            // Booleans
            ret.Vip = (a.Vip == b.Vip);
            ret.IsFlagholder = (a.IsFlagholder == b.IsFlagholder);
            ret.IsCommander = (a.IsCommander == b.IsCommander);
            ret.IsSquadLeader = (a.IsSquadLeader == b.IsSquadLeader);
            ret.IsManDown = (a.IsManDown == b.IsManDown);
            ret.IsAlive = (a.IsAlive == b.IsAlive);
            ret.IsAIPlayer = (a.IsAIPlayer == b.IsAIPlayer);
            ret.IsRemote = (a.IsRemote == b.IsRemote);
            ret.IsValid = (a.IsValid == b.IsValid);
            ret.IsConnected = (a.IsConnected == b.IsConnected);
            ret.TkData_Punished = (a.TkData_Punished == b.TkData_Punished);

            // Strings

            // Child models
            ret.Score = a.Score - b.Score;
            ret.Position = a.Position - b.Position;

            ret.LastUpdate = DateTime.Now;

            return ret;
        }

        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                if (value.Equals(_lastUpdate)) return;
                _lastUpdate = value;
                OnPropertyChanged();
            }
        }

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
            private set { _score = value; }
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

        public int TeamId
        {
            get { return _teamId; }
            set
            {
                if (value == _teamId) return;
                _teamId = value;
                OnPropertyChanged();
            }
        }

        [ExpandableObject()]
        public Team Team
        {
            get { return _team; }
            set
            {
                if (Equals(value, _team)) return;
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

        public long NucleusId
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

        [ExpandableObject()]
        [NotMapped]
        public PlayerPosition Position
        {
            get { return _position; }
            set
            {
                if (value == _position) return;
                _position = value;
                OnPropertyChanged();
            }
        }

        public string PositionString
        {
            get { return _positionString; }
            set
            {
                if (value == _positionString) return;
                _positionString = value;
                Position.ParseFrom(PositionString);
                OnPropertyChanged();
            }
        }


        public Player Clone()
        {
            var newObj = (Player)this.MemberwiseClone();
            newObj.Score = this.Score.Clone();
            newObj.PositionString = this.PositionString;
            return newObj;
        }

        public override string ToString()
        {
            return Name;
        }


    }
}
