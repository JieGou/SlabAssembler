using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.Manufacture
{
    public class Part
    {
        public string Name { get; set; }
        public string ReferenceName { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int Modulation { get; set; }
        public PivotPoint PivotPoint { get; set; }
        public UsageType UsageType { get; set; }
        public string Layer { get; set; }

        public Part(JObject data)
        {
            Name = (string) data["Name"];
            ReferenceName = (string) data["ReferenceName"];
            Width = (float) data["Width"];
            Height = (float) data["Height"];
            Modulation = (int) data["Modulation"];
            PivotPoint = (PivotPoint) ((int) data["PivotPoint"]);
            UsageType = (UsageType)((int) data["UsageType"]);
            Layer = (string) data["Layer"];
        }

        public Part(string name, string referenceName)
        {
            Name = name;
            ReferenceName = referenceName;
        }
    }
}
