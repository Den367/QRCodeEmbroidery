using System;

using System.IO;

using EmbroideryFile;
using EmbroideryFile.QRCode;
using EmbroideryFile.QRCodeConverting;

namespace QRCodeGen
{
    internal class QRCodeFileWriter : IWriteQRCodeFile
    {
        public void WriteQRCode(string[] args)
        {

            IQRCodeStreamer qrcodeGen;
            string fileName = string.Empty;
            string outputPath = string.Empty;
            string ext;
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Use following format to start app:");
                    Console.WriteLine("qrcodegen [type] [source file name] [destination path]");
                    System.Environment.Exit(0);
                    break;
                case 1:
                    fileName = args[0];
                    outputPath = Environment.CurrentDirectory;
                    break;
                case 2:
                    fileName = args[1];
                    outputPath = args[2];
                    if (string.IsNullOrWhiteSpace(outputPath))
                        outputPath = Environment.CurrentDirectory;
                    break;

                case 3:
                    fileName = args[1];
                    outputPath = args[2];
                    if (string.IsNullOrWhiteSpace(outputPath))
                        outputPath = Environment.CurrentDirectory;
                    break;
            }

            switch (args[0].ToLower())
            {
                case "dst":
                case "/dst":
                case "-dst":
                    qrcodeGen = new QrcodeDst();
                    ext = "dst";
                    break;
                case "svg":
                case "/svg":
                case "-svg":
                    qrcodeGen = new QrcodeSvg();
                    ext = "svg";
                    break;
                case "png":
                case "/png":
                case "-png":
                    qrcodeGen = new QrcodePng();
                    ext = "png";
                    break;
                default:
                    ext = "dst";
                    qrcodeGen = new QrcodeDst();
                    break;

            }
            outputPath = string.Format(@"{0}\qrcode{1}.{2}", outputPath, Guid.NewGuid(), ext);
            using (var inputStreamReader = new StreamReader(fileName))
            {
                var text = inputStreamReader.ReadToEnd();
                using (Stream outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    if (qrcodeGen != null)
                    {
                        qrcodeGen.QrStitchInfo = new QRCodeStitchInfo {QrCodeText = text};
                        qrcodeGen.FillStream(outStream);
                    }
                }
            }
        }
    }
}

