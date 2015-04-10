using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmbroideryFile
{
    /// <summary>
    /// Information for DST embroidery file format
    /// </summary>
    public class DstInfo
    {
        public DstHeader Header { get;set;}
      

        public DstInfo()
        {
            Header = new DstHeader();
        }
    }
}
