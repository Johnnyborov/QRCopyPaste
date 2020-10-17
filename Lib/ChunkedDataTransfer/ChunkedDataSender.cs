using System;
using System.Threading.Tasks;

namespace ChunkedDataTransfer
{
    public class ChunkedDataSender
    {
        private readonly IDataSender dataSender;

        public ChunkedDataSender(IDataSender dataSender)
        {
            this.dataSender = dataSender;
        }


        public async Task Send<TData>(TData data)
        {
            if (data is string dataStr)
            {
                this.dataSender.Send(dataStr);
                await Task.Delay(250);

                this.dataSender.Send(dataStr);
                await Task.Delay(250);

                this.dataSender.Stop();
            }
        }


        public async Task ResendLast(int[] ids)
        {
            throw new NotImplementedException();
        }


        public void StopSending()
        {
            this.dataSender.Stop();
            throw new NotImplementedException();
        }


        public void ClearCache()
        {
            throw new NotImplementedException();
        }
    }
}
