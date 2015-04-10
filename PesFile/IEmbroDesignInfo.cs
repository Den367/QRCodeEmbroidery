
namespace EmbroideryFile
{
    public interface IDesignInfo
    {
        string DesignName { get;  }
        /// <summary>
        /// Width of embroidery design (0.1 mm)
        /// </summary>
         int Width { get;  }
        /// <summary>
        /// Height of embroidery design (0.1 mm)
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Minimum value of horizontal stitch coords 
        /// </summary>
        int Xmin { get; }
        /// <summary>
        /// Minimum value of vertical stitch coords 
        /// </summary>
        int Ymin { get;  }

        /// <summary>
        /// Maximum value of Y stitch coords
        /// </summary>
        int Xmax { get; }
        /// <summary>
        /// Maximum value of Y stitch coords
        /// </summary>
        int Ymax { get;  }

        int TotalStitchCount { get; set; }
        int ColorChangeCount { get; set; }
        int StitchBlockCount { get; set; }
        int JumpsBlocksCount { get; set; }
    }
}
