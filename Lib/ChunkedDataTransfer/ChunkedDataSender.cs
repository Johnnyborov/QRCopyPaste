using System;
using System.Threading.Tasks;

namespace ChunkedDataTransfer
{
    public class ChunkedDataSender
    {
        public int ChunkSize { get; set; } = 500;


        private readonly IDataSender dataSender;

        public ChunkedDataSender(IDataSender dataSender)
        {
            this.dataSender = dataSender;
        }


        public async Task SendAsync<TData>(TData data)
        {
            if (data is null)
                throw new ArgumentNullException($"{nameof(data)} is null in {nameof(SendAsync)}");

            if (data is string dataStr)
            {
                //split by this.ChunkSize
                await this.dataSender.SendAsync(dataStr);
                await this.dataSender.SendAsync(dataStr);
                this.dataSender.Stop();
            }
        }


        public async Task ResendLastAsync(int[] ids)
        {
            throw new NotImplementedException();
        }


        public void StopSending()
        {
            this.dataSender.Stop();
            throw new NotImplementedException();
        }
    }
}
