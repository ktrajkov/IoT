using IoT.Broker;
using IoT.Data.Contracts.TransportModels;
using IoT.WSManager;
using IoT.WSManager.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;
using static uPLibrary.Networking.M2Mqtt.MqttClient;

namespace IoT.WebMVC.IoTHandler
{
    public class IoTMessageHandler : WebSocketHandler
    {
        private IBroker _broker;
        public IoTMessageHandler(IBroker broker, WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
            _broker = broker;
            _broker.Subscribe(ChanelType.Relay.ToString(), ChanelType.FirmwareUpdate.ToString());
            _broker.MsgPublishReceived += OnMsgPublishReceived;
        }


        public virtual async Task Log(string log)
        {
            _broker.SendData(ChanelType.Logger.ToString(), log);

            var json = new Message
            {
                Data = JsonConvert.SerializeObject(log, _jsonSerializerSettings),
                MessageType = MessageType.Text
            };

            //get clientId from database to send
            await InvokeClientMethodAsync("Kalin", Actions.Log.ToString(), new object[] { json });
        }


        public virtual async Task UpdateTemps(JObject jobject)
        {
            //Save to DB   
            var tempsTM = jobject.ToObject<TempsTM>();

            foreach (var temp in tempsTM.Temps)
            {
                _broker.SendData(ChanelType.Temp.ToString() + temp.Id, temp.Value.ToString());
            }

            var json = new Message
            {
                Data = JsonConvert.SerializeObject(jobject, _jsonSerializerSettings),
                MessageType = MessageType.Text
            };
            await InvokeClientMethodAsync("Kalin", Actions.UpdateTemps.ToString(), new object[] { json });
        }

        private void OnMsgPublishReceived(object sender, MsgPublishReceivedEventArgs e)
        {
            SendMessageToAllAsync(new Message { Data = e, MessageType = MessageType.Text }).ConfigureAwait(false);
        }
    }

    public enum Actions
    {
        UpdateTemps,
        Log,
        FirmwareUpdate
    }

    public enum ChanelType
    {
        Temp,
        Relay,
        Logger,
        FirmwareUpdate
    }
}
