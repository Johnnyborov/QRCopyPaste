using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZXing;
using ZXing.Presentation;

namespace QRSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _buttonName;
        private ImageSource _imageSource;

        public string ButtonName
        {
            get => this._buttonName;
            set
            {
                this._buttonName = value;
                OnPropertyChanged(nameof(ButtonName));
            }
        }

        public ImageSource ImageSource
        {
            get => this._imageSource;
            set
            {
                this._imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }



        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;

            var writableBitmap = barcodeWriter.Write("el contento");
            this.ImageSource = writableBitmap;

            this.ButtonName = "123";


            var barcodeReader = new ZXing.BarcodeReader();
            //var barcodeResult = barcodeReader.Decode(new BitmapSourceLuminanceSource(writableBitmap));


            var bmp = WritableBitmapToBitmap(writableBitmap);
            var barcodeResult = barcodeReader.Decode(new BitmapSourceLuminanceSource(CreateBitmapSourceFromGdiBitmap(bmp)));


            MessageBox.Show($"{barcodeResult.Text}");

        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            var barcodeReader = new ZXing.BarcodeReader();

            var bmp = GetBitmapFromScreen();
            var barcodeResult = barcodeReader.Decode(new BitmapSourceLuminanceSource(CreateBitmapSourceFromGdiBitmap(bmp)));

            MessageBox.Show($"{barcodeResult.Text}");
        }

        //+
        private static Bitmap GetBitmapFromScreen()
        {
            var bitmap = new Bitmap(1920, 1080);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        //+
        private Bitmap WritableBitmapToBitmap(WriteableBitmap writeBmp)
        {
            Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeBmp));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }
            return bmp;
        }

        //+
        private static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
