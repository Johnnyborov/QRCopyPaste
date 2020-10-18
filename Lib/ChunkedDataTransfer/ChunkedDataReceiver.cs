using System;
using System.Text;

namespace ChunkedDataTransfer
{
    public class ChunkedDataReceiver
    {
        public event StringDataReceivedEventHandler OnStringDataReceived;
        public event ByteDataReceivedEventHandler OnByteDataReceived;


        private readonly StringBuilder sb;

        public ChunkedDataReceiver()
        {
            this.sb = new StringBuilder();
        }


        public void StartReceiving()
        {
            this.sb.Clear();
        }


        public void ClearCache()
        {
            throw new NotImplementedException();
        }


        public void ProcessChunk(string dataChunk)
        {
            sb.Append(dataChunk);

            if (true) // fully received
            {
                string fullData = sb.ToString();
                this.sb.Clear();
                //this.OnStringDataReceived?.Invoke(fullData);
            }
        }
    }
}
