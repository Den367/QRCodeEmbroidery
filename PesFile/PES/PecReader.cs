using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace EmbroideryFile
{
    public class PecReaderParser
    {
        //BinaryReader fileIn;
        //int imageWidth;
        //int imageHeight;
        int totalStitches = 0;
        int colorChanges =  0;
        int jumpStitches = 0;
        //int stitchBlocks = 0;
       // List<stitchBlock> blocks = new List<stitchBlock>();
        List<intPair> colorTable = new List<intPair>();     
        //Int64 startStitches = 0;       
        //Coords translateStart;
        EmbroideryData result = new EmbroideryData();

        public EmbroideryData DesignInfo { get { return result; } }

        /// <summary>
        /// Reads embroidery data from PEC block
        /// </summary>
        /// <param name="reader"> <see cref="BinaryReader"/> to read PEC from</param>
        /// <param name="insidePES">Indicates PEC block is inside PES file</param>
        public void LoadPec(BinaryReader reader, bool insidePES, int pecStartOffset)
        {
            int prevX = 0;
            int prevY = 0;
            int maxX = 0;
            int minX = 0;
            int maxY = 0;
            int minY = 0;
            int colorNum = 0;
            int colorIndex = 0;
            List<Point> tempStitches = new List<Point>();
            Color color;
            List<CoordsBlock> ResultCoordList = new List<CoordsBlock>();
            CoordsBlock currentCoordList = new CoordsBlock();
             bool thisPartIsDone = false;
            bool jumpStitch = false;
            reader.BaseStream.Position = pecStartOffset;
                StringBuilder stringBuff = new StringBuilder();
                if (!insidePES)
                {
                    result.Type = EmbroType.Pec;
                    for (int i = 0; i < 11; i++) stringBuff.Append(reader.ReadChar());
                    if (!stringBuff.ToString().StartsWith("#PEC")) throw new Exception("Missing #PEC at the beginning of file");
                }
                else
                {
                    result.Type = EmbroType.Pes;
                    for (int i = 0; i < 3; i++) stringBuff.Append(reader.ReadChar());
                }

                if (!stringBuff.ToString().EndsWith("LA:"))         throw new Exception("Missing LA: before design name");
                
                stringBuff.Clear();

                for (int i = 0; i < 15; i++)  stringBuff.Append(reader.ReadChar());
                result.DesignName = stringBuff.ToString().Trim();

                if (!insidePES) reader.BaseStream.Position = 0x38;
                else reader.BaseStream.Position = pecStartOffset + 48;
                colorChanges = reader.ReadByte() + 1;
                List<byte> colorList = new List<byte>();
                for (int x = 0; x < colorChanges; x++)   colorList.Add(reader.ReadByte());
                
            if (!insidePES)
                reader.BaseStream.Position = 0x21C;
            else reader.BaseStream.Position = pecStartOffset + 532;
               
            // first of all add first color 

                colorIndex = colorList[colorNum];
                //result.ColorMap.Add(++colorNum, colorIndex);
                color = ColorIndex.ColorByIndex(colorIndex);
                //if (!result.ColourInfo.ContainsKey(colorIndex)) result.ColourInfo.Add(colorIndex, color);
                SetColorInfo(currentCoordList, colorNum, colorIndex, result);
                while (!thisPartIsDone)
                {
                    byte val1;  byte val2;
                    val1 = reader.ReadByte();    val2 = reader.ReadByte();
                    if (val1 == 255 && val2 == 0)
                    {
                        //end of stitches
                        thisPartIsDone = true;

                        //add the last block
                        ResultCoordList.Add(currentCoordList);

                    }
                    else if (val1 == 254 && val2 == 176)
                    {
                        //! COLOR SWITCH & start a new block                        
                        if (currentCoordList.Count > 0)
                        {
                            ResultCoordList.Add(currentCoordList);
                        }
                        ResultCoordList.Add(new CoordsBlock() { Stop = true});
                        colorNum++;
                        colorIndex = colorList[colorNum];
                        currentCoordList = new CoordsBlock();
                        
                       
                        SetColorInfo(currentCoordList, colorNum, colorIndex, result);
                        //read useless(?) byte
                        reader.ReadByte();
                    }
                    else
                    {
                        int deltaX = 0;  int deltaY = 0;
                        if ((val1 & 128) == 128)//$80
                        {
                            //this is a JUMP stitch create single block
                            jumpStitch = true;
                            if (currentCoordList.Count > 0) ResultCoordList.Add(currentCoordList);
                            else currentCoordList = null;


                            currentCoordList = new CoordsBlock();
                            
                            currentCoordList.colorIndex = colorIndex;
                            currentCoordList.Color = ColorIndex.ColorByIndex(colorIndex);


                           

                            //currentCoordList.Add(new Coords { X = prevX, Y = prevY });
                            jumpStitches++;
                            deltaX = ((val1 & 15) * 256) + val2;
                            if ((deltaX & 2048) == 2048) //$0800
                            {
                                deltaX = deltaX - 4096;
                            }
                            //read next byte for Y value
                            val2 = reader.ReadByte();
                        }
                        else
                        {
                            //normal stitch
                            totalStitches++;
                            deltaX = val1;
                            if (deltaX > 63)                          
                                deltaX = deltaX - 128;                            
                        }
                        if ((val2 & 128) == 128)//$80
                        {
                            //this is a jump stitch
                            int val3 = reader.ReadByte();
                            deltaY = ((val2 & 15) * 256) + val3;
                            if ((deltaY & 2048) == 2048)    deltaY = deltaY - 4096;
                            ResultCoordList.Add(GetJumpStitchBlock(prevX, prevY, prevX + deltaX, prevY + deltaY, ColorIndex.ColorByIndex(colorIndex)));
                        }
                        else
                        {
                            //normal stitch
                            deltaY = val2;
                            if (deltaY > 63) deltaY = deltaY - 128;                            
                        }
                        prevX = prevX + deltaX;
                        prevY = prevY + deltaY;
                        if (!jumpStitch) currentCoordList.Add(new Coords { X = prevX, Y = prevY });
                        else jumpStitch = false;
                        //tempStitches.Add(new Point(prevX , prevY ));
                      
                        if (prevX > maxX)    maxX = prevX;
                        else if (prevX < minX)           minX = prevX;                        
                        if (prevY > maxY)                     
                            maxY = prevY;                        
                        else if (prevY < minY)                        
                            minY = prevY;                        
                    }
                }
              

                //result.stitchBlocks = blocks;
                result.Blocks = ResultCoordList;
                result.StitchBlockCount = ResultCoordList.Count;
            result.JumpsBlocksCount = jumpStitches  ;
            result.TotalStitchCount = totalStitches + jumpStitches;
            result.ColorChangeCount = colorChanges;
                result.Width = maxX - minX;
                result.Height = maxY - minY;
                result.Xmin = minX;
                result.Xmax = maxX;
                result.Ymin = minY;
                result.Ymax = maxY;
                
        }

        void SetColorInfo(CoordsBlock block, int colorNum, int colorIndex, EmbroideryData data)
        {
            block.colorIndex = colorIndex;
            if (!result.ColorMap.ContainsKey(colorNum)) result.ColorMap.Add(colorNum, colorIndex);
            Color color =  ColorIndex.ColorByIndex(colorIndex);
            block.Color = color;
            if (!data.ColourInfo.ContainsKey(colorIndex)) data.ColourInfo.Add(colorIndex, color);
        }

        CoordsBlock GetJumpStitchBlock(int X1, int Y1, int X2, int Y2, Color color)
        {
            var result = new CoordsBlock();
            result.Jumped = true;
            result.Add(new Coords() { X = X1, Y = Y1 });
            result.Add(new Coords() { X = X2, Y = Y2 });
            result.Color = color;
            return result;
        }
    }
}
