using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Commands
{
    public class SimpleCommand
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public RconClient RconClient { get; private set; }

        public delegate void CommandDoneDelegate(object sender, string command, List<string> rawLines);
        public event CommandDoneDelegate CommandDone;
        protected virtual void OnCommandDone(string command, List<string> rawlines)
        {
            CommandDoneDelegate handler = CommandDone;
            if (handler != null) handler(this, command, rawlines);
        }

        public SimpleCommand(RconClient rconClient)
        {
            RconClient = rconClient;
        }

        public async Task<List<string>> Exec(string command)
        {
            log.Trace("SimpleCommand: Start exec: \"" + command + "\"");
            var qi = new RconQueueItem(command, RconClient.RconState.AsyncCommand);
            RconClient.EnqueueCommand(qi);
            var ret = await qi.TaskCompletionSource.Task;
            log.Trace("SimpleCommand: End exec: \"" + command + "\" (" + ret.Count + " lines)");
            OnCommandDone(command, ret);
            return ret;
        }
    }
}
