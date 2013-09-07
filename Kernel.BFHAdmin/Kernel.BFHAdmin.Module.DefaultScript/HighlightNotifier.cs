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
    public class HighlightNotifier : IAmAModule
    {
        private RconClient _rconClient;

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.ServerInfoCommand.RoundEnd += ServerInfoCommandOnRoundEnd;
            _rconClient.ServerInfoCommand.RoundStart += ServerInfoCommandOnRoundStart;
        }

        public void ModuleLoadComplete()
        {
        }

        private void ServerInfoCommandOnRoundStart(object sender, ServerInfo lastRoundServerInfo)
        {
            _rconClient.SendMessageAll("KD: Round start.");
        }

        private void ServerInfoCommandOnRoundEnd(object sender, ServerInfo lastRoundServerInfo)
        {
            _rconClient.SendMessageAll("KD: Round end.");
        }

    }
}
