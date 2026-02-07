using Fleck;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BBDemoApp
{
    class BBWebSocketServer
    {
        private readonly WebSocketServer _server;
        private readonly int _port;
        private readonly List<IWebSocketConnection> _allSockets = new List<IWebSocketConnection>();

        public BBWebSocketServer(int port)
        {
            _port = port;
            _server = new WebSocketServer("ws://"+IP+":"+port+"/");
        }

        public string IP
        {
            get
            {
                IPHostEntry host;
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "?";
            }
        }

        public void Start()
        {
             _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Socket opened by " + socket.ConnectionInfo.ClientIpAddress);
                    _allSockets.Add(socket);
                    try { OnOpen(socket); }
                    catch (Exception e) { Console.Error.WriteLine(e); }
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Socket closed by " + socket.ConnectionInfo.ClientIpAddress);
                    _allSockets.Remove(socket);
                };
                socket.OnMessage = (message) =>
                {
                    try { ProcessMessage(message, socket); }
                    catch (Exception e) { Console.Error.WriteLine(e); }
                };

                socket.OnError = (e) =>
                {
                    Console.WriteLine("Socket error on " + socket.ConnectionInfo.ClientIpAddress);
                    Console.WriteLine(e);
                    _allSockets.Remove(socket);
                };
            });
        }

        public void SendAll(string message)
        {
            foreach (var socket in _allSockets)
            {
                socket.Send(message);
            }
        }
    
        public void Stop()
        {
 	        foreach (var socket in _allSockets)
            {
                socket.Close();
            }
        }

        public event Action<string, IWebSocketConnection> ProcessMessage;
        public event Action<IWebSocketConnection> OnOpen;

    }
}
