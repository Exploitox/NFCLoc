using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Threading;
using ZeroKey.Server.Service.Communication;

namespace ZeroKey.Server.Service
{
    public partial class ZKServer : ServiceBase
    {
        private static readonly int _port = 2000;
        private static bool _running = true;
        private static readonly IPAddress _ip = IPAddress.Parse(GetLocalIpAddress());
        private static TcpListener _server;
        public readonly X509Certificate2 _cert = new X509Certificate2("server.pfx", "xu#++m!Q~4DDGtH!Yy+§6w.6J#V8yFQS");
        public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();  // Information about users + connections info.
        public Thread _thread;

        public ZKServer()
        {
            InitializeComponent();
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "ZeroKey Server";
                eventLog.WriteEntry("Server has been successfully started.", EventLogEntryType.Information, 101, 1);
            }
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            // TCP Server
            LoadUsers();
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);
            _server = new TcpListener(_ip, _port);
            _server.Start();
            Console.WriteLine("[{0}] Server is running properly!", DateTime.Now);
            Console.WriteLine("[{0}] Listen to IP {1}", DateTime.Now, GetLocalIpAddress());

            _thread = new Thread(Listen);
            _thread.Start();
        }

        protected override void OnStop()
        {
            _server.Stop();
            _thread.Abort();
            _running = false;
        }

        private void Listen()  // Listen to incoming connections.
        {
            while (_running)
            {
                TcpClient tcpClient = _server.AcceptTcpClient();   // Accept incoming connection.
                Client client = new Client(this, tcpClient);     // Handle in another thread.
            }
        }

        string usersFileName = Environment.CurrentDirectory + "\\users.dat";
        public void SaveUsers()  // Save users data
        {
            try
            {
                Console.WriteLine("[{0}] Saving users...", DateTime.Now);
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFileName, FileMode.Create, FileAccess.Write);
                bf.Serialize(file, users.Values.ToArray());  // Serialize UserInfo array
                file.Close();
                Console.WriteLine("[{0}] Users saved!", DateTime.Now);
            }
            catch (Exception e)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ZeroKey Server";
                    eventLog.WriteEntry(e.Message, EventLogEntryType.Error, 101, 1);
                }
            }
        }

        public void LoadUsers()  // Load users data
        {
            try
            {
                Console.WriteLine("[{0}] Loading users...", DateTime.Now);
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFileName, FileMode.Open, FileAccess.Read);
                UserInfo[] infos = (UserInfo[])bf.Deserialize(file);      // Deserialize UserInfo array
                file.Close();
                users = infos.ToDictionary((u) => u.UserName, (u) => u);  // Convert UserInfo array to Dictionary
                Console.WriteLine("[{0}] Users loaded! ({1})", DateTime.Now, users.Count);
            }
            catch {;}
        }

        private static string GetLocalIpAddress()
        {
            string localIp;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address.ToString();
            }
            if (String.IsNullOrEmpty(localIp))
                return "No IPv4 address in the System!";
            else
                return localIp;
        }

    }
}
