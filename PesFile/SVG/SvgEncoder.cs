using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using Svg;
using ListOfCoordsBlock = System.Collections.Generic.List<EmbroideryFile.CoordsBlock>;
using IEnumerableCoordsBlock = System.Collections.Generic.IEnumerable<EmbroideryFile.CoordsBlock>;

namespace EmbroideryFile
{
    public class SvgEncoder : ISvgEncode
    {
        private const int Width = 480;
        private const int Height = 480;
        private XmlTextWriter _xWrite;
        private Stream _stream;
        private EmbroideryData embro;

        public SvgEncoder()
        {
            _xWrite = null;

        }

        /// <summary>
        /// Encodes embroidery to svg
        /// </summary>
        /// <param name="svgStream">destination svg <see cref="Stream"/></param>
        /// <param name="embroData"> <see cref="EmbroideryData"/></param>
        public SvgEncoder(Stream svgStream, EmbroideryData embroData)
        {
            _xWrite = null;
            _stream = svgStream;
            embro = embroData;
        }

        #region [Public Properties]

        #endregion [Public Properties]

        #region [Public Methods]

        public void WriteSvg()
        {
            double xScale;
            double yScale;


            WriteSvgStartWithAttr();
            var width = embro.ScaledWidth;
            var height = embro.ScaledHeight;
            var size = width > height ? width : height;
            WriteWidth(size);
            WriteHeight(size);

            //WriteViewBox(0, 0, size, size);
             WriteViewBox(0, 0, width, height);

            //double shiftX = (embro.Xmax - embro.Xmin) / 2;
            //double shiftX = -embro.Xmin;
            //double shiftY = (embro.Ymax - embro.Ymin) / 2;
            //double shiftY = -embro.Ymin;
            WriteFilter();
            _xWrite.WriteStartElement("g");

            xScale = 1.0;
            yScale = 1.0;
            _xWrite.WriteAttributeString("transform", string.Format("scale({0},{1})", xScale, yScale));
            //_xWrite.WriteAttributeString("filter", "url(#ShaderFilter)");

            // polylines
            WriteStitchBlocks(embro);

            _xWrite.WriteEndElement(); // g
            _xWrite.WriteEndElement(); // svg
            //xWrite.WriteEndDocument();
            _xWrite.Flush();

        }

        private void WriteSvgStartWithAttr()
        {
            if (_stream == null) _stream = new MemoryStream();
            if (_xWrite != null) _xWrite.Close();
            _xWrite = new XmlTextWriter(_stream, Encoding.UTF8);
            _xWrite.Formatting = Formatting.Indented;

            _xWrite.WriteStartElement("svg");
            _xWrite.WriteAttributeString("xmlns", "svg", null, "http://www.w3.org/2000/svg");
            _xWrite.WriteAttributeString("xmlns", null, null, "http://www.w3.org/2000/svg");
            _xWrite.WriteAttributeString("pagecolor", "#ffffff");

            _xWrite.WriteAttributeString("bordercolor", "#666666");
            _xWrite.WriteAttributeString("borderopacity", "1.0");

            _xWrite.WriteAttributeString("version", "1.1");
            _xWrite.WriteAttributeString("showgrid", "false");
            _xWrite.WriteAttributeString("preserveAspectRatio", "xMidYMid meet");

        }

        private void WriteSvgBoundAttr(double size, ListOfCoordsBlock blocks)
        {



            int minX = XCoordMin(blocks);
            int minY = YCoordMin(blocks);
            int maxX = XCoordMax(blocks);
            int maxY = YCoordMax(blocks);
            int width = maxX - minX;
            int height = maxY - minY;

            //double h = (double) height;
            //double w = (double) width;
            //double m = (h > w) ? h : w;
            //double scale = size/m;
            //double xScale = scale;
            //double yScale = -scale;
            //double newW = w*scale;
            //double newH = h*scale;
            //double newMinX = minX*scale;
            //double newMinY = minY*scale;
            //double newMaxX = maxX*scale;
            //double newMaxY = maxY*scale;
            //double translateX = -newMinX;
            //double translateY = -newMinY;

            WriteWidth(size);
            WriteHeight(size);
            //WriteViewBox(minX, minY, width, height);
            WriteViewBox(0, 0, width, height);

            _xWrite.WriteStartElement("g");


        }

        private void WriteSvgForCoordList(double size, ListOfCoordsBlock blocks)
        {
            WriteSvgStartWithAttr();
            WriteSvgBoundAttr(size, blocks);
            WriteStitchBlocksZeroShifted(blocks);
            WriteG_Svg_Endelement();
        }


        private void WriteG_Svg_Endelement()
        {
            _xWrite.WriteEndElement(); // g
            _xWrite.WriteEndElement(); // svg
            _xWrite.Flush();
        }

        /// <summary>
        /// Read created Svg as a string
        /// </summary>
        /// <returns></returns>
        public string ReadSvgString()
        {
            if (_stream != null)
            {
                var reader = new StreamReader(_stream);
                _stream.Position = 0;
                return reader.ReadToEnd();
            }
            return string.Empty;
        }

