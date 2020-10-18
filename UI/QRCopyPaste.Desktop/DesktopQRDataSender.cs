using ChunkedDataTransfer;
using System.Threading.Tasks;

namespace QRCopyPaste
{
    public class DesktopQRDataSender : IDataSender
    {
        private readonly ISenderViewModel senderViewModel;

        public DesktopQRDataSender(ISenderViewModel senderViewModel)
        {
            this.senderViewModel = senderViewModel;
        }


        public async Task SendAsync(string data)
        {
            var imgSource = QRSenderHelper.CreateQRWritableBitampFromString(data);
            this.senderViewModel.ImageSource = imgSource;
            await Task.Delay(500);
        }

        public void Stop()
        {
            this.senderViewModel.ImageSource = null;
        }
    }
}
