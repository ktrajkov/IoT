namespace IoT.Broker
{
    public class MsgPublishReceivedEventArgs
    {
        public string Chanel { get; set; }

        public object Value { get; set; }
    }
}
