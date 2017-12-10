using IoT.CM.Clients;
using IoT.CM.Models;
using IoT.CM.Settings;
using Microsoft.Extensions.Options;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;

namespace IoT.CM.Managers
{
    public class AdafruitClientManager : BaseClientManager
    {
        private ClientSettings _clientSettings;

        public AdafruitClientManager(IOptions<CMSettings> options)
        {
            _clientSettings = options.Value.AdafruitClientSettings;
        }
        private string _username = "kaluhckua";
        private string _key = "7f4e0351e04dcc8081bc81caff53270d67152c28";
        private string _baseTopic = $"kaluhckua/feeds/";

        public override event EventHandler<MsgReceivedEventArgs> MsgReceived;

        public override IClient Connect(string username, string key)
        {

            AdafruitClient client = new AdafruitClient(_clientSettings.HostName, _clientSettings.Port, false, MqttSslProtocols.None, null, null);
            var clientId = "Adafruit";


            client.Connect(clientId, _username, _key);
            client.ConnectionClosed += OnConnectionClosed;
            client.MqttMsgPublishReceived += OnReceived;
            return client;
        }

        public override void Subscribe(IClient client, params string[] chanels)
        {
            foreach (var chanel in chanels)
            {
                ((MqttClient)client).Subscribe(new string[] { GetTopic(chanel) }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
        }

        public override void SendMessage(IClient client, string chanel, string message)
        {
            ((MqttClient)client).Publish(GetTopic(chanel), Encoding.UTF8.GetBytes(message));
        }

        private void OnReceived(object sender, MqttMsgPublishEventArgs e)
        {

            var message = new MsgReceivedEventArgs
            {
                Value = Encoding.Default.GetString((e).Message),
                Chanel = GetChanel(e.Topic)
            };
            MsgReceived(sender, message);
        }
        private string GetTopic(string chanel)
        {
            return $"{_baseTopic}{chanel.ToLower()}";
        }

        private string GetChanel(string topic)
        {
            var baseTopicLenght = _baseTopic.Length;
            var chanel = topic.Substring(baseTopicLenght, topic.Length - baseTopicLenght);
            return chanel;
        }
    }
}
