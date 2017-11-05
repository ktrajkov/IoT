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

namespace IoT.WebMVC.IoTHandler
{
    public class IoTMessageHandler : WebSocketHandler
    {
        private IBroker _broker;
        public IoTMessageHandler(IBroker broker, WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
            _broker = broker;
        }

        public virtual async Task UpdateTemps(JObject jobject)
        {
            //Save to DB   
            var tempsTM = jobject.ToObject<TempsTM>();

            foreach (var temp in tempsTM.Temps)
            {
                _broker.SendData(ChanelType.Temp.ToString().ToLower() + temp.Id, temp.Value.ToString());
            }

            var json = new Message
            {
                Data = JsonConvert.SerializeObject(jobject, _jsonSerializerSettings),
                MessageType = MessageType.Text
            };
            await InvokeClientMethodToAllAsync(Actions.UpdateTemps.ToString(), json);
        }
    }

    public enum Actions
    {
        UpdateTemps
    }

    public enum ChanelType
    {
        Temp
    }
}
