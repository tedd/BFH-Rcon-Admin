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
    public class ServerInfo 
    {
        public enum GameStatus
        {
            Unknown = 0,
            Running = 1,
            EndScreen = 2
        }

        private Team _team1 = new Team();
        public Team Team1
        {
            get { return _team1; }
            set { _team1 = value; }
        }
        private Team _team2 = new Team();
        public Team Team2
        {
            get { return _team2; }
            set { _team2 = value; }
        }

        public string Version { get; set; }
        public GameStatus CurrentGameStatus { get; set; }
        public int MaxPlayers { get; set; }
        public int Players { get; set; }
        public int Joining { get; set; }
        public string MapName { get; set; }
        public string NextMapName { get; set; }
        public string ServerName { get; set; }
        public string Unknown0 { get; set; }
        public string Unknown1 { get; set; }
        public int ElapsedRoundTime { get; set; }
        public int RemainingTime { get; set; }
        public string GameMode { get; set; }
        public string ModDir { get; set; }
        public string WorldSize { get; set; }
        public int TimeLimit { get; set; }
        public bool AutoBalance { get; set; }
        public bool RankedStatus { get; set; }
        public decimal WallTime { get; set; }
        public int ReservedSlots { get; set; }  
        public int TotalRounds { get; set; }
        public int CurrentRound { get; set; }

        public ServerInfo Clone()
        {
            return JsonConvert.DeserializeObject<ServerInfo>(JsonConvert.SerializeObject(this));
        }
    }
}
