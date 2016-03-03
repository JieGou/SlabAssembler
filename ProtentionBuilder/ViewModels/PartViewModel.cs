using System;
using System.Collections.Generic;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    class PartViewModel : ModelBase
    {
        public Part Part { get; set; }
        public List<string> UsageTypesList { get; set; }
        public List<string> PivotPointList { get; set; }
        public ICommand SaveCommand { get; set; }
        private ConfigurationsManager _manager;

        public PartViewModel(ConfigurationsManager manager, Part part)
        {
            _manager = manager;
            Part = part;
            Part.PropertyChanged += Part_PropertyChanged;
            UsageTypesList = new List<string>();
            foreach (UsageType u in Enum.GetValues(typeof(UsageType)))
                UsageTypesList.Add(u.ToNameString());

            PivotPointList = new List<string>();
            foreach (PivotPoint p in Enum.GetValues(typeof(PivotPoint)))
                PivotPointList.Add(p.ToNameString());

            SaveCommand = new RelayCommand(ExecuteSaveCommand, ValidatePart);
        }

        private void Part_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private bool ValidatePart()
        {
            return (!String.IsNullOrEmpty(Part.ReferenceName))
                && (!String.IsNullOrEmpty(Part.Layer))
                && (Part.Width > 0)
                && (Part.Height > 0)
                && (Part.Modulation > 0);
        }

        private void ExecuteSaveCommand()
        {
            _manager.SavePart(Part);
        }
        
    }
}
