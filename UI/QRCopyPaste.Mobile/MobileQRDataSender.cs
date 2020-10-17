using ChunkedDataTransfer;
using System;

namespace QRCopyPaste.Mobile
{
    public class MobileQRDataSender : IDataSender
    {
        private readonly Action<string> sendAction;
        private readonly Action stopAction;

        public MobileQRDataSender(
            Action<string> sendAction,
            Action stopAction)
        {
            this.sendAction = sendAction;
            this.stopAction = stopAction;
        }


        public void Send(string data)
        {
            this.sendAction.Invoke(data);
        }

        public void Stop()
        {
            this.stopAction.Invoke();
        }
    }
}
