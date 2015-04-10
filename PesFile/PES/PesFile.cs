/*
Embroidery Reader - an application to view .pes embroidery designs

Copyright (C) 2011  Nathan Crawford
 
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
 
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA
02111-1307, USA.

A copy of the full GPL 2 license can be found in the docs directory.
You can contact me at http://www.njcrawford.com/contact/.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace EmbroideryFile
{
    public enum statusEnum { NotOpen, IOError, ParseError, Ready };

   

    public struct intPair
    {
        public int a;
        public int b;
    }

    public class PesFile :PecFile
    {
        BinaryReader fileIn;
        //int imageWidth;
        //int imageHeight;
        string _filename;
        public readonly List<Int64> pesHeader = new List<Int64>();
        public readonly List<Int16> embOneHeader = new List<short>();
        //public List<Int16> sewSegHeader = new List<short>();
        public readonly List<Int16> embPunchHeader = new List<short>();
        //public List<Int16> sewFigSegHeader = new List<short>();
        internal readonly List<stitchBlock> blocks = new List<stitchBlock>();
        public readonly List<intPair> colorTable = new List<intPair>();
        private statusEnum _readyStatus = statusEnum.NotOpen;
        Int64 startStitches = 0;
        string lastError = "";
        string pesNum = "";
        Point translateStart;


        //means we couldn't figure out some or all
        //of the colors, best guess will be used
        private bool colorWarning = false;

        private bool formatWarning = false;

        private bool classWarning = false;
        #region [Constructors]
        public PesFile(string filename):base(filename)
        {
            OpenFile(filename);
        }

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="stream"></param>
        public PesFile(Stream stream):base(stream)
        { 
           
        }
        #endregion [Constructors]


        #region [Interface members]
        /// <summary>
        /// Implements <see cref="IGetEmbroideryData"/> interface member
        /// </summary>
        //public EmbroideryData Design { get { return parser.DesignInfo; } }

        public override void LoadEmbroidery(Stream stream)
        {           
            BinaryReader reader = new BinaryReader(stream);
           OpenFile(reader);
        }

        #endregion [Interface members]


        private void OpenFile(string filename)
        {

                _filename = filename;
                fileIn = new System.IO.BinaryReader(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                OpenFile(fileIn);

        }


        private void OpenFile(BinaryReader reader)
        {    
            
            string startFileSig = "";
            for (int i = 0; i < 8; i++)//8 bytes
            {
                startFileSig += reader.ReadChar();
            }
            if (!startFileSig.StartsWith("#PES"))//this is not a file that we can read
            {
                _readyStatus = statusEnum.ParseError;
                lastError = "Missing #PES at beginning of file";
                reader.Close();
                return;
            }

            parser.LoadPec(reader, true, reader.ReadInt32());

        }

        //void readCSewFigSeg(System.IO.BinaryReader file)
        //{
        //    startStitches = fileIn.BaseStream.Position;

        //    bool doneWithStitches = false;
        //    int xValue = -100;
        //    int yValue = -100;
        //    stitchBlock currentBlock;
        //    int blockType; //if this is equal to newColorMarker, it's time to change color
        //    int colorIndex = 0;
        //    int remainingStitches;
        //    List<Point> stitchData;
        //    stitchData = new List<Point>();
        //    currentBlock = new stitchBlock();

        //    while (!doneWithStitches)
        //    {
        //        //reset variables
        //        xValue = 0;
        //        yValue = 0;

        //        blockType = file.ReadInt16();
        //        if (blockType == 16716)
        //            break;
        //        colorIndex = file.ReadInt16();
        //        if (colorIndex == 16716)
        //            break;
        //        remainingStitches = file.ReadInt16();
        //        if (remainingStitches == 16716)
        //            break;
        //        while (remainingStitches >= 0)
        //        {
        //            xValue = file.ReadInt16();
        //            if (xValue == -32765)
        //            {
        //                break;//drop out before we start eating into the next section 
        //            }
        //            if (remainingStitches == 0)
        //            {
        //                int junk2 = 0;
        //                junk2 = blocks.Count;

        //                file.ReadBytes(24);
        //                if (file.ReadInt16() == -1)
        //                    doneWithStitches = true;

        //                currentBlock.stitches = new Point[stitchData.Count];
        //                stitchData.CopyTo(currentBlock.stitches);
        //                currentBlock.colorIndex = colorIndex;
        //                currentBlock.color = ColorIndex.ColorByIndex(colorIndex);
        //                currentBlock.stitchesInBlock = stitchData.Count;
        //                blocks.Add(currentBlock);
        //                stitchData = new List<Point>();
        //                currentBlock = new stitchBlock();

        //                file.ReadBytes(48);

        //                break;
        //            }
        //            else if (xValue == 16716 || xValue == 8224)
        //            {
        //                doneWithStitches = true;
        //                break;
        //            }
        //            yValue = fileIn.ReadInt16();
        //            if (yValue == 16716 || yValue == 8224)
        //            {
        //                doneWithStitches = true;
        //                break;
        //            }
        //            stitchData.Add(new Point(xValue - translateStart.X, yValue + imageHeight - translateStart.Y));
        //            remainingStitches--;
        //        }
        //    }
        //    if (stitchData.Count > 1)
        //    {
        //        currentBlock.stitches = new Point[stitchData.Count];
        //        stitchData.CopyTo(currentBlock.stitches);
        //        currentBlock.colorIndex = colorIndex;
        //        currentBlock.color = ColorIndex.ColorByIndex(colorIndex);
        //        currentBlock.stitchesInBlock = stitchData.Count;
        //        blocks.Add(currentBlock);
        //    }
        //}

        List<stitchBlock> filterStitches(List<stitchBlock> input, int threshold)
        {
            List<stitchBlock> retval = new List<stitchBlock>();
            List<Point> tempStitchData = new List<Point>();
            for (int x = 0; x < input.Count; x++)
            {

                for (int i = 0; i < input[x].stitches.Length; i++)
                {
                    if (i > 0)//need a previous point to check against, can't check the first
                    {
                        double diffx = Math.Abs(input[x].stitches[i].X - input[x].stitches[i - 1].X);
                        double diffy = Math.Abs(input[x].stitches[i].Y - input[x].stitches[i - 1].Y);
                        if (Math.Sqrt(Math.Pow(diffx, 2.0) + Math.Pow(diffy, 2.0)) < threshold) //check distance between this point and the last one
                        {
                            if (tempStitchData.Count == 0 && i > 1)//first stitch of block gets left out without this, except for very first stitch
                            {
                                tempStitchData.Add(input[x].stitches[i - 1]);
                            }
                            tempStitchData.Add(input[x].stitches[i]);
                        }
                        else//stitch is too far from the previous one
                        {
                            if (tempStitchData.Count > 2)//add the block and start a new one
                            {
                                stitchBlock tempBlock = new stitchBlock();
                                tempBlock.color = input[x].color;
                                tempBlock.colorIndex = input[x].colorIndex;
                                tempBlock.stitches = new Point[tempStitchData.Count];
                                tempStitchData.CopyTo(tempBlock.stitches);
                                retval.Add(tempBlock);
                                tempStitchData = new List<Point>();
                            }
                            else//reset variables
                            {
                                tempStitchData = new List<Point>();
                            }
                        }
                    }
                    else //just add the first one, don't have anything to compare against
                    {
                        tempStitchData.Add(input[x].stitches[i]);
                    }
                }
                if (tempStitchData.Count > 2)
                {
                    stitchBlock tempBlock = new stitchBlock();
                    tempBlock.color = input[x].color;
                    tempBlock.colorIndex = input[x].colorIndex;
                    tempBlock.stitches = new Point[tempStitchData.Count];
                    tempStitchData.CopyTo(tempBlock.stitches);
                    retval.Add(tempBlock);
                    tempStitchData = new List<Point>();
                }
            }
            return retval;
        }

        //public int GetWidth()
        //{
        //    return imageWidth;
        //}

        //public int GetHeight()
        //{
        //    return imageHeight;
        //}

        public string GetFileName()
        {
            if (_filename == null)
            {
                return "";
            }
            else
            {
                return _filename;
            }
        }

        public void saveDebugInfo()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(System.IO.Path.ChangeExtension(_filename, ".txt"));
            outfile.Write(getDebugInfo());
            outfile.Close();
        }

        public string getDebugInfo()
        {
            System.IO.StringWriter outfile = new System.IO.StringWriter();
            string name = "";
            outfile.WriteLine("PES header");
            outfile.WriteLine("PES number:\t" + pesNum);
            for (int i = 0; i < pesHeader.Count; i++)
            {
                name = (i + 1).ToString();
                outfile.WriteLine(name + "\t" + pesHeader[i].ToString());
            }
            if (embOneHeader.Count > 0)
            {
                outfile.WriteLine("CEmbOne header");
                for (int i = 0; i < embOneHeader.Count; i++)
                {
                    switch (i + 1)
                    {
                        case 22:
                            name = "translate x";
                            break;
                        case 23:
                            name = "translate y";
                            break;
                        case 24:
                            name = "width";
                            break;
                        case 25:
                            name = "height";
                            break;
                        default:
                            name = (i + 1).ToString();
                            break;
                    }

                    outfile.WriteLine(name + "\t" + embOneHeader[i].ToString());
                }
            }
            if (embPunchHeader.Count > 0)
            {
                outfile.WriteLine("CEmbPunch header");
                for (int i = 0; i < embPunchHeader.Count; i++)
                {
                    switch (i + 1)
                    {
                        default:
                            name = (i + 1).ToString();
                            break;
                    }

                    outfile.WriteLine(name + "\t" + embPunchHeader[i].ToString());
                }
            }

            outfile.WriteLine("stitches start: " + startStitches.ToString());
            outfile.WriteLine("block info");
            outfile.WriteLine("number\tcolor\tstitches");
            for (int i = 0; i < this.blocks.Count; i++)
            {
                outfile.WriteLine((i + 1).ToString() + "\t" + blocks[i].colorIndex.ToString() + "\t" + blocks[i].stitchesInBlock.ToString());
            }
            outfile.WriteLine("color table");
            outfile.WriteLine("number\ta\tb");
            for (int i = 0; i < colorTable.Count; i++)
            {
                outfile.WriteLine((i + 1).ToString() + "\t" + colorTable[i].a.ToString() + ", " + colorTable[i].b.ToString());
            }
            if (blocks.Count > 0)
            {
                outfile.WriteLine("Extended stitch debug info");
                for (int blocky = 0; blocky < blocks.Count; blocky++)
                {
                    outfile.WriteLine("block " + (blocky + 1).ToString() + " start");
                    for (int stitchy = 0; stitchy < blocks[blocky].stitches.Length; stitchy++)
                    {
                        outfile.WriteLine(blocks[blocky].stitches[stitchy].X.ToString() + ", " + blocks[blocky].stitches[stitchy].Y.ToString());
                    }
                }
            }
            outfile.Close();
            return outfile.ToString();
        }

        public statusEnum getStatus()
        {
            return _readyStatus;
        }

        public string getLastError()
        {
            return lastError;
        }

        public bool getColorWarning()
        {
            return colorWarning;
        }

        public bool getFormatWarning()
        {
            return formatWarning;
        }

        public bool getClassWarning()
        {
            return classWarning;
        }

       



    }
}
