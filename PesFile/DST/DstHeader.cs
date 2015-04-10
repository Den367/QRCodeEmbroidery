using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace EmbroideryFile
{
    /// <summary>
    /// Structured header of DST file
    /// </summary>
    [Serializable]
    public class DstHeader: IDesignInfo
    {
        public DstHeader()
        {
           designName[0] = Convert.ToByte('L');
            designName[1]=Convert.ToByte('A');
            designName[2] = 0x3A;
            designName[19] = 0x0D; 
           stitchCount[0] = Convert.ToByte('S');
            stitchCount[1] = Convert.ToByte('T');
            stitchCount[2] = 0x3A;
            stitchCount[10] = 0x0D; ;
           colorChangeCount[0] = Convert.ToByte('C');
            colorChangeCount[1] = Convert.ToByte('O');
            colorChangeCount[2] = 0x3A;
            colorChangeCount[6] = 0x0D; 
           xMax[0] = Convert.ToByte('+');
            xMax[1] = Convert.ToByte('X');
            xMax[2] = 0x3A;
            xMin[0] = Convert.ToByte('-');
            xMin[1] = Convert.ToByte('X');
            xMin[2] = 0x3A;
           yMax[0] = Convert.ToByte('+');
            yMax[1] = Convert.ToByte('Y');
            yMax[2] = 0x3A;
            yMin[0] = Convert.ToByte('-');
            yMin[1] = Convert.ToByte('Y');
            yMin[2] = 0x3A;
            xMax[8] = 0x0D;
            yMax[8] = 0x0D;
            xMin[8] = 0x0D;
            yMin[8] = 0x0D;
           xOffset[0] = Convert.ToByte('A');
            xOffset[1] = Convert.ToByte('X');
            xOffset[2] = 0x3A;
            yOffset[0] = Convert.ToByte('A');
            yOffset[1] = Convert.ToByte('Y');
            yOffset[2] = 0x3A;
           pD = Enumerable.Repeat(Convert.ToByte('*'), 11).ToArray();
           pD[0] = 0x50; pD[1] = 0x44; pD[2] = 0x3A; pD[9] = 0x0D;  pD[10] = 0x1A;
        }
#region [Reader & Writer]
      

        public void WriteToStream(Stream stream)
        {
            stream.Write(designName, 0, 20);
            stream.Write(stitchCount, 0, 11);
            stream.Write(colorChangeCount, 0, 7);
            stream.Write(xMax, 0, 9);
            stream.Write(xMin, 0, 9);
            stream.Write(yMax, 0, 9);
            stream.Write(yMin, 0, 9);
            stream.Write(xOffset, 0, 10);
            stream.Write(yOffset, 0, 10);
            stream.Write(mX, 0, 10);
            stream.Write(mY, 0, 10);
            stream.Write(pD, 0, 11);
            WriteBytes(stream, 0x20, 387);

        }

        void WriteBytes(StreamWriter writer, byte b, int n)
        {
            for (int i = 0; i < n; i++)
                writer.Write(b);
        }

        void WriteBytes(Stream stream, byte b, int n)
        {
           
                stream.Write(Enumerable.Repeat(b, n).ToArray(), 0 , n);
        }
       

        void Read(Stream stream)
        {

            stream.Read(designName, 0, 20);
            stream.Read(stitchCount, 0, 11);
            stream.Read(colorChangeCount, 0, 7);
            stream.Read(xMax, 0, 9);
            stream.Read(xMin, 0, 9);
            stream.Read(yMax, 0, 9);
            stream.Read(yMin, 0, 9);
            stream.Read(xOffset, 0, 10);
            stream.Read(yOffset, 0, 10);
            stream.Read(mX, 0, 10);
            stream.Read(mY, 0, 10);
        }
#endregion [Reader & Writer]

       

        public DstHeader(Stream stream)
        {

            Read(stream);

        }
        /// <summary>
        /// Design_Name (LA) 20 chars
        /// </summary>	
         byte[] designName = new byte[20];
        public string DesignName { get{
                StringBuilder name = new StringBuilder();
                for (int i = 3; i < 19; i++)
                    name.Append(Convert.ToChar(designName[i]));
                return name.ToString();            
            }
            set
            {
                var name = value;
                
                if (name.Length > 16) name = name.Substring(0, 16);
                var len = name.Length;
                for (int i = 3;  i < len + 3;i++)
                {
                    designName[i] =Convert.ToByte(name[i - 3]);
                }
               
            }
        }
        /// <summary>
        /// Stitch count, 7 digits padded by leading 0's 'ST'
        /// </summary>
         byte[] stitchCount = new byte[11];

        public int TotalStitchCount { 
            get {
            StringBuilder count = new StringBuilder();
            for (int i = 3; i < 10; i++)
                 count.Append(Convert.ToChar(stitchCount[i]));
            return Convert.ToInt32(count.ToString());
        }
            set 
            {
                string count = value.ToString();
                
                Buffer.BlockCopy( Encoding.ASCII.GetBytes(count.PadLeft(7 , ' ')), 0, stitchCount, 3,  7);
            }
        }
        /// <summary>
        /// Color change count, 3 digits padded by leading 0's case 'CO'
        /// </summary>
        byte[] colorChangeCount = new byte[7];

        public int ColorChangeCount { get {
            StringBuilder count = new StringBuilder();
            for (int i = 3; i < 6; i++)
                 count.Append(Convert.ToChar(colorChangeCount[i]));
            return Convert.ToInt32(count.ToString());
        }
            set
            {
                string colorChanges;
                if (value == null) colorChanges = "0";
                else colorChanges = value.ToString();               
                Buffer.BlockCopy(colorChanges.PadLeft(3 , ' ').ToCharArray(), 0, colorChangeCount, 3, colorChanges.Length < 3 ? colorChanges.Length : 3);
            }
        }

        /// <summary>
        /// Design extents (+X), 5 digits padded by leading 0's
        /// </summary>
        byte[] xMax = new byte[9];

        public int Xmax { get{
        StringBuilder value = new StringBuilder();
            for (int i = 3; i < 8; i++)
                 value.Append(Convert.ToChar(xMax[i]));
            return Convert.ToInt32(value.ToString());
    }
            set {
                string xmax = value.ToString();               
               Buffer.BlockCopy(xmax.PadLeft(5 , ' ').ToCharArray(), 0, xMax, 3, 5);
            }
        }


        /// <summary>
        /// Design extents (-X), 5 digits padded by leading 0's
        /// </summary>
        byte[] xMin = new byte[9];
        public int Xmin
        {
            get
            {
                StringBuilder value = new StringBuilder();
                for (int i = 3; i < 8; i++)
                    if (xMin[i] != 0x20) value.Append(Convert.ToChar(xMin[i]));
                return -Convert.ToInt32(value.ToString());
            }
            set
            {
                string xmin = value.ToString();                
                Buffer.BlockCopy(xmin.PadLeft(5 , ' ').ToCharArray(), 0, xMax, 3, 5);
            }
        }

        /// <summary>
        /// Design extents (+Y), 5 digits padded by leading 0's
        /// </summary>
        byte[] yMax = new byte[9];
        public int Ymax
        {
            get
            {
                StringBuilder value = new StringBuilder();
                for (int i = 3; i < 8; i++)
                    if (yMax[i] != 0x20) value.Append(Convert.ToChar(yMax[i]));
                return Convert.ToInt32(value.ToString());
            }
            set
            {
                string ymax = value.ToString();
                Buffer.BlockCopy(ymax.PadLeft(5 , ' ').ToCharArray(), 0, yMax, 3, 5);
            }
        }

        /// <summary>
        /// Design extents (-Y), 5 digits padded by leading 0's
        /// </summary>
        byte[] yMin = new byte[9];
        public int Ymin
        {
            get
            {
                StringBuilder value = new StringBuilder();
                for (int i = 3; i < 8; i++)
                    if (yMin[i] != 0x20) value.Append(Convert.ToChar(yMin[i]));
                return -Convert.ToInt32(value.ToString());
            }
            set
            {
                string ymin = value.ToString();
                Buffer.BlockCopy(ymin.PadLeft(5 , ' ').ToCharArray(), 0, yMin, 3, 5);
            }
        }

        /// <summary>
        /// 'AX' Relative coordinates of last point, 6 digits, padded with leading spaces, first char may be +/-
        /// </summary> 
        byte[] xOffset = new byte[10];
        public int XOffset
        {
            get
            {
                StringBuilder value = new StringBuilder();
                for (int i = 3; i < 9; i++)
                    if (xOffset[i] != 0x20) value.Append(Convert.ToChar(xOffset[i]));
                return Convert.ToInt32(value.ToString());
            }
            set
            {
                string xoffset = value.ToString();
                Buffer.BlockCopy(xoffset.PadLeft(6 , ' ').ToCharArray(), 0, xOffset, 3, 6);
            }
        }

        /// <summary>
        /// 'AY':
        /// </summary>
        byte[] yOffset = new byte[10];
        public int YOffset
        {
            get
            {
                StringBuilder value = new StringBuilder();
                for (int i = 3; i < 9; i++)
                    if (yOffset[i] != 0x20) value.Append(Convert.ToChar(yOffset[i]));
                return Convert.ToInt32(value.ToString());
            }
            set
            {
                string yoffset = value.ToString();
                Buffer.BlockCopy(yoffset.PadLeft(6 , ' ').ToCharArray(), 0, yOffset, 3, 6);
            }
        }

        /// <summary>
        /// 'MX'):	//Coordinates of last point in previous file of multi-volume design, 6 digits, padded with leading spaces, first char may be +/-
        /// </summary>
        byte[] mX = new byte[10];
         public int MX
         {
             get
             {
                 StringBuilder value = new StringBuilder();
                 for (int i = 3; i < 9; i++)
                     if (mX[i] != 0x20) value.Append(Convert.ToChar(mX[i]));
                 return Convert.ToInt32(value.ToString());
             }
             set
             {
                 string mx = value.ToString();
                 Buffer.BlockCopy(mx.PadLeft(6 , ' ').ToCharArray(), 0, mX, 3, 6);
             }
         }
        /// <summary>
        /// //('M','Y'):
        /// </summary>
         byte[] mY = new byte[10];
         public int MY
         {
             get
             {
                 StringBuilder value = new StringBuilder();
                 for (int i = 3; i < 9; i++)
                     if (mY[i] != 0x20) value.Append(Convert.ToChar(mY[i]));
                 return Convert.ToInt32(value.ToString());
             }
             set
             {
                 string my = value.ToString(CultureInfo.InvariantCulture);
                 Buffer.BlockCopy(my.PadLeft(6, ' ').ToCharArray(), 0, mY, 3, 6);
             }

         }

         byte[] pD = new byte[11];
       
         public int Height { get { return Ymax - Ymin; } }
         public int Width { get { return Xmax - Xmin; } }
         public int StitchBlockCount { get; set; }
         public int JumpsBlocksCount { get; set; }
        //'P','D' store this string as-is, it will be saved as-is, 6 characters

       
    }

}
