using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using EmbroideryFile.QR;

namespace EmbroideryFile
{
    public class PesBuilder 
    {

        PecBuilder _pec;
        int _designWidth = 0;
        int _designHeight = 0;

        int _designXOffset = 0;
        int _designYOffset = 0;
        List<List<Coords>> _blocks;
        readonly Stream _stream;
        int _stitchBlockCount;
        QrCodeStitcher _stitcher;
        QRCodeStitchInfo _info;
        Stream _pecStream;

          public PesBuilder(QrCodeStitcher stitcher, Stream strm)
        {
            _stream = strm;
            _stitcher = stitcher;
            _info = _stitcher.Info;
            _blocks = _stitcher.GetQRCodeStitches();
            
              _pecStream = new MemoryStream();
              _pec = new PecBuilder(_stitcher);

              FillDesignAttributes();
            
        }

        public PesBuilder(PecBuilder pec)
        {
            _pec = pec;
        
            _stitcher = _pec.Stitcher;
            _info = _pec.Stitcher.Info;

            FillDesignAttributes();
        }

        void FillDesignAttributes()
        {
            _designWidth = _pec.GetDesignWidth();
            _designHeight = _pec.GetDesignHeight();

            _designXOffset = _stitcher.GetDesignXOffset;
            _designYOffset = _stitcher.GetDesignYOffset;
            _blocks = _stitcher.GetQRCodeStitches();
            _stitchBlockCount = _blocks.Count;
        }

        public void WritePesStructureToStream()
        {
            if (_stream != null)
                using (_stream)
                {
                    using (Stream pesStream = GetPesStream())
                    {
                    pesStream.Position = 0;
                    pesStream.CopyTo(_stream, (int)pesStream.Length);
                    }
                }
        }
        /// <summary>
        /// Returns filled with PES structure stream 
        /// </summary>
        /// <returns></returns>
        public Stream GetPesStream()
        {
            Stream stream = new MemoryStream(); 

            stream.Write(GetPesHeader(), 0 , 8);
            stream.Write(GetPecStart(), 0, 4);
            stream.Write(GetGt100Kb(), 0, 2);
            stream.Write(GetFixedOnesWithSevenOne(), 0, 10);
            stream.Write(GetCEmbOneBlockBytes(), 0, 73);
            //stream.Write(GetCSewFigSeg(_blocks), 0, GetCSewSegStitchesByteCount() + 23);
            using (var sewSegStream = GetCSewFigSeg(_blocks))
            {
                sewSegStream.Position = 0;
                sewSegStream.CopyTo(stream, (int)sewSegStream.Length);
                //sewSegStream.Flush();
            }

            using (_pecStream = _pec.GetPecStream())
            {
                _pecStream.Position = 0;
                _pecStream.CopyTo(stream, (int)_pecStream.Length);
                //_pecStream.Flush();
            }
            return stream;

        }


        byte[] GetPesHeader()
        {
            return new byte[] {0x23, 0x50, 0x45, 0x53, 0x30, 0x30, 0x30, 0x31};
        }

        byte[] GetPecStart()
        {
            byte[] pecStartBytes = new byte[4];
            int pecStartAddr =0x68 /* byte after CSewSeg */ + 6 /*color info, stitches in block*/+ GetCSewSegStitchesByteCount() + 2 /* end of block*/ + 1 + 7;
            pecStartBytes[0] = (byte)pecStartAddr;
            pecStartBytes[1] = (byte)(pecStartAddr >> 8);
            pecStartBytes[2] = (byte)(pecStartAddr >> 16);
            pecStartBytes[3] = (byte)(pecStartAddr >> 24);
            return pecStartBytes;
        }

        byte[] GetGt100Kb()
        {
            byte[]  gtHundred = new byte[] { 0x00, 0x00 };
            if (GetCSewSegStitchesByteCount() > 102400) gtHundred[0] = 1;
            return gtHundred;

        }
        int GetCSewSegStitchesByteCount()
        {
            return _blocks.Sum(s => s.Count) * 4;
        }


