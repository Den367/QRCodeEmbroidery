using System.Collections.Generic;
using System.Drawing;

namespace EmbroideryFile
{
    public static class ExtAsPoint
    {
        public static Point[] AsPoints(this List<Coords> coords, float xFactor, float yFactor)
        {
            Point[] points = new Point[coords.Count];
            int i = 0;
            coords.ForEach(c =>
                {
                    points[i].X = (int) (c.X*xFactor);
                    points[i].Y = (int) (c.Y*yFactor);
                    i++;
                }
                );
            return points;
        }
    }
}
