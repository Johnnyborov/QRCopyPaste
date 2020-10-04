using System.Windows.Media;

namespace QRSender
{
    public interface IImageSourceHolder
    {
        public ImageSource ImageSource { get; set; }
        public int SenderProgress { get; set; }
    }
}