        // 01 00 01 00 FF FF 00 00 07 00
        byte[] GetFixedOnesWithSevenOne()
        {

            byte[] fixedOnes = new byte[] { 0x01, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x07, 0x00 };
            return fixedOnes;
        }
        public byte[] GetDesignOffsetBytes()
        {

            byte[] designOffsetBytes = new byte[4];
            //designOffsetBytes[0] = 0;
            //designOffsetBytes[1] = 0;
            //designOffsetBytes[2] = 0;
            //designOffsetBytes[3] = 0;

            designOffsetBytes[0] = (byte)_designXOffset;
            designOffsetBytes[1] = (byte)(_designXOffset >> 8);
            designOffsetBytes[2] = (byte)_designYOffset;
            designOffsetBytes[3] = (byte)(_designYOffset >> 8);
            return designOffsetBytes;

        }
     

        byte[] GetCEmbOneBlockBytes()
        {
            byte[] cembone = new byte[73];
            Buffer.BlockCopy(new byte[] { 0x43, 0x45, 0x6D, 0x62, 0x4F, 0x6E, 0x65 }, 0, cembone, 0, 7);
            //43 45 6D 62 4F 6E 65 B3 02 AE 02 24 05 20 05 B3
//02 AE 02 24 05 20 05 00 00 80 3F 00 00 00 00 00
//00 00 00 00 00 80 3F 00 00 7A 44 00 00 7A 44 01
//00
            Buffer.BlockCopy(new byte[] { 
                0xAF, 0x02, 0xAF, 0x02, 0x20, 0x05, 0x21, 0x05, 0xAF, 0x02, 0xAF, 0x02, 0x20, 0x05, 0x21, 0x05,
                0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xAF, 0x02, 0x21, 0x05, 0x71, 0x02,/* */
                0x72, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x07 ,0x00 }, 0, cembone, 7, 66);
            //Buffer.BlockCopy(GetDesignOffsetBytes(), 0, cembone, 49, 4);
            //Buffer.BlockCopy(_pec.GetDesignSizeBytes(), 0, cembone, 53, 4);
            //Buffer.BlockCopy(new byte[] { 0x01, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x07, 0x00 }, 0, cembone, 65, 8);
            return cembone;
        }

        byte[] GetCSegSewName()
        {
           
            byte[] segsew = new byte[7] {0x43, 0x53, 0x65, 0x77, 0x53, 0x65, 0x67};

            return segsew;
        }

        Stream GetCSewFigSeg(List<List<Coords>> blocks)
        {
            Stream stream = new MemoryStream();


            stitchBlock currentBlock;
            int StitchesCount = blocks.Sum(block => block.Count);

            currentBlock = new stitchBlock();
            stream.Write(GetCSegSewName(), 0, 7);
            //  write prefix
            //  (color start indicator) (all color changes have the same value for this field; 
            //  if the value here is not the same as the first block, it's not a color change)
            //(color index)
            //(number of stitches in this block)
            stream.Write(GetColorInfo(), 0, 4);

            stream.WriteAsUInt16(StitchesCount);

            foreach (Coords stitchPoint in blocks.SelectMany(stitch => stitch))
                stream.WritePesCoords(ShiftCoords(stitchPoint, _designXOffset, _designYOffset));
            
            stream.Write(GetColorTable(), 0, 10);
            return stream;

        }

        byte[] GetColorInfo()
        {
            return new byte[] { 0x00, 0x00, 0x14, 0x00 };
        }
        
        byte[] GetColorTable()
        { 
          // write fixed color table:
             return new byte[] { 0x01, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00};
        }

        Coords ShiftCoords(Coords coords, int X, int Y)
        {
            coords.X += X;
            coords.Y += Y;
            return coords;
        }
        
 }
    public static class StreamExtension
    {
        public static void WritePesCoords(this Stream stream, Coords sP)
        {
            stream.WriteByte((byte)sP.X);
            stream.WriteByte((byte)(sP.X >> 8));
            stream.WriteByte((byte)sP.Y);
            stream.WriteByte((byte)(sP.Y >> 8));
        }

        public static void WriteAsUInt16(this Stream stream, int Num)
        {
            stream.WriteByte((byte)Num);
            stream.WriteByte((byte)(Num >> 8));

        }
    }

   
}
