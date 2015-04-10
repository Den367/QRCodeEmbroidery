using System;
using System.Collections.Generic;
using System.Drawing;

namespace EmbroideryFile
{
    /// <summary>
    /// Contains list of stitch coordinates <see cref="Coords"/> and color information 
    /// </summary>
    [Serializable]
    public sealed class CoordsBlock : List<Coords>
    {

        /// <summary>
        /// Color is not changed, it is jumped from previous block
        /// </summary>
        public bool Jumped { get; set; }

        /// <summary>
        /// Stop to change sewing thread
        /// </summary>
        public bool Stop { get; set; }

        /// <summary>
        /// Drawing color info <see cref="System.Drawing.Color"/>
        /// </summary>

        public Color Color { get; set; }

        public Int32 colorIndex { get; set; }

        public CoordsBlock(Color color)
        {

            Color = color;
        }
        public CoordsBlock()
        {

            Color = Color.Black;
        }

    }
}
