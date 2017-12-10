using IoT.CM.Clients;
using IoT.CM.Models;
using IoT.CM.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.CM.Managers
{
    public class ClientHandler
    {
        //TODO: List of BaseClientManagers
        private BaseClientManager _baseClientManager;
        private ClientConnectionManager _clientConnectionManager;

        public event EventHandler<MsgReceivedEventArgs> MsgReceived;

        public ClientHandler(ClientConnectionManager clientConnectionManager, BaseClientManager baseClientManager)
        {
            _baseClientManager = baseClientManager;
            _clientConnectionManager = clientConnectionManager;
        }

        public void OnConnected(int id)
        {
            //TODO: get id from db
            if(!_clientConnectionManager.Contains(id))
            {
                var extClient = _baseClientManager.Connect("kaluhckua", "7f4e0351e04dcc8081bc81caff53270d67152c28");
                _baseClientManager.MsgReceived += MsgReceived;
                _baseClientManager.Subscribe(extClient, ChanelType.Relay.ToString(), ChanelType.FirmwareUpdate.ToString());
                _clientConnectionManager.Add(id, extClient);
            }
        }

        public void SendMessage(int id, string chanel, string message)
        {
            var clients = _clientConnectionManager.GetAllClients(id);
            foreach (var client in clients)
            {
                if (client is AdafruitClient && _baseClientManager is AdafruitClientManager) _baseClientManager.SendMessage(client, chanel, message);
            }
        }
    }
}
