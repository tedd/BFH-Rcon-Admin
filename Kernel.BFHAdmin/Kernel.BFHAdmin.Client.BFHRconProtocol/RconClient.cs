using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Localizer;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Commands;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol
{
    //[ImplementPropertyChanging]
    public class RconClient : NotifyPropertyBase
    {
        //http://www.battlefieldheroes.com/en/forum/showthread.php?tid=193367
        // http://www.battlefieldheroes.com/en/forum/archive/index.php/thread-331640.html
        // Lists: http://www.battlefieldheroes.com/en/forum/showthread.php?tid=132702
        internal enum RconState
        {
            Unknown,
            Ready,
            Login,
            Auth,
            AsyncCommand,
        }

        public string ServerVersion { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }
        public string Password { get; private set; }

        private int _defaultCommandTimeoutMs = 1000;
        public int DefaultCommandTimeoutMs
        {
            get { return _defaultCommandTimeoutMs; }
            set { _defaultCommandTimeoutMs = value; }
        }

        private int _statusPullDelayMs = 5000;
        public int StatusPullDelayMs
        {
            get { return _statusPullDelayMs; }
            set { _statusPullDelayMs = value; }
        }
        private int _sendDelayMs = 50;
        public int SendDelayMs
        {
            get { return _sendDelayMs; }
            set { _sendDelayMs = value; }
        }

        private Socket socket;
        private NetworkStream networkStream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private bool _connected = false;
        private bool CurrentStateWaitingForFirst = false;
        private RconState CurrentState = RconState.Unknown;
        private DateTime CurrentStateTimeout = DateTime.Now.AddSeconds(10);
        private Queue<RconQueueItem> CommandQueue = new Queue<RconQueueItem>();
        private RconQueueItem CurrentCommand;

        public PlayerListCommand PlayerListCommand
        {
            get { return _playerListCommand; }
            set
            {
                if (Equals(value, _playerListCommand)) return;
                _playerListCommand = value;
                OnPropertyChanged();
            }
        }

        public ServerInfoCommand ServerInfoCommand
        {
            get { return _serverInfoCommand; }
            set
            {
                if (Equals(value, _serverInfoCommand)) return;
                _serverInfoCommand = value;
                OnPropertyChanged();
            }
        }

        public GetAdminListCommand GetAdminListCommand
        {
            get { return _getAdminListCommand; }
            set
            {
                if (Equals(value, _getAdminListCommand)) return;
                _getAdminListCommand = value;
                OnPropertyChanged();
            }
        }

        public ClientChatBufferCommand ClientChatBufferCommand
        {
            get { return _clientChatBufferCommand; }
            set
            {
                if (Equals(value, _clientChatBufferCommand)) return;
                _clientChatBufferCommand = value;
                OnPropertyChanged();
            }
        }

        public SimpleCommand Command { get; set; }

        private List<IAmAModule> Modules = new List<IAmAModule>();

        public void AddModule(IAmAModule module)
        {
            lock (Modules)
            {
                Modules.Add(module);
                module.Register(this);
            }
        }

        public void Exec_mm_saveConfig()
        {
            Command.Exec("mm saveConfig");
        }
        public void SendMessageAll(string message)
        {
            Command.Exec("exec game.sayAll \"" + message + "\"");
        }
        public void SendMessagePlayer(string playerName, string message)
        {
            Command.Exec("exec game.sayToPlayerWithName " + playerName +" \"" + message + "\"");
        }

        #region Events
        public delegate void ConnectedDelegate(object sender);
        public event ConnectedDelegate Connected;
        protected virtual void OnConnected()
        {
            ConnectedDelegate handler = Connected;
            if (handler != null) handler(this);
        }

        public delegate void DisconnectedDelegate(object sender);
        public event DisconnectedDelegate Disconnected;
        protected virtual void OnDisconnected()
        {
            DisconnectedDelegate handler = Disconnected;
            if (handler != null) handler(this);
        }

        public delegate void AuthFailedDelegate(object sender);
        public event AuthFailedDelegate AuthFailed;
        protected virtual void OnAuthFailed()
        {
            AuthFailedDelegate handler = AuthFailed;
            if (handler != null) handler(this);
        }

        public delegate void ReceivedLineDelegate(object sender, string line);
        public event ReceivedLineDelegate ReceivedLine;
        protected virtual void OnReceivedLine(string line)
        {
            ReceivedLineDelegate handler = ReceivedLine;
            if (handler != null) handler(this, line);
        }

        public event ReceivedLineDelegate ReceivedUnhandledLine;
        protected virtual void OnReceivedUnhandledLine(string line)
        {
            ReceivedLineDelegate handler = ReceivedUnhandledLine;
            if (handler != null) handler(this, line);
        }


        #endregion

        public RconClient()
        {
            PlayerListCommand = new PlayerListCommand(this);
            ServerInfoCommand = new ServerInfoCommand(this);
            GetAdminListCommand = new GetAdminListCommand(this);
            ClientChatBufferCommand = new ClientChatBufferCommand(this);
            Command = new SimpleCommand(this);

            LoadModules();
        }

        private void LoadModules()
        {
            var files = System.IO.Directory.GetFiles(Directory.GetCurrentDirectory());
            foreach (var f in files)
            {
                var file = f.ToLower();
                if (file.Contains(".module.") && file.EndsWith(".dll"))
                {
                    var module = Kernel.Module.ModuleLoader.Load(file);
                    var classes = module.GetClasses<IAmAModule>();
                    foreach (var c in classes)
                    {
                        var m = c.CreateInstance<IAmAModule>();
                        AddModule(m);
                    }
                }
            }
        }

        public async Task Connect(string address, int port, string password)
        {
            Address = address;
            Port = port;
            Password = password;

            if (socket != null)
                throw new Exception("Can't connect on already open socket!");


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Run(() => socket.Connect(Address, Port));
            networkStream = new NetworkStream(socket);
            streamReader = new StreamReader(networkStream, Encoding.ASCII);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII);
            CommandQueue = new Queue<RconQueueItem>();
            CurrentState = RconState.Login;
            _connected = true;

            Task.Run(() => StartReadLoop());
            Task.Run(() => StartWriteLoop());
        }

        private async void StartPollLoop()
        {
            while (true)
            {
                PlayerListCommand.RefreshPlayerList();
                ServerInfoCommand.RefreshServerInfo();
                GetAdminListCommand.RefreshAdminList();
                ClientChatBufferCommand.RefreshClientChatBufferCommand();

                await Task.Delay(StatusPullDelayMs);
                if (!_connected)
                    return;
            }
        }

        private async void StartWriteLoop()
        {
            while (true)
            {
                await Task.Delay(SendDelayMs);
                if (!_connected)
                    return;

                //// If a command uses too long to return we move on
                //if (CurrentState != RconState.Ready)
                //{
                //    if (DateTime.Now > CurrentStateTimeout)
                //        CurrentState = RconState.Ready;
                //}

                if (CurrentState == RconState.Ready)
                {
                    lock (CommandQueue)
                    {

                        if (CommandQueue.Count > 0)
                        {
                            var cmd = CommandQueue.Dequeue();
                            CurrentCommand = cmd;
                            CurrentState = cmd.PutsInState;
                            CurrentStateWaitingForFirst = true;
                            CurrentStateTimeout = DateTime.Now.AddMilliseconds(DefaultCommandTimeoutMs);
                            Write(cmd.Command + "\n" + "\x002" + "done11done\n");
                        }
                    }
                }
            }
        }

        private async void StartReadLoop()
        {
            while (true)
            {
                if (!_connected)
                    return;

                string line = await streamReader.ReadLineAsync();
                if (line == null || !socket.Connected)
                {
                    Disconnect();
                    return;
                }

                Debug.WriteLine("READ: " + line);
                ProcessLine(line);

            }
        }

        //### Battlefield Heroes ModManager Rcon v8.5.
        private Regex reg_Greet = new Regex(@"^### (Battlefield Heroes ModManager Rcon .*)\.");
        //### Digest seed: MqOTwJPFbqNKzqHI
        private Regex reg_Seed = new Regex(@"^### Digest seed: (.*?)\s*$");
        // 7.2	1	16	0	0	lake	seaside_skirmish	Konge.net Battlefield Heroes Server	National	0	3	3	0	British	0	3	3	0	10	-1	gpm_ctf	bfheroes	(1024, 1024)	0	0	1	0	0	14699.5590077	0	3	1
        //private Regex reg_ServerInfo = new Regex(@"^\d+(\.\d+)?\s+\d+\s+\d+\s+\d+\s+\d+$");
        private List<string> _asyncCommandReturnBuffer;
        private PlayerListCommand _playerListCommand;
        private ServerInfoCommand _serverInfoCommand;
        private GetAdminListCommand _getAdminListCommand;
        private ClientChatBufferCommand _clientChatBufferCommand;


        private void ProcessLine(string line)
        {
            if (line == null)
                return;
            OnReceivedLine(line);

            // Command completed?
            bool currentStateDone = line.Contains("done11done");

            switch (CurrentState)
            {
                #region Login/Auth
                case RconState.Login:
                    var greet = reg_Greet.Match(line);
                    if (greet.Success)
                        Debug.WriteLine("Version: " + greet.Groups[1].Value);
                    var seed = reg_Seed.Match(line);
                    if (seed.Success)
                    {
                        CurrentState = RconState.Auth;
                        Debug.WriteLine(seed.Groups[1].Value + Password);
                        var ret = "login " + HashString(seed.Groups[1].Value + Password);
                        Write(ret + "\n");
                        return;
                    }
                    break;
                case RconState.Auth:
                    if (line.StartsWith("Authentication failed"))
                    {
                        Debug.WriteLine("Auth failed");
                        CurrentState = RconState.Unknown;
                        OnAuthFailed();
                        Disconnect();
                        return;
                    }
                    if (line.StartsWith("Authentication successful"))
                    {
                        Debug.WriteLine("Auth Success");
                        CurrentState = RconState.Ready;
                        OnConnected();
                        // Start polling
                        Task.Run(() => StartPollLoop());
                        return;
                    }
                    break;
                #endregion
                case RconState.AsyncCommand:
                    if (CurrentCommand == null)
                        break;
                    if (CurrentStateWaitingForFirst)
                    {
                        CurrentStateWaitingForFirst = false;
                        _asyncCommandReturnBuffer = new List<string>();
                    }
                    if (currentStateDone)
                    {
                        CurrentCommand.TaskCompletionSource.SetResult(_asyncCommandReturnBuffer);
                        CurrentCommand = null;
                        break;
                    }
                    _asyncCommandReturnBuffer.Add(line);
                    break;

                default:
                    OnReceivedUnhandledLine(line);
                    break;

            }


            if (currentStateDone)
                CurrentState = RconState.Ready;

        }

        internal void EnqueueCommand(RconQueueItem rconQueueItem)
        {
            lock (CommandQueue)
            {
                CommandQueue.Enqueue(rconQueueItem);
            }
        }

        //public void Send_exec_mapList_list()
        //{
        //    EnqueueCommand(new RconQueueItem("exec mapList list", RconState.bf2cc_getadminlist));
        //}

        private async void Write(string str)
        {
            Debug.WriteLine("WRITE: " + str);
            await streamWriter.WriteAsync("\x002" + str);
            await streamWriter.FlushAsync();
            //          byte[] outBuffer = Wrap4Rcon(str);
            //            await networkStream.WriteAsync(outBuffer, 0, outBuffer.Length);
            //            await networkStream.FlushAsync();
        }

        private string HashString(string Value)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.ASCII.GetBytes(Value);
            bytes = provider.ComputeHash(bytes);
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str = str + bytes[i].ToString("x2").ToLower();
            }
            return str;
        }

        public void Disconnect()
        {
            Debug.WriteLine("DISCONNECT");
            _connected = false;
            networkStream.Close(100);
            socket.Disconnect(false);
            socket = null;
            networkStream = null;
            OnDisconnected();
        }

        public void SendRaw(string text)
        {
            Write(text + "\n");
        }


    }
}


