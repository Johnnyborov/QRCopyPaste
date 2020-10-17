using Moq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChunkedDataTransfer.Tests
{
    public class ChunkedDataSenderTests
    {
        [Fact]
        public async Task Send_CallsDataSender_AtLeastOnce()
        {
            var dataSenderMock = new Mock<IDataSender>();

            string testData = "This is a test data part string";
            var chunkedDataSender = new ChunkedDataSender(dataSenderMock.Object);
            await chunkedDataSender.Send(testData);

            dataSenderMock.Verify(x => x.Send(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Send_TransfersAllDataIntact()
        {
            var sb = new StringBuilder();

            var dataSenderMock = new Mock<IDataSender>();
            dataSenderMock
                .Setup(x => x.Send(It.IsAny<string>()))
                .Callback<string>(dataPartStr => sb.Append(dataPartStr));

            string testData = "This is a test data part string";
            var chunkedDataSender = new ChunkedDataSender(dataSenderMock.Object);
            await chunkedDataSender.Send(testData);

            string received = sb.ToString();
            Assert.Equal(testData, received);
        }
    }
}
