using IoT.Broker;
using IoT.Data.Contracts.TransportModels;
using IoT.CM;
using IoT.CM.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;
using static uPLibrary.Networking.M2Mqtt.MqttClient;
using IoT.CM.WS;
using IoT.CM.Managers;
using IoT.CM.Models;

namespace IoT.WebMVC.IoTHandler
{
    public class IoTMessageHandler : WebSocketHandler
    {
        public IoTMessageHandler(WebSocketConnectionManager connectionManager, ClientHandler clientHandler) : base(connectionManager, clientHandler)
        {
            ClientHandler.MsgReceived += clientHandler_MsgReceived;
        }

        private void clientHandler_MsgReceived(object sender, MsgReceivedEventArgs e)
        {
            SendMessageToAllAsync(new Message { Data = e, MessageType = MessageType.Text }).ConfigureAwait(false);
        }
       
        public virtual async Task Log(string log)
        {
            ClientHandler.SendMessage(1, ChanelType.Logger.ToString(), log);

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
                ClientHandler.SendMessage(1, ChanelType.Temp.ToString() + temp.Id, temp.Value.ToString());
            }

            var json = new Message
            {
                Data = JsonConvert.SerializeObject(jobject, _jsonSerializerSettings),
                MessageType = MessageType.Text
            };
            await InvokeClientMethodAsync("Kalin", Actions.UpdateTemps.ToString(), new object[] { json });
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
