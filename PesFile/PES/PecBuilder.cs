
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EmbroideryFile.QR;

namespace EmbroideryFile
{
    /// <summary>
    /// Builds PEC block from <see cref="List<List<Coords>>"/>
    /// </summary>
    public class PecBuilder
    {
        int _designWidth = 0;
        int _designHeight = 0;
        List<List<Coords>> _blocks;
        readonly Stream _stream;
        int _stitchBlockCount;
        QrCodeStitcher _stitcher;
        QRCodeStitchInfo _info;

        public QrCodeStitcher Stitcher
        {
            set
            {
                _stitcher = value;
                _info = _stitcher.Info;
                _blocks = _stitcher.GetQRCodeStitches();
                _stitchBlockCount = _blocks.Count;
            }
            get { return _stitcher; }
        }

        public PecBuilder(QrCodeStitcher stitcher)
        {
        
            _stitcher = stitcher;
            _info = _stitcher.Info;
            _blocks = _stitcher.GetQRCodeStitches();
            _stitchBlockCount = _blocks.Count;

        }



        public PecBuilder(QrCodeStitcher stitcher, Stream strm)
        {
            _stream = strm;
            _stitcher = stitcher;
            _info = _stitcher.Info;
            _blocks = _stitcher.GetQRCodeStitches();
            _stitchBlockCount = _blocks.Count;
            
        }

       public long GetPecSize ()
       {
           if (_stream != null)
               return _stream.Length;
           else return 0;
       }

         /// <summary>
        /// Build Pec file structure
        /// </summary>
        /// <param name="lanes"></param>
       public void WritePecStructureToStream()
       {
           if (_stream != null)
               using (_stream)
               {
                   _stream.Write(new byte[] { 0x23, 0x50, 0x45, 0x43, 0x30, 0x30, 0x30, 0x31}, 0, 8);
                   

                   using (Stream pecStream = GetPecStream())
                   {
                       pecStream.Position = 0;
                       pecStream.CopyTo(_stream, (int)pecStream.Length);
                   }

               }
       }

         public Stream GetPecStream()
         {

             Stream stream = new MemoryStream();

            stream.Write(new byte[]  { 0x4C,0x41,0x3A},0,3);
            byte[] Name = Enumerable.Repeat((byte)0x20, 16).ToArray();
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(_info.DesignName), 0, Name, 0, _info.DesignName.Length);

            stream.Write(Name,0,16);
            stream.WriteByte(0x0D);

            byte[] fixedInSpace = new byte[] { 0xFF, 0x00, 0x06, 0x26 };// start at 12: FF 00 06 26
            byte[] FirstSpaceBlock = Enumerable.Repeat((byte)0x20, 492).ToArray();
            byte[] ColorCountINdexList = new byte[] { 0x00, 0x14 };
            Buffer.BlockCopy(fixedInSpace,0,FirstSpaceBlock, 12,  4)    ;
            Buffer.BlockCopy(ColorCountINdexList, 0, FirstSpaceBlock, 28, 2);
            stream.Write(FirstSpaceBlock, 0, 492);
// ?
            stream.Write(new byte[] { 0x00, 0x00 }, 0, 2);
            Stream StitchData = GetAllStitchesBlocks();
             long stitchDataBytesCount = StitchData.Length;
             long stitchDataBytesCountCalculated = GetStitchBlocksByteCount();
            // PixelGraphicStart 3 bytes            
             stream.Write(GetGraphicsStartBytes((int)stitchDataBytesCount), 0, 3);

             stream.Write(new byte[] { 0x31, 0xFF,0xF0 }, 0, 3);
            // Design Size 4 bytes
             stream.Write(GetDesignSizeBytes(),0,4);             
            // Unknown variable bytes
             byte[] UnknownVar = new byte[8]{0xE0,0x01,0xB0,0x01,0x91,0x2C,0x91,0x2C};
             stream.Write(UnknownVar, 0, 8);
             stream.Write(Enumerable.Repeat((byte)0x00, 4).ToArray(), 0, 4);
           using( StitchData)         
          {
              StitchData.Position = 0;
              StitchData.CopyTo(stream, (int)stitchDataBytesCount);
          }

             // End of stitch data
           stream.Write(GetEndOfStitches(), 0, 2);
            // PixelGraphics
         byte[] space2 = GetPixelGraphics();
         stream.Write(space2, 0, 228 * 2);

         return stream;
         }



         Stream GetAllStitchesBlocks()
         {
             Stream strm = new MemoryStream();
             
             strm.Write(GetFirstJumpStitch(_info.Dimension, _info.cellSize), 0, 4);
             //
             List<Coords> block;

             int stitchCount;
             for (int i = 0; i < _stitchBlockCount; i++)
             {
                 block = _blocks[i];
                 stitchCount = block.Count - 1;
                 for (int j = 0; j < stitchCount; j++)
                     strm.Write(GetStitchBytes(block[j], block[j + 1]), 0, 2);
                 if (i < _stitchBlockCount - 1) strm.Write(GetJumpStitchBytes(block[stitchCount], _blocks[i + 1][0]), 0, 4);
             }
             return strm;

         }

         void WriteAllStitchesBlocksToStream()
         {

             _stream.Write(GetFirstJumpStitch(_info.Dimension, _info.cellSize), 0, 4);
             //
             List<Coords> block;

             int stitchCount;
             for (int i = 0; i < _stitchBlockCount ; i++)
             {
                 block = _blocks[i];
                 stitchCount = block.Count - 1;
                 for (int j = 0; j < stitchCount; j++)
                     _stream.Write(GetStitchBytes(block[j], block[j + 1]), 0, 2);
                 if (i < _stitchBlockCount - 1) _stream.Write(GetJumpStitchBytes(block[stitchCount], _blocks[i + 1][0]), 0, 4);
             }

         }

