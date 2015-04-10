using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;


namespace EmbroideryFile
{
    /// <summary>
    /// Loads stitches from a <see cref="Stream"/>
    /// It is supposed what data in a <see cref="Stream"/> are stored in Tajama DST format
    /// </summary>
    public class DstFile : IGetEmbroideryData
    {
        EmbroideryData _design;
        DstHeader _header;
        private const int positionAfterHeader = 512;
        Stream _inputStream;
        Stream _outputStream;


        public EmbroideryData Design { get { return _design; } set { _design = value; } }

        public DstHeader GetDstInfo { get { return _header; } }

        public IDesignInfo DesignInfo { get { return (_design as IDesignInfo); } }



        #region [ctor]
        public DstFile()
        {
            Init();
        }


        public DstFile(Stream embroStream)
        {
            Init();
            LoadEmbroidery(embroStream);
            FillDesignInfo();           
        }

        void Init()
        { 
         _design = new EmbroideryData();
            _header = new DstHeader();
        }

        #endregion [ctor]



        #region []
        /// <summary>
        /// Write design to <see cref="Stream"/> in DST  format
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="stream"></param>
        public void WriteStitchesToDstStream(List<CoordsBlock> coords, Stream stream)
        {
               _design.Blocks = coords;
               FillHeader( _header);
               Write(stream);
                                         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        void FillHeader( DstHeader header)
    {
       

         header.DesignName = _design.DesignName ?? string.Empty;
           
            header.TotalStitchCount = _design.GetTotalStitches();
            header.ColorChangeCount = _design.GetColorChanges();
            header.Xmax = _design.GetXCoordMax();

            header.Xmin = _design.GetXCoordMin();
            header.Ymax = _design.GetYCoordMax();
            header.Ymin = _design.GetYCoordMin();
            header.XOffset = 0;
            header.YOffset = 0;
            header.MX = 0;
            header.MY = 0;
       

    }
        #endregion []

        #region [Loading]
        /// <summary>
        /// Fills <see cref="EmbroideryData"/> after loading header from a <see cref="Stream"/>
        /// </summary>
        void FillDesignInfo()
   {
       if (_design != null && _header != null )
       {
          
         
           _design.DesignName = _header.DesignName;
           _design.Height = _header.Height;
           _design.Width = _header.Width;
           _design.Xmax = _header.Xmax;
           _design.Ymax = _header.Ymax;
           _design.Xmin = _header.Xmin;
           _design.Ymin = _header.Ymin;
           _design.ColorChangeCount = _header.ColorChangeCount;
           _design.JumpsBlocksCount = _design.Blocks.Count(b => b.Jumped == true);
           _design.StitchBlockCount = _header.StitchBlockCount;
           _design.TotalStitchCount = _header.TotalStitchCount;
           _design.Type = EmbroType.Dst;
       }

   }

        public void LoadEmbroidery(Stream embroStream)
        {
            _inputStream = embroStream;

            _header = new DstHeader(embroStream);
            _inputStream.Position = positionAfterHeader;
                _design.Blocks = ReadStitches();

        }




        #region [old]
        List<CoordsBlock> ReadStitchesOld()
        {
            List<CoordsBlock> ResultCoordList = new List<CoordsBlock>();
            CoordsBlock currentCoordList = new CoordsBlock();
            int n;
            byte[] stitch = new byte[3];
            int coCo = 0;
            int dX, dY, curAbsX = 0, curAbsY = 0;
            int blockIndex = 0;
            bool endFileFlag = false, jumpFlag = false;
            Random rand = new Random();
            _design.ColorMap.Add(blockIndex, coCo);
            _design.ColourInfo.Add(coCo, Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));
            //READ STITCH RECORDS
            while (endFileFlag != true)
            {
                n = _inputStream.Read(stitch, 0, 3);
                if (n == 3)
                {
                    dX = decode_record_dx(stitch[0], stitch[1], stitch[2]);
                    dY = decode_record_dy(stitch[0], stitch[1], stitch[2]);
                    switch (GetStitchType(stitch[2]))
                    {

                        case DstStitchType.NORMAL:
                            if (jumpFlag)
                            {
                                jumpFlag = false;
                                ResultCoordList.Add(currentCoordList);
                                currentCoordList = new CoordsBlock();
                                blockIndex++;
                            }
                            curAbsX = curAbsX + dX; curAbsY = curAbsY + dY;
                            currentCoordList.Add(new Coords { X = curAbsX, Y = curAbsY });
                            break;
                        case DstStitchType.JUMP:
                            if (jumpFlag != true)
                            {
                                jumpFlag = true;
                                if (currentCoordList.Count > 0)
                                {
                                    ResultCoordList.Add(currentCoordList);
                                    blockIndex++;
                                }
                                currentCoordList = new CoordsBlock();
                                currentCoordList.Jumped = true;

                            }
                            curAbsX = curAbsX + dX; curAbsY = curAbsY + dY;
                            break;
                        case DstStitchType.STOP:
                            coCo++;
                            if (currentCoordList.Count > 0)
                            {
                                ResultCoordList.Add(currentCoordList);
                                blockIndex++;
                            }
                            _design.ColorMap.Add(blockIndex, coCo);
                            _design.ColourInfo.Add(coCo, Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));
                            ResultCoordList.Add(GetStopBlock());
                            currentCoordList = new CoordsBlock();
                            blockIndex++;
                            break;
                        case DstStitchType.END:
             endFileFlag = true;
                            break;
                        case DstStitchType.UNKNOWN:
                            endFileFlag = true;
                            break;
                    }
                }
                else endFileFlag = true;



            }
            return ResultCoordList;
        }
        #endregion [old]

