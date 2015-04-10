using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using System.Drawing;

namespace EmbroideryFile
{
    public class StitchToBmp
    {
        private Point _translateStart;
        private readonly IEnumerable<CoordsBlock> _blocks;
        private int _width;
        private int _height;
        private float _scale;

        public StitchToBmp(List<CoordsBlock> blocks, int size)
        {

            _blocks = blocks;
            CalculateScaling(size);
            CalcTranslate();

        }

        private bool WidthGTOREQHeight()
        {
            return GetWidth() >= GetHeight();
        }

        private void CalculateScaling(int size)
        {
            var width = GetWidth();
            var height = GetHeight();
            if (WidthGTOREQHeight())
            {
                _width = size;
                _height = size*height/width;
                _scale = ((float) _width)/((float) width);
            }
            else
            {
                _height = size;
                _width = size*width/height;
                _scale = ((float) _height)/((float) height);
            }

        }

        /// <summary>
        /// Calculate translating
        /// </summary>
        private void CalcTranslate()
        {

            _translateStart.X = -(int) (GetXMin()*_scale);
            _translateStart.Y = -(int) (GetYMin()*_scale);

        }

        private int GetXMin()
        {
            return _blocks.SelectMany(block => block.AsEnumerable()).Min(coord => coord.X);
        }

        private int _yMin;

        private int GetYMin()
        {
            return _blocks.SelectMany(block => block.AsEnumerable()).Min(coord => coord.Y);
        }

        private int _blockWidth = -1;

        private int GetWidth()
        {
            if (_blockWidth < 0)
            {
                int xmin = GetXMin();
                int xmax = _blocks.SelectMany(block => block.AsEnumerable()).Max(coord => coord.X);
                _blockWidth = xmax - xmin;
            }
            return _blockWidth;
        }

        private int _blocksHeight = -1;

        private int GetHeight()
        {
            if (_blocksHeight < 0)
            {
                int ymin = GetYMin();
                int ymax = _blocks.SelectMany(block => block.AsEnumerable()).Max(coord => coord.Y);
                _blocksHeight = ymax - ymin;
            }
            return _blocksHeight;
        }

        public Bitmap DesignToBitmap(Single threadThickness)
        {
            Bitmap drawArea = new Bitmap(_width, _height);

            using (Graphics xGraph = Graphics.FromImage(drawArea))
            {
                Pen tempPen;
                xGraph.TranslateTransform(_translateStart.X, _translateStart.Y);
                xGraph.FillRectangle(Brushes.Transparent, 0, 0, _width, _height);
                IEnumerable<stitchBlock> tmpblocks = GetScaledPointBlock(_scale, _scale);
                //for (int i = 0; i < tmpblocks.Count; i++)
                foreach (var stitchBlock in tmpblocks)
                {
                    if ((stitchBlock.stitches.Length > 1)) //must have 2 points to make a line
                    {
                        tempPen = new Pen(stitchBlock.color)
                            {
                                Width = threadThickness,
                                StartCap = LineCap.Round,
                                EndCap = LineCap.Round,
                                LineJoin = LineJoin.Round
                            }
                            ;
                        xGraph.SmoothingMode = SmoothingMode.AntiAlias;
                        xGraph.DrawLines(tempPen, stitchBlock.stitches);
                    }
                }
            }

            return drawArea;

        }



        public IEnumerable<stitchBlock> GetScaledPointBlock(float xscale, float yscale)
        {
            int ci = 0;
            if (0.0 == xscale) xscale = 1.0f;
            if (0.0 == yscale) yscale = 1.0f;
            return (_blocks.Where(b => b.Jumped != true).Select(b =>
                                                                new stitchBlock()
                                                                    {
                                                                        color = b.Color,
                                                                        colorIndex = ci++,
                                                                        stitches = b.AsPoints(xscale, yscale),
                                                                        stitchesInBlock = b.Count
                                                                    })).ToList();

        }

        public void FillStreamWithPng(Stream stream)
        {
            using (var bitmap = DesignToBitmap(0.1F))
            {



                var format = bitmap.GetImageFormat();
                //create a collection of all parameters that we will pass to the encoder
                // EncoderParameters encoderParams = new EncoderParameters();

                //set the quality parameter for the codec
                //encoderParams.Param[0] = qualityParam;
                //var encoder = GetEncoderInfo("image/png");
                //encoderParams.Param[0] = new EncoderParameter(encoder, 0x00);
                bitmap.Save(stream, ImageFormat.Png);
                //bitmap.Save(@"c:\test.png", ImageFormat.Png);

            }
        }
    }
}
