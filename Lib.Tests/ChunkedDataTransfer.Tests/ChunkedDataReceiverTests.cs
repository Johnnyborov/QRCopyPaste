using Xunit;

namespace ChunkedDataTransfer.Tests
{
    public class ChunkedDataReceiverTests
    {
        [Fact]
        public void ChunkedDataReceiver_ReceivesAllParts()
        {
            string testData = "This is a test data part string";

            var chunkedDataReceiver = new ChunkedDataReceiver();
            string received = null;
            chunkedDataReceiver.OnDataReceived += data => received = data;
            chunkedDataReceiver.StartReceiving();

            chunkedDataReceiver.ProcessChunk("This");
            chunkedDataReceiver.ProcessChunk(" is a test data part");
            chunkedDataReceiver.ProcessChunk(" ");
            chunkedDataReceiver.ProcessChunk("string");

            Assert.Equal(testData, received);
        }
    }
}
