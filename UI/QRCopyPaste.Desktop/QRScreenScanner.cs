using ChunkedDataTransfer;
using System;
using System.Threading.Tasks;
using ZXing;

namespace QRCopyPaste
{
    public class QRScreenScanner
    {
        public event DataReceivedEventHandler OnDataReceived;
        public event DataReceivedEventHandler OnError;


        private static bool _stopRequested = false;
        private static bool _isRunning = false;

        public bool StartScanning()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequested());
                return true;
            }

            return false;
        }


        private async Task RunScansUntilStopRequested()
        {
            while (!_stopRequested)
            {
                try
                {
                    var data = await WaitForSuccessfullyDecodedQRAsync();
                    this.OnDataReceived?.Invoke(data);
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
