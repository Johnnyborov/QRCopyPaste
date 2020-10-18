using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChunkedDataTransfer
{
    public class ChunkedDataSender
    {
        public event ProgressChangedEventHandler OnProgressChanged;
        public int ChunkSize { get; set; } = 500;


        private QRPackage _lastQRPackage = null;
        private bool _stopRequested = false;
        private bool _isRunning = false;

        private readonly IDataSender dataSender;

        public ChunkedDataSender(IDataSender dataSender)
        {
            this.dataSender = dataSender;
        }


        public async Task SendAsync<TData>(TData data)
        {
            if (_isRunning)
                throw new Exception("Data sending is already in progress.");
            _isRunning = true;

            if (data is null)
                throw new ArgumentNullException($"{nameof(data)} is null in {nameof(SendAsync)}");

            if (data is string dataStr && dataStr == string.Empty
                || data is byte[] dataBytes && dataBytes.Length == 0)
                throw new Exception($"{nameof(data)} is empty in {nameof(SendAsync)}");

            var packageCreator = new PackageCreator(this.ChunkSize);
            var qrPackage = packageCreator.CreateQRPackage(data);
            _lastQRPackage = qrPackage;

            await this.dataSender.SendAsync(qrPackage.QRPackageInfoMessage);
            await this.SendAllDataPartsAsync(qrPackage.QRDataPartsMessages);

            this.dataSender.Stop();
            _isRunning = false;
            _stopRequested = false;
        }


        public async Task ResendLastAsync(int[] selectiveIDs)
        {
            if (_lastQRPackage == null)
                throw new Exception("There is nothing to resend.");
            if (_isRunning)
                throw new Exception("Data sending is already in progress.");
            _isRunning = true;

            var qrPackage = _lastQRPackage;

            await this.dataSender.SendAsync(qrPackage.QRPackageInfoMessage);
            await this.SendAllDataPartsAsync(qrPackage.QRDataPartsMessages, selectiveIDs);

            this.dataSender.Stop();
            _isRunning = false;
            _stopRequested = false;
        }


        public void StopSending()
        {
            this.dataSender.Stop();
            _stopRequested = true;
        }


        private async Task SendAllDataPartsAsync(string[] dataParts, int[] selectiveIDs = null)
        {
            this.OnProgressChanged?.Invoke(1);

            var numberOfParts = dataParts.Length;
            for (int i = 0; i < numberOfParts; i++)
            {
                if (_stopRequested)
                    return;

                int progress = 1 + 99 * (i + 1) / numberOfParts;
                this.OnProgressChanged?.Invoke(progress);

                if (selectiveIDs != null && !selectiveIDs.Contains(i))
                    continue;

                string dataPart = dataParts[i];

                await this.dataSender.SendAsync(dataPart);
            }
        }
    }
}
