using IoT.CM.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace IoT.CM.WS
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _connections = new ConcurrentDictionary<string, WebSocket>();

        public IEnumerable<WebSocket> GetAllSocket()
        {
            return _connections.Values;
        }

        public WebSocket GetSocketById(string id)
        {
            return _connections.FirstOrDefault(p => p.Key == id).Value;
        }

        public string GetClientId(WebSocket socket)
        {
            return _connections.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void AddConnection(Client client, WebSocket socket)
        {
            WebSocket removedClient;
            _connections.TryRemove(client.Id, out removedClient);
            _connections.TryAdd(client.Id, socket);
        }

        public WebSocket RemoveConnection(WebSocket socket)
        {
            var clientId = GetClientId(socket);
            WebSocket removedConnection;
            _connections.TryRemove(clientId, out removedConnection);

            return removedConnection;
        }
    }
}
