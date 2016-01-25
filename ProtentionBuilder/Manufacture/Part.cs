using System;
using System.Xml.Serialization;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.Manufacture
{
    [Serializable]
    public class Part
    {
        public string ReferenceName { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public PivotPoint PivotPoint { get; set; }
        public UsageType UsageType { get; set; }
        public string Layer { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Modulation { get; set; }

        public Part() { }

        public Part(string name, string referenceName)
        {
            Name = name;
            ReferenceName = referenceName;
        }
    }
}
