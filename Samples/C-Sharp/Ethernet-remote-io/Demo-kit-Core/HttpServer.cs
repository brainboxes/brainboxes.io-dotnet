using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Demo_kit_Core
{
    class HttpServer : IDisposable
    {
        private readonly int _port;
        private readonly string _listenOn;
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private Queue<HttpListenerContext> _queue;

        public HttpServer(int port = 8888, int maxThreads = 10)
        {
            _port = port;
            _listenOn = "http://+:" + port + "/";
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
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
        public string ServerAddress
        {
            get
            {
                return "http://" + this.IP + ":" + this._port + "/";
            }
        }

        public void Start()
        {
            _listener.Prefixes.Add(_listenOn);
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5)
                {
                    Console.WriteLine("Could not start server. 2 options:\n 1. re-run this exe as Admin or\n 2. Open an Admin Command Prompt and type: ");
                //    Console.WriteLine("netsh http add urlacl url=" + _listenOn + " user=" + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                    return;
                }
                throw e;
            }
            _listenerThread.Start();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }

        public void Dispose()
        { Stop(); }

        public void Stop()
        {
            _stop.Set();
            if(_listenerThread.ThreadState != ThreadState.Unstarted)
            {
                _listenerThread.Join();
            }
            foreach (Thread worker in _workers)
            {
                if(worker != null && worker.ThreadState != ThreadState.Unstarted)
                {
                    worker.Join();
                }
            }
            if(_listener.IsListening)
            {
                _listener.Stop();
            }
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                    return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch { return; }
        }

        private void Worker()
        {
            WaitHandle[] wait = new[] { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try { ProcessRequest(context); }
                catch (Exception e) { Console.Error.WriteLine(e); }
            }
        }

        public event Action<HttpListenerContext> ProcessRequest;
    }
}
