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
    }
}
