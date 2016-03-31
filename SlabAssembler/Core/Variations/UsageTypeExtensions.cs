using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.Core.Variations
{
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

        public static Color ToColor(this UsageType usage)
        {
            switch (usage)
            {
                case UsageType.Form:
                    return Color.FromRgb(255, 255, 255);
                case UsageType.Box:
                    return Color.FromRgb(255, 255, 255);
                case UsageType.Lp:
                    return Color.FromRgb(25, 25, 200);
                case UsageType.StartLp:
                    return Color.FromRgb(116, 88, 173);
                case UsageType.EndLp:
                    return Color.FromRgb(200, 25, 25);
                case UsageType.Ld:
                    return Color.FromRgb(38, 240, 120);
                case UsageType.Lds:
                    return Color.FromRgb(255, 244, 40);
                case UsageType.Head:
                    return Color.FromRgb(200, 0, 100);
                default:
                    throw new ArgumentOutOfRangeException(nameof(usage), usage, null);
            }
        }
    }
}
