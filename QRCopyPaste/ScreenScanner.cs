using System.Threading.Tasks;
using System.Windows;
using ZXing;
using static QRCopyPaste.HelperFunctions;

namespace QRCopyPaste
{
    public static class ScreenScanner
    {
        public static void StartScanner()
        {
            Task.Run(() => WaitForQR());
        }


        private static async Task WaitForQR()
        {
            Result barcodeResult = null;
            while (barcodeResult == null)
            {
                var bmp = CreateBitmapFromScreen();
                barcodeResult = DecodeFromQR(bmp);
                await Task.Delay(50);
            }

            MessageBox.Show($"Scanned: {barcodeResult.Text}");
        }
    }
}
