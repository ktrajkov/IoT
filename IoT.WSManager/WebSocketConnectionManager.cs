using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.WSManager
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        private ConcurrentDictionary<string, List<string>> _groups = new ConcurrentDictionary<string, List<string>>();

        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }
        
        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public List<string> GetAllFromGroup(string GroupID)
        {
            if (_groups.ContainsKey(GroupID))
            {
                return _groups[GroupID];
            }

            return default(List<string>);
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void AddSocket(string cliendId, WebSocket socket)
        {
            WebSocket removedSocket;
            _sockets.TryRemove(cliendId, out removedSocket);
            _sockets.TryAdd(cliendId, socket);
        }

        public void AddToGroup(string socketID, string groupID)
        {
            if (_groups.ContainsKey(groupID))
            {
                var list = _groups[groupID];
                list.Add(socketID);
                _groups[groupID] = list;

                return;
            }

            _groups.TryAdd(groupID, new List<string> { socketID });
        }

        public void RemoveFromGroup(string socketID, string groupID)
        {
            if (_groups.ContainsKey(groupID))
            {
                var list = _groups[groupID];
                list.Remove(socketID);
                _groups[groupID] = list;

                return;
            }
        }

        public WebSocket RemoveSocket(WebSocket socket)
        {
            var id = GetId(socket);
            WebSocket removedSocket;             
            _sockets.TryRemove(id, out removedSocket);

            return removedSocket;          
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
