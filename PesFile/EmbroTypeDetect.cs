using System;
using System.Text;
using System.IO;

namespace EmbroideryFile
{
    public class EmbroideryTypeDetector
    {
        private Stream stream;

        public EmbroideryTypeDetector(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Detects type of an embroidery file
        /// </summary>
        /// <returns></returns>
        public EmbroType DetectType()
        {
            string prefix = ReadPrefix(stream, 4);
            switch (prefix)
            {
                case "#PES":
                    return EmbroType.Pes;
                case "#PEC":
                    return EmbroType.Pec;

            }
            prefix = ReadPrefix(stream, 3);
            switch (prefix)
            {
                case "LA:":
                    return EmbroType.Dst;
                default:
                    throw new Exception("Type of an embroidery has not been detected!");

            }
        }

        #region [Aux]

        private string ReadPrefix(Stream stream, int Length)
        {
            byte[] firstBytes = new byte[Length];
            stream.Read(firstBytes, 0, Length);
            stream.Position = 0;
            return Encoding.ASCII.GetString(firstBytes);
        }

        #endregion [Aux]
    }
}
