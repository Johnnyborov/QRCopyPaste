using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public class ComplexEncoder
    {
        private IImageSourceHolder _imageSourceHolder;

        public ComplexEncoder(IImageSourceHolder imageSourceHolder)
        {
            this._imageSourceHolder = imageSourceHolder;
        }


        public async Task CreateComplex(string msg)
        {
            var dataParts = SplitStringToChunks(msg, QRSenderSettings.ChunkSize).ToArray();

            await SendQRMessageSettingsAsync(dataParts);
            await SendAllDataPartsAsync(dataParts);

            this._imageSourceHolder.ImageSource = null; // Remove last DataPart QR from screen.
        }


        private async Task SendAllDataPartsAsync(string[] dataParts)
        {
            var numberOfParts = dataParts.Length;
            for (int i = 0; i < numberOfParts; i++)
            {
                string dataPart = dataParts[i];

                var dataPartWritableBitmap = EncodeToQR(dataPart);
                this._imageSourceHolder.ImageSource = dataPartWritableBitmap;

                await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
            }
        }


        private async Task SendQRMessageSettingsAsync(string[] dataParts)
        {
            var numberOfParts = dataParts.Length;
            var settings = numberOfParts.ToString();

            var settingsWritableBitmap = EncodeToQR(settings);
            this._imageSourceHolder.ImageSource = settingsWritableBitmap;

            await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
        }


        private static IEnumerable<string> SplitStringToChunks(string fullStr, int chunkSize)
        {
            var numberOfChunks =
                fullStr.Length % chunkSize == 0
                ? fullStr.Length / chunkSize
                : fullStr.Length / chunkSize + 1;


            var chunks =
                Enumerable.Range(0, numberOfChunks)
                .Select(chunkNum =>
                {
                    int substringSize =
                        chunkNum * chunkSize + chunkSize > fullStr.Length
                        ? fullStr.Length % chunkSize
                        : chunkSize;

                    return fullStr.Substring(chunkNum * chunkSize, substringSize);
                });


            return chunks;
        }
    }
}
