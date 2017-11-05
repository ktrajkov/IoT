using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.Broker
{
    public interface IBroker
    {
        void Connect();
        void OnConnectionClosed(object sender, EventArgs e);
        void SendData(string chanel, string data);
    }
}
