using System.IO;


namespace EmbroideryFile.QRCodeConverting
{
    public interface IQRCodeStreamer
    {
        void FillStream(Stream stream);
        QRCodeStitchInfo QrStitchInfo { set; }
    }
}
