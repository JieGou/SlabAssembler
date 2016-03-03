using System;
using System.Collections;
using System.Windows.Input;
using System.Xml.Serialization;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.Building
{
    [Serializable]
    public class Part : IEqualityComparer
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

        bool IEqualityComparer.Equals(object x, object y)
        {
            return ((Part)x).ReferenceName == ((Part)y).ReferenceName;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {  
            return ((Part) obj).ReferenceName.GetHashCode();
        }
    }
}
