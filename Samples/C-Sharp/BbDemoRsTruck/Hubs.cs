using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BbDemoRsTruck
{
    public delegate void SubscribeHandler();
    public delegate void ConnectedHandler(string connectionId);
    public class Hubs : Hub
    {
        public static event SubscribeHandler Subscription;
        public static event ConnectedHandler Connect; 

        public override Task OnConnected()
        {
            Connect?.Invoke(Context.ConnectionId);
            return base.OnConnected();
        }
        public void Send(string message)
        {
            Clients.All.broadcastMessage(message);
        }
    }
}
