using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using IoT.CM.Models;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace IoT.CM.Clients
{
    public class AdafruitClient : MqttClient, IClient
    {
        public string Username { get; set; }
        public string Key { get; set; }

        public AdafruitClient(string brokerHostName) : base(brokerHostName) { }

        public AdafruitClient(string brokerHostName, int brokerPort, bool secure, MqttSslProtocols sslProtocol,
            RemoteCertificateValidationCallback userCertificateValidationCallback,
            LocalCertificateSelectionCallback userCertificateSelectionCallback) : base(brokerHostName, brokerPort, secure, sslProtocol, userCertificateValidationCallback, userCertificateSelectionCallback) { }
        
        public new void Publish(string chanel, byte[] data)
        {
            Publish(chanel, data);
        }
    }   
}
