using IoT.CM.Clients;
using IoT.CM.Models;
using System;

namespace IoT.CM.Managers
{
    public abstract class BaseClientManager
    {
        public abstract IClient Connect(string username, string key);

        public abstract event EventHandler<MsgReceivedEventArgs> MsgReceived;

        public virtual void OnConnectionClosed(object sender, EventArgs e)
        {
            //Connect();
        }

        public abstract void Subscribe(IClient client, params string[] chanels);

        public abstract void SendMessage(IClient client, string chanel, string message);
    }
}
