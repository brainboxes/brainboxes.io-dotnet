using System;
using Fleck;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BbDemoRsTruck
{
    class BBWebSocketServer
    {
        private readonly WebSocketServer _server;
        private readonly int _port;
        private readonly List<IWebSocketConnection> _allSockets = new List<IWebSocketConnection>();
        public BBWebSocketServer(int port)
        {
            _port = port;
            // bind to 0.0.0.0 rather than specific ip so websocket server is available on
            // localhost as well as all available interfaces
            //_server = new WebSocketServer("ws://"+IP+":"+port+"/");
            _server = new WebSocketServer("ws://0.0.0.0:" + port + "/");
            _server.ListenerSocket.NoDelay = true;
            //_server.RestartAfterListenError = true;
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
    //protected IDisposable webapp;
    //public BBWebSocketServer()
    //{

    //}
    //public void Start(int port)
    //{
    //    string url = "http://*:" + port;
    //    webapp = WebApp.Start(url);
    //    Console.WriteLine("Server running on {0}", url);

    //}
    //public void Stop()
    //{
    //    webapp.Dispose();
    //    webapp = null;
    //}
    //public void Configuration(IAppBuilder app)
    //{
    //    DefaultFilesOptions options = new DefaultFilesOptions();
    //    options.DefaultFileNames.Clear();
    //    options.DefaultFileNames.Add("index.html");
    //    app.UseDefaultFiles(options);
    //}
}


