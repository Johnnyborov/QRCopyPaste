using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Presentation;
using ZXing.QrCode.Internal;

namespace QRSender
{
    public static class HelperFunctions
    {
        public static string GetStringHash(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr))
                return string.Empty;

            var dataBytes = Encoding.UTF8.GetBytes(dataStr);
            using var sha = new SHA256Managed();
            var hashBytes = sha.ComputeHash(dataBytes);
            var hashStr = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hashStr;
        }


        public static WriteableBitmap EncodeToQR(string data)
        {
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

            var writableBitmap = barcodeWriter.Write(data);
            return writableBitmap;
        }


        public static Result DecodeFromQR(Bitmap bitmap)
        {
            var luminanceSource = new BitmapSourceLuminanceSource(CreateBitmapSourceFromBitmap(bitmap));

            var barcodeReader = new ZXing.BarcodeReader();
            var barcodeResult = barcodeReader.Decode(luminanceSource);
            return barcodeResult;
        }


        public static Bitmap CreateBitmapFromScreen()
        {
            var bitmap = new Bitmap(1920, 1080);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }


        private static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            try
            {
                const int bytesPerPixel = 4;
                var bufferSize = (rect.Width * rect.Height) * bytesPerPixel;

                var bitmapSource = BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    bufferSize,
                    bitmapData.Stride
                );
                return bitmapSource;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
