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
            int numberOfParts = 1;
            var settings = numberOfParts.ToString();

            var settingsWritableBitmap = EncodeToQR(settings);
            this._imageSourceHolder.ImageSource = settingsWritableBitmap;
            await Task.Delay(500);


            for (int i = 0; i < numberOfParts; i++)
            {
                string msgPart = msg;
                var dataPartWritableBitmap = EncodeToQR(msgPart);
                this._imageSourceHolder.ImageSource = dataPartWritableBitmap;
                await Task.Delay(500);
            }

            this._imageSourceHolder.ImageSource = null;
        }
    }
}
