namespace ChunkedDataTransfer
{
    public interface IDataSender
    {
        void Send(string data);
        void Stop();
    }
}