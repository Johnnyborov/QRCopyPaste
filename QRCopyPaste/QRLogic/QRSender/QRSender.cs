using System;
using System.Threading.Tasks;

namespace QRCopyPaste
{
    public class QRSender
    {
        private static bool _stopRequested = false;
        private static bool _isRunning = false;
        private ISenderViewModel _senderViewModel;

        public QRSender(ISenderViewModel senderViewModel)
        {
            this._senderViewModel = senderViewModel;
        }


        public async Task SendData<TData>(TData data)
        {
            if (_isRunning)
                throw new Exception("Data sending is already in progress.");
            _isRunning = true;

            var qrPackage = QRPackageCreator.CreateQRPackage(data);

            await SendQRMessageSettingsAsync(qrPackage.QRPackageInfoMessage);
            await SendAllDataPartsAsync(qrPackage.QRDataPartsMessages);

            this._senderViewModel.ImageSource = null; // Remove last DataPart QR from screen.
            _isRunning = false;
            _stopRequested = false;
        }


        public static void RequestStop()
        {
            _stopRequested = true;
        }


        private async Task SendAllDataPartsAsync(string[] dataParts)
        {
            this._senderViewModel.SenderProgress = 1;

            var numberOfParts = dataParts.Length;
            for (int i = 0; i < numberOfParts; i++)
            {
                if (_stopRequested)
                    return;

                string dataPart = dataParts[i];

                var dataPartWritableBitmap = QRSenderHelper.CreateQRWritableBitampFromString(dataPart);
                this._senderViewModel.ImageSource = dataPartWritableBitmap;

                await Task.Delay(QRSenderSettings.SendDelayMilliseconds);

                this._senderViewModel.SenderProgress = 1 + 99 * (i + 1) / numberOfParts;
            }
        }


        private async Task SendQRMessageSettingsAsync(string settingsMessage)
        {
            var settingsWritableBitmap = QRSenderHelper.CreateQRWritableBitampFromString(settingsMessage);
            this._senderViewModel.ImageSource = settingsWritableBitmap;

            await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
        }
    }
}
