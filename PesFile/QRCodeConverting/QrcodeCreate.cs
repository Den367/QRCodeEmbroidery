using System.Text;

using MessagingToolkit.QRCode.Codec;


namespace EmbroideryFile.QRCode
{
    internal class QRCodeCreator
    {

        public bool[][] GetQRCodeMatrix(string DataToEncode)
        {

            if (string.IsNullOrEmpty(DataToEncode))
                return null;

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.CharacterSet = "UTF8";
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeScale = 1;
            qrCodeEncoder.QRCodeVersion = -1;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
            return qrCodeEncoder.CalQrcode(Encoding.ASCII.GetBytes(DataToEncode));

        }

       
    }
}
