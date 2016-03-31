using System;
using System.Xml.Serialization;

namespace Urbbox.SlabAssembler.Core.Variations
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