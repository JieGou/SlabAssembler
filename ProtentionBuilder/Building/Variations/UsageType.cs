using System;
using System.Xml.Serialization;

namespace Urbbox.AutoCAD.ProtentionBuilder.Building.Variations
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

    public static class UsageTypeExtensions
    {
        public static string ToNameString(this UsageType usage)
        {
            switch (usage)
            {
                case UsageType.Form:
                    return "Forma";
                case UsageType.Box:
                    return "Caixa";
                case UsageType.Lp:
                    return "LP";
                case UsageType.StartLp:
                    return "LP de Partida";
                case UsageType.EndLp:
                    return "LP Final";
                case UsageType.Ld:
                    return "LD";
                case UsageType.Lds:
                    return "LDS";
                case UsageType.Head:
                    return "Cabeça";
                default:
                    throw new ArgumentOutOfRangeException(nameof(usage), usage, null);
            }
        }

        public static UsageType ToUsageType(this string usage)
        {
            switch (usage)
            {
                case "Forma":
                    return UsageType.Form;
                case "Caixa":
                    return UsageType.Box;
                case "LP":
                    return UsageType.Lp;
                case "LP de Partida":
                    return UsageType.StartLp;
                case "LP Final":
                    return UsageType.EndLp;
                case "LD":
                    return UsageType.Ld;
                case "LDS":
                    return UsageType.Lds;
                case "Cabeça":
                    return UsageType.Head;
                default:
                    throw new ArgumentOutOfRangeException(nameof(usage), usage, null);
            }
        }
    }
}