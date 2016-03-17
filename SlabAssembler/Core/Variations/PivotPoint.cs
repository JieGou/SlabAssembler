using System;
using System.Xml.Serialization;

namespace Urbbox.SlabAssembler.Core.Variations
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

    public static class PivotPointExtensions
    {
        public static string ToNameString(this PivotPoint point)
        {
            switch (point)
            {
                case PivotPoint.MiddleCenter:
                    return "Centro";

                case PivotPoint.MiddleLeft:
                    return "Meio à Esquerda";

                case PivotPoint.MiddleRight:
                    return "Meio à Direita";

                case PivotPoint.MiddleTop:
                    return "Topo ao Meio";

                case PivotPoint.MiddleBottom:
                    return "Meio em Baixo";

                case PivotPoint.TopLeft:
                    return "Topo à Esquerda";

                case PivotPoint.TopRight:
                    return "Topo à Direita";

                case PivotPoint.BottomLeft:
                    return "Embaixo à Esquerda";

                case PivotPoint.BottomRight:
                    return "Embaixo à Direita";

                default:
                    throw new ArgumentOutOfRangeException(nameof(point), point, null);
            }
        }

        public static PivotPoint ToPivotType(this string point)
        {
            switch (point)
            {
                case "Centro":
                    return PivotPoint.MiddleCenter;

                case "Meio à Esquerda":
                    return PivotPoint.MiddleLeft;

                case "Meio à Direita":
                    return PivotPoint.MiddleRight;

                case "Topo ao Meio":
                    return PivotPoint.MiddleTop;

                case "Meio em Baixo":
                    return PivotPoint.MiddleBottom;

                case "Topo à Esquerda":
                    return PivotPoint.TopLeft;

                case "Topo à Direita":
                    return PivotPoint.TopRight;

                case "Embaixo à Esquerda":
                    return PivotPoint.BottomLeft;

                case "Embaixo à Direita":
                    return PivotPoint.BottomRight;

                default:
                    throw new ArgumentOutOfRangeException(nameof(point), point, null);
            }
        }
    }
}
