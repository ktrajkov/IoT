namespace IoT.CM.Common
{
    public enum MessageType
    {
        Text,
        ClientMethodInvocation,
        ConnectionEvent
    }

    public class Message
    {
        public MessageType MessageType { get; set; }
        public object Data { get; set; }
    }
}