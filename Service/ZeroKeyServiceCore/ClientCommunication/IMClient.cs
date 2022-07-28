using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ZeroKey.Service.Core.ClientCommunication
{
    public class IMClient
    {
        Thread tcpThread;      // Receiver
        bool _conn = false;    // Is connected/connecting?
        bool _logged = false;  // Is logged in?
        string _user;          // Username
        string _pass;          // Password
        bool reg;              // Register mode

        public string Server { get; set; }  // Address of server. In this case - local IP address.
        public int Port { get { return 2000; } }

        public bool IsLoggedIn { get { return _logged; } }
        public string UserName { get { return _user; } }
        public string Password { get { return _pass; } }

        // Start connection thread and login or register.
        void connect(string user, string password, bool register)
        {
            if (!_conn)
            {
                _conn = true;
                _user = user;
                _pass = password;
                reg = register;
                tcpThread = new Thread(new ThreadStart(SetupConn));
                tcpThread.Start();
            }
        }
        public void Login(string user, string password, string ip)
        {
            Server = ip;
            connect(user, password, false);
        }
        public void Register(string user, string password, string ip)
        {
            Server = ip;
            connect(user, password, true);
        }
        public void Disconnect()
        {
            if (_conn)
                CloseConn();
        }

        public void IsAvailable(string user)
        {
            if (_conn)
            {
                bw.Write(IM_IsAvailable);
                bw.Write(user);
                bw.Flush();
            }
        }
        public void SendMessage(string to, string msg)
        {
            if (_conn)
            {
                bw.Write(IM_Send);
                bw.Write(to);
                bw.Write(msg);
                bw.Flush();
            }
        }

        // Events
        public event EventHandler LoginOK;
        public event EventHandler RegisterOK;
        public event IMErrorEventHandler LoginFailed;
        public event IMErrorEventHandler RegisterFailed;
        public event EventHandler Disconnected;
        public event IMAvailEventHandler UserAvailable;
        public event IMReceivedEventHandler MessageReceived;

        protected virtual void OnLoginOK()
        {
            if (LoginOK != null)
                LoginOK(this, EventArgs.Empty);
        }
        protected virtual void OnRegisterOK()
        {
            if (RegisterOK != null)
                RegisterOK(this, EventArgs.Empty);
        }
        protected virtual void OnLoginFailed(IMErrorEventArgs e)
        {
            if (LoginFailed != null)
                LoginFailed(this, e);
        }
        protected virtual void OnRegisterFailed(IMErrorEventArgs e)
        {
            if (RegisterFailed != null)
                RegisterFailed(this, e);
        }
        protected virtual void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }
        protected virtual void OnUserAvail(IMAvailEventArgs e)
        {
            if (UserAvailable != null)
                UserAvailable(this, e);
        }
        protected virtual void OnMessageReceived(IMReceivedEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        TcpClient client;
        NetworkStream netStream;
        SslStream ssl;
        BinaryReader br;
        BinaryWriter bw;

        void SetupConn()  // Setup connection and login
        {
            try
            {
                client = new TcpClient(Server, Port); // Connect to the server.
            }
            catch
            {
                return;
            }
            netStream = client.GetStream();
            ssl = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
            ssl.AuthenticateAsClient("InstantMessengerServer");
            // Now we have encrypted connection.

            br = new BinaryReader(ssl, Encoding.UTF8);
            bw = new BinaryWriter(ssl, Encoding.UTF8);

            // Receive "hello"
            int hello = br.ReadInt32();
            if (hello == IM_Hello)
            {
                // Hello OK, so answer.
                bw.Write(IM_Hello);

                bw.Write(reg ? IM_Register : IM_Login);  // Login or register
                bw.Write(UserName);
                bw.Write(Password);
                bw.Flush();

                byte ans = br.ReadByte();  // Read answer.
                if (ans == IM_OK)  // Login/register OK
                {
                    if (reg)
                        OnRegisterOK();  // Register is OK.
                    OnLoginOK();  // Login is OK (when registered, automatically logged in)
                    Receiver(); // Time for listening for incoming messages.
                }
                else
                {
                    IMErrorEventArgs err = new IMErrorEventArgs((IMError)ans);
                    if (reg)
                        OnRegisterFailed(err);
                    else
                        OnLoginFailed(err);
                }
            }
            if (_conn)
                CloseConn();
        }
        void CloseConn() // Close connection.
        {
            br.Close();
            bw.Close();
            ssl.Close();
            netStream.Close();
            client.Close();
            OnDisconnected();
            _conn = false;
        }
        void Receiver()  // Receive all incoming packets.
        {
            _logged = true;

            try
            {
                while (client.Connected)  // While we are connected.
                {
                    byte type = br.ReadByte();  // Get incoming packet type.

                    if (type == IM_IsAvailable)
                    {
                        string user = br.ReadString();
                        bool isAvail = br.ReadBoolean();
                        OnUserAvail(new IMAvailEventArgs(user, isAvail));
                    }
                    else if (type == IM_Received)
                    {
                        string from = br.ReadString();
                        string msg = br.ReadString();
                        OnMessageReceived(new IMReceivedEventArgs(from, msg));
                    }
                }
            }
            catch (IOException) { }

            _logged = false;
        }

        // Packet types
        public const int IM_Hello = 2012;      // Hello
        public const byte IM_OK = 0;           // OK
        public const byte IM_Login = 1;        // Login
        public const byte IM_Register = 2;     // Register
        public const byte IM_TooUsername = 3;  // Too long username
        public const byte IM_TooPassword = 4;  // Too long password
        public const byte IM_Exists = 5;       // Already exists
        public const byte IM_NoExists = 6;     // Doesn't exist
        public const byte IM_WrongPass = 7;    // Wrong password
        public const byte IM_IsAvailable = 8;  // Is user available?
        public const byte IM_Send = 9;         // Send message
        public const byte IM_Received = 10;    // Message received

        public static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Uncomment this lines to disallow untrusted certificates.
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;
            //else
            //    return false;

            return true; // Allow untrusted certificates.
        }
    }
}
