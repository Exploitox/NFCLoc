using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NFCLoc.Service.Core
{
    public class SerialContext
    {
        private static Dictionary<string, double> _currentIds = new Dictionary<string, double>();
        static object _idSyncRoot = new object();
        const int TagTimeoutMs = 500;

        private static Dictionary<string, SerialConnection> _openPorts = new Dictionary<string, SerialConnection>();
        static object _readerSyncRoot = new object();

        private static Dictionary<string, int> _inaccessiblePorts = new Dictionary<string, int>();

        DateTime _startTime = DateTime.Now;

        Timer _reEnumeratePortsTimer = null;
        Timer _clearFailedPortsTimer = null;

        public SerialContext()
        {
            // check for new serial devices continuously
            _reEnumeratePortsTimer = new Timer(CheckPorts, new object(), 2000, 5000);
            // this allows us to retry ports that were blocked out before. It will usually result in 3 "Access exceptions" for ports that are already in use.
            _clearFailedPortsTimer = new Timer(ClearFailedPorts, new object(), 60000, 60000);
        }

        private void ClearFailedPorts(object state)
        {
            lock (_readerSyncRoot)
            {
                _inaccessiblePorts.Clear();
            }
        }

        private void CheckPorts(object state)
        {
            lock(_readerSyncRoot)
            {
                List<string> deadReaders = new List<string>();
                foreach(KeyValuePair<string, SerialConnection> kvp in _openPorts)
                {
                    if(!kvp.Value.Port.IsOpen)
                    {
                        deadReaders.Add(kvp.Key);
                    }
                }
                foreach(string s in deadReaders)
                {
                    _openPorts.Remove(s);
                }
                EnumeratePorts();
            }
        }

        public void Stop()
        {
            _reEnumeratePortsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _clearFailedPortsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (_readerSyncRoot)
            {
                foreach (SerialConnection sp in _openPorts.Values)
                {
                    if (sp.Port.IsOpen)
                    {
                        sp.Port.Write("STOP\n");
                    }
                }
                Thread.Sleep(500);
                foreach (SerialConnection sp in _openPorts.Values)
                {
                    sp.Port.DataReceived -= P_DataReceived;
                    if (sp.Port.IsOpen)
                        sp.Port.Close();
                }
                _openPorts.Clear();
            }

            lock (_idSyncRoot)
            {                
                _currentIds.Clear();
            }
        }

        public List<string> GetReaders()
        {
            return _openPorts.Keys.ToList();
        }

        // I need to re-run this occasionally in case a new device has been added?
        private void EnumeratePorts()
        {
            lock (_readerSyncRoot)
            {
                string[] ports = SerialPort.GetPortNames();
                List<string> devices = new List<string>();
                foreach (string portName in ports)
                {
                    if (_openPorts.ContainsKey(portName))
                    {
                        continue;
                    }
                    if(_inaccessiblePorts.ContainsKey(portName) && _inaccessiblePorts[portName] > 3)
                    {
                        continue;
                    }
                    SerialPort p = new SerialPort(portName, 115200);
                    if (!p.IsOpen)
                    {
                        try
                        {
                            p.Open();
                            if (p.IsOpen)
                            {
                                Thread.Sleep(500);
                                p.Write("SHAKE\n");
                                int i = 0;
                                while(p.BytesToRead <= 0 && i < 30)
                                {
                                    Thread.Sleep(10);
                                    i++;
                                }
                                if (p.BytesToRead <= 0)
                                {
                                    p.Close();
                                    continue;
                                }
                                string s = p.ReadLine();
                                if (s.Trim() == "NFC READER")
                                {
                                    if (_inaccessiblePorts.ContainsKey(portName))
                                        _inaccessiblePorts.Remove(portName);
                                    p.Write("IDENT\n");
                                    i = 0;
                                    while (p.BytesToRead <= 0 && i < 25)
                                    {
                                        Thread.Sleep(10);
                                        i++;
                                    }
                                    if (p.BytesToRead <= 0)
                                    {
                                        p.Close();
                                        continue;
                                    }
                                    s = p.ReadLine();
                                    p.Write("START\n");
                                    i = 0;
                                    while (p.BytesToRead <= 0 && i < 25)
                                    {
                                        Thread.Sleep(10);
                                        i++;
                                    }
                                    if (p.BytesToRead <= 0)
                                    {
                                        p.Close();
                                        continue;
                                    }
                                    s = p.ReadLine();
                                    devices.Add(s.Trim());
                                    p.DataReceived += P_DataReceived;
                                    _openPorts.Add(portName, new SerialConnection() { Port = p, ReaderName = s });
                                }
                                else
                                {
                                    p.Close();
                                }
                            }
                        }
                        catch
                        {
                            // couldnt open the port for some reason. already open elsewhere?
                            if (_inaccessiblePorts.ContainsKey(portName))
                                _inaccessiblePorts[portName] = _inaccessiblePorts[portName] + 1;
                            else
                                _inaccessiblePorts.Add(portName, 1);
                        }
                    }
                }
            }
        }

        private void P_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string s = (sender as SerialPort)?.ReadLine();
            s = s.Trim();
            if (s == "OK")
                return;
            lock (_idSyncRoot)
            {
                if (_currentIds.ContainsKey(s))
                {
                    _currentIds[s] = (DateTime.Now - _startTime).TotalMilliseconds;
                }
                else
                {
                    _currentIds.Add(s, (DateTime.Now - _startTime).TotalMilliseconds);
                }
            }
        }

        public List<string> GetIds()
        {
            lock(_idSyncRoot)
            {
                List<string> deadTokens = new List<string>();
                foreach(KeyValuePair<string, double> kvp in _currentIds)
                {
                    if((DateTime.Now - _startTime).TotalMilliseconds > (kvp.Value + 200))
                    {
                        deadTokens.Add(kvp.Key);
                    }
                }
                foreach (string id in deadTokens)
                    _currentIds.Remove(id);
                return _currentIds.Keys.ToList();
            }
        }
    }

    class SerialConnection
    {
        public SerialPort Port { get; set; }
        public string ReaderName { get; set; }
    }
}