        /// <summary>
        /// Reads and encode stitches form  <see cref="Stream"/> class
        /// </summary>
        /// <param name="stream">Suggested  position has been specified already</param>
        /// <returns></returns>
        List<CoordsBlock> ReadStitches()
        {
            List<CoordsBlock> ResultCoordList = new List<CoordsBlock>();
            CoordsBlock currentCoordList = new CoordsBlock();
            int n;
            byte[] stitch = new byte[3];
            int coCo = 0;
            int dX, dY, curAbsX = 0, curAbsY = 0;
            int blockIndex = 0;
            bool endFileFlag = false, jumpFlag = false;
            Color color;
            Random rand = new Random();
            _design.ColorMap.Add(blockIndex, coCo);
            color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            _design.ColourInfo.Add(coCo, color);
            //READ STITCH RECORDS
            while (endFileFlag != true)
            {
                n = _inputStream.Read(stitch, 0, 3);
                if (n == 3)
                {
                    dX = decode_record_dx(stitch[0], stitch[1], stitch[2]);
                    dY = decode_record_dy(stitch[0], stitch[1], stitch[2]);
                    switch (GetStitchType(stitch[2]))
                    {

                        case DstStitchType.NORMAL:
                            if (jumpFlag)
                            {
                                jumpFlag = false;
                                ResultCoordList.Add(currentCoordList);
                                currentCoordList = new CoordsBlock(color);
                                blockIndex++;
                            }
                            curAbsX = curAbsX + dX; curAbsY = curAbsY - dY;
                            currentCoordList.Add(new Coords { X = curAbsX, Y = curAbsY });
                            break;
                        case DstStitchType.JUMP:
                            if (jumpFlag != true)
                            {

                                jumpFlag = true;
                                if (currentCoordList.Count > 0)
                                {
                                    ResultCoordList.Add(currentCoordList);
                                    blockIndex++;
                                }
                                currentCoordList = new CoordsBlock(color) {Jumped = true};
                            }
                            curAbsX = curAbsX + dX; curAbsY = curAbsY - dY;

                            break;
                        case DstStitchType.STOP:
                            coCo++;

                            if (currentCoordList.Count > 0)
                            {
                                ResultCoordList.Add(currentCoordList);
                                blockIndex++;
                            }
                            _design.ColorMap.Add(blockIndex, coCo);
                            color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                            _design.ColourInfo.Add(coCo, color);
                            ResultCoordList.Add(GetStopBlock());
                            currentCoordList = new CoordsBlock(color);
                            blockIndex++;
                            break;
                        case DstStitchType.END:
                            endFileFlag = true;
                            break;
                        case DstStitchType.UNKNOWN:
                            endFileFlag = true;
                            break;
                    }
                }
                else endFileFlag = true;



            }
            return ResultCoordList;
        }
        #endregion [Loading]

        #region [Writing]
        void WriteStitchBlocks()
        {
            int blockIndex = 0;
            int i;
            int stitchCount;
            List<CoordsBlock> Blocks = _design.Blocks;
            Coords coords = new Coords() { X = 0, Y = 0 };

            //first point
            Coords prevCoords = coords;
            CoordsBlock prevBlock = null;
            foreach (CoordsBlock stitches in Blocks)
            {
                WriteTrimStitch();
                stitchCount = stitches.Count;
                if (prevBlock != null &&  stitchCount > 1)
                {
                    prevCoords = stitches.First();
                    WriteJumpsSeqByte(prevCoords, prevBlock.Last());
                   
                   
                }
                if (stitchCount > 1) prevBlock = stitches;
                for (i = 1; i < stitchCount; i++)
                {
                    coords = stitches[i];
                    WriteDstStitch(coords.X, coords.Y, prevCoords.X, prevCoords.Y, _outputStream);
                    prevCoords = coords;
                }
               
                blockIndex++;
                WriteTrimStitch();
               
            }
            //write 
        }

       
        void WriteHeader()
        {

            _header.WriteToStream(_outputStream);

        }

