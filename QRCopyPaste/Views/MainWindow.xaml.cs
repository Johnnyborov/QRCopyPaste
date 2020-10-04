﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace QRCopyPaste
{
    public partial class MainWindow : Window, INotifyPropertyChanged, ISenderViewModel, IReceiverViewModel
    {
        #region Fields

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

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region Methods

        private void StartScanningBtn_Click(object sender, RoutedEventArgs e)
        {
            var qrReceiver = new QRReceiver(this);
            if (qrReceiver.StartScanning(receivedData => HandleReceivedData(receivedData), errorMsg => MessageBox.Show($"Error: {errorMsg}")))
                MessageBox.Show("Scanning started.");
            else
                MessageBox.Show("Scanning is already running.");
        }


        private static void HandleReceivedData(object receivedData)
        {
            if (receivedData.GetType() == typeof(string))
            {
                Thread thread = new Thread(() => Clipboard.SetText((string)receivedData));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                
                MessageBox.Show($"Data copied to clipboard.");
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


        private async void SendClipboardTextBtn_Click(object sender, RoutedEventArgs e)
        {
            var stringData = Clipboard.GetData(DataFormats.Text);
            if (string.IsNullOrEmpty((string)stringData))
            {
                MessageBox.Show($"There is no text in clipboard right now.");
                return;
            }


            try
            {
                var qrSender = new QRSender(this);
                await qrSender.SendData(stringData);
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
                    var qrSender = new QRSender(this);
                    await qrSender.SendData(binaryData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while sending file: {ex.Message}");
                }
            }
        }


        private void StopSendingBtn_Click(object sender, RoutedEventArgs e)
        {
            QRSender.RequestStop();
        }


        private void ClearCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            QRReceiver.ClearCache();
        }


        private async void ResendLastBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string idsStr = ResendIDsTextBox.Text;
                int[] ids =
                    string.IsNullOrEmpty(idsStr)
                    ? null
                    : idsStr.Split(" ").Select(idStr => int.Parse(idStr)).ToArray();

                await QRSender.ResendLast(ids);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while resending: {ex.Message}");
            }
        }

        #endregion
    }
}