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
    public class PlayerCommands : IAmAModule
    {
        private RconClient _rconClient;

        public void Register(RconClient rconClient)
        {
            _rconClient = rconClient;
            _rconClient.ClientChatBufferCommand.ChatLineReceived += ClientChatBufferCommandOnChatLineReceived;
        }

        private void ClientChatBufferCommandOnChatLineReceived(object sender, ChatHistoryItem chatHistoryItem)
        {
            var msg = chatHistoryItem.Message.ToLower();
            if (msg.StartsWith("/") || msg.StartsWith("|"))
            {
                if (msg == "/deaths" || msg == "|deaths")
                {
                    var player = _rconClient.PlayerListCommand.GetPlayer(chatHistoryItem.From);
                    _rconClient.SendMessagePlayer(player, "Your kills/deaths this round: " + player.Score.Kills + "/" + player.Score.Deaths);
                }
            }
        }

        public void ModuleLoadComplete()
        {
            
        }

    }
}
