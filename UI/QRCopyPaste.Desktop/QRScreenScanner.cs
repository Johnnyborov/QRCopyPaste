using System;
using System.Threading.Tasks;
using ZXing;

namespace QRCopyPaste
{
    public class QRScreenScanner
    {
        public event QRTextDataReceivedEventHandler OnQRTextDataReceived;
        public event ErrorHappenedEventHandler OnError;


        private static bool _stopRequested = false;
        private static bool _isRunning = false;

        public bool StartScanning()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequestedAsync());
                return true;
            }

            return false;
        }


        private async Task RunScansUntilStopRequestedAsync()
        {
            while (!_stopRequested)
            {
                try
                {
                    var data = await WaitForSuccessfullyDecodedQRAsync();
                    this.OnQRTextDataReceived?.Invoke(data);
                }
                catch (Exception ex)
                {
                    OnError(ex.Message);
                }
            }
        }


        private static async Task<string> WaitForSuccessfullyDecodedQRAsync()
        {
            Result barcodeResult = null;
            while (barcodeResult == null)
            {
                var bitmap = QRMessageScannerHelper.CreateBitmapFromScreen();
                barcodeResult = QRMessageScannerHelper.GetBarcodeResultFromQRBitmap(bitmap);
                await Task.Delay(50);
            }

            return barcodeResult.Text;
        }
    }
}
