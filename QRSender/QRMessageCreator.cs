﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace QRSender
{
    public static class QRMessageCreator
    {
        public static QRMessagesPackage CreateQRMessagesPackage<TData>(TData data)
        {
            string stringDataToSend;
            string dataType;

            if (data is string dataStr)
            {
                stringDataToSend = dataStr;
                dataType = "string";
            }
            else if (data is byte[] dataBytes)
            {
                stringDataToSend = Convert.ToBase64String(dataBytes);
                dataType = "byte[]";
            }
            else
            {
                throw new NotSupportedException($"Unsupported data type {data.GetType()} during {nameof(QRMessagesPackage)} creation.");
            }


            var qrMessagesPackage = CreateQRMessagesPackageFromString(stringDataToSend, dataType);
            return qrMessagesPackage;
        }


        private static QRMessagesPackage CreateQRMessagesPackageFromString(string data, string dataType)
        {
            var dataPartsMessages = SplitStringToChunks(data, QRSenderSettings.ChunkSize).ToArray();
            var settingsMessage = CreateQRSettingsMessage(dataPartsMessages, dataType);

            var qrMessagesPackage = new QRMessagesPackage
            {
                QRSettingsMessage = settingsMessage,
                QRDataPartsMessages = dataPartsMessages,
            };

            return qrMessagesPackage;
        }


        private static string CreateQRSettingsMessage(string[] dataParts, string dataType)
        {
            var qrMessageSettings = new QRMessageSettings
            {
                NumberOfParts = dataParts.Length,
                SenderDelay = QRSenderSettings.SendDelayMilliseconds,
                DataType = dataType,
            };

            var settings = JsonSerializer.Serialize(qrMessageSettings);
            return settings;
        }


        private static IEnumerable<string> SplitStringToChunks(string fullStr, int chunkSize)
        {
            var numberOfChunks =
                fullStr.Length % chunkSize == 0
                ? fullStr.Length / chunkSize
                : fullStr.Length / chunkSize + 1;


            var chunks =
                Enumerable.Range(0, numberOfChunks)
                .Select(chunkNum =>
                {
                    int substringSize =
                        chunkNum * chunkSize + chunkSize > fullStr.Length
                        ? fullStr.Length % chunkSize
                        : chunkSize;

                    return fullStr.Substring(chunkNum * chunkSize, substringSize);
                });


            return chunks;
        }
    }
}
