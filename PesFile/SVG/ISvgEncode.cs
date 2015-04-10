using System;
using System.Collections.Generic;
using System.IO;

namespace EmbroideryFile
{
    public interface ISvgEncode: IDisposable
    {
        void WriteSvg();
        string ReadSvgString();
        string GetSvgFromCoordsLists(int size,List<CoordsBlock> blocks);
        void FillStreamWithSvgFromCoordsLists(Stream strm, int size, List<CoordsBlock> blocks);
    }
}
