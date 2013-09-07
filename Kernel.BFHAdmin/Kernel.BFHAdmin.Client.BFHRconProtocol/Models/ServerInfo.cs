using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Interfaces;
using Newtonsoft.Json;
using PropertyChanging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class ServerInfo : NotifyPropertyBase, ITypeCloneable<ServerInfo>
    {
        public enum GameStatus
        {
            Unknown = 0,
            Running = 1,
            EndScreen = 2
        }

        private Team _team1 = new Team();
        private Team _team2 = new Team();
        private int _currentRound;
        private int _totalRounds;
        private int _reservedSlots;
        private decimal _wallTime;
        private bool _rankedStatus;
        private bool _autoBalance;
        private int _timeLimit;
        private string _worldSize;
        private string _modDir;
        private string _gameMode;
        private int _remainingTime;
        private int _elapsedRoundTime;
        private string _unknown1;
        private string _unknown0;
        private string _serverName;
        private string _nextMapName;
        private string _mapName;
        private int _joining;
        private int _players;
        private int _maxPlayers;
        private GameStatus _currentGameStatus;
        private string _version;
        private bool _isPregame;

        [ExpandableObject()]
        public Team Team1
        {
            get { return _team1; }
            set
            {
                if (Equals(value, _team1)) return;
                _team1 = value;
                OnPropertyChanged();
            }
        }
        [ExpandableObject()]
        public Team Team2
        {
            get { return _team2; }
            set
            {
                if (Equals(value, _team2)) return;
                _team2 = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get { return _version; }
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }

        public GameStatus CurrentGameStatus
        {
            get { return _currentGameStatus; }
            set
            {
                if (value == _currentGameStatus) return;
                _currentGameStatus = value;
                OnPropertyChanged();
            }
        }

        public int MaxPlayers
        {
            get { return _maxPlayers; }
            set
            {
                if (value == _maxPlayers) return;
                _maxPlayers = value;
                OnPropertyChanged();
            }
        }

        public int Players
        {
            get { return _players; }
            set
            {
                if (value == _players) return;
                _players = value;
                OnPropertyChanged();
            }
        }

        public int Joining
        {
            get { return _joining; }
            set
            {
                if (value == _joining) return;
                _joining = value;
                OnPropertyChanged();
            }
        }

        public string MapName
        {
            get { return _mapName; }
            set
            {
                if (value == _mapName) return;
                _mapName = value;
                OnPropertyChanged();
            }
        }

        public string NextMapName
        {
            get { return _nextMapName; }
            set
            {
                if (value == _nextMapName) return;
                _nextMapName = value;
                OnPropertyChanged();
            }
        }

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                if (value == _serverName) return;
                _serverName = value;
                OnPropertyChanged();
            }
        }

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

        public int ElapsedRoundTime
        {
            get { return _elapsedRoundTime; }
            set
            {
                if (value == _elapsedRoundTime) return;
                _elapsedRoundTime = value;
                OnPropertyChanged();
            }
        }

        public int RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                if (value == _remainingTime) return;
                _remainingTime = value;
                OnPropertyChanged();
            }
        }

        public string GameMode
        {
            get { return _gameMode; }
            set
            {
                if (value == _gameMode) return;
                _gameMode = value;
                OnPropertyChanged();
            }
        }

        public string ModDir
        {
            get { return _modDir; }
            set
            {
                if (value == _modDir) return;
                _modDir = value;
                OnPropertyChanged();
            }
        }

        public string WorldSize
        {
            get { return _worldSize; }
            set
            {
                if (value == _worldSize) return;
                _worldSize = value;
                OnPropertyChanged();
            }
        }

        public int TimeLimit
        {
            get { return _timeLimit; }
            set
            {
                if (value == _timeLimit) return;
                _timeLimit = value;
                OnPropertyChanged();
            }
        }

        public bool AutoBalance
        {
            get { return _autoBalance; }
            set
            {
                if (value.Equals(_autoBalance)) return;
                _autoBalance = value;
                OnPropertyChanged();
            }
        }

        public bool RankedStatus
        {
            get { return _rankedStatus; }
            set
            {
                if (value.Equals(_rankedStatus)) return;
                _rankedStatus = value;
                OnPropertyChanged();
            }
        }

        public decimal WallTime
        {
            get { return _wallTime; }
            set
            {
                if (value == _wallTime) return;
                _wallTime = value;
                OnPropertyChanged();
            }
        }

        public int ReservedSlots
        {
            get { return _reservedSlots; }
            set
            {
                if (value == _reservedSlots) return;
                _reservedSlots = value;
                OnPropertyChanged();
            }
        }

        public int TotalRounds
        {
            get { return _totalRounds; }
            set
            {
                if (value == _totalRounds) return;
                _totalRounds = value;
                OnPropertyChanged();
            }
        }

        public int CurrentRound
        {
            get { return _currentRound; }
            set
            {
                if (value == _currentRound) return;
                _currentRound = value;
                OnPropertyChanged();
            }
        }

        public bool IsPregame
        {
            get { return _isPregame; }
            set
            {
                if (value.Equals(_isPregame)) return;
                _isPregame = value;
                OnPropertyChanged();
            }
        }

        public ServerInfo Clone()
        {
            var ret = (ServerInfo)this.MemberwiseClone();
            ret.Team1 = this.Team1.Clone();
            ret.Team2 = this.Team2.Clone();
            return ret;
        }

        public static ServerInfo operator -(ServerInfo a, ServerInfo b)
        {
            // Produce a delta object containing differences
            var ret = new ServerInfo();

            // Int
            ret.WallTime = a.WallTime - b.WallTime;
            ret.RemainingTime = a.RemainingTime - b.RemainingTime;
            ret.ElapsedRoundTime = a.ElapsedRoundTime - b.ElapsedRoundTime;
            ret.Joining = a.Joining - b.Joining;
            ret.Players = a.Players - b.Players;
            ret.MaxPlayers = a.MaxPlayers - b.MaxPlayers;

            // Bool
            ret.RankedStatus = (a.RankedStatus == b.RankedStatus);
            ret.IsPregame = (a.IsPregame == b.IsPregame);

            // Custom
            ret.Team1 = a.Team1 - b.Team1;
            ret.Team2 = a.Team2 - b.Team2;

            return ret;
        }

    }
}
