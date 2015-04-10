using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EmbroideryFile
{
    public class EmbroideryParserFactory : IDisposable
    {
        private bool _disposed;
        private Stream _stream = null;
        private EmbroideryTypeDetector typeDetector = null;

        public EmbroideryParserFactory(Stream stream)
        {
            this._stream = stream;
            typeDetector = new EmbroideryTypeDetector(stream);

        }


        public IGetEmbroideryData CreateParser()
        {

            switch (typeDetector.DetectType())
            {
                case EmbroType.Dst:
                    return new DstFile(_stream);
                case EmbroType.Pes:
                    return new PesFile(_stream);
                case EmbroType.Pec:
                    return new PecFile(_stream);
                default:
                    return null;

            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _disposed == false)
            {
                if (_stream != null) _stream.Close();
                _disposed = true;
            }


        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
