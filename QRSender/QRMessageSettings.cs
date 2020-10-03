using System.Collections.Generic;

namespace QRSender
{
    public class QRMessageSettings
    {
        public int NumberOfParts { get; set; }
        public HashSet<string> ReceivedIDs { get; set; } // TODO: Replace this with PartsIDsToReceive.
        public int SenderDelay { get; set; }
        public string DataType { get; set; }
    }
}
