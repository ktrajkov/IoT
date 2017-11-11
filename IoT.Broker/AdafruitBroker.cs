using IoT.Data.Contracts.TransportModels;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace IoT.Broker
{
    public class AdafruitBroker : IBroker
    {
        public MqttClient Client;

        private const string broker = "io.adafruit.com";
        private const int port = 1883;
        private const string username = "kaluhckua";
        private const string key = "7f4e0351e04dcc8081bc81caff53270d67152c28";
        private const string feeds = "kaluhckua/feeds/";

        public event EventHandler<EventArgs> MsgPublishReceived;

        public AdafruitBroker()
        {
            Client = new MqttClient(broker, port, false, MqttSslProtocols.None, null, null);
            Client.ConnectionClosed += OnConnectionClosed;
            Client.MqttMsgPublishReceived += OnPublishReceived;
            Client.MqttMsgSubscribed += OnSubscribed;
            Connect();
        }

        public void Connect()
        {
            var clientId = Guid.NewGuid().ToString();
            Client.Connect(clientId, username, key);
        }

        public void OnConnectionClosed(object sender, EventArgs e)
        {
            Connect();
        }

        public void SendData(string chanel, string data)
        {
            string feed = feeds + chanel;
            Client.Publish(feed, Encoding.UTF8.GetBytes(data));
        }

        private void Client_MqttMsgSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {

        } 

        public void Subscribe(string chanel)
        {
            Client.Subscribe(new string[] { feeds + chanel.ToLower() }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public void OnPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            MsgPublishReceived(sender, e);
        }

        public void OnSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {

        }
    }
}
