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
        public Modulation Modulation { get; set; }
        public PivotPoint PivotPoint { get; set; }
        public UsageType UsageType { get; set; }
        public string Layer { get; set; }

        public Part() { }

        public Part(string name, string referenceName)
        {
            Name = name;
            ReferenceName = referenceName;
        }
    }
}
