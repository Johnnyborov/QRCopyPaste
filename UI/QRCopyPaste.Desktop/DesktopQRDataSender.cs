using ChunkedDataTransfer;

namespace QRCopyPaste
{
    public class DesktopQRDataSender : IDataSender
    {
        private readonly ISenderViewModel senderViewModel;

        public DesktopQRDataSender(ISenderViewModel senderViewModel)
        {
            this.senderViewModel = senderViewModel;
        }


        public void Send(string data)
        {
            var imgSource = QRSenderHelper.CreateQRWritableBitampFromString(data);
            this.senderViewModel.ImageSource = imgSource;
        }

        public void Stop()
        {
            this.senderViewModel.ImageSource = null;
        }
    }
}
