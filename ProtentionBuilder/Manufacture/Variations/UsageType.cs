using System;
using System.ComponentModel;
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
    }
}