using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ZXing;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public class ComplexScanner
    {
        private static bool _stopRequested = false;
        private static bool _isRunning = false;

        private static int _scanCycle = 0;
        private static Stopwatch _stopwatchSinceLastSuccessfulQRRead = new Stopwatch();
        private static Dictionary<string, Dictionary<int, string>> _receivedItems = new Dictionary<string, Dictionary<int, string>>();


        private static IReceiverViewModel _receiverViewModel;

        public ComplexScanner(IReceiverViewModel receiverViewModel)
        {
            _receiverViewModel = receiverViewModel;
        }


        public bool StartScanner(Action<object> messageReceivedAction, Action<string> onErrorAction)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequested(messageReceivedAction, onErrorAction));
                return true;
            }

            return false;
        }


        public static void ClearItems()
        {
            _receivedItems.Clear();
            _receiverViewModel.Progress = 0;
        }


        private async Task RunScansUntilStopRequested(Action<object> messageReceivedAction, Action<string> onErrorAction)
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


        private async Task ScanForFullMessageAsync(Action<object> messageReceivedAction)
        {
            var qrMessageSettings = await ScanQRMessageSettingsAsync();
            if (qrMessageSettings == null || qrMessageSettings.MsgIntegrity != Constants.QRSettingsMessageIntegrityCheckID)
                return; // Was not a QRMessageSettings (or failed to deserialize for some other reason).

            _scanCycle++;
            _receiverViewModel.ScanCycle = _scanCycle;
            _receiverViewModel.Progress = 1;
            var dataStrParts = await ScanAllDataStrPartsAsync(qrMessageSettings);
            var fullDataStr = string.Join("", dataStrParts.Values);

            var dataHash = GetStringHash(fullDataStr);
            if (dataHash != qrMessageSettings.DataHash)
            {
                var missingPartsCount = qrMessageSettings.NumberOfParts - dataStrParts.Count;
                throw new Exception(
                    $"Received data is incorrect.\n" +
                    $"{dataStrParts.Count} out of {qrMessageSettings.NumberOfParts} parts received.\n" +
                    $"{missingPartsCount} parts missing."
                );
            }


            var fullData = ConvertToInitialTypeFromString(fullDataStr, qrMessageSettings.DataType);
            messageReceivedAction(fullData);
        }


        private static object ConvertToInitialTypeFromString(string dataStr, string dataType)
        {
            if (dataType == Constants.StringTypeName)
                return dataStr;
            else if (dataType == Constants.ByteArrayTypeName)
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


        private async Task<Dictionary<int, string>> ScanAllDataStrPartsAsync(QRMessageSettings qrMessageSettings)
        {
            if (!_receivedItems.ContainsKey(qrMessageSettings.DataHash))
                _receivedItems.Add(qrMessageSettings.DataHash, new Dictionary<int, string>());
 
            var dataParts = _receivedItems[qrMessageSettings.DataHash];
            _receiverViewModel.Progress = 1 + 99 * dataParts.Count / qrMessageSettings.NumberOfParts;

            var maxScanTime = qrMessageSettings.SenderDelay * (qrMessageSettings.NumberOfParts + 1) * 3 / 2;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _stopwatchSinceLastSuccessfulQRRead.Restart();
            while (dataParts.Count < qrMessageSettings.NumberOfParts
                && stopwatch.ElapsedMilliseconds < maxScanTime
                && _stopwatchSinceLastSuccessfulQRRead.ElapsedMilliseconds < QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead)
            {
                var delay = qrMessageSettings.SenderDelay * 1 / 7; // Scan has to be faster than the display time.
                var dataPartResult = await WaitForSuccessfullyDecodedQRAsync(delay, qrMessageSettings.SenderDelay * 3 / 2);
                if (dataPartResult == null)
                    continue; // Couldn't read QR code or there were none (e.g. didn't recieve all parts but sender already stopped).

                var currentDataStr = dataPartResult.Text;
                var currentData = TryDeserialize<QRDataPartMessage>(currentDataStr);
                if (currentData == null || currentData.MsgIntegrity != Constants.QRDataPartMessageIntegrityCheckID)
                    continue; // Was not a QRDataPartMessage (or failed to deserialize for some other reason).

                if (!dataParts.ContainsKey(currentData.ID))
                {
                    dataParts.Add(currentData.ID, currentData.Data);
                }
                _receiverViewModel.Progress = 1 + 99 * dataParts.Count / qrMessageSettings.NumberOfParts;
            }

            return dataParts;
        }


        private static async Task<Result> WaitForSuccessfullyDecodedQRAsync(int delay, int maxScanTime)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Result barcodeResult = null;
            while (barcodeResult == null && stopwatch.ElapsedMilliseconds < maxScanTime)
            {
                var bitmap = CreateBitmapFromScreen();
                barcodeResult = DecodeFromQR(bitmap);
                await Task.Delay(delay);
            }

            if (barcodeResult != null)
                _stopwatchSinceLastSuccessfulQRRead.Restart();

            return barcodeResult;
        }
    }
}
