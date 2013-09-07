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
using System.Windows.Documents;
using System.Windows.Markup.Localizer;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Commands;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Interfaces;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Utils;
using NLog;
using PropertyChanging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol
{
    //[ImplementPropertyChanging]
    public class RconClient : NotifyPropertyBase
    {
        //http://www.battlefieldheroes.com/en/forum/showthread.php?tid=193367
        // http://www.battlefieldheroes.com/en/forum/archive/index.php/thread-331640.html
        // Lists: http://www.battlefieldheroes.com/en/forum/showthread.php?tid=132702
        private static Logger log = LogManager.GetCurrentClassLogger();
        internal enum RconState
        {
            Unknown,
            Ready,
            Login,
            Auth,
            AsyncCommand,
        }

        private string _doneCommand;

        public string ServerVersion { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }
        public string Password { get; private set; }

        public bool DebugProtocolData = true;

        //private int _defaultCommandTimeoutMs = 1000;
        //public int DefaultCommandTimeoutMs
        //{
        //    get { return _defaultCommandTimeoutMs; }
        //    set { _defaultCommandTimeoutMs = value; }
        //}

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
        //private DateTime CurrentStateTimeout = DateTime.Now.AddSeconds(10);
        private Queue<RconQueueItem> CommandQueue = new Queue<RconQueueItem>();
        private RconQueueItem CurrentCommand;
        private Action _pollingTimerCancelAction;

        public RconClientConfig Config
        {
            get { return _config; }
            set
            {
                if (Equals(value, _config)) return;
                _config = value;
                OnPropertyChanged();
            }
        }

        [ExpandableObject()]
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

        [ExpandableObject()]
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
        //[ExpandableObject()]
        //public GetAdminListCommand GetAdminListCommand
        //{
        //    get { return _getAdminListCommand; }
        //    set
        //    {
        //        if (Equals(value, _getAdminListCommand)) return;
        //        _getAdminListCommand = value;
        //        OnPropertyChanged();
        //    }
        //}
        [ExpandableObject()]
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

        private List<IAmAModule> _modules = new List<IAmAModule>();

        public IEnumerable<IAmAModule> Modules
        {
            get
            {
                List<IAmAModule> modules;
                lock (_modules)
                {
                    modules = new List<IAmAModule>(_modules);
                }
                foreach (var module in modules)
                {
                    yield return module;
                }
            }
        }
        public void AddModule(IAmAModule module)
        {
            AddModuleInternal(module);
            module.ModuleLoadComplete();
        }
        private void AddModuleInternal(IAmAModule module)
        {
            var moduleName = module.GetType().FullName;
            log.Debug("RconClient registering module: " + moduleName);
            lock (_modules)
            {
                _modules.Add(module);
                OnPropertyChanged("Modules");
            }
            try
            {
                module.Register(this);
            }
            catch (Exception exception)
            {
                log.ErrorException("Exception: RconClient registering module \"" + moduleName + "\" failed: ", exception);
            }
        }

        public Task<List<string>> Exec_mm_saveConfig()
        {
            return Command.Exec("mm saveConfig");
        }
        public Task<List<string>> SendMessageAll(string message)
        {
            return Command.Exec("exec game.sayAll \"" + message + "\"");
        }
        public Task<List<string>> SendMessagePlayer(string playerName, string message)
        {
            return Command.Exec("exec game.sayToPlayerWithName " + playerName + " \"" + message + "\"");
        }
        public Task<List<string>> SendMessagePlayer(Player player, string message)
        {
            return SendMessagePlayer(player.Name, message);
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
            log.Debug("RconClient startup");
            Random rnd = new Random();
            _doneCommand = "$^!(\"done."+rnd.Next(0,int.MaxValue) + ".done\")!^$"; // Some random string that won't show up in any of our data
            PlayerListCommand = new PlayerListCommand(this);
            ServerInfoCommand = new ServerInfoCommand(this);
            //GetAdminListCommand = new GetAdminListCommand(this);
            ClientChatBufferCommand = new ClientChatBufferCommand(this);
            Command = new SimpleCommand(this);

            LoadModules();
        }

        private void LoadModules()
        {
            var dir = Directory.GetCurrentDirectory();
            log.Debug("LoadModules(): Start: Scanning directory for files matching \"*.module.*dll\": " + dir);
            List<Task> loadTasks = new List<Task>();
            // Get files in current directory
            var files = System.IO.Directory.GetFiles(dir);
            foreach (var f in files)
            {
                // Check if the filename indicates they are a module
                var file = f.ToLower();
                if (file.Contains(".module.") && file.EndsWith(".dll"))
                {
                    log.Debug("LoadModules(): Async loading module: \"" + file + "\"");
                    // Attempt to load file
                    var task = Task.Run(
                        () =>
                        {

                            Module.Module module = null;
                            try
                            {
                                module = Kernel.Module.ModuleLoader.Load(file);
                                if (module == null)
                                    throw new Exception("module returnes is null.");

                            }
                            catch (Exception exception)
                            {
                                log.ErrorException("LoadModules(): Exception: Loading module \"" + file + "\" failed: ", exception);
                            }
                            // ReSharper disable PossibleNullReferenceException
                            var classes = module.GetClasses<IAmAModule>();
                            // ReSharper restore PossibleNullReferenceException
                            foreach (var c in classes)
                            {
                                try
                                {
                                    var m = c.CreateInstance<IAmAModule>();
                                    AddModuleInternal(m);
                                }
                                catch (Exception exception)
                                {
                                    log.ErrorException("LoadModules(): Exception: Instancing class \"" + c.FullName + "\" in \"" + file + "\" failed: ", exception);
                                }
                            }


                        });
                    loadTasks.Add(task);
                }
            }

            // Wait for all modules to load
            foreach (var t in loadTasks)
            {
                // Should be safe without try-catch since we catch internally (above here)
                t.Wait();
            }

            // Send "ModuleLoadComplete" once all modules has been loaded
            lock (_modules)
            {
                foreach (var m in _modules)
                {
                    try
                    {
                        m.ModuleLoadComplete();
                    }
                    catch (Exception exception)
                    {
                        log.ErrorException("LoadModules(): Exception: Calling ModuleLoadComplete() on class \"" + m.GetType() + "\": ", exception);
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

            log.Info(string.Format("Connect(): Address: {0}, Port: {1}", Address, Port));

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Run(() => socket.Connect(Address, Port));
            //socket.Conn)
            // TODO: Change to "TcpClient"?

            networkStream = new NetworkStream(socket);
            streamReader = new StreamReader(networkStream, Encoding.ASCII);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII);
            CommandQueue = new Queue<RconQueueItem>();
            CurrentState = RconState.Login;
            _connected = true;
            log.Info("Connect(): Connected!");

            Task.Run(() => ReadLoop());
            Task.Run(() => WriteLoop());
        }

        private async void StartPollingTimers()
        {
            log.Trace("StartPollingTimers(): Start.");

            var c1 = new CancellationTokenSource();
            var c2 = new CancellationTokenSource();
            var c3 = new CancellationTokenSource();
            var c4 = new CancellationTokenSource();
            _pollingTimerCancelAction = new Action(() =>
                              {
                                  c1.Cancel();
                                  c2.Cancel();
                                  c3.Cancel();
                                  c4.Cancel();
                              });

            PeriodicTaskFactory.Start(() => PlayerListCommand.RefreshPlayerList(), cancelToken: c1.Token, intervalInMilliseconds: this.Config.PollIntervalMs_PlayerListCommand, delayInMilliseconds: 1000, synchronous: false);
            PeriodicTaskFactory.Start(() => ServerInfoCommand.RefreshServerInfo(), cancelToken: c2.Token, intervalInMilliseconds: this.Config.PollIntervalMs_ServerInfoCommand, delayInMilliseconds: 1000, synchronous: false);
            PeriodicTaskFactory.Start(() => ClientChatBufferCommand.RefreshClientChatBufferCommand(), cancelToken: c3.Token, intervalInMilliseconds: this.Config.PollIntervalMs_ClientChatBufferCommand, delayInMilliseconds: 1000, synchronous: false);
            //PeriodicTaskFactory.Start(() => GetAdminListCommand.RefreshAdminList(), cancelToken: c4.Token, intervalInMilliseconds: this.Config.PollIntervalMs_AdminListCommand, delayInMilliseconds: 1000, synchronous: false);
            //while (true)
            //{
            //    PlayerListCommand.RefreshPlayerList();
            //    ServerInfoCommand.RefreshServerInfo();
            //    ClientChatBufferCommand.RefreshClientChatBufferCommand();
            //    //GetAdminListCommand.RefreshAdminList();

            //    await Task.Delay(StatusPullDelayMs);
            //    if (!_connected)
            //        break;
            //}
            log.Trace("StartPollingTimers(): End");
        }

        private async void WriteLoop()
        {
            log.Trace("WriteLoop(): Start. Interval: " + SendDelayMs + " ms");
            while (true)
            {
                await Task.Delay(SendDelayMs);
                if (!_connected)
                    break;

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
                            //CurrentStateTimeout = DateTime.Now.AddMilliseconds(DefaultCommandTimeoutMs);
                            log.Trace("WriteLoop(): Sending command: \"" + cmd.Command + "\"");
                            Write(cmd.Command + "\n" + "\x002" + _doneCommand +"\n");
                        }
                    }
                }
            }
            log.Trace("WriteLoop(): End");
        }

        private async void ReadLoop()
        {
            log.Trace("ReadLoop(): Start");
            while (true)
            {
                if (!_connected)
                    return;

                string line = await streamReader.ReadLineAsync();
                if (line == null || !socket.Connected)
                {
                    Disconnect();
                    break;
                }

                if (DebugProtocolData)
                    log.Trace("ReadLoop(): Read: \"" + line + "\"");
                ProcessLine(line);

            }
            log.Trace("ReadLoop(): End");

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
        private RconClientConfig _config = new RconClientConfig();


        private void ProcessLine(string line)
        {
            if (line == null)
                return;

            if (DebugProtocolData)
                log.Trace("ProcessLine(): " + line);

            OnReceivedLine(line);

            // Command completed?
            bool currentStateDone = line.Contains("unknown command: '"+ _doneCommand + "'");

            switch (CurrentState)
            {
                #region Login/Auth
                case RconState.Login:
                    var greet = reg_Greet.Match(line);
                    if (greet.Success)
                    {
                        ServerVersion = greet.Groups[1].Value;
                        log.Debug("ProcessLine(): Server version: " + ServerVersion);
                    }
                    var seed = reg_Seed.Match(line);
                    if (seed.Success)
                    {
                        CurrentState = RconState.Auth;
                        var ret = "login " + HashString(seed.Groups[1].Value + Password);
                        log.Trace("ProcessLine(): Sending login info: " + ret);
                        Write(ret + "\n");
                        return;
                    }
                    break;
                case RconState.Auth:
                    if (line.StartsWith("Authentication failed"))
                    {
                        log.Error("Authentication failed!");
                        CurrentState = RconState.Unknown;
                        OnAuthFailed();
                        Disconnect();
                        return;
                    }
                    if (line.StartsWith("Authentication successful"))
                    {
                        log.Debug("ProcessLine(): Authentication successful. We are logged in.");
                        CurrentState = RconState.Ready;
                        OnConnected();
                        // Start polling
                        StartPollingTimers();
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
                        var cc = CurrentCommand;
                        CurrentCommand = null;
                        // Special state - we need to free up for next command, return data to async waiting and completely exit (so we don't re-set CurrentState when new command has started)
                        CurrentState = RconState.Ready;
                        cc.TaskCompletionSource.SetResult(_asyncCommandReturnBuffer);
                        return;
                    }
                    _asyncCommandReturnBuffer.Add(line);
                    break;

                default:
                    log.Trace("ProcessLine(): Unhandled line: " + line);
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
                log.Trace("EnqueueCommand(): " + rconQueueItem.Command);
                CommandQueue.Enqueue(rconQueueItem);
            }
        }

        //public void Send_exec_mapList_list()
        //{
        //    EnqueueCommand(new RconQueueItem("exec mapList list", RconState.bf2cc_getadminlist));
        //}

        private async void Write(string str)
        {
            log.Trace("Write(): " + str);
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
            log.Trace("Disconnect(): Start");
            if (_pollingTimerCancelAction != null)
            {
                _pollingTimerCancelAction.Invoke();
                _pollingTimerCancelAction = null;
            }
            _connected = false;
            networkStream.Close(100);
            socket.Disconnect(false);
            socket = null;
            networkStream = null;
            log.Trace("Disconnect(): End");
            OnDisconnected();
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