using IoT.Data.Contracts.TransportModels;
using IoT.WSManager;
using IoT.WSManager.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace IoT.WebMVC.IoTHandler
{
    public class IoTMessageHandler : WebSocketHandler
    {
        public IoTMessageHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager) { }

        public virtual async Task UpdateTemps(JObject jobject)
        {
            //Save to DB   
           var tempsTM = jobject.ToObject<TempsTM>();

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

   
}
