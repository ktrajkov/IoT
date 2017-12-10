using IoT.CM.Common;
using IoT.CM.Managers;
using IoT.CM.Models;
using IoT.CM.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.CM.WS
{
    public abstract class WebSocketHandler
    {
        protected WebSocketConnectionManager ConnectionManager { get; set; }
        protected ClientHandler ClientHandler { get; set; }
        protected JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        public WebSocketHandler(WebSocketConnectionManager connectionManager, ClientHandler clientHandler)
        {
            ConnectionManager = connectionManager;
            ClientHandler = clientHandler;
        }

        public virtual async Task OnConnected(Client client, WebSocket socket)
        {
            ConnectionManager.AddConnection(client, socket);

            if (client.ClientType == ClientType.ESP8266)
            {
                //TODO: Get ClientId from DB
                ClientHandler.OnConnected(1);
            }

            await SendMessageAsync(socket, new Message()
            {
                MessageType = MessageType.ConnectionEvent,
                Data = client.Id
            }).ConfigureAwait(false);

        }

        public async Task ReceiveAsync(WebSocket socket, string clientId, WebSocketReceiveResult result, string serializedInvocationDescriptor)
        {
            try
            {
                var invocationDescriptor = JsonConvert.DeserializeObject<InvocationDescriptor>(serializedInvocationDescriptor);
                var method = this.GetType().GetMethod(invocationDescriptor.MethodName);
                if (method == null)
                {
                    await SendMessageAsync(socket, new Message()
                    {
                        MessageType = MessageType.Text,
                        Data = $"Cannot find method {invocationDescriptor.MethodName}"
                    }).ConfigureAwait(false);
                    return;
                }

                try
                {
                    method.Invoke(this, invocationDescriptor.Arguments);
                }
                catch (TargetParameterCountException ex)
                {
                    await SendMessageAsync(socket, new Message()
                    {
                        MessageType = MessageType.Text,
                        Data = $"The {invocationDescriptor.MethodName} method does not take {invocationDescriptor.Arguments.Length} parameters!"
                    }).ConfigureAwait(false);
                }

                catch (ArgumentException ex)
                {
                    await SendMessageAsync(socket, new Message()
                    {
                        MessageType = MessageType.Text,
                        Data = $"The {invocationDescriptor.MethodName} method takes different arguments!"
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task SendMessageAsync(string socketId, Message message)
        {
            var socket = ConnectionManager.GetSocketById(socketId);
            if (socket != null)
                await SendMessageAsync(socket, message).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(WebSocket socket, Message message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var serializedMessage = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            var encodedMessage = Encoding.UTF8.GetBytes(serializedMessage);
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: encodedMessage,
                                                                  offset: 0,
                                                                  count: encodedMessage.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        public async Task SendMessageToAllAsync(Message message)
        {
            foreach (var socket in ConnectionManager.GetAllSocket())
            {
                if (socket.State == WebSocketState.Open)
                    await SendMessageAsync(socket, message).ConfigureAwait(false);
            }
        }

        public async Task InvokeClientMethodAsync(string socketId, string methodName, object[] arguments)
        {
            var message = new Message()
            {
                MessageType = MessageType.ClientMethodInvocation,
                Data = JsonConvert.SerializeObject(new InvocationDescriptor()
                {
                    MethodName = methodName,
                    Arguments = arguments
                }, _jsonSerializerSettings)
            };

            await SendMessageAsync(socketId, message).ConfigureAwait(false);
        }

        //public async Task InvokeClientMethodToAllAsync(string methodName, params object[] arguments)
        //{
        //    foreach (var socket in ConnectionManager.GetAllSocket())
        //    {
        //        if (socket.State == WebSocketState.Open)
        //            await InvokeClientMethodAsync(socket, methodName, arguments).ConfigureAwait(false);
        //    }
        //}


        public virtual async Task OnDisconnected(WebSocket socket)
        {
            ConnectionManager.RemoveConnection(socket);
            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                  statusDescription: "Closed by the WebSocketManager",
                                  cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        public void OnReceiveException(WebSocket socket)
        {
            ConnectionManager.RemoveConnection(socket);
        }
    }
}