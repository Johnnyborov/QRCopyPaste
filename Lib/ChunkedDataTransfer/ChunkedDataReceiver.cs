using System.Text;

namespace ChunkedDataTransfer
{
    public class ChunkedDataReceiver
    {
        public event DataReceivedEventHandler OnDataReceived;


        private readonly StringBuilder sb;

        public ChunkedDataReceiver()
        {
            this.sb = new StringBuilder();
        }


        public void StartReceiving()
        {
            this.sb.Clear();
        }


        public void ProcessChunk(string dataChunk)
        {
            sb.Append(dataChunk);

            if (true) // fully received
            {
                string fullData = sb.ToString();
                this.sb.Clear();
                this.OnDataReceived?.Invoke(fullData);
            }
        }
    }
}
