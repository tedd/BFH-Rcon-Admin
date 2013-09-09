using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using NLog;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Commands
{
    public class PlayerListCommand
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        //public Dictionary<string, Player> Players = new Dictionary<string, Player>();
        public RconClient RconClient { get; private set; }
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        public PlayerListCommand(RconClient rconClient)
        {
            RconClient = rconClient;
        }

        #region Events
        public delegate void PlayerJoinedDelegate(object sender, Player player);
        public event PlayerJoinedDelegate PlayerJoined;
        protected virtual void OnPlayerJoined(Player player)
        {
            PlayerJoinedDelegate handler = PlayerJoined;
            if (handler != null) handler(this, player);
        }

        public delegate void PlayerLeftDelegate(object sender, Player player);
        public event PlayerLeftDelegate PlayerLeft;
        protected virtual void OnPlayerLeft(Player player)
        {
            PlayerLeftDelegate handler = PlayerLeft;
            if (handler != null) handler(this, player);
        }
        public delegate void PlayerUpdatedDelegate(object sender, Player player);
        public event PlayerUpdatedDelegate PlayerUpdated;
        protected virtual void OnPlayerUpdated(Player player)
        {
            PlayerUpdatedDelegate handler = PlayerUpdated;
            if (handler != null) handler(this, player);
        }

        public delegate void PlayerUpdateStartDelegate(object sender);
        public event PlayerUpdateStartDelegate PlayerUpdateStart;
        protected virtual void OnPlayerUpdateStart()
        {
            PlayerUpdateStartDelegate handler = PlayerUpdateStart;
            if (handler != null) handler(this);
        }

        public delegate void PlayerUpdateDoneDelegate(object sender);
        public event PlayerUpdateDoneDelegate PlayerUpdateDone;
        protected virtual void OnPlayerUpdateDone()
        {
            PlayerUpdateDoneDelegate handler = PlayerUpdateDone;
            if (handler != null) handler(this);
        }

        #endregion

        public async void RefreshPlayerList()
        {
            log.Trace("RefreshPlayerList(): Start");
            var qi = new RconQueueItem("bf2cc pl", RconClient.RconState.AsyncCommand);
            RconClient.EnqueueCommand(qi);
            var lines = await qi.TaskCompletionSource.Task;
            int added = 0, removed = 0;
            DateTime updateTimestamp = DateTime.Now;
            // Process result
            OnPlayerUpdateStart();
            var _plPlayerDiff = new Dictionary<Player, bool>();
            foreach (var line in lines)
            {

                var plSplit = line.Split(Utils.Tab);
                var playerName = plSplit[1];
                //Debug.WriteLine("Player info for: " + playerName);
                Player p = GetPlayer(plSplit[1]);
                bool newPlayer = false;
                if (p == null)
                {
                    newPlayer = true;
                    p = new Player();
                    lock (_players)
                    {
                        _players.Add(playerName, p);
                        added++;
                    }
                }
                _plPlayerDiff.Add(p, true);
                p.Index = plSplit[0];
                p.Name = plSplit[1];
                p.TeamId = int.Parse(plSplit[2]);
                // Link to correct team object
                if (p.TeamId == 1)
                    p.Team = RconClient.ServerInfoCommand.ServerInfo.Team1;
                if (p.TeamId == 2)
                    p.Team = RconClient.ServerInfoCommand.ServerInfo.Team2;

                p.Ping = int.Parse(plSplit[3]);
                p.IsConnected = Utils.BoolParse(plSplit[4]);
                p.IsValid = Utils.BoolParse(plSplit[5]);
                p.IsRemote = Utils.BoolParse(plSplit[6]);
                p.IsAIPlayer = Utils.BoolParse(plSplit[7]);
                p.IsAlive = Utils.BoolParse(plSplit[8]);
                p.IsManDown = Utils.BoolParse(plSplit[9]);
                p.ProfileId = int.Parse(plSplit[10]);
                p.IsFlagholder = Utils.BoolParse(plSplit[11]);
                p.Suicide = int.Parse(plSplit[12]);
                p.TimeToSpawn = decimal.Parse(plSplit[13], NumberStyles.Float, CultureInfo.InvariantCulture);
                p.SquadId = int.Parse(plSplit[14]);
                p.IsSquadLeader = Utils.BoolParse(plSplit[15]);
                p.IsCommander = Utils.BoolParse(plSplit[16]);
                p.SpawnGroup = int.Parse(plSplit[17]);
                p.Address = plSplit[18];
                p.Score.DamageAssists = int.Parse(plSplit[19]);
                p.Score.PassengerAssists = int.Parse(plSplit[20]);
                p.Score.TargetAssists = int.Parse(plSplit[21]);
                p.Score.Revives = int.Parse(plSplit[22]);
                p.Score.TeamDamages = int.Parse(plSplit[23]);
                p.Score.TeamVehicleDamages = int.Parse(plSplit[24]);
                p.Score.CpCaptures = int.Parse(plSplit[25]);
                p.Score.CpDefends = int.Parse(plSplit[26]);
                p.Score.CpAssists = int.Parse(plSplit[27]);
                p.Score.CpNeutralizes = int.Parse(plSplit[28]);
                p.Score.CpNeutralizeAssists = int.Parse(plSplit[29]);
                p.Score.Suicides = int.Parse(plSplit[30]);
                p.Score.Kills = int.Parse(plSplit[31]);
                p.Score.TeamKills = int.Parse(plSplit[32]);
                p.VehicleType = int.Parse(plSplit[33]);
                p.Kit = plSplit[34]; //kit.templateName
                p.ConnectedAt = decimal.Parse(plSplit[35], NumberStyles.Float, CultureInfo.InvariantCulture);
                //ki.connectedAt
                p.Score.Deaths = int.Parse(plSplit[36]);
                p.Score.Score = int.Parse(plSplit[37]);
                p.VehicleName = plSplit[38];
                p.Score.Rank = int.Parse(plSplit[39]);
                p.PositionString = plSplit[40];
                p.IdleTime = int.Parse(plSplit[41]); //ki.idleTime
                p.Cdkeyhash = plSplit[42];
                p.TkData_Punished = Utils.BoolParse(plSplit[43]); //tkData.punished
                p.TkData_TimesPunished = int.Parse(plSplit[44]); //tkData.timesPunished
                p.TkData_TimesForgiven = int.Parse(plSplit[45]); //tkData.timesForgiven
                p.Vip = Utils.BoolParse(plSplit[46]);
                p.NucleusId = Int64.Parse(plSplit[47]);
                p.LastUpdate = updateTimestamp;
                if (newPlayer)
                {
                    RconClient.ServerInfoCommand.ServerInfo.Team1.AddPlayer(p);
                    OnPlayerJoined(p);
                }

                OnPlayerUpdated(p);
            }

            foreach (var kvp in new Dictionary<string, Player>(_players))
            {
                if (!_plPlayerDiff.ContainsKey(kvp.Value))
                {
                    _players.Remove(kvp.Key);
                    RconClient.ServerInfoCommand.ServerInfo.Team1.RemovePlayer(kvp.Value);
                    OnPlayerLeft(kvp.Value);
                    removed++;
                }
            }

            OnPlayerUpdateDone();

            log.Trace("RefreshPlayerList(): End (" + lines.Count + " players, " + added + " new, " + removed + " removed)");

        }


        public Player GetPlayer(string playerName)
        {
            lock (_players)
            {
                if (_players.ContainsKey(playerName))
                    return _players[playerName];
            }
            return null;
        }
        public IEnumerable<Player> GetPlayers()
        {
            List<Player> players;
            lock (_players)
            {
                players = new List<Player>(_players.Values);
            }
            foreach (var player in players)
            {
                yield return player;
            }
        }

    }
}
