using System;
using System.Collections.Generic;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    class PartViewModel : ViewModelBase
    {
        public Part Part { get; set; }
        public List<string> UsageTypesList { get; set; }
        public List<string> PivotPointList { get; set; }

        public PartViewModel(Part part)
        {
            Part = part;
            UsageTypesList = new List<string>();
            foreach (UsageType u in Enum.GetValues(typeof(UsageType)))
                UsageTypesList.Add(u.ToNameString());

            PivotPointList = new List<string>();
            foreach (PivotPoint p in Enum.GetValues(typeof(PivotPoint)))
                PivotPointList.Add(p.ToNameString());
        }

    }
}
