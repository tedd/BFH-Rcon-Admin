using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class RconClientConfig: NotifyPropertyBase
    {
        private int _pollIntervalMsAdminListCommand = 20* 1000;
        private int _pollIntervalMsClientChatBufferCommand = 3 * 1000;
        private int _pollIntervalMsPlayerListCommand = 1 * 1000;
        private int _pollIntervalMsServerInfoCommand = 1 * 1000;

        public int PollIntervalMs_AdminListCommand
        {
            get { return _pollIntervalMsAdminListCommand; }
            set
            {
                if (value == _pollIntervalMsAdminListCommand) return;
                _pollIntervalMsAdminListCommand = value;
                OnPropertyChanged();
            }
        }

        public int PollIntervalMs_ClientChatBufferCommand
        {
            get { return _pollIntervalMsClientChatBufferCommand; }
            set
            {
                if (value == _pollIntervalMsClientChatBufferCommand) return;
                _pollIntervalMsClientChatBufferCommand = value;
                OnPropertyChanged();
            }
        }

        public int PollIntervalMs_PlayerListCommand
        {
            get { return _pollIntervalMsPlayerListCommand; }
            set
            {
                if (value == _pollIntervalMsPlayerListCommand) return;
                _pollIntervalMsPlayerListCommand = value;
                OnPropertyChanged();
            }
        }

        public int PollIntervalMs_ServerInfoCommand
        {
            get { return _pollIntervalMsServerInfoCommand; }
            set
            {
                if (value == _pollIntervalMsServerInfoCommand) return;
                _pollIntervalMsServerInfoCommand = value;
                OnPropertyChanged();
            }
        }
    }
}
