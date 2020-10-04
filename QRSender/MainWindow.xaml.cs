using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public partial class MainWindow : Window, INotifyPropertyChanged, IImageSourceHolder, IReceiverViewModel
    {
        private int _scanCycle;
        public int ScanCycle
        {
            get => this._scanCycle;
            set
            {
                if (this._scanCycle != value)
                {
                    this._scanCycle = value;
                    OnPropertyChanged(nameof(ScanCycle));
                }
            }
        }

        private int _progress;
        public int Progress
        {
            get => this._progress;
            set
            {
                if (this._progress != value)
                {
                    this._progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

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



        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            TestingShit.QREncodeDecode();
        }


        private void EncodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var data = "el contento";

            var writableBitmap = EncodeToQR(data);
            this.ImageSource = writableBitmap;

            MessageBox.Show($"Encoded: {data}");
        }


        private void DecodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = CreateBitmapFromScreen();
            var barcodeResult = DecodeFromQR(bitmap);

            if (barcodeResult != null)
                MessageBox.Show($"Decoded: {barcodeResult.Text}");
            else
                MessageBox.Show($"No QR detected.");
        }


        private void StartScannerBtn_Click(object sender, RoutedEventArgs e)
        {
            var scanner = new ComplexScanner(this);
            if (scanner.StartScanner(receivedData => HandleReceivedData(receivedData), errorMsg => MessageBox.Show($"Error: {errorMsg}")))
                MessageBox.Show("Scanner started.");
            else
                MessageBox.Show("Scanner already running.");
        }


        private static void HandleReceivedData(object receivedData)
        {
            string msg;

            if (receivedData.GetType() == typeof(string))
                msg = (string)receivedData;
            else if (receivedData.GetType() == typeof(byte[]))
            { msg = ((byte[])receivedData)[4] == 253 ? "correct" : "incorrect"; System.IO.File.WriteAllBytes("2QRSender.pdb", (byte[])receivedData); }
            else
                throw new Exception($"Unsupported data type {receivedData.GetType()} in {nameof(HandleReceivedData)}.");

            MessageBox.Show($"Scanned: {msg}");
        }


        private async void SendStringDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var stringData = "this is a complex message";

            try
            {
                var encoder = new ComplexEncoder(this);
                await encoder.SendData(stringData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while sending: {ex.Message}");
            }
        }


        private async void SendBinaryDataBtn_Click(object sender, RoutedEventArgs e)
        {
            //var binaryData = new byte[] { 0, 1, 3, 66, 253, 255, 0, 254, 177, 222, 234 };
            var binaryData = System.IO.File.ReadAllBytes("QRSender.pdb");

            try
            {
                var encoder = new ComplexEncoder(this);
                await encoder.SendData(binaryData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while sending: {ex.Message}");
            }
        }


        private void StopSendingBtn_Click(object sender, RoutedEventArgs e)
        {
            ComplexEncoder.RequestStop();
        }

        private void ClearItemsBtn_Click(object sender, RoutedEventArgs e)
        {
            ComplexScanner.ClearItems();
        }
    }
}
