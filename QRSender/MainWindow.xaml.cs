using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;

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

        private int _receiverProgress;
        public int ReceiverProgress
        {
            get => this._receiverProgress;
            set
            {
                if (this._receiverProgress != value)
                {
                    this._receiverProgress = value;
                    OnPropertyChanged(nameof(ReceiverProgress));
                }
            }
        }

        private int _senderProgress;
        public int SenderProgress
        {
            get => this._senderProgress;
            set
            {
                if (this._senderProgress != value)
                {
                    this._senderProgress = value;
                    OnPropertyChanged(nameof(SenderProgress));
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
            if (receivedData.GetType() == typeof(string))
            {
                Thread thread = new Thread(() => Clipboard.SetText($"blabla: {(string)receivedData}"));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                
                MessageBox.Show($"Received and copied to clipboard text: {(string)receivedData}");
            }
            else if (receivedData.GetType() == typeof(byte[]))
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, (byte[])receivedData);
                }
            }
            else
            {
                throw new Exception($"Unsupported data type {receivedData.GetType()} in {nameof(HandleReceivedData)}.");
            }
        }


        private async void SendClipboardBtn_Click(object sender, RoutedEventArgs e)
        {
            var stringData = Clipboard.GetData(DataFormats.Text);

            try
            {
                var encoder = new ComplexEncoder(this);
                await encoder.SendData(stringData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while sending clipboard text: {ex.Message}");
            }
        }


        private async void SendFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var binaryData = File.ReadAllBytes(openFileDialog.FileName);

                try
                {
                    var encoder = new ComplexEncoder(this);
                    await encoder.SendData(binaryData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while sending file: {ex.Message}");
                }
            }
        }


        private void StopSendingBtn_Click(object sender, RoutedEventArgs e)
        {
            ComplexEncoder.RequestStop();
        }

        private void ClearCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            ComplexScanner.ClearItems();
        }
    }
}
