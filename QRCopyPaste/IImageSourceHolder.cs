using System.Windows.Media;

namespace QRCopyPaste
{
    public interface IImageSourceHolder
    {
        public ImageSource ImageSource { get; set; }
        public int SenderProgress { get; set; }
    }
}
