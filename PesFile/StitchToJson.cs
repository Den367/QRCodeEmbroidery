using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Script.Serialization;
using EmbroideryFile;

namespace EmbroideryFile
{
    public static class StitchToJson
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static string ToJsonCoords(this EmbroideryData design)
        {
            //var minX = design.GetXCoordMin();
            //var minY = design.GetYCoordMin();
            //var blocks = design.Blocks;

            var result = from needle in design.Blocks
                         select
                             new JsonCoordsBlock
                                 {
                                     jump = needle.Jumped,
                                     stop = needle.Stop,
                                     color = needle.Color.Name,
                                     needle = needle
                                 };
            return Serializer.Serialize(result);
        }

        public static IEnumerable<JsonCoordsBlock> GetCoordsBlockList(this string jsonCoords)
        {
            try
            {
                var result = Serializer.Deserialize<IList<JsonCoordsBlock>>(jsonCoords);
                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                throw;
            }

        }
    }
}

[Serializable]
public class JsonCoordsBlock
{

    public bool jump { get; set; }
    public bool stop { get; set; }
    public string color { get; set; }

    public List<Coords> needle { get; set; }
}