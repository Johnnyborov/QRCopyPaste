using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;
using System.Text;

namespace ChunkedDataTransfer
{
    public static class Class1
    {
        private const string _testStr = "xTest";

        public static string GetTestString()
        {
            var unzippedDataBytesToSend = Encoding.UTF8.GetBytes(_testStr);
            var zippedDataStrToSend = GetZippedStringDataToSend(unzippedDataBytesToSend);

            var unzippedDataBytesReceived = GetUnzippedDataBytes(zippedDataStrToSend);
            var unzippedDataStrReceived = Encoding.UTF8.GetString(unzippedDataBytesReceived);
            return unzippedDataStrReceived;
        }



        private static string GetZippedStringDataToSend(byte[] unzippedDataBytes)
        {
            var unzippedDataBytesStream = new MemoryStream(unzippedDataBytes);
            var zippedDataBytesStream = new MemoryStream();
            GZip.Compress(unzippedDataBytesStream, zippedDataBytesStream, true, 4096, 9);

            var zippedDataBytes = zippedDataBytesStream.ToArray();
            var zippedStringDataToSend = Convert.ToBase64String(zippedDataBytes);
            return zippedStringDataToSend;
        }



        private static byte[] GetUnzippedDataBytes(string zippedDataStr)
        {
            var zippedDataBytesStream = new MemoryStream(Convert.FromBase64String(zippedDataStr));
            var unzippedDataBytesStream = new MemoryStream();
            GZip.Decompress(zippedDataBytesStream, unzippedDataBytesStream, true);

            var unzippedDataBytes = unzippedDataBytesStream.ToArray();
            return unzippedDataBytes;
        }
    }
}
