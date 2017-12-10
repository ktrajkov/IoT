using IoT.CM.Clients;
using IoT.CM.Models;
using IoT.CM.Models.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace IoT.CM.Managers
{
    public class ClientConnectionManager
    {
        private ConcurrentDictionary<int, List<IClient>> _connections = new ConcurrentDictionary<int, List<IClient>>();

        public List<IClient> GetAllClients(int id)
        {
            if(_connections.ContainsKey(id))
            {
                return _connections[id];
            }
            return null;
        }

        public bool Contains(int id)
        {
            return _connections.ContainsKey(id);
        }

        public void Add(int id, IClient client)
        {
            //TODO: Get username and key from DB
            //TODO: should wotk with list of IClient          
            _connections.TryAdd(id, new List<IClient> { client });
        }
    }   
}
