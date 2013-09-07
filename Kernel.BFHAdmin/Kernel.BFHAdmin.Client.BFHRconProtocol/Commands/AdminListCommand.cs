using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using NLog;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Commands
{
    public class GetAdminListCommand
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public RconClient RconClient { get; private set; }
        public List<AdminListItem> AdminList { get; private set; }
        //37.191.131.64:60095	Admin
        private Regex reg_Admin = new Regex(@"^(?<ipport>(?<ip>\d+\.\d+\.\d+\.\d+):(?<port>\d+))\s+(?<name>.*?)\s*$");

        public GetAdminListCommand(RconClient rconClient)
        {
            RconClient = rconClient;
            AdminList = new List<AdminListItem>();

            log.Debug("Loaded");
        }

        public async void RefreshAdminList()
        {
            log.Trace("RefreshAdminList(): Start");
            var qi = new RconQueueItem("bf2cc getadminlist", RconClient.RconState.AsyncCommand);
            RconClient.EnqueueCommand(qi);
            var lines = await qi.TaskCompletionSource.Task;

            AdminList.Clear();
            foreach (var line in lines)
            {
                var admins = reg_Admin.Match(line);
                if (admins.Success)
                {
                    var name = admins.Groups["name"].Value;
                    var ip = admins.Groups["ip"].Value;
                    var port = int.Parse(admins.Groups["port"].Value);
                    Debug.WriteLine("Admin: " + name + " -> " + ip + ":" + port);
                    AdminList.Add(new AdminListItem() { Name=name, Address=ip, Port=port});
                }
            }
            log.Trace("RefreshAdminList(): End (" + lines.Count + " admins)");
        }
    }
}
