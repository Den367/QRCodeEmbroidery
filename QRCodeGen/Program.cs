

namespace QRCodeGen
{
    class Program
    {
        private static void Main(string[] args)
        {
            IWriteQRCodeFile qrCodeWriter = new QRCodeFileWriter();
            qrCodeWriter.WriteQRCode(args);
        }
    }
}