        /// <summary>
        /// Writes generated DST to <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        public void Write(Stream stream)
        {
            _outputStream = stream;           
            
                WriteHeader();
                WriteStitchBlocks();
                stream.Position = 0;
            
        }

        #endregion [Writing]

        #region [stitch block aux]
        CoordsBlock GetStopBlock()
        {
            return new CoordsBlock() { Stop = true };
        }

        CoordsBlock GetJumpBlock()
        {
            return new CoordsBlock() { Jumped = true };
        }

        CoordsBlock GetJumpBlock(Coords coords, Coords prevCoords)
        {
            CoordsBlock result = new CoordsBlock() { Jumped = true };
            result.Add(prevCoords);
            result.Add(coords);
            return result;
        }


        void WriteJumpsSeqByte(Coords coords, Coords prevCoords)
        {
            WriteJumpsSeqByte(coords.X, coords.Y, prevCoords.X, prevCoords.Y);
        }

        void WriteJumpsSeqByte(int X, int Y, int prevX, int prevY)
        {

            int deltaX, deltaY;
            bool negX = prevX > X;
            bool negY = prevY > Y;
            int i;

            int distX = Math.Abs(prevX - X);
            int distY = Math.Abs(prevY - Y);
 
            int lastX = distX % 121;
            int lastY = distY % 121;

            int steps = ((distX > distY) ? distX : distY) / 121;

            if (steps > 0)           
            {

                if (distX > distY)
                {
                    deltaX = 121;
                    if (distY != 0) deltaY = (121*distY/distX);
                    else deltaY = 0;
                    lastY = distY - (deltaY * steps);
                }

                else
                {
                    deltaY = 121;
                    if (distX != 0) deltaX = (121*distX/distY);
                    else deltaX = 0;
                    lastX = distX - (deltaX * steps);
                }

                for ( i = 0; i < steps; i++)
                {
                    _outputStream.Write(
                        encode_record((negX ? -1 : 1)*deltaX, (negY ? -1 : 1)*deltaY, DstStitchType.JUMP), 0, 3);
                }
            }
            

            if ((lastX != 0) || (lastY != 0))
            {
                _outputStream.Write(encode_record((negX ? -1 : 1) * lastX, (negY ? -1 : 1) * lastY, DstStitchType.JUMP), 0, 3);
            }

        }

        private void WriteTrimStitch()
        {
            _outputStream.Write(encode_record(0, 0, DstStitchType.JUMP), 0, 3);
        }

        void WriteDstStitch(Coords coords, Coords prevCoords)
        {

            if (Math.Abs(prevCoords.X - coords.X) > 121 || Math.Abs(prevCoords.Y - coords.Y) > 121) WriteJumpsSeqByte(coords, prevCoords);
            else
            {
               _outputStream.Write(encode_record(coords.X - prevCoords.X, coords.Y - prevCoords.Y, DstStitchType.JUMP), 0, 3);
            }
        }

        void WriteDstStitch(int X, int Y, int prevX, int prevY, Stream stream)
        {

            if ((Math.Abs(X - prevX) > 121) || (Math.Abs(Y - prevY) > 121)) WriteJumpsSeqByte(X, Y, prevX, prevY);
            else
            {
                stream.Write(encode_record(X - prevX, Y - prevY, DstStitchType.NORMAL), 0, 3);
            }
        }

        #endregion 


        #region [Auxilary]
        /// <summary>
        /// Returns type of stitch according to value of third byte of stitch:
        /// 0x03: NORMAL
        /// 0x83: JUMP
        /// 0xC3: STOP
        /// </summary>
        /// <param name="b2"> <paramref name="b2"/></param>
        /// <returns></returns>
        DstStitchType GetStitchType(byte b2)
        {
            if (b2 == 243) return DstStitchType.END;
            switch (b2 & 0xC3)
            {
                case 0x03: return DstStitchType.NORMAL;
                case 0x83: return DstStitchType.JUMP;
                case 0xC3: return (DstStitchType.STOP);
                default: return (DstStitchType.UNKNOWN);
            };
        }
        int getbit(byte b, int pos)
        {
            int bit;
            bit = (b >> pos) & 1;
            return (bit);
        }

        Coords GetShiftedCoords(ref int absX, ref int absY, Coords coords)
        {
            absX = coords.X + absX;
            absY = coords.Y + absY;
            coords.X = absX;
            coords.Y = absY;
            return coords;
        }

