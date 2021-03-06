﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;
using NLog;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Commands
{
    public class ServerInfoCommand: NotifyPropertyBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public RconClient RconClient { get; private set; }

        private bool _inRound { get; set; }
        
        [ExpandableObject()]
        public ServerInfo ServerInfo
        {
            get { return _serverInfo; }
            private set
            {
                if (Equals(value, _serverInfo)) return;
                _serverInfo = value;
                OnPropertyChanged();
            }
        }

        private ServerInfo _lastRoundServerInfo;
        private int _lastElapsedRoundTime = 0;
        private ServerInfo _serverInfo;

        public delegate void RoundStartDelegate(object sender, ServerInfo lastRoundServerInfo);
        public event RoundStartDelegate RoundStart;
        protected virtual void OnRoundStart(ServerInfo lastroundserverinfo)
        {
            RoundStartDelegate handler = RoundStart;
            if (handler != null) handler(this, lastroundserverinfo);
        }
        public delegate void RoundEndDelegate(object sender, ServerInfo lastRoundServerInfo);
        public event RoundEndDelegate RoundEnd;
        protected virtual void OnRoundEnd(ServerInfo lastroundserverinfo)
        {
            RoundEndDelegate handler = RoundEnd;
            if (handler != null) handler(this, lastroundserverinfo);
        }


        public delegate void ServerInfoUpdatedDelegate(object sender, ServerInfo serverInfo);
        public event ServerInfoUpdatedDelegate ServerInfoUpdated;
        protected virtual void OnServerInfoUpdated(ServerInfo serverinfo)
        {
            ServerInfoUpdatedDelegate handler = ServerInfoUpdated;
            if (handler != null) handler(this, serverinfo);
        }



        public ServerInfoCommand(RconClient rconClient)
        {
            RconClient = rconClient;
            ServerInfo = new ServerInfo();
        }

        public async void RefreshServerInfo()
        {
            log.Trace("RefreshServerInfo(): Start");
            var qi = new RconQueueItem("bf2cc si", RconClient.RconState.AsyncCommand);
            RconClient.EnqueueCommand(qi);
            var lines = await qi.TaskCompletionSource.Task;

            foreach (var line in lines)
            {
                //var serverInfo = reg_ServerInfo.Match(line);
                //if (serverInfo.Success)
                //{
                var siSplit = line.Split(Utils.Tab);
                log.Trace("RefreshServerInfo(): Server info for: " + siSplit[7]);
                // 7.2	1	16	0	0	lake	seaside_skirmish	Konge.net Battlefield Heroes Server	National	0	3	3	0	British	0	3	3	0	10	-1	gpm_ctf	bfheroes	(1024, 1024)	0	0	1	0	0	14699.5590077	0	3	1
                ServerInfo.Version = siSplit[0];
                ServerInfo.CurrentGameStatus = (ServerInfo.GameStatus)Enum.Parse(typeof(ServerInfo.GameStatus), siSplit[1]);
                ServerInfo.MaxPlayers = int.Parse(siSplit[2]);
                ServerInfo.Players = int.Parse(siSplit[3]);
                ServerInfo.Joining = int.Parse(siSplit[4]);
                ServerInfo.MapName = siSplit[5];
                ServerInfo.MapInfo = RconClient.GetMapInfoFromName(siSplit[5]);
                ServerInfo.NextMapName = siSplit[6];
                ServerInfo.NextMapInfo = RconClient.GetMapInfoFromName(siSplit[6]);
                ServerInfo.ServerName = siSplit[7];
                ServerInfo.Team1.Name = siSplit[8];
                ServerInfo.Team1.TicketState = int.Parse(siSplit[9]);
                ServerInfo.Team1.StartTickets = int.Parse(siSplit[10]);
                ServerInfo.Team1.Tickets = int.Parse(siSplit[11]);
                ServerInfo.Unknown0 = siSplit[12];
                ServerInfo.Team2.Name = siSplit[13];
                ServerInfo.Team2.TicketState = int.Parse(siSplit[14]);
                ServerInfo.Team2.StartTickets = int.Parse(siSplit[15]);
                ServerInfo.Team2.Tickets = int.Parse(siSplit[16]);
                ServerInfo.Unknown1 = siSplit[17];
                ServerInfo.ElapsedRoundTime = int.Parse(siSplit[18]);
                ServerInfo.RemainingTime = int.Parse(siSplit[19]);
                ServerInfo.GameMode = siSplit[20];
                ServerInfo.GameModeType = (ServerInfo.GameType)Enum.Parse(typeof(ServerInfo.GameType), siSplit[20]);
                ServerInfo.ModDir = siSplit[21];
                ServerInfo.WorldSize = siSplit[22];
                ServerInfo.TimeLimit = int.Parse(siSplit[23]);
                ServerInfo.AutoBalance = Utils.BoolParse(siSplit[24]);
                ServerInfo.RankedStatus = Utils.BoolParse(siSplit[25]);
                ServerInfo.Team1.Count = int.Parse(siSplit[26]);
                ServerInfo.Team2.Count = int.Parse(siSplit[27]);
                ServerInfo.WallTime = decimal.Parse(siSplit[28], NumberStyles.Float, CultureInfo.InvariantCulture);
                ServerInfo.ReservedSlots = int.Parse(siSplit[29]);
                ServerInfo.TotalRounds = int.Parse(siSplit[30]);
                ServerInfo.CurrentRound = int.Parse(siSplit[31]);

                ServerInfo.IsPregame = !(ServerInfo.Team1.Count > 1 && ServerInfo.Team2.Count > 1);

                OnServerInfoUpdated(ServerInfo);

                // Round ends when:
                // gpm_tdm  - Team tickets reach 0
                // gpm_ctf  - Team tickets reach 0
                // gpm_hoth - Team tickets reach 0

                if (_lastElapsedRoundTime > ServerInfo.ElapsedRoundTime)
                {
                    // Round time has decreased, new round
                }
                if (ServerInfo.CurrentGameStatus != ServerInfo.GameStatus.EndScreen && ServerInfo.Team1.Tickets > 0 && ServerInfo.Team2.Tickets > 0)
                {
                    if (!_inRound && _lastElapsedRoundTime > ServerInfo.ElapsedRoundTime)
                    {
                        // Round starting (and we have seen a previous round)
                        OnRoundStart(_lastRoundServerInfo);
                    }
                    _inRound = true;
                }
                else
                {
                    if (_inRound)
                    {
                        // Round done
                        OnRoundEnd(_lastRoundServerInfo);
                    }
                    _inRound = false;
                }

                //if (_lastElapsedRoundTime > ServerInfo.ElapsedRoundTime)
                if (_lastElapsedRoundTime>ServerInfo.ElapsedRoundTime || (_lastRoundServerInfo != null && ServerInfo.CurrentGameStatus != _lastRoundServerInfo.CurrentGameStatus))
                {
                    switch (ServerInfo.CurrentGameStatus)
                    {
                        case ServerInfo.GameStatus.Running:
//                            OnRoundStart(_lastRoundServerInfo);
                            break;
                        case ServerInfo.GameStatus.EndScreen:
                            // Doesn't work
                            break;
                    }

                }
                _lastElapsedRoundTime = ServerInfo.ElapsedRoundTime;
                //}
                log.Trace("RefreshServerInfo(): End");
            }
        }

    }
}
