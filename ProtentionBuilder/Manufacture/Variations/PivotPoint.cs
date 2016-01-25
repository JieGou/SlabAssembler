using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations
{
    public enum PivotPoint
    {
        [XmlEnum]
        MiddleCenter,

        [XmlEnum]
        MiddleLeft,

        [XmlEnum]
        MiddleRight,

        [XmlEnum]
        MiddleTop,

        [XmlEnum]
        MiddleBottom,

        [XmlEnum]
        TopLeft,

        [XmlEnum]
        TopRight,

        [XmlEnum]
        BottomLeft,

        [XmlEnum]
        BottomRight
    }
}
