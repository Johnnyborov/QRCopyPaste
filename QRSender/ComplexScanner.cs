using System.Threading.Tasks;
using System.Windows;
using ZXing;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public static class ComplexScanner
    {
        public static void StartScanner()
        {
            Task.Run(() => ScanForFullMessage());
        }


        private static async Task ScanForFullMessage()
        {
            var settingsResult = await WaitForQR();
            var settingsData = settingsResult.Text;
            int numberOfParts = int.Parse(settingsData);

            string fullData = "";
            int currentPart = 0;
            while (currentPart < numberOfParts)
            {
                var dataPartResult = await WaitForQR();
                var currentData = dataPartResult.Text;

                if (currentData != settingsData)
                {
                    currentPart++;
                    fullData += currentData;
                }

                await Task.Delay(50);
            }

            MessageBox.Show($"Scanned: {fullData}");
        }


        private static async Task<Result> WaitForQR()
        {
            Result barcodeResult = null;
            while (barcodeResult == null)
            {
                var bmp = CreateBitmapFromScreen();
                barcodeResult = DecodeFromQR(bmp);
                await Task.Delay(50);
            }

            return barcodeResult;
        }
    }
}
