using IoT.Data.Contracts.TransportModels;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

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

        public AdafruitBroker()
        {
            Client = new MqttClient(broker, port, false, MqttSslProtocols.None, null, null);
            Client.ConnectionClosed += OnConnectionClosed;
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
    }
}
