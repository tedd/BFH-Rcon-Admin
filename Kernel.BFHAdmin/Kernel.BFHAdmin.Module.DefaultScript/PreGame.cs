using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Module.DefaultScript
{
    public class PreGame : IAmAModule
    {
        private RconClient _rconClient;

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.PlayerListCommand.PlayerJoined += PlayerListCommandOnPlayerJoined;
            _rconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
        }

        public void ModuleLoadComplete()
        {
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            if (!_rconClient.ServerInfoCommand.ServerInfo.IsPregame)
                return;

            _rconClient.SendMessageAll("KD: Pre-game starting. In pre-game widget use is FREE.");
        }

        private void PlayerListCommandOnPlayerJoined(object sender, Player player)
        {
            if (!_rconClient.ServerInfoCommand.ServerInfo.IsPregame)
                return;

            var playerCount = _rconClient.ServerInfoCommand.ServerInfo.Players + 1;
            _rconClient.SendMessageAll("KD: " + player.Name + " just joined the fight. You are now " + playerCount + " players.");
            _rconClient.SendMessagePlayer(player, "KD: Welcome to pre-game. In this mode widget use is FREE.");
            _rconClient.SendMessagePlayer(player, "KD: Pre-game starts when there are less than two players on either team.");
            _rconClient.SendMessagePlayer(player, "KD: Enjoy!");

        }
    }
}
