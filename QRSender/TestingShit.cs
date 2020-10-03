using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public static class TestingShit
    {
        public static void QREncodeDecode()
        {
            string data = "test msg";

            var writableBitmap = EncodeToQR(data);
            //var barcodeResult = barcodeReader.Decode(new BitmapSourceLuminanceSource(writableBitmap));

            var bmp = WritableBitmapToBitmap(writableBitmap);
            var barcodeResult = DecodeFromQR(bmp);

            MessageBox.Show($"{barcodeResult.Text}");
        }


        private static Bitmap WritableBitmapToBitmap(WriteableBitmap writableBitmap)
        {
            Bitmap bitmap;
            using (MemoryStream ms = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();

                var frame = BitmapFrame.Create(writableBitmap);
                encoder.Frames.Add(frame);
                encoder.Save(ms);

                bitmap = new Bitmap(ms);
            }
            return bitmap;
        }
    }
}
