using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.Broker
{
    public interface IBroker
    {
        void Connect();
        void Subscribe(params string[] chanels);
        void OnConnectionClosed(object sender, EventArgs e);
        void OnPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e);
        void OnSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e);
        event  EventHandler<MsgPublishReceivedEventArgs> MsgPublishReceived;
        void SendData(string chanel, string data);
    }
}
