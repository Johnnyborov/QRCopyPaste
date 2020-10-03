using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZXing;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public static class ComplexScanner
    {
        private static bool _stopRequested = false;
        private static bool _isRunning = false;


        public static bool StartScanner(Action<string> messageReceivedAction)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequested(messageReceivedAction));
                return true;
            }

            return false;
        }


        private static async Task RunScansUntilStopRequested(Action<string> messageReceivedAction)
        {
            while (!_stopRequested)
            {
                await ScanForFullMessageAsync(messageReceivedAction);
            }
        }


        private static async Task ScanForFullMessageAsync(Action<string> messageReceivedAction)
        {
            var qrMessageSettings = await ScanQRMessageSettingsAsync();
            var dataParts = await ScanAllDataPartsAsync(qrMessageSettings);
            var fullData = string.Join("", dataParts.Values);

            messageReceivedAction(fullData);
        }


        private static async Task<QRMessageSettings> ScanQRMessageSettingsAsync()
        {
            var settingsResult = await WaitForSuccessfullyDecodedQRAsync();
            var settingsData = settingsResult.Text;

            var qrMessageSettings = new QRMessageSettings
            {
                NumberOfParts = int.Parse(settingsData),
                ReceivedIDs = GetReceivedIDs(settingsData),
            };

            return qrMessageSettings;
        }


        private static HashSet<string> GetReceivedIDs(string settingsData)
        {
            var receivedIDs = new HashSet<string>();
            var settingsDataID = settingsData;
            receivedIDs.Add(settingsDataID);
            return receivedIDs;
        }


        private static async Task<Dictionary<string, string>> ScanAllDataPartsAsync(QRMessageSettings qrMessageSettings)
        {
            var dataParts = new Dictionary<string, string>();

            var receivedIDs = qrMessageSettings.ReceivedIDs;
            int currentPart = 0;
            while (currentPart < qrMessageSettings.NumberOfParts)
            {
                var dataPartResult = await WaitForSuccessfullyDecodedQRAsync();
                var currentData = dataPartResult.Text;
                var currentDataID = currentData;

                if (!receivedIDs.Contains(currentDataID))
                {
                    receivedIDs.Add(currentDataID);
                    dataParts.Add(currentDataID, currentData);
                    currentPart++;
                }

                await Task.Delay(QRReceiverSettings.SendDelayMilliseconds);
            }

            return dataParts;
        }


        private static async Task<Result> WaitForSuccessfullyDecodedQRAsync()
        {
            Result barcodeResult = null;
            while (barcodeResult == null)
            {
                var bitmap = CreateBitmapFromScreen();
                barcodeResult = DecodeFromQR(bitmap);
                await Task.Delay(QRReceiverSettings.SendDelayMilliseconds);
            }

            return barcodeResult;
        }
    }
}
