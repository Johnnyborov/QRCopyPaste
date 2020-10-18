namespace ChunkedDataTransfer
{
    public class QRDataPartMessage
    {
        public string MsgIntegrity { get; set; }
        public int ID { get; set; }
        public string Data { get; set; }
        public string DataHash { get; set; }
        public string ObjectID { get; set; }
    }
}
