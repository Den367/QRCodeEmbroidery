using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.Xml;

namespace EmbroideryFile
{
    /// <summary>
    /// Contains stitch blocks and color map info to create svg
    /// </summary>
    public class EmbroideryData : IDesignInfo
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof (EmbroideryData));

        #region [ctor]

        public EmbroideryData()
        {
            ColourInfo = new Dictionary<int, Color>();
            ColorMap = new Dictionary<int, int>();

            Blocks = new List<CoordsBlock>();
        }

        #endregion [ctor]


        #region [Lists]

        /// <summary>
        /// List of embroidery blocks
        /// </summary>
        /// 
        [XmlIgnore]
        public List<CoordsBlock> Blocks { get; set; }

        #endregion [Lists]

        #region [Dictionaries]

        /// <summary>
        /// Map of block (stitches between stops) to color index 
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, int> ColorMap { get; set; }

        /// <summary>
        /// Dictionary of #RGB color representation by color index
        /// </summary>        
        [XmlIgnore]
        public Dictionary<int, Color> ColourInfo { get; set; }

        #endregion [Dictionaries]

        public string DesignName { get; set; }

        /// <summary>
        /// Width of embroidery design (0.1 mm)
        /// </summary>
        public int Width { get; set; }

        public int ScaledWidth { get; set; }
        public int ScaledHeight { get; set; }

        /// <summary>
        /// Height of embroidery design (0.1 mm)
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Minimum value of horizontal stitch coords 
        /// </summary>
        public int Xmin { get; set; }

        /// <summary>
        /// Minimum value of vertical stitch coords 
        /// </summary>
        public int Ymin { get; set; }

        /// <summary>
        /// Maximum value of Y stitch coords
        /// </summary>
        public int Xmax { get; set; }

        /// <summary>
        /// Maximum value of Y stitch coords
        /// </summary>
        public int Ymax { get; set; }

        private int _totalStiches;

        public int TotalStitchCount
        {
            get { return _totalStiches; }
            set { _totalStiches = value; }
        }

        private int _colorChanges;

        public int ColorChangeCount
        {
            get { return _colorChanges; }
            set { _colorChanges = value; }
        }

        private int _stitchBlocks;

        public int StitchBlockCount
        {
            get { return _stitchBlocks; }
            set { _stitchBlocks = value; }
        }

        private int _jumpStitches;

        public int JumpsBlocksCount
        {
            get { return _jumpStitches; }
            set { _jumpStitches = value; }
        }

        public EmbroType Type { get; set; }


        public int GetTotalStitches()
        {

            _totalStiches = Blocks.SelectMany(block => block.AsEnumerable()).Count();
            return _totalStiches;

        }

        public int GetColorChanges()
        {

            _colorChanges = Blocks.Count(block => block.Stop == true);
            return _colorChanges;

        }


        public int GetBlocksCount()
        {

            _stitchBlocks = Blocks.Count();
            return _stitchBlocks;

        }

        public int GetJumpStitches()
        {

            _jumpStitches = Blocks.Count(block => block.Jumped == true);
            return _jumpStitches;

        }

        public int GetXCoordMin()
        {

            return Blocks.SelectMany(block => block.AsEnumerable()).Min(coord => coord.X);

        }

        public int GetYCoordMin()
        {

            return Blocks.SelectMany(block => block.AsEnumerable()).Min(coord => coord.Y);

        }

        public int GetXCoordMax()
        {

            return Blocks.SelectMany(block => block.AsEnumerable()).Max(coord => coord.X);

        }

        public int GetYCoordMax()
        {

            return Blocks.SelectMany(block => block.AsEnumerable()).Max(coord => coord.Y);

        }

        public void ShiftXY(int x, int y)
        {
            x = -x;
            y = -y;
            foreach (var block in Blocks)
            {
                foreach (var stitch in block)
                {
                    var X = stitch.X + x;
                    var Y = stitch.Y + y;
                    stitch.X = X;
                    stitch.Y = Y;
                }
            }
        }

        private int GetWidth()
        {
            int xmin = GetXCoordMin();
            int xmax = GetXCoordMax();
            return Math.Abs(xmax - xmin);
        }


        private int GetHeight()
        {
            int ymin = GetYCoordMin();
            int ymax = GetYCoordMax();
            return Math.Abs(ymax - ymin);
        }


        private bool WidthGTOREQHeight()
        {
            return GetWidth() >= GetHeight();
        }

        private float GetScaling(int size)
        {
            var width = GetWidth();
            var height = GetHeight();
            if (width >= height)
            {

                return ((float) size/(float) width);
            }
            else
            {
                return ((float) size/(float) height);
            }

        }

        public void ShiftToZero()
        {
            ShiftXY(GetXCoordMin(), GetYCoordMin());
        }


        public void Resize(int size)
        {
            ShiftToZero();
            var scale = GetScaling(size);
            ScaledWidth = (int) (Width*scale);
            ScaledHeight = (int) (Height*scale);
            foreach (var block in Blocks)
            {
                foreach (var stitch in block)
                {
                    stitch.X = (int) (stitch.X*scale);
                    stitch.Y = (int) (stitch.Y*scale);
                }
            }
        }

        public void Centerize(int size)
        {
            int ymin = GetYCoordMin();
            int ymax = GetYCoordMax();
            int xmin = GetXCoordMin();
            int xmax = GetXCoordMax();
            int width = xmax - xmin;
            int height = ymax - ymin;
            int shiftX = (size - width)/2 - xmin;
            int shiftY = (size - height)/2 - ymin;

            ShiftXY(-shiftX, -shiftY);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder, new XmlWriterSettings {OmitXmlDeclaration = true}))
            {
                serializer.Serialize(writer, this);
            }
            return builder.ToString();
        }
    }
}