        /// <summary>
        /// Returns first offset to start point
        /// </summary>
        /// <param name="QRDim"></param>
        /// <param name="DotSize"></param>
        /// <returns></returns>
        byte[] GetFirstJumpStitch(int QRDim, int DotSize )
        {

            int deltaX = - (QRDim / 2 * DotSize);
            int deltaY =  deltaX;
            return GetJumpStitchBytes(deltaX, deltaY);
        
        }

        byte[] GetEndOfStitches()
        {
            return new byte[2]{ 0xFF, 0x00 };
        }

        byte[] GetColorSwitchBytes()
        {
            return new byte[3] {0xFE, 0xB0, 0x00};
        }


        byte[] GetJumpStitchBytes(Coords point1, Coords point2  )
        {

        
             int deltaX = point2.X - point1.X;
             int deltaY = point2.Y - point1.Y;
             return GetJumpStitchBytes(deltaX, deltaY);

        }

        byte[] GetJumpStitchBytes(int deltaX, int deltaY)
        {
//            128 <= kx <= 254,
//ky 	2 	Jump stitch, lower four bytes (nibble) of kx is multiplication factor for jump stitch ky, direction of jump is determined as follows:
//(kx and 15) <= 7 		jump in positive direction. length = ky + (kx and 15) x 256
//(kx and 15) >=8 		jump in negative direction. length = (ky-256) + ((kx and 15)-15) x 256 


            byte[] jumpStitchData = new byte[4];

            if (deltaX < 0) deltaX = deltaX | 0x1000;
            jumpStitchData[0] = (byte)(0x80 | ((deltaX >> 8) & 0xF));
            jumpStitchData[1] = (byte)(deltaX /*- ((val1 & 0xF) << 8)*/);
            if (deltaY < 0) deltaY = deltaY | 0x1000;
            jumpStitchData[2] = (byte)(0x80 | ((deltaY >> 8) & 0xF));
            jumpStitchData[3] = (byte)(deltaY /* - ((val1 & 0xF) << 8)*/);

            return jumpStitchData;
        }

        /// <summary>
        /// Returns bytes for normal stitch
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
         byte[] GetStitchBytes(Coords point1, Coords point2  )
        {

            byte[] StitchData = new byte[2];                       

             int deltaX = point2.X - point1.X;
             int deltaY = point2.Y - point1.Y;
            if (deltaX < 0) deltaX = deltaX + 0x80;
            if (deltaY < 0) deltaY = deltaY + 0x80;
            StitchData[0] = (byte)deltaX;
            StitchData[1] = (byte)deltaY;            
              return StitchData; 
        }

  

        public int GetStitchBlocksByteCount()
        {
            int stitchBytesCount = _blocks.Sum(block => block.Count - 1);
            return (stitchBytesCount * 2) + (_blocks.Count - 1) * 4;

        }

        public int GetDesignWidth()
         {
             if (_designWidth == 0) 
   
             {

                 int minX = _blocks.SelectMany(block => block.AsEnumerable()).Min(stitch => stitch.X);
                 int maxX = _blocks.SelectMany(block => block.AsEnumerable()).Max(stitch => stitch.X);
                 _designWidth = (maxX - minX) ;
             }
           return _designWidth;
         }

        public int GetDesignHeight()
         {
             if (_designHeight == 0)
             {

                 int minY = _blocks.SelectMany(block => block.AsEnumerable()).Min(stitch => stitch.Y);
                 int maxY = _blocks.SelectMany(block => block.AsEnumerable()).Max(stitch => stitch.Y);
                 _designHeight = (maxY - minY) ;
             }
             return _designWidth;
         }

         public byte[] GetDesignSizeBytes()
         {

             byte[] designSizeBytes = new byte[4];
             int width;
             int height;
             if (_designWidth == 0) width = GetDesignWidth();
             else width = _designWidth;

             if (_designHeight == 0) height = GetDesignHeight();
             else height = _designHeight;
             designSizeBytes[0] = (byte)width;
             designSizeBytes[1] = (byte)(width >> 8);
             designSizeBytes[2] = (byte)height;
             designSizeBytes[3] = (byte)(height >> 8);
             return designSizeBytes;
 
         }
         byte[] GetGraphicsStartBytes(int stitchBytesCount)
         {

             //pecstart + graphic + 512 		End of stitch data
             int resultOffset = 21/*? offset + two bytes of end of stitches */  + stitchBytesCount + 4 ;
             byte[] graphicStart = new byte[3];
             graphicStart[0] = GetLowByte(resultOffset);
             graphicStart[1] = GetHiByte(resultOffset);
             graphicStart[2] = Convert.ToByte(resultOffset >> 16);
             return graphicStart;
         }

         byte[] GetPixelGraphics()
         {
             byte[] tumbnail = Enumerable.Repeat((byte)0x00, 228 * 2).ToArray(); ;
          
             int cur = 0;
             int dim = _info.Dimension;
             bool[][] M = _info.Matrix;
             for (int i = 0; i < dim; i++)
                 for (int j = 0; j < dim; j = j + 8)
                 {
                     cur = 0;
                     for (byte b = 0; (b < 8) && ((j + b) < dim); b++)
                     {
                         if ((M[i][j  + b]) == true)
                             cur = cur | (0x01 << b);
                         
                     }
                     tumbnail[i * 38 / dim  + j * 38 / dim + 5] = (byte)cur;
                 }
             return tumbnail;
         }
         internal byte GetHiByte(int Value)
         {
             byte result = (byte)(Value >> 8);
             return result;
         }

         internal byte GetLowByte(int Value)
         {
             byte result = (byte)Value;
             return result;
         }
    }
}
