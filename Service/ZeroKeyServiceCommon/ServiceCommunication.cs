using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZeroKey.Service.Common
{
    public static class ServiceCommunication
    {
        public static string ReadNetworkMessage(ref TcpClient client)
        {
            if (client == null)
            {
                return "";
            }
            if (!client.Connected)
            {
                return "";
            }
            var buffer = new byte[10000];
            try
            {
                var len = client.GetStream().Read(buffer, 0, buffer.Length);
                var shortBuffer = new byte[len];
                Array.Copy(buffer, shortBuffer, len);
                return Encoding.UTF8.GetString(shortBuffer);
            }
            catch
            {
                return "";
            }
        }
        public static int SendNetworkMessage(ref TcpClient client, string message)
        {
            if (client == null)
            {
                client = new TcpClient();
            }
            try
            {
                if (!client.Connected)
                {
                    client.Connect(IPAddress.Loopback, 28417);
                }
                client.GetStream().Write(Encoding.UTF8.GetBytes(message), 0, message.Length);
                return message.Length;
            }
            catch
            {
                return 0;
            }
        }
    }
}
