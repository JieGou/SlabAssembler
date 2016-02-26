using System;
using System.Windows.Input;
using System.Xml.Serialization;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.Building
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

        [XmlIgnore]
        public ICommand EditCommand { get; set; }
        [XmlIgnore]
        public ICommand DeleteCommand { get; set; }
        [XmlIgnore]
        public ICommand SaveCommand { get; set; }

        public Part() { }

        public Part(string name, string referenceName)
        {
            Name = name;
            ReferenceName = referenceName;
        }
    }
}
