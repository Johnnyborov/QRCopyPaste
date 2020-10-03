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

            var dataHash = GetStringHash(fullDataStr);
            if (dataHash != qrMessageSettings.DataHash)
                throw new Exception("Received data is incorrect.");


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
            var qrMessageSettings = TryDeserialize<QRMessageSettings>(settingsData);
            return qrMessageSettings;
        }


        private static TData TryDeserialize<TData>(string dataStr)
        {
            try
            {
                var data = JsonSerializer.Deserialize<TData>(dataStr);
                return data;
            }
            catch (JsonException)
            {
                return default;
            }
        }


        private static HashSet<string> GetReceivedIDs(string settingsData)
        {
            var receivedIDs = new HashSet<string>();
            var settingsDataID = settingsData;
            receivedIDs.Add(settingsDataID);
            return receivedIDs;
        }


        private static async Task<Dictionary<int, string>> ScanAllDataStrPartsAsync(QRMessageSettings qrMessageSettings)
        {
            var receivedIDs = new HashSet<int>();
            var dataParts = new Dictionary<int, string>();

            var maxScanTime = qrMessageSettings.SenderDelay * (qrMessageSettings.NumberOfParts + 1);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int currentPart = 0;
            while (currentPart < qrMessageSettings.NumberOfParts && stopwatch.ElapsedMilliseconds < maxScanTime)
            {
                var delay = qrMessageSettings.SenderDelay * 1 / 3; // Scan has to be faster than the display time.
                var dataPartResult = await WaitForSuccessfullyDecodedQRAsync(delay, 5);
                var currentDataStr = dataPartResult.Text;
                var currentData = TryDeserialize<QRDataPartMessage>(currentDataStr);
                if (currentData == null)
                    continue; // Was not a QRDataPartMessage (or failed to deserialize for some other reason).

                if (!receivedIDs.Contains(currentData.ID))
                {
                    receivedIDs.Add(currentData.ID);
                    dataParts.Add(currentData.ID, currentData.Data);
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
