using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSimpleTcp;

namespace ZeroKey.Server.Main
{
    public class Program
    {
        private static SimpleTcpServer server;

        static void Main(string[] args)
        {
            // instantiate
            string ip = GetLocalIP();
            int port = 9000;

            if (ip != "")
            {
                server = new SimpleTcpServer($"{ip}:{port}");
                Console.WriteLine($"Listening on {ip}:{port}");
            }
            else
            {
                Console.WriteLine("FAILED: No IP address available on this system.");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }

            // set events
            server.Events.ClientConnected += ClientConnected;
            server.Events.ClientDisconnected += ClientDisconnected;
            server.Events.DataReceived += DataReceived;

            // let's go!
            server.Start();

            // once a client has connected...
            server.Send("[ClientIp:Port]", "Hello, world!");
            Console.ReadKey();
        }

        static void ClientConnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"[{e.IpPort}] client connected");
        }

        static void ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"[{e.IpPort}] client disconnected: {e.Reason}");
        }

        static void DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"[{e.IpPort}]: {Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count)}");
        }

        static string GetLocalIP()
        {
            string localIp;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address.ToString();
            }
            if (String.IsNullOrEmpty(localIp))
                return "";
            else
                return localIp;
        }
    }
}
