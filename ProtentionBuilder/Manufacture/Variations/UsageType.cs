using System.Xml.Serialization;

namespace Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations
{
    public enum UsageType
    {
        [XmlEnum]
        Form,

        [XmlEnum]
        Box,

        [XmlEnum]
        Lp,

        [XmlEnum]
        StartLp,

        [XmlEnum]
        EndLp,

        [XmlEnum]
        Ld,

        [XmlEnum]
        Lds,

        [XmlEnum]
        Head
    }
}