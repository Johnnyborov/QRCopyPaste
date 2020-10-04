using System.Windows.Media;

namespace QRSender
{
    public interface IImageSourceHolder
    {
        public ImageSource ImageSource { get; set; }
    }
}
