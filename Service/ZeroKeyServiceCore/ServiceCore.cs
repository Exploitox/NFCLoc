using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ZeroKey.Service.Common;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Management;
using SuperSimpleTcp;
using ZeroKey.Libraries;

namespace ZeroKey.Service.Core
{
    public class ServiceCore
    {
        private static bool _debug = true;
        private static string _logFile = "";
        private CompositionContainer _container;
        private static readonly string AppPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly()?.Location ?? string.Empty).DirectoryName;
        private ServiceState _state = ServiceState.Stopped;
        public static readonly SystemState SystemStatus = new SystemState();
        private Config _applicationConfiguration;

        private TcpListener _credentialListener;
        private Thread _credentialListenThread;
        private TcpListener _registrationListener;
        private Thread _registrationListenThread;

        SimpleTcpClient client;
        private int trys = 0;
        private bool IsConnected = false;
        private bool GotConfig = false;

        private bool _runListenLoops = false;
        //private List<Client> Connections = new List<Client>();

        //[DllImport("WinAPIWrapper", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int PCSC_GetID([In, Out] IntPtr id, [In, Out] IntPtr err);

        [ImportMany] private IEnumerable<Lazy<IZeroKeyServicePlugin>> _plugins;

        private Thread _readerThread = null;

        public ServiceCore(bool isDebug)
        {
            _debug = isDebug;
            if (isDebug)
                try
                {
                    if (File.Exists(AppPath + "\\log-old.txt"))
                    {
                        File.Delete(AppPath + "\\log-old.txt");
                    }
                    if (File.Exists(AppPath + "\\log.txt"))
                    {
                        File.Move(AppPath + "\\log.txt", AppPath + "\\log-old.txt");
                    }
                }
                catch { }
        }

        private void InitLog()
        {
            string currentDateTime = DateTime.Now.ToString("ddMMyy-HHmmss");
            _logFile = Path.Combine(AppPath, "logs", $"log-{currentDateTime}.txt");
            if (!Directory.Exists(Path.Combine(AppPath, "logs")))
            {
                Directory.CreateDirectory(Path.Combine(AppPath, "logs"));
            }
            File.Create(_logFile).Close();
        }