/*

Available commands: (commands marked with a * are restricted on ranked servers)
  ?: Displays list the available commands or displays help on the specified command.
  ban <playerid> <period> "<reason>": Ban a player for a specified timer period from the server with a message.
  banby <playerid> <bannedby> <period> "<reason>": Ban a player for a specified timer period from the server with a message.
  banlist: Ban a player for a specified timer period from the server with a message.
  bf2cc: Execute a bf2cc sub command.
    check: Send some check details to the client.
    clientchatbuffer: Send the clients buffered chat.
    getadminlist: Returns a list of admins (excluding the requesting admin).
    getplayerhash: Unknown
    monitor [0|1]: Register interests in all game state changes.
    pause: Pause the server if its running.
    pl: Prints player information
    sendadminchat <admin_name> <message>: Say a message to another admin.
    sendplayerchat <playerid> <message>: Send a message to a specific player.
    sendserverchat <message>: Say a message to the players as the admin.
    serverchatbuffer: Send the servers buffered chat.
    setadminname <admin_name>: Set the name of the current admin
    si: Prints server information
    togglepause: Unpause the server if its paused.
    unpause: Unpause the server if its paused.
  bm: Execute a BanManager sub command.
    addBan Key|Address "<nick>" "<period>" "<ip>" "<cdkeyhash>" "<profileid>" "<by>" "<reason>": Rcon command to add a ban.
    banPlayer <playerid> <period> "<reason>" <bannedby>: Ban a player for a specified time period from the server with a message.
    clearBans: Clear all bans
    listBans: Ban a player for a specified timer period from the server with a message.
    reloadBans: Reload bans.
    removeBan <cdkeyhash|ip> <reason>: Remove a ban.
    updateBan <cdkeyhash|ip> Key|Address "<nick>" "<period>" "<ip>" "<cdkeyhash>" "<profileid>" "<by>" "<reason>" "<datetime>": Update a ban.
  clearbans: Ban a player for a specified timer period from the server with a message.
  exec [arg1] [arg2] ... [argn]: Execute a console command on the server.
  help: Displays list the available commands or displays help on the specified command.
  kick <playerid> "<reason>": Kick a player from the server with a message.
  kicker: Execute a Kicker sub command.
    addBanPattern <word>: Add a patten which if said will get a player ban.
    addBanWord <word>: Add a word which if said will get a player ban.
    addKickPattern <word>: Add a patten which if said will get a player warn / kicked / ban.
    addKickWord <word>: Add a word which if said will get a player warn / kicked / ban.
    clearBanPatterns: Removes all ban patterns.
    clearBanWords: Removes all ban words.
    clearKickPatterns: Removes all kick patterns.
    clearKickWords: Removes all kick words.
    listWords: List the current ban / kick words.
    removeBanPattern <word>: Remove a pattern for the ban list.
    removeBanWord <word>: Remove a word for the ban list.
    removeKickPattern <word>: Remove a pattern for the kick pattern list.
    removeKickWord <word>: Remove a word for the kick list.
  list: List the players on the server.
  listlocked: Print a list of locked settings
  login <password|digest>: Authenticates with the server.
  logout: Logout from rcon.
  map <map> <gametype> [<size>]: Changes the server to a specific map
  maplist: Prints the maplist of the server
  mm: Execute a ModManager sub command.
    listModules: List the loaded modules.
    loadModule <module_name>: Load a new module.
    printRunningConfig: Write the config to ctx.
    reloadModule <module_name>: Reload and existing module.
    saveConfig: Start and existing module.
    setParam <module_name> <param> <value>: Sets a module parameter.
    shutdownModule <module_name>: Shutdown and existing module.
    startModule <module_name>: Start and existing module.
  profileid: Prints your profileid ( in game only ).
  profileids: Print a list of players and their profileid's
  unban <address|cdkeyhash>: Unban a player with an optional reason.
  users: List the all logged in rcon users.
active rcon users:

*/