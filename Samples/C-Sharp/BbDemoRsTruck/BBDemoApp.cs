using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fleck;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace BbDemoRsTruck
{
    class BBDemoApp
    {
        public BBDemoApp(int listenOnPort = 80, int webSocketListenOnPort = 8989)
        {
            httpServer = new HttpServer(listenOnPort);
            socketServer = new BBWebSocketServer(webSocketListenOnPort);
            remoteIO = new RemoteIO();

            _cacheFiles();

            httpServer.ProcessRequest += HttpAction;
            socketServer.OnOpen += newSocketOpen;
            socketServer.ProcessMessage += syncEDDevices;
            remoteIO.OnChange += updateAllBrowsers;

        }
        public string ServerAddress { get { return httpServer.ServerAddress; } }

        private void _cacheFiles()
        {
            _cache = new Dictionary<string, byte[]>();

            DirectoryInfo contentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Content");

            List<DirectoryInfo> dirWithFiles = contentDir.GetDirectories().ToList();
            List<DirectoryInfo> assetFiles = dirWithFiles[0].GetDirectories().ToList();
            assetFiles.Add(contentDir);

            foreach (DirectoryInfo dir in assetFiles)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    byte[] buffer;
                    if (file.Extension == ".png" || file.Extension == ".jpg" || file.Extension == ".ico")
                    {
                        //image files can just be read as is
                        buffer = File.ReadAllBytes(file.FullName);
                    }
                    else
                    {
                        //text files need to be converted to UTF8
                        TextReader tr = new StreamReader(file.FullName);
                        string msg = tr.ReadToEnd();
                        buffer = Encoding.UTF8.GetBytes(msg);
                    }
                    //set the key as the web path which has forward slash instead of windows path which uses back slash
                    _cache.Add(file.FullName.Replace(contentDir.FullName, "").Replace('\\', '/'), buffer);
                }
            }

        }

        //protected HttpListener listener;
        protected HttpServer httpServer;
        protected BBWebSocketServer socketServer;
        protected RemoteIO remoteIO;

        private Dictionary<string, byte[]> _cache;

        public void StartServers()
        {
            httpServer.Start();
            socketServer.Start();
            remoteIO.Start();
        }

        /// <summary>
        /// Action when a HTTP request is made, if available the requested file is served from cache
        /// </summary>
        /// <param name="context"></param>
        private void HttpAction(HttpListenerContext context)
        {
            //returns just the relative path ??
            string path = context.Request.Url.AbsolutePath;

            Console.WriteLine("New HTTP Request:\t{0}\t", path);

            //if( context.Request.IsWebSocketRequest) //does not work unless Win 8 or server 2012!!!! have to use Fleck instead (3rd party lib)

            string page = null;
            if (_cache.ContainsKey(path))
            {
                page = path;
            }
            else if (path == "" || path == "/")
            {
                page = "/index.html";
            }
            else
            {
                Console.WriteLine("404 page not found");
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }
            //serve cached page
            byte[] buffer = _cache[page];

            if (!path.EndsWith("qr-ip.png")) //cache everything except the qr code for the ip address which may change
            {
                context.Response.Headers.Add(HttpResponseHeader.Expires,
                    DateTime.UtcNow.AddDays(2).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
            }
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);

            context.Response.Close();
        }

        /// <summary>
        /// event called when a browser wants to change the state of the ED Devices
        /// The message is deserialised and sent to the REmote IO class which decides
        /// what to do with the new requested state
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socket"></param>
        private void syncEDDevices(string message, IWebSocketConnection socket)
        {
            //the current socket is looking to mutate the current state
            AnalogEDState requestedState = JsonConvert.DeserializeObject<AnalogEDState>(message);
            Console.Write("\nED Data Recieved.\t");
            remoteIO.UpdateState(requestedState);
        }

        /// <summary>
        /// event called by RemoteIO class, when status of devices change
        /// then update all browsers through web socket connection
        /// </summary>
        /// <param name="newState"></param>
        private void updateAllBrowsers(AnalogEDState newState)
        {
            Console.WriteLine("Updating browsers");
            string newMessage = JsonConvert.SerializeObject(newState);
            socketServer.SendAll(newMessage);
        }

        /// <summary>
        /// When a new socket connects send the current remote IO demo state
        /// </summary>
        /// <param name="socket"></param>
        private void newSocketOpen(IWebSocketConnection socket)
        {
            Console.WriteLine("Sending Current ED State");
            string currentState = JsonConvert.SerializeObject(remoteIO.state);
            socket.Send(currentState);
        }

        public void StopServers()
        {
            httpServer.Stop();
            socketServer.Stop();
            remoteIO.Stop();
        }
    }
}
