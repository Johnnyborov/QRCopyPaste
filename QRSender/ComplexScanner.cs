using System.Collections.Generic;
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


            var receivedIDs = new HashSet<string>();
            var settingsDataID = settingsData;
            receivedIDs.Add(settingsDataID);

            int currentPart = 0;
            var dateParts = new Dictionary<string, string>();
            while (currentPart < numberOfParts)
            {
                var dataPartResult = await WaitForQR();
                var currentData = dataPartResult.Text;
                var currentDataID = currentData;

                if (!receivedIDs.Contains(currentDataID))
                {
                    receivedIDs.Add(currentDataID);
                    dateParts.Add(currentDataID, currentData);

                    currentPart++;
                }

                await Task.Delay(50);
            }

            var fullData = string.Join("", dateParts.Values);

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
