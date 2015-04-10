using System.Collections.Generic;
using System.Drawing.Imaging;
using  System.IO;
using EmbroideryFile.QR;
using EmbroideryFile.QRCodeConverting;

namespace EmbroideryFile.QRCode
{
    public class QrcodePng:IQRCodeStreamer
    {

        private readonly StitchToBmp _bitmapCreator;
        private readonly IQRCodeStitchGeneration _stitchGen;
        private ImageFormat _format;
        private const int Size = 420;

        #region [Public Properties]

        public void FillStream(Stream stream)
        {
            FillStreamWithPng(stream);
        }

        public QRCodeStitchInfo QrStitchInfo
        {
            set { _stitchGen.Info = value; }
        }


        #endregion [Public Properties]

        public QrcodePng()
        {
            _stitchGen = new QrCodeStitcher();
            _bitmapCreator = new StitchToBmp(_stitchGen.GetQRCodeInvertedYStitchBlocks(), Size);
        }
        public QrcodePng(string qRCodeText, int size)
        {
            _stitchGen = new QrCodeStitcher { Info = new QRCodeStitchInfo { QrCodeText = qRCodeText } };
            _bitmapCreator = new StitchToBmp(_stitchGen.GetQRCodeInvertedYStitchBlocks(), size);
        }

        private Dictionary<string, ImageCodecInfo> encoders = null;

        /// <summary>
        /// A quick lookup for getting image encoders
        /// </summary>
        public Dictionary<string, ImageCodecInfo> Encoders()
        {
            //get accessor that creates the dictionary on demand

            //if the quick lookup isn't initialised, initialise it
            if (encoders == null)
            {
                encoders = new Dictionary<string, ImageCodecInfo>();
            }

            //if there are no codecs, try loading them
            if (encoders.Count == 0)
            {
                //get all the codecs
                foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                {
                    //add each codec to the quick lookup
                    encoders.Add(codec.MimeType.ToLower(), codec);
                }
            }

            //return the lookup
            return encoders;

        }

        #region [Public Methods]

        public void FillStreamWithPng(Stream stream)
        {
            _bitmapCreator.FillStreamWithPng(stream);

        }
        #endregion [Public Methods]

    }
}

