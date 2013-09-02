using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol
{
    internal class RconQueueItem
    {
        public string Command;
        public RconClient.RconState PutsInState; // To be obsolete
        public TaskCompletionSource<List<string>> TaskCompletionSource = new TaskCompletionSource<List<string>>();

        public RconQueueItem() {}
        public RconQueueItem(string command, RconClient.RconState putsInState)
        {
            Command = command;
            PutsInState = putsInState;
        }
    }
}