        public void FillStreamWithSvgFromCoordsLists(Stream strm, int size, ListOfCoordsBlock blocks)
        {
            _stream = strm;

            WriteSvgForCoordList((double) size, blocks);

        }

        public string GetSvgFromCoordsLists(int size, ListOfCoordsBlock blocks)
        {
            WriteSvgForCoordList((double) size, blocks);
            return ReadSvgString();
        }


        public void SaveSvgToPngStream(System.IO.Stream stream, Stream pngStream)
        {
            try
            {
                stream.Position = 0;
                var svgDocument = SvgDocument.Open<SvgDocument>(stream);
                var bitmap = svgDocument.Draw();
                bitmap.Save(pngStream, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
                throw;
            }



        }

        #endregion [Public]

        private void WriteWidth(double width)
        {
            _xWrite.WriteAttributeString("width", width.ToString(CultureInfo.InvariantCulture));


        }

        private void WriteHeight(double height)
        {
            _xWrite.WriteAttributeString("height", height.ToString(CultureInfo.InvariantCulture));

        }


        private void WriteFilter()
        {
            _xWrite.WriteRaw(@"<defs namespace='svg'>
        <filter id='ShaderFilter' filterUnits='userSpaceOnUse' x='-10%' y='-10%' width='110%' height='110%'>
          <feGaussianBlur in='SourceAlpha' stdDeviation='4' result='blur'/>
          <feOffset in='blur' dx='4' dy='4' result='offsetBlur' id='feOffset3043'/>
          <feSpecularLighting in='blur' surfaceScale='5' specularConstant='.75' specularExponent='20' lighting-color='#bbbbbb' result='specOut'>
            <fePointLight x='-500' y='-1000' z='2000'/>
          </feSpecularLighting>
          <feComposite in='specOut' in2='SourceAlpha' operator='in' result='specOut'/>
          <feComposite in='SourceGraphic' in2='specOut' operator='arithmetic' k1='0' k2='1' k3='1' k4='0' result='litPaint'/>
          <feMerge>
            <feMergeNode in='offsetBlur'/>
            <feMergeNode in='litPaint'/>
          </feMerge>        
        </filter>
      </defs>");

        }


        private void WriteViewBox(float minX, float minY, float width, float height)
        {
            _xWrite.WriteAttributeString("viewBox", string.Format("{0} {1} {2} {3}", minX, minY, width, height));
        }


        private void FillLinesInfo(List<Coords> coords, Color colour, int needle)
        {
            if (coords.Count == 0) return;

            StringBuilder draw = new StringBuilder();

            coords.ForEach(p => draw.AppendFormat("{0} {1},", p.X, p.Y));
            draw.Remove(draw.Length - 1, 1); // remove last comma
            _xWrite.WriteStartElement("polyline");
            _xWrite.WriteAttributeString("id", string.Format("needle_{0}", needle));
            _xWrite.WriteAttributeString("stroke", string.Format("rgb({0},{1},{2})", colour.R, colour.G, colour.B));
            _xWrite.WriteAttributeString("fill", "none");
            _xWrite.WriteAttributeString("points", draw.ToString());
            _xWrite.WriteEndElement();

        }


        private void WriteStitchBlocks(EmbroideryData info)
        {
            WriteStitchBlocksZeroShifted(info.Blocks);

        }


        private void WriteStitchBlocks(ListOfCoordsBlock blocks)
        {

            int ci = 0;
            if (blocks.Count > 0)
                blocks.ForEach(block =>
                    {
                        if (!(block.Jumped || block.Stop))
                        {
                            FillLinesInfo(block, block.Color, ci);
                            ci++;
                        }
                    });


        }

        private void WriteStitchBlocksZeroShifted(ListOfCoordsBlock blocks)
        {
            int minX = XCoordMin(blocks);
            int minY = YCoordMin(blocks);
            ShiftXY(blocks, -minX, -minY);
            //int x = minX;
            //int y = minY;
            //minX = XCoordMin(blocks);
            // minY = YCoordMin(blocks);
            WriteStitchBlocks(blocks);
        }

        private int XCoordMin(IEnumerableCoordsBlock blocks)
        {
            return blocks.SelectMany(block => block.AsEnumerable()).Min(coord => coord.X);
        }

        private int YCoordMin(IEnumerableCoordsBlock blocks)
        {
            return blocks.SelectMany(block => block.AsEnumerable()).Min(coord => coord.Y);
        }

        private int XCoordMax(IEnumerableCoordsBlock blocks)
        {
            return blocks.SelectMany(block => block.AsEnumerable()).Max(coord => coord.X);
        }

        private int YCoordMax(IEnumerableCoordsBlock blocks)
        {
            return blocks.SelectMany(block => block.AsEnumerable()).Max(coord => coord.Y);
        }

        private void ShiftXY(IEnumerableCoordsBlock blocks, int x, int y)
        {
            blocks.SelectMany(block => block.AsEnumerable()).ToList().ForEach(coord =>
                {
                    coord.Y += y;
                    coord.X += x;
                });
        }

        public void Dispose()
        {
            if (_stream != null) _stream.Close();
            if (_xWrite != null) _xWrite.Close();
        }
    }
}

