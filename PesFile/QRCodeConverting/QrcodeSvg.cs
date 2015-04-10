using System.IO;
using EmbroideryFile.QR;
using EmbroideryFile.QRCodeConverting;

namespace EmbroideryFile.QRCode
{
    public class QrcodeSvg:IQRCodeStreamer
    {
        private readonly ISvgEncode _encoder;
        private readonly IQRCodeStitchGeneration _stitchGen;
        private const int Size = 1000;
        #region [Public Properties]

        public void FillStream(Stream stream)
        {
            FillStreamWithSvg(stream, Size);
        }

        public QRCodeStitchInfo QrStitchInfo
        {
            set { _stitchGen.Info = value; }
        }

        #endregion [Public Properties]

        public QrcodeSvg()
        {
            _encoder = new SvgEncoder();
            _stitchGen = new QrCodeStitcher();

        }

        #region [Public Methods]

        public void FillStreamWithSvg(Stream stream, int size)
        {
            _encoder.FillStreamWithSvgFromCoordsLists(stream, size, _stitchGen.GetQRCodeStitchBlocks());
        }

        public string ReadSvgStringFromStream()
        {
            return _encoder.ReadSvgString();
        }

        /// <summary>
        /// Provide Svg representation of stitches for QR code 
        /// </summary>
        /// <returns></returns>
        public string GetSvg(int size)
        {
            return _encoder.GetSvgFromCoordsLists(size, _stitchGen.GetQRCodeStitchBlocks());
        }

        #endregion [Public Methods]
    }
}
