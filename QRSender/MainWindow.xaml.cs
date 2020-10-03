using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public partial class MainWindow : Window, INotifyPropertyChanged, IImageSourceHolder
    {
        private ImageSource _imageSource;

        public ImageSource ImageSource
        {
            get => this._imageSource;
            set
            {
                if (this._imageSource != value)
                {
                    this._imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }



        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            TestingShit.QREncodeDecode();
        }


        private void encodeBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "el contento";

            var writableBitmap = EncodeToQR(msg);
            this.ImageSource = writableBitmap;

            MessageBox.Show($"Encoded: {msg}");
        }


        private void decodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var bmp = CreateBitmapFromScreen();
            var barcodeResult = DecodeFromQR(bmp);

            if (barcodeResult != null)
                MessageBox.Show($"Decoded: {barcodeResult.Text}");
            else
                MessageBox.Show($"No QR detected.");
        }


        private void startScannerBtn_Click(object sender, RoutedEventArgs e)
        {
            ComplexScanner.StartScanner();
            MessageBox.Show("Scanner started.");
        }

        private async void createComplexMsgBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "this is a complex message";

            var encoder = new ComplexEncoder(this);
            await encoder.CreateComplex(msg);
        }
    }
}