        Coords GetRealativeStitchCoords(byte[] stitch)
        {

            Coords coords = new Coords { X = decode_record_dx(stitch[0], stitch[1], stitch[2]), Y = decode_record_dy(stitch[0], stitch[1], stitch[2]) };
            return coords;

        }
        int decode_record_dx(byte b0, byte b1, byte b2)
        {
            int x = 0;
            x += getbit(b2, 2) * (+81);
            x += getbit(b2, 3) * (-81);
            x += getbit(b1, 2) * (+27);
            x += getbit(b1, 3) * (-27);
            x += getbit(b0, 2) * (+9);
            x += getbit(b0, 3) * (-9);
            x += getbit(b1, 0) * (+3);
            x += getbit(b1, 1) * (-3);
            x += getbit(b0, 0) * (+1);
            x += getbit(b0, 1) * (-1);
            return (x);
        }

        int decode_record_dy(byte b0, byte b1, byte b2)
        {
            int y = 0;
            y += getbit(b2, 5) * (+81);
            y += getbit(b2, 4) * (-81);
            y += getbit(b1, 5) * (+27);
            y += getbit(b1, 4) * (-27);
            y += getbit(b0, 5) * (+9);
            y += getbit(b0, 4) * (-9);
            y += getbit(b1, 7) * (+3);
            y += getbit(b1, 6) * (-3);
            y += getbit(b0, 7) * (+1);
            y += getbit(b0, 6) * (-1);
            return (y);
        }

       

        byte setbit(int pos)
        {
            return (byte)(1 << (byte)pos);
        }

        byte[] encode_record(int x, int y, DstStitchType stitchType)
        {
            byte b0, b1, b2;
            b0 = b1 = b2 = 0;
            byte[] b = new byte[3];
            // cannot encode values >+121 or < -121.

            if (x >= +41) { b2 += setbit(2); x -= 81; };
            if (x <= -41) { b2 += setbit(3); x += 81; };
            if (x >= +14) { b1 += setbit(2); x -= 27; };
            if (x <= -14) { b1 += setbit(3); x += 27; };
            if (x >= +5) { b0 += setbit(2); x -= 9; };
            if (x <= -5) { b0 += setbit(3); x += 9; };
            if (x >= +2) { b1 += setbit(0); x -= 3; };
            if (x <= -2) { b1 += setbit(1); x += 3; };
            if (x >= +1) { b0 += setbit(0); x -= 1; };
            if (x <= -1) { b0 += setbit(1); x += 1; };
            if (x != 0)
            {
                //error

            };
            if (y >= +41) { b2 += setbit(5); y -= 81; };
            if (y <= -41) { b2 += setbit(4); y += 81; };
            if (y >= +14) { b1 += setbit(5); y -= 27; };
            if (y <= -14) { b1 += setbit(4); y += 27; };
            if (y >= +5) { b0 += setbit(5); y -= 9; };
            if (y <= -5) { b0 += setbit(4); y += 9; };
            if (y >= +2) { b1 += setbit(7); y -= 3; };
            if (y <= -2) { b1 += setbit(6); y += 3; };
            if (y >= +1) { b0 += setbit(7); y -= 1; };
            if (y <= -1) { b0 += setbit(6); y += 1; };
            if (y != 0)
            {
                //error

            };
            switch (stitchType)
            {
                case DstStitchType.NORMAL:
                    b2 += (byte)3;
                    break;
                case DstStitchType.END:
                    b2 = (byte)243;
                    b0 = b1 = (byte)0;
                    break;
                case DstStitchType.JUMP:
                    b2 += (byte)131;
                    break;
                case DstStitchType.STOP:
                    b2 += (byte)195;
                    break;
                default:
                    b2 += 3;
                    break;
            };
            b[0] = b0; b[1] = b1; b[2] = b2;
            return b;
        }

        int atorgb(byte[] val)
        {
            // expect string beginning with 6 hex digits
            int i;
            int rgb;
            rgb = 0;
            for (i = 0; i < 6; i++)
            {
                if (val[i] >= '0' && val[i] <= '9')
                {
                    rgb = rgb * 16 + val[i] - '0';
                }
                else if (val[i] >= 'A' && val[i] <= 'F')
                {
                    rgb = rgb * 16 + val[i] - 'A';
                }
                else if (val[i] >= 'a' && val[i] <= 'f')
                {
                    rgb = rgb * 16 + val[i] - 'a';
                }
                else if (val[i] == ',' || val[i] == '\0')
                {
                    // early delimiter
                    break;
                }
                else
                {
                    // unknown character
                    break;
                };
            };
            return (rgb);
        }
        #endregion [Auxilary]

    }

}

