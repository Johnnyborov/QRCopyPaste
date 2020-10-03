using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            int chunkSize = 3;
            var dataParts = SplitStringToChunks(msg, chunkSize).ToArray();

            var numberOfParts = dataParts.Length;
            var settings = numberOfParts.ToString();

            var settingsWritableBitmap = EncodeToQR(settings);
            this._imageSourceHolder.ImageSource = settingsWritableBitmap;
            await Task.Delay(250);

            string fullData = "";
            for (int i = 0; i < numberOfParts; i++)
            {
                string msgPart = dataParts[i];
                var dataPartWritableBitmap = EncodeToQR(msgPart);
                this._imageSourceHolder.ImageSource = dataPartWritableBitmap;
                await Task.Delay(250);
                fullData += msgPart;
            }
            MessageBox.Show($"Sent: {fullData}");

            this._imageSourceHolder.ImageSource = null;
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
