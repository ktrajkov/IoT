using IoT.Data.Contracts.TransportModels;
using System;
using System.Text;
using System.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Microsoft.Extensions.Options;

namespace IoT.Broker
{
    public class AdafruitBroker : IBroker
    {
        private MqttClient _client;
        private string _username;
        private string _key;
        private string _baseTopic;

        public event EventHandler<MsgPublishReceivedEventArgs> MsgPublishReceived;

        public AdafruitBroker(IOptions<BrokerSettings> options)
        {
            _username = options.Value.Username;
            _key = options.Value.Key;
            _baseTopic = $"{_username}/feeds/";

            _client = new MqttClient(options.Value.HostName, options.Value.Port, false, MqttSslProtocols.None, null, null);
            _client.ConnectionClosed += OnConnectionClosed;
            _client.MqttMsgPublishReceived += OnPublishReceived;
            _client.MqttMsgSubscribed += OnSubscribed;
            Connect();
        }

        public void Connect()
        {
            var clientId = Guid.NewGuid().ToString();
            _client.Connect(clientId, _username, _key);
        }

        public void OnConnectionClosed(object sender, EventArgs e)
        {
            Connect();
        }

        public void SendData(string chanel, string data)
        {
            _client.Publish(GetTopic(chanel), Encoding.UTF8.GetBytes(data));
        }

        public void Subscribe(params string[] chanels)
        {
            foreach (var chanel in chanels)
            {
                _client.Subscribe(new string[] { GetTopic(chanel) }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
        }

        public void OnPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = new MsgPublishReceivedEventArgs
            {
                Value = Encoding.Default.GetString((e).Message),
                Chanel = GetChanel(e.Topic)
            };

            MsgPublishReceived(sender, message);
        }

        public void OnSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
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
