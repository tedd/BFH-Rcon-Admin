using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Interfaces;
using Newtonsoft.Json;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{

    public class PlayerScore : NotifyPropertyBase, ITypeCloneable<PlayerScore>
    {
        // Note: When adding/removing fields you also need to update fields in both Clone() and -() methods.
        private int _rank;
        private int _score;
        private int _deaths;
        private int _kills;
        private int _suicides;
        private int _cpNeutralizeAssists;
        private int _cpNeutralizes;
        private int _cpAssists;
        private int _cpDefends;
        private int _cpCaptures;
        private int _teamVehicleDamages;
        private int _teamDamages;
        private int _revives;
        private int _targetAssists;
        private int _passengerAssists;
        private int _damageAssists;
        private int _teamKills;
        public int TeamKills
        {
            get { return _teamKills; }
            set
            {
                if (value == _teamKills) return;
                _teamKills = value;
                OnPropertyChanged();
            }
        }

        public int DamageAssists
        {
            get { return _damageAssists; }
            set
            {
                if (value == _damageAssists) return;
                _damageAssists = value;
                OnPropertyChanged();
            }
        }

        public int PassengerAssists
        {
            get { return _passengerAssists; }
            set
            {
                if (value == _passengerAssists) return;
                _passengerAssists = value;
                OnPropertyChanged();
            }
        }

        public int TargetAssists
        {
            get { return _targetAssists; }
            set
            {
                if (value == _targetAssists) return;
                _targetAssists = value;
                OnPropertyChanged();
            }
        }

        public int Revives
        {
            get { return _revives; }
            set
            {
                if (value == _revives) return;
                _revives = value;
                OnPropertyChanged();
            }
        }

        public int TeamDamages
        {
            get { return _teamDamages; }
            set
            {
                if (value == _teamDamages) return;
                _teamDamages = value;
                OnPropertyChanged();
            }
        }

        public int TeamVehicleDamages
        {
            get { return _teamVehicleDamages; }
            set
            {
                if (value == _teamVehicleDamages) return;
                _teamVehicleDamages = value;
                OnPropertyChanged();
            }
        }

        public int CpCaptures
        {
            get { return _cpCaptures; }
            set
            {
                if (value == _cpCaptures) return;
                _cpCaptures = value;
                OnPropertyChanged();
            }
        }

        public int CpDefends
        {
            get { return _cpDefends; }
            set
            {
                if (value == _cpDefends) return;
                _cpDefends = value;
                OnPropertyChanged();
            }
        }

        public int CpAssists
        {
            get { return _cpAssists; }
            set
            {
                if (value == _cpAssists) return;
                _cpAssists = value;
                OnPropertyChanged();
            }
        }

        public int CpNeutralizes
        {
            get { return _cpNeutralizes; }
            set
            {
                if (value == _cpNeutralizes) return;
                _cpNeutralizes = value;
                OnPropertyChanged();
            }
        }

        public int CpNeutralizeAssists
        {
            get { return _cpNeutralizeAssists; }
            set
            {
                if (value == _cpNeutralizeAssists) return;
                _cpNeutralizeAssists = value;
                OnPropertyChanged();
            }
        }

        public int Suicides
        {
            get { return _suicides; }
            set
            {
                if (value == _suicides) return;
                _suicides = value;
                OnPropertyChanged();
            }
        }

        public int Kills
        {
            get { return _kills; }
            set
            {
                if (value == _kills) return;
                _kills = value;
                OnPropertyChanged();
            }
        }

        public int Deaths
        {
            get { return _deaths; }
            set
            {
                if (value == _deaths) return;
                _deaths = value;
                OnPropertyChanged();
            }
        }

        public int Score
        {
            get { return _score; }
            set
            {
                if (value == _score) return;
                _score = value;
                OnPropertyChanged();
            }
        }

        public int Rank
        {
            get { return _rank; }
            set
            {
                if (value == _rank) return;
                _rank = value;
                OnPropertyChanged();
            }
        }

        public PlayerScore Clone()
        {
            return (PlayerScore)this.MemberwiseClone();
        }

        public static PlayerScore operator -(PlayerScore a, PlayerScore b)
        {
            // Produce a delta object containing differences
            var ret = new PlayerScore();
            ret.Rank = a.Rank - b.Rank;
            ret.Score = a.Score - b.Score;
            ret.Deaths = a.Deaths - b.Deaths;
            ret.Kills = a.Kills - b.Kills;
            ret.Suicides = a.Suicides - b.Suicides;
            ret.CpNeutralizeAssists = a.CpNeutralizeAssists - b.CpNeutralizeAssists;
            ret.CpNeutralizes = a.CpNeutralizes - b.CpNeutralizes;
            ret.CpAssists = a.CpAssists - b.CpAssists;
            ret.CpDefends = a.CpDefends - b.CpDefends;
            ret.CpCaptures = a.CpCaptures - b.CpCaptures;
            ret.TeamVehicleDamages = a.TeamVehicleDamages - b.TeamVehicleDamages;
            ret.TeamDamages = a.TeamDamages - b.TeamDamages;
            ret.Revives = a.Revives - b.Revives;
            ret.TargetAssists = a.TargetAssists - b.TargetAssists;
            ret.PassengerAssists = a.PassengerAssists - b.PassengerAssists;
            ret.DamageAssists = a.DamageAssists - b.DamageAssists;
            ret.TeamKills = a.TeamKills - b.TeamKills;

            return ret;
        }

        public override string ToString()
        {
            return Score.ToString();
        }

    }
}
