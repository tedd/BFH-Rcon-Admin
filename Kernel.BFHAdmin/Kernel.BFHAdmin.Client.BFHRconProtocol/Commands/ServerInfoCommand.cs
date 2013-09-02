using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Commands
{
    public class ServerInfoCommand
    {
        public RconClient RconClient { get; private set; }
        public ServerInfo ServerInfo { get; private set; }
        private ServerInfo _lastRoundServerInfo;
        private int _lastElapsedRoundTime = 0;

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
            var qi = new RconQueueItem("bf2cc si", RconClient.RconState.AsyncCommand);
            RconClient.EnqueueCommand(qi);
            var lines = await qi.TaskCompletionSource.Task;

            foreach (var line in lines)
            {
                //var serverInfo = reg_ServerInfo.Match(line);
                //if (serverInfo.Success)
                //{
                var siSplit = line.Split(Utils.Tab);
                Debug.WriteLine("Server info for: " + siSplit[7]);
                // 7.2	1	16	0	0	lake	seaside_skirmish	Konge.net Battlefield Heroes Server	National	0	3	3	0	British	0	3	3	0	10	-1	gpm_ctf	bfheroes	(1024, 1024)	0	0	1	0	0	14699.5590077	0	3	1
                ServerInfo.Version = siSplit[0];
                ServerInfo.CurrentGameStatus = (ServerInfo.GameStatus)Enum.Parse(typeof(ServerInfo.GameStatus), siSplit[1]);
                ServerInfo.MaxPlayers = int.Parse(siSplit[2]);
                ServerInfo.Players = int.Parse(siSplit[3]);
                ServerInfo.Joining = int.Parse(siSplit[4]);
                ServerInfo.MapName = siSplit[5];
                ServerInfo.NextMapName = siSplit[6];
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

                OnServerInfoUpdated(ServerInfo);

                //if (_lastElapsedRoundTime > ServerInfo.ElapsedRoundTime)
                if (_lastElapsedRoundTime>ServerInfo.ElapsedRoundTime || _lastRoundServerInfo != null && ServerInfo.CurrentGameStatus != _lastRoundServerInfo.CurrentGameStatus)
                {
                    switch (ServerInfo.CurrentGameStatus)
                    {
                        case ServerInfo.GameStatus.Running:
                            OnRoundStart(_lastRoundServerInfo);
                            break;
                        case ServerInfo.GameStatus.EndScreen:
                            OnRoundEnd(_lastRoundServerInfo);
                            
                            break;
                    }

                }
                _lastElapsedRoundTime = ServerInfo.ElapsedRoundTime;
                //}
            }
        }

    }
}
