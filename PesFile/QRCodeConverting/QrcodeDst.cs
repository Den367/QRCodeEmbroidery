using System.IO;
using EmbroideryFile.QR;
using EmbroideryFile.QRCodeConverting;

namespace EmbroideryFile.QRCode
{
    public class QrcodeDst: IQRCodeStreamer
    {
        private DstFile _dst;

        private readonly IQRCodeStitchGeneration _stitchGen;

        #region [Public Properties]

        public QRCodeStitchInfo QrStitchInfo
        {
            set { _stitchGen.Info = value; }
        }

        #endregion [Public Properties]

        public QrcodeDst()
        {
            _dst = new DstFile();
            _stitchGen = new QrCodeStitcher();

        }

        #region [Public Methods]
        public void FillStream(Stream stream)
        {
            FillStreamWithDst(stream);
        }

        public void FillStreamWithDst(Stream stream)
        {
            _dst.WriteStitchesToDstStream(_stitchGen.GetQRCodeStitchBlocks(), stream);
        }

        #endregion [Public Methods]
    }
}
