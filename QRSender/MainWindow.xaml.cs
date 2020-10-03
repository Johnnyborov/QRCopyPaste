using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using static QRSender.HelperFunctions;

namespace QRSender
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ImageSource _imageSource;

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



        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            TestingShit.QREncodeDecode();
        }


        private void encodeBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = "el contento";

            var writableBitmap = EncodeToQR(msg);
            this.ImageSource = writableBitmap;

            MessageBox.Show($"{msg}");
        }


        private void decodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var bmp = CreateBitmapFromScreen();
            var barcodeResult = DecodeFromQR(bmp);

            MessageBox.Show($"{barcodeResult.Text}");
        }
    }
}
