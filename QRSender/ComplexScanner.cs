using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ZXing;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public static class ComplexScanner
    {
        private static bool _stopRequested = false;
        private static bool _isRunning = false;


        public static bool StartScanner(Action<object> messageReceivedAction, Action<string> onErrorAction)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequested(messageReceivedAction, onErrorAction));
                return true;
            }

            return false;
        }


        private static async Task RunScansUntilStopRequested(Action<object> messageReceivedAction, Action<string> onErrorAction)
        {
            while (!_stopRequested)
            {
                try
                {
                    await ScanForFullMessageAsync(messageReceivedAction);
                }
                catch (Exception ex)
                {
                    onErrorAction(ex.Message);
                }
            }
        }


        private static async Task ScanForFullMessageAsync(Action<object> messageReceivedAction)
        {
            var qrMessageSettings = await ScanQRMessageSettingsAsync();
            if (qrMessageSettings == null)
                return; // Was not a QRMessageSettings (or failed to deserialize for some other reason).

            var dataStrParts = await ScanAllDataStrPartsAsync(qrMessageSettings);
            var fullDataStr = string.Join("", dataStrParts.Values);

            var fullData = ConvertToInitialTypeFromString(fullDataStr, qrMessageSettings.DataType);
            messageReceivedAction(fullData);
        }


        private static object ConvertToInitialTypeFromString(string dataStr, string dataType)
        {
            if (dataType == "string")
                return dataStr;
            else if (dataType == "byte[]")
                return Convert.FromBase64String(dataStr);
            else
                throw new Exception($"Unsupported data type {dataType} in {nameof(ConvertToInitialTypeFromString)}.");
        }


        private static async Task<QRMessageSettings> ScanQRMessageSettingsAsync()
        {
            var settingsResult = await WaitForSuccessfullyDecodedQRAsync(QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds, int.MaxValue);
            var settingsData = settingsResult.Text;

            try
            {
                var qrMessageSettings = JsonSerializer.Deserialize<QRMessageSettings>(settingsData);
                qrMessageSettings.ReceivedIDs = GetReceivedIDs(settingsData);

                return qrMessageSettings;
            }
            catch (JsonException) // Was not a QRMessageSettings (or failed to deserialize for some other reason).
            {
                return null;
            }
        }


        private static HashSet<string> GetReceivedIDs(string settingsData)
        {
            var receivedIDs = new HashSet<string>();
            var settingsDataID = settingsData;
            receivedIDs.Add(settingsDataID);
            return receivedIDs;
        }


        private static async Task<Dictionary<string, string>> ScanAllDataStrPartsAsync(QRMessageSettings qrMessageSettings)
        {
            var dataParts = new Dictionary<string, string>();

            var maxScanTime = qrMessageSettings.SenderDelay * (qrMessageSettings.NumberOfParts + 1);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var receivedIDs = qrMessageSettings.ReceivedIDs;
            int currentPart = 0;
            while (currentPart < qrMessageSettings.NumberOfParts && stopwatch.ElapsedMilliseconds < maxScanTime)
            {
                var delay = qrMessageSettings.SenderDelay * 2 / 3; // Scan has to be faster than the display time.
                var dataPartResult = await WaitForSuccessfullyDecodedQRAsync(delay, 2);
                var currentData = dataPartResult.Text;
                var currentDataID = currentData;

                if (!receivedIDs.Contains(currentDataID))
                {
                    receivedIDs.Add(currentDataID);
                    dataParts.Add(currentDataID, currentData);
                    currentPart++;
                }
            }

            return dataParts;
        }


        private static async Task<Result> WaitForSuccessfullyDecodedQRAsync(int delay, int maxAttempts)
        {
            int attempts = 0;
            Result barcodeResult = null;
            while (barcodeResult == null && attempts < maxAttempts)
            {
                var bitmap = CreateBitmapFromScreen();
                barcodeResult = DecodeFromQR(bitmap);
                await Task.Delay(delay);
                attempts++;
            }

            if (barcodeResult == null)
                throw new Exception($"MaxAttempts={maxAttempts} reached while waiting to decode QR.");

            return barcodeResult;
        }
    }
}
