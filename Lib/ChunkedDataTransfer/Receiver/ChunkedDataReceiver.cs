using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ChunkedDataTransfer
{
    public class ChunkedDataReceiver
    {
        public event StringDataReceivedEventHandler OnStringDataReceived;
        public event ByteDataReceivedEventHandler OnByteDataReceived;
        public event ProgressChangedEventHandler OnProgressChanged;
        public event ReceivingStartedEventHandler OnReceivingStarted;
        public event ReceivingStoppedEventHandler OnReceivingStopped;
        public event ChunkReceivedEventHandler OnChunkReceived;
        public event NotificationEventHandler OnNotification;


        private static Dictionary<string, Dictionary<int, string>> _receivedItemsCache;
        private static Dictionary<string, QRPackageInfoMessage> _receivedPackageInfoMessages;
        private bool _isRunning = false;
        private string _currentlyReceivingObjectID;

        public ChunkedDataReceiver()
        {
            _receivedItemsCache = new Dictionary<string, Dictionary<int, string>>();
            _receivedPackageInfoMessages = new Dictionary<string, QRPackageInfoMessage>();
        }


        public void StartReceiving()
        {
            this._isRunning = true;
        }


        public void ClearCache()
        {
            _receivedItemsCache.Clear();
            this.OnProgressChanged?.Invoke(0);
        }


        public void StopReceivingAll()
        {
            this.OnReceivingStopped?.Invoke(null);
            this._isRunning = false;
            this._currentlyReceivingObjectID = null;
        }


        public void StopReceiving(string objectID)
        {
            var qrPackageInfoMessage = _receivedPackageInfoMessages[objectID];
            var dataParts = _receivedItemsCache[qrPackageInfoMessage.DataHash];

            this.NotifyIfNotAllDataPartsReceived(qrPackageInfoMessage, dataParts);

            this.OnReceivingStopped?.Invoke(objectID);
            this._currentlyReceivingObjectID = null;
        }


        public void ProcessChunk(string dataChunk)
        {
            if (!this._isRunning)
                return;

            if (TryDeserialize<QRPackageInfoMessage>(dataChunk, out var qrPackageInfoMessage2)
                && qrPackageInfoMessage2.MsgIntegrity == Constants.QRPackageInfoMessageIntegrityCheckID)
            {
                if (!_receivedPackageInfoMessages.ContainsKey(qrPackageInfoMessage2.DataHash))
                    _receivedPackageInfoMessages.Add(qrPackageInfoMessage2.ObjectID, qrPackageInfoMessage2);

                this.OnProgressChanged?.Invoke(1);
                if (this._currentlyReceivingObjectID is null)
                    this.OnReceivingStarted?.Invoke(qrPackageInfoMessage2.ObjectID);
                this._currentlyReceivingObjectID = qrPackageInfoMessage2.ObjectID;
            }

            if (this._currentlyReceivingObjectID is null)
                return;

            if (TryDeserialize<QRDataPartMessage>(dataChunk, out var dataPartMessage)
                && dataPartMessage.MsgIntegrity == Constants.QRDataPartMessageIntegrityCheckID)
            {
                if (!_receivedPackageInfoMessages.ContainsKey(dataPartMessage.ObjectID))
                    return;

                this.OnChunkReceived?.Invoke(dataPartMessage.ObjectID);


                var qrPackageInfoMessage = _receivedPackageInfoMessages[dataPartMessage.ObjectID];

                if (!_receivedItemsCache.ContainsKey(qrPackageInfoMessage.DataHash))
                    _receivedItemsCache.Add(qrPackageInfoMessage.DataHash, new Dictionary<int, string>());
                var dataParts = _receivedItemsCache[qrPackageInfoMessage.DataHash];


                if (!dataParts.ContainsKey(dataPartMessage.ID)
                    && HashHelper.GetStringHash(dataPartMessage.Data) == dataPartMessage.DataHash)
                {
                    dataParts.Add(dataPartMessage.ID, dataPartMessage.Data);
                }


                int progress = 1 + 99 * dataParts.Count / qrPackageInfoMessage.NumberOfParts;
                this.OnProgressChanged?.Invoke(progress);

                if (dataParts.Count == qrPackageInfoMessage.NumberOfParts)
                {
                    var fullDataStr = string.Join("", dataParts.ToList().OrderBy(p => p.Key).Select(p => p.Value));
                    ThrowIfFullDataHashIsWrong(qrPackageInfoMessage, fullDataStr);

                    var fullData = ConvertToInitialTypeFromString(fullDataStr, qrPackageInfoMessage.DataType);
                    this.NotifyDataReceived(fullData, qrPackageInfoMessage.DataType);

                    this.OnReceivingStopped?.Invoke(null);
                    this._currentlyReceivingObjectID = null;
                }
            }
        }


        private void NotifyIfNotAllDataPartsReceived(QRPackageInfoMessage qrPackageInfoMessage, Dictionary<int, string> dataParts)
        {
            if (dataParts.Count != qrPackageInfoMessage.NumberOfParts)
            {
                var idsNotReceived = new List<string>();
                for (int i = 0; i < qrPackageInfoMessage.NumberOfParts; i++)
                {
                    if (!dataParts.ContainsKey(i))
                        idsNotReceived.Add(i.ToString());
                }

                var missingPartsCount = qrPackageInfoMessage.NumberOfParts - dataParts.Count;
                this.OnNotification?.Invoke(
                    $"Data was not fully received.\n" +
                    $"{dataParts.Count} out of {qrPackageInfoMessage.NumberOfParts} parts received.\n" +
                    $"{missingPartsCount} parts missing.\n" +
                    $"Missing IDs list:\n" +
                    string.Join(" ", idsNotReceived)
                );
            }
        }


        private static void ThrowIfFullDataHashIsWrong(QRPackageInfoMessage qrPackageInfoMessage, string fullDataStr)
        {
            var dataHash = HashHelper.GetStringHash(fullDataStr);
            if (dataHash != qrPackageInfoMessage.DataHash)
                throw new Exception($"Received data is incorrect. Hashes differ.");
        }


        private static object ConvertToInitialTypeFromString(string zippedDataStr, string dataType)
        {
            if (dataType == Constants.StringTypeName)
            {
                var unzippedDataBytes = GetUnzippedDataBytes(zippedDataStr);
                var unzippedDataStr = Encoding.UTF8.GetString(unzippedDataBytes);
                return unzippedDataStr;
            }
            else if (dataType == Constants.ByteArrayTypeName)
            {
                var unzippedDataBytes = GetUnzippedDataBytes(zippedDataStr);
                return unzippedDataBytes;
            }
            else
            {
                throw new Exception($"Unsupported data type {dataType} in {nameof(ConvertToInitialTypeFromString)}.");
            }
        }


        private void NotifyDataReceived(object fullData, string dataType)
        {
            if (dataType == Constants.StringTypeName)
                this.OnStringDataReceived?.Invoke((string)fullData);
            else if (dataType == Constants.ByteArrayTypeName)
                this.OnByteDataReceived?.Invoke((byte[])fullData);
            else
                throw new Exception($"Unsupported data type {dataType} in {nameof(NotifyDataReceived)}.");
        }


        private static byte[] GetUnzippedDataBytes(string zippedDataStr)
        {
            var zippedDataBytesStream = new MemoryStream(Convert.FromBase64String(zippedDataStr));
            var unzippedDataBytesStream = new MemoryStream();
            GZip.Decompress(zippedDataBytesStream, unzippedDataBytesStream, true);

            var unzippedDataBytes = unzippedDataBytesStream.ToArray();
            return unzippedDataBytes;
        }


        private static bool TryDeserialize<TData>(string dataStr, out TData data)
        {
            try
            {
                data = JsonSerializer.Deserialize<TData>(dataStr);
                return true;
            }
            catch (JsonException)
            {
                data = default;
                return false;
            }
        }
    }
}
