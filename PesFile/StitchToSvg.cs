using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace EmbroideryFile
{
    internal class StitchToSvg
    {
        XElement GetPolyline(List<Coords> coords, string color)
        {
            StringBuilder draw = new StringBuilder();
            XAttribute stroke;
            int c = 0;
            coords.ForEach(p => draw.AppendFormat("{0} {1},",p.X,p.Y));
            draw.Remove(draw.Length - 1, 1); // remove last comma
            XElement e = new XElement("polyline");
            if (color != null) {
                stroke = new XAttribute("stroke", color);
                e.Add(stroke);
            }
            XAttribute id = new XAttribute("id", string.Format("needle{0}", c++));
            e.Add(id);
            XAttribute points = new XAttribute("points", draw.ToString());
            e.Add(points);
            return e;
        }

         XElement GetPolyline(List<Coords> coords)
        {
            StringBuilder draw = new StringBuilder();
            int c = 0;
            coords.ForEach(p => draw.AppendFormat("{0} {1},",p.X,p.Y));
            draw.Remove(draw.Length - 1, 1); // remove last comma
            XElement e = new XElement("polyline");
            XAttribute id = new XAttribute("id", string.Format("needle{0}", c++));
            e.Add(id);
            XAttribute points = new XAttribute("points", draw.ToString());
            e.Add(points);
            return e;
        }

        XElement FillStitchBlocks(XDocument xD, List<List<Coords>> blocks)
        {
            XElement gE = xD.Descendants("g").Single();
            blocks.ForEach(b => { gE.Add(GetPolyline(b));  });
            return gE;
        }

        void FillColorInfo(XElement gE, Dictionary<int,int> colorMap, Dictionary<int, string> colorInfo)
        {
            XAttribute stroke;
            int ci = 0;
            foreach(XElement needle in gE.Elements())
            {
                if (colorMap.ContainsKey(ci) && colorInfo.ContainsKey(colorMap[ci]))
                {
                    stroke = new XAttribute("stroke", colorInfo[colorMap[ci]]);
                    needle.Add(stroke);
                }
                ci++;
            }
        }


    }
}
