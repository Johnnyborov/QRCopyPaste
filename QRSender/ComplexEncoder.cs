using System;
using System.Threading.Tasks;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public class ComplexEncoder
    {
        private static bool _stopRequested = false;
        private static bool _isRunning = false;
        private IImageSourceHolder _imageSourceHolder;

        public ComplexEncoder(IImageSourceHolder imageSourceHolder)
        {
            this._imageSourceHolder = imageSourceHolder;
        }


        public async Task SendData<TData>(TData data)
        {
            if (_isRunning)
                throw new Exception("Data sending is already in progress.");
            _isRunning = true;

            var qrMessagesPackage = QRMessageCreator.CreateQRMessagesPackage(data);

            await SendQRMessageSettingsAsync(qrMessagesPackage.QRSettingsMessage);
            await SendAllDataPartsAsync(qrMessagesPackage.QRDataPartsMessages);

            this._imageSourceHolder.ImageSource = null; // Remove last DataPart QR from screen.
            _isRunning = false;
            _stopRequested = false;
        }


        public static void RequestStop()
        {
            _stopRequested = true;
        }


        private async Task SendAllDataPartsAsync(string[] dataParts)
        {
            this._imageSourceHolder.SenderProgress = 1;

            var numberOfParts = dataParts.Length;
            for (int i = 0; i < numberOfParts; i++)
            {
                if (_stopRequested)
                    return;

                string dataPart = dataParts[i];

                var dataPartWritableBitmap = EncodeToQR(dataPart);
                this._imageSourceHolder.ImageSource = dataPartWritableBitmap;

                await Task.Delay(QRSenderSettings.SendDelayMilliseconds);

                this._imageSourceHolder.SenderProgress = 1 + 99 * (i + 1) / numberOfParts;
            }
        }


        private async Task SendQRMessageSettingsAsync(string settingsMessage)
        {
            var settingsWritableBitmap = EncodeToQR(settingsMessage);
            this._imageSourceHolder.ImageSource = settingsWritableBitmap;

            await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
        }
    }
}