        // load plugins
        public void LoadPlugins()
        {
            Log("Plugins loading");
            try
            {
                // load extension catalog
                var catalog = new AggregateCatalog();
                //Adds all the parts found in the same assembly as the Program class
                catalog.Catalogs.Add(new DirectoryCatalog(AppPath + @"\Plugins\"));
                Log("Plugin directory: " + AppPath + @"\Plugins\");
                //Create the CompositionContainer with the parts in the catalog
                _container = new CompositionContainer(catalog);

                //Fill the imports of this object
                try
                {
                    this._container.ComposeParts(this);
                }
                catch
                {
                    //LogEntry(ex, "Unable to load extensions");
                }
                foreach (Lazy<IZeroKeyServicePlugin> plugin in _plugins)
                {
                    try
                    {
                        //Log("Starting plugin");
                        plugin.Value.PluginLoad();
                        Log("Plugin " + plugin.Value.GetPluginName() + " passed Load event");
                    }
                    catch
                    {
                        Log("Plugin threw an exception on Load event");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception loading plugins: " + ex.Message);
            }
            Log(_plugins.Count().ToString() + " Plugin(s) loaded");
            if (_plugins.Any()) return;
            
            // No plugins loaded
            Log("No plugins loaded. Closing service ...");
            Stop();
        }

        // thread to monitor nfc reader
        private void ScanForId()
        {
            Log("NFC Reading started");
            //Thread.Sleep(10000);
            var currentTokens = new List<string>();
            var sCardContext = new SCardContext();
            var serialContext = new SerialContext();
            // basically keep running until we're told to stop
            while(_state == ServiceState.Starting || _state == ServiceState.Running)
            {
                // start dll and just call it 
                //IntPtr idloc = Marshal.AllocHGlobal(100);
                //IntPtr errloc = Marshal.AllocHGlobal(100);
                //int len = PCSC_GetID(idloc, errloc);
                //string error = Marshal.PtrToStringAnsi(errloc);
                //string id = "";
                //if(len > 0)
                //    id = Marshal.PtrToStringAnsi(idloc, len);
                ////else
                ////    Log("Read error: " + error);

                //Marshal.FreeHGlobal(idloc);
                //Marshal.FreeHGlobal(errloc);
                var ls = sCardContext.GetIds();
                ls = ls.Concat(serialContext.GetIds()).ToList();
                foreach (var id in ls)
                {
                    //string id = ls.FirstOrDefault() ?? "";
                    // check the id of the token

                    if (!currentTokens.Contains(id) && id != "")
                    {
                        Log("NFCTagDownEvent");
                        // we just got a new token (state change)

                        // load parameters from config
                        if (SystemStatus.AwaitingToken)
                        {
                            // this is where we capture it and show the next screen
                            if (SystemStatus.RegistrationClient != null)
                            {
                                var c = SystemStatus.RegistrationClient;
                                ServiceCommunication.SendNetworkMessage(ref c, JsonConvert.SerializeObject(new NetworkMessage() { Type = MessageType.Token, Token = id }));
                                SystemStatus.RegistrationClient = c;
                            }
                        }
                        else
                        {
                            // check config
                            try
                            {
                                foreach (var u in _applicationConfiguration.Users)
                                {
                                    var hashedToken = Crypto.Hash(Crypto.Hash(id) + u.Salt);
                                    foreach (var e in u.Events)
                                    {
                                        if (hashedToken != e.Token) continue;
                                        foreach (var plugin in _plugins)
                                        {
                                            if (plugin.Value.GetPluginName() != e.PluginName) continue;
                                            plugin.Value.NcfRingDown(id, e.Parameters, SystemStatus);

                                            Log("Plugin " + plugin.Value.GetPluginName() + " passed TagDown event");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error while checking config: {ex.Message}");
                            }
                        }
                    }
                    currentTokens.Remove(id);
                }
                foreach (var id in currentTokens)
                {
                    Log("NFCTagUpEvent");
                    // we just lost the token (state change)
                    if (SystemStatus.AwaitingToken)
                    {
                        // they just lifted it. reset this
                        SystemStatus.AwaitingToken = false;
                    }
                    else
                    {
                        // check config
                        try
                        {
                            foreach (var u in _applicationConfiguration.Users)
                            {
                                var hashedToken = Crypto.Hash(Crypto.Hash(id) + u.Salt);
                                foreach (var e in u.Events)
                                {
                                    if (hashedToken != e.Token) continue;
                                    foreach (var plugin in _plugins)
                                    {
                                        if (plugin.Value.GetPluginName() != e.PluginName) continue;
                                        plugin.Value.NcfRingUp(id, e.Parameters, SystemStatus);
                                        Log("Plugin " + plugin.Value.GetPluginName() + " passed TagUp event");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error while checking config: {ex.Message}");
                        }
                    }
                }
                currentTokens = ls;
                // sleep for configured delay?
                Thread.Sleep(100);
            }
            serialContext.Stop();
            Log("NFC Reading stopped");
        }

        public void Stop()
        {
            Log("Core stopping");
            _state = ServiceState.Stopping;
            // give the reader loop a chance to exit
            Thread.Sleep(200);
            // the NFC thread will stop by itself now
            if(_readerThread != null)
            {
                if (_readerThread.IsAlive)
                    _readerThread.Join();
                _readerThread = null;
            }
            StopNetwork();
            // we need to unload plugins now
            foreach (Lazy<IZeroKeyServicePlugin> plugin in _plugins)
            {
                try
                {
                    //Log("Starting plugin");
                    Log("Plugin " + plugin.Value.GetPluginName() + " passed Unload event");
                    plugin.Value.PluginUnload();
                }
                catch (Exception ex)
                {
                    Log("Plugin threw an exception on Unload event: " + ex.Message);
                }
            }

            SaveConfig();
            _state = ServiceState.Stopped;
            Log("Core stopped");
        }

        public void Start()
        {
            InitLog();
            Log("Core starting");
            LoadConfig();
            _state = ServiceState.Starting;
            InitialiseNetwork();
            _readerThread = new Thread(new ThreadStart(ScanForId));
            _readerThread.Start();
            _state = ServiceState.Running;
            Log("Core started");
        }

        private void InitialiseNetwork()
        {
            Log("Initializing Network");
            if (_registrationListener != null)
            {
                _registrationListener.Stop();
                _registrationListener = null;
            }
            if (_credentialListener != null)
            {
                _credentialListener.Stop();
                _credentialListener = null;
            }

            _credentialListener = new TcpListener(IPAddress.Loopback, 28416); // no reason
            _registrationListener = new TcpListener(IPAddress.Loopback, 28417); // no reason

            _runListenLoops = true;
            // credential provider listener
            _credentialListenThread = new Thread(new ThreadStart(ListenForCredentialProvider));
            _credentialListenThread.Start();

            // need to use another thread to listen for incoming connections
            _registrationListenThread = new Thread(new ThreadStart(ListenForRegistration));
            _registrationListenThread.Start();
        }

        private void StopNetwork()
        {
            Log("Network Shutting Down");
            _runListenLoops = false;
            try
            {
                SystemStatus.CredentialData.ProviderActive = false;
                SystemStatus.CredentialData.Client = null;
                SystemStatus.AwaitingToken = false;
                SystemStatus.RegistrationClient = null;
                _credentialListener.Stop();
                _credentialListener = null;
                _registrationListener.Stop();
                _registrationListener = null;
            }
            catch (Exception ex)
            {
                Log("TCP error stopping listener: " + ex.Message);
            }
        }

        
        private void ListenForCredentialProvider()
        {
            Log("Credential Network Active");
            try
            {
                if (_credentialListener != null)
                    _credentialListener.Start(3);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ZeroKey Service Core";
                    eventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 101, 1);
                }
            }

            while (_runListenLoops && _credentialListener != null && (_state == ServiceState.Running || _state == ServiceState.Starting))
            {
                try
                {
                    TcpClient tc = _credentialListener.AcceptTcpClient();
                    // save the client to call it when an event happens (that we're listening for)
                    Log("TCP: credential client connected");

                    Thread newClientThread = new Thread(new ParameterizedThreadStart(ReadNetwork));
                    newClientThread.Start(tc);
                    SystemStatus.CredentialData.Client = tc;
                    SystemStatus.CredentialData.ProviderActive = true;
                    //Connections.Add(new Client() { ClientConnection = tc, ClientProcess = newClientThread });
                }
                catch (Exception ex)
                {
                    // we failed to accept a connection. should log and work out why
                    Log("TCP: Accept credential client failed: " + ex.Message);
                }

                if (SystemStatus.CredentialData.Client != null &&
                    SystemStatus.CredentialData.Client.Connected) continue;
                SystemStatus.CredentialData.ProviderActive = false;
                SystemStatus.CredentialData.Client = null;
            }
            Log("Credential Network Inactive");
        }

        private void ListenForRegistration()
        {
            Log("Registration Network Active");
            try
            {
                if (_registrationListener != null)
                    _registrationListener.Start(3);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ZeroKey Service Core";
                    eventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 101, 1);
                }
            }
            
            while (_runListenLoops && _registrationListener != null && (_state == ServiceState.Running || _state == ServiceState.Starting))
            {
                try
                {
                    TcpClient tc = _registrationListener.AcceptTcpClient();
                    // save the client to call it when an event happens (that we're listening for)
                    Log("TCP: registration client connected");

                    Thread newClientThread = new Thread(new ParameterizedThreadStart(ReadNetwork));
                    newClientThread.Start(tc);
                    SystemStatus.RegistrationClient = tc;
                    //Connections.Add(new Client() { ClientConnection = tc, ClientProcess = newClientThread });
                }
                catch (Exception ex)
                {
                    // we failed to accept a connection. should log and work out why
                    Log("TCP: Accept registration client failed: " + ex.Message);
                }

                if (SystemStatus.RegistrationClient != null && SystemStatus.RegistrationClient.Connected) continue;
                SystemStatus.AwaitingToken = false;
                SystemStatus.RegistrationClient = null;
            }
            Log("Registration Network Inactive");
        }

        private void ReadNetwork(object tc)
        {
            TcpClient client = tc as TcpClient;
            while ((_state == ServiceState.Running || _state == ServiceState.Starting) && client != null && client.Connected)
            {
                // do a read
                try
                {
                    string message = ServiceCommunication.ReadNetworkMessage(ref client);
                    // do something with data the provider sent us
                    if (message == "")
                    {
                        goto EndConnection;
                    }
                    else
                    {
                        NetworkMessage nm;
                        Log("Message received from network: " + message);
                        try
                        {
                            nm = JsonConvert.DeserializeObject<NetworkMessage>(message);
                        }
                        catch
                        {
                            Log(message);
                            continue;
                        }
                        // the first message should probably be the type of message we're receiving
                        switch (nm.Type)
                        {
                            case MessageType.CancelRegistration:
                                {
                                    SystemStatus.AwaitingToken = false;
                                    break;
                                }
                            case MessageType.GetToken:
                                {
                                    // we don't need to do anything here except store this connection and wait for a ring swipe
                                    // SystemStatus.CredentialData.Client
                                    SystemStatus.RegistrationClient = client;
                                    SystemStatus.AwaitingToken = true;
                                    break;
                                }
                            case MessageType.RegisterToken:
                                {
                                    // save this token against this username to be selected from the list later
                                    RegisterToken(nm.Username, nm.Token, nm.TokenFriendlyName);
                                    break;
                                }
                            case MessageType.GetState:
                                {
                                    // return the current configuration for this user
                                    bool userfound = false;
                                    UserServerState uss = new UserServerState();
                                    if (_applicationConfiguration.Users != null)
                                    {
                                        foreach (User u in _applicationConfiguration.Users)
                                        {
                                            if (u.Username == nm.Username)
                                            {
                                                uss.UserConfiguration = u;
                                                // also need to send a list of plugins.
                                                userfound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!userfound)
                                        uss.UserConfiguration = new User();
                                    uss.Plugins = new List<PluginInfo>();
                                    foreach(Lazy<IZeroKeyServicePlugin> p in _plugins)
                                    {
                                        uss.Plugins.Add(new PluginInfo() { Name = p.Value.GetPluginName(), Parameters = p.Value.GetParameters() });
                                    }
                                    ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(uss));
                                    break;
                                }
                            case MessageType.Message:
                                {
                                    Log(nm.Message);
                                    break;
                                }
                            case MessageType.AssociatePluginToToken:
                                {
                                    // get a plugin name, token, and if this plugin requires a credential, do the provider swap and lock the pc
                                    RegisterCredential(nm.Username, nm.Password, nm.Token, nm.PluginName);
                                    break;
                                }
                            case MessageType.UserCredential:
                                {
                                    //Log(message);
                                    Log("Credential received");
                                    if (SystemStatus.RegistrationClient != null && SystemStatus.RegistrationClient.Connected)
                                    {
                                        TcpClient otc = SystemStatus.RegistrationClient;
                                        ServiceCommunication.SendNetworkMessage(ref otc, JsonConvert.SerializeObject(new NetworkMessage(MessageType.UserCredential) { Username = nm.Username, Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(nm.Password)) }));
                                        SystemStatus.RegistrationClient = otc;
                                    }
                                    // so this is where the user registration credential provider sends an actual user credential 
                                    break;
                                }
                            case MessageType.Delete:
                                {
                                    Log("Deleting item");
                                    if(String.IsNullOrEmpty(nm.Username))
                                    {
                                        break; // no username? lets not modify the config
                                    }
                                    if(!String.IsNullOrEmpty(nm.Token) && !String.IsNullOrEmpty(nm.PluginName))
                                    {
                                        // delete an event
                                        RemoveEvent(nm.Username, nm.Token, nm.PluginName);
                                    }
                                    else if(!String.IsNullOrEmpty(nm.Token))
                                    {
                                        // delete a token entirely (this will also delete all its events)
                                        RemoveToken(nm.Username, nm.Token);
                                    }
                                    break;
                                }
                            case MessageType.RegisterAll:
                                {
                                    Log("Registering new token against all plugins");
                                    string dht = RegisterToken(nm.Username, nm.Token, nm.TokenFriendlyName);
                                    foreach(Lazy<IZeroKeyServicePlugin> p in _plugins)
                                    {
                                        RegisterCredential(nm.Username, nm.Password, dht, p.Value.GetPluginName());
                                    }
                                    break;
                                }
                            case MessageType.UpdateFriendlyName:
                                {
                                    Log("Update token friendly name");

                                    UpdateFriendlyName(nm);

                                    break;
                                }
                            case MessageType.Token:
                            case MessageType.State:
                            default:
                                // failed
                                Log("Unknown network message received: " + message);
                                break;                            
                        }
                    }
                }
                catch
                {
                    Log("TCP Client disconnected");
                    if (client.Connected)
                        client.Close();
                }
            }
            EndConnection:
            Log("TCP Client network loop ended");
            if (client.Connected)
                client.Close();
            client = null;

            if (SystemStatus.RegistrationClient == null || !SystemStatus.RegistrationClient.Connected)
                SystemStatus.AwaitingToken = false;
            if (SystemStatus.CredentialData.Client == null || !SystemStatus.CredentialData.Client.Connected)
                SystemStatus.CredentialData.ProviderActive = false;
        }

        private void UpdateFriendlyName(NetworkMessage networkMessage)
        {
            var token = networkMessage.Token;

            var isUpdated = false;

            if (_applicationConfiguration.Users != null)
            {
                foreach (var user in _applicationConfiguration.Users)
                {
                    var existToken = user.Tokens.FirstOrDefault(x => x.Key == token);
                    if (Equals(existToken, default(KeyValuePair<string, string>)))
                        continue;

                    user.Tokens[token] = networkMessage.TokenFriendlyName;
                    isUpdated = true;
                    break;
                }
            }

            if (isUpdated)
                SaveConfig();
        }

        private void LoginOK(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            Debug.WriteLine("Login successful.");
            IsConnected = true;
        }

        private void Disconnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            Debug.WriteLine("Disconnected.");
            IsConnected = false;
        }

        private void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);

            Debug.WriteLine("Got response from server... sync file now...");

            // Testing now which config this is

            // Integer list:
            //  0 = null
            //  1 = Application.config
            //  2 = medatixx.json
            int IsConfig = 0;

            // Serializing Application.config
            string charObj = message.Substring(0, 1);
            string parseConfig = message.Remove(0, 1);

            if (charObj == "1")
                IsConfig = 1;
            if (charObj == "2")
                IsConfig = 2;

            switch (IsConfig)
            {
                default:
                    // Random message, ignore it.
                    break;

                case 1:
                    // Application.config
                    File.WriteAllText(AppPath + @"\Application.config", parseConfig);

                    string sc = File.ReadAllText(AppPath + @"\Application.config");
                    _applicationConfiguration = JsonConvert.DeserializeObject<Config>(sc);
                    GotConfig = true;
                    Log("Configuration loaded from authentication server.");
                    break;

                case 2:
                    // medatixx.json
                    File.WriteAllText(AppPath + @"\medatixx.json", parseConfig);
                    break;
            }
        }

        private bool LoadConfig()
        {
            // read in our JSON file
            // decrypt?
            // store this in an object of some kind

            // Read local database config
            try
            {
                var db = new IniFile(AppPath + @"\Database.ini");
                string ip = db.Read("IP", "Login");
                int port = 9000;

                if (!String.IsNullOrEmpty(ip))
                {
                    client = new SimpleTcpClient($"{ip}:{port}");
                    client.Events.Connected += LoginOK;
                    client.Events.Disconnected += Disconnected;
                    client.Events.DataReceived += DataReceived;
                    client.Connect();
                    Debug.WriteLine("Login requested!");

                    // Wait for response
                    Thread.Sleep(8000);
                    if (IsConnected)
                    {
                        RequestConfig();
                        if (GotConfig)
                            return true;
                    }
                }
                // db.Write("IsOnline","false","Settings");
            }
            catch
            {
                Log("Failed to connect with remote authentication server.");
            }

            try
            {
                if (File.Exists(AppPath + @"\Application.config"))
                {
                    string sc = File.ReadAllText(AppPath + @"\Application.config");
                    _applicationConfiguration = JsonConvert.DeserializeObject<Config>(sc);
                    Log("Configuration loaded from " + AppPath + @"\Application.config");
                    return true;
                }
                else
                {
                    Log("No configuration file to read");
                    _applicationConfiguration = new Config();
                    _applicationConfiguration.Users = new List<User>();
                    _applicationConfiguration.Users.Add(new User()
                    {
                        Username = GetCurrentUsername(),
                        Events = new List<Event>(),
                        Salt = new Random().Next(1000000, 9999999).ToString(),
                        Tokens = new Dictionary<string, string>()
                    });
                    return true;
                }
            }
            catch(Exception ex)
            {
                Log("Failed to read application config: " + ex.Message);
            }
            return false;
        }

        private bool SaveConfig()
        {
            // Try remote push
            try
            {
                if (IsConnected)
                {
                    client.Send(1 + JsonConvert.SerializeObject(_applicationConfiguration));
                    Debug.WriteLine("Local configuration -> server.");
                }
                else
                {
                    bool Connection = RequestLogin();
                    if (Connection)
                    {
                        client.Send(1 + JsonConvert.SerializeObject(_applicationConfiguration));
                        Debug.WriteLine("Local configuration -> server.");
                    }
                }
            }
            catch
            {
                Log("Failed to connect with remote authentication server.");
            }

            File.WriteAllText(AppPath + @"\Application.config", JsonConvert.SerializeObject(_applicationConfiguration));
            Log("Configuration saved to " + AppPath + @"\Application.config");
            return true;
        }

        private bool RequestLogin()
        {
            var db = new IniFile(AppPath + @"\Database.ini");
            string ip = db.Read("IP", "Login");
            int port = 9000;

            if (!String.IsNullOrEmpty(ip))
            {
                client = new SimpleTcpClient($"{ip}:{port}");
                client.Connect();
                Debug.WriteLine("Login requested!");

                // Wait for response
                Thread.Sleep(8000);
                if (IsConnected)
                    return true;
            }                
            return false;
        }

        private void RequestConfig()
        {
            client.Send("gimme config");
            Thread.Sleep(8000);
        }

        private void RemoveToken(string user, string token)
        {
            //string hashedToken = Crypto.Hash(rawToken);

            foreach (var u in _applicationConfiguration.Users.Where(u => u.Username.ToLower() == user.ToLower()))
            {
                if (u.Tokens.ContainsKey(token))
                    u.Tokens.Remove(token);
                List<Event> remove = u.Events.Where(e => e.Token == token).ToList();
                foreach(Event e in remove)
                {
                    u.Events.Remove(e);
                }
            }
            Log("Token unregistered");
            SaveConfig();
        }

        private void RemoveEvent(string user, string token, string pluginName)
        {
            foreach (User u in _applicationConfiguration.Users)
            {
                if (u.Username.ToLower() != user.ToLower()) continue;
                List<Event> remove = u.Events.Where(e => e.Token == token && e.PluginName == pluginName).ToList();
                foreach (var e in remove)
                {
                    u.Events.Remove(e);
                }
            }
            Log("Plugin unregistered");
            SaveConfig();
        }

        private string RegisterToken(string user, string rawToken, string name)
        {
            // hash the token
            // remove the token registered anywhere else
            User target = null;
            string hashedToken = Crypto.Hash(rawToken);

            if (_applicationConfiguration.Users != null)
            {
                foreach(var u in _applicationConfiguration.Users)
                {
                    if (u.Tokens.ContainsKey(Crypto.Hash(hashedToken + u.Salt)))
                        RemoveToken(u.Username, rawToken);
                    if (u.Username == user)
                        target = u;
                }
            }
            else
            {
                _applicationConfiguration.Users = new List<User>();
            }
            if(target == null)
            {
                User u = new User();
                u.Username = user;
                u.Events = new List<Event>();
                u.Tokens = new Dictionary<string, string>();
                u.Salt = new Random().Next(1000000, 9999999).ToString();
                _applicationConfiguration.Users.Add(u);
                target = u;
            }
            string dht = Crypto.Hash(hashedToken + target.Salt);
            Log("Token registered");
            target.Tokens.Add(dht, name);
            SaveConfig();
            return dht;
        }

        private void RegisterCredential(string user, string password, string tokenId, string pluginName)
        {
            string loggedInUser = GetCurrentUsername();
            string domain = "";
            domain = loggedInUser.Substring(0,loggedInUser.LastIndexOf('\\'));
            user = user.Replace(domain + @"\", "");
            
            if (loggedInUser.Substring(loggedInUser.LastIndexOf('\\')+1).ToLower() == user.ToLower())
            {
                // check to see if there is a domain?

                // password is already encoded
                foreach(User u in _applicationConfiguration.Users)
                {
                    if (!String.Equals(u.Username, GetCurrentUsername(), StringComparison.CurrentCultureIgnoreCase)) continue;
                    Lazy<IZeroKeyServicePlugin> lp = _plugins.FirstOrDefault(x => x.Value.GetPluginName() == pluginName);
                    if (lp == null) break;
                    Dictionary<string, object> p = new Dictionary<string, object>();
                    if (lp.Value.GetParameters().FirstOrDefault(y => y.Name == "Username") != null)
                        p.Add("Username", user);
                    if (lp.Value.GetParameters().FirstOrDefault(y => y.Name == "Password") != null)
                        p.Add("Password", password);
                    if (lp.Value.GetParameters().FirstOrDefault(y => y.Name == "Domain") != null)
                        p.Add("Domain", domain);

                    u.Events.Add(new Event()
                    {
                        PluginName = pluginName,
                        Token = tokenId,
                        Parameters = p
                    });
                    break;
                }
            }
            SaveConfig();
            // make the registration credential provider not active on the system anymore
            //UseNFCCredential();
        }

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(_logFile, DateTime.Now.ToString("[dd-MM-yy HH:mm:ss] ") + message + Environment.NewLine);
            }
            catch
            {
                // ignored
            }
        }

        private string GetCurrentUsername()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            return (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        }
        
        /// <summary>
        /// Usage: var timer = SetInterval(DoThis, 1000);
        /// UI Usage: BeginInvoke((Action)(() =>{ SetInterval(DoThis, 1000); }));
        /// </summary>
        /// <returns>Returns a timer object which can be stopped and disposed.</returns>
        public static System.Timers.Timer SetInterval(Action Act, int Interval)
        {
            System.Timers.Timer tmr = new System.Timers.Timer();
            tmr.Elapsed += (sender, args) => Act();
            tmr.AutoReset = true;
            tmr.Interval = Interval;
            tmr.Start();

            return tmr;
        }
    }
}
