using System;
using System.Collections.Generic;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;
using Urbbox.AutoCAD.ProtentionBuilder.Views;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    class PartViewModel : ModelBase
    {
        public Part Part { get; set; }
        public List<string> UsageTypesList { get; set; }
        public List<string> PivotPointList { get; set; }
        public ICommand SaveCommand { get; set; }
        private ConfigurationsManager _manager;
        private PartWindow _partWindow;

        public PartViewModel(PartWindow partWindow, ConfigurationsManager configurationsManager, Part part)
        {
            this._partWindow = partWindow;
            this._manager = configurationsManager;
            this.Part = part;
            this.Part.PropertyChanged += Part_PropertyChanged;
            this.SaveCommand = new RelayCommand(ExecuteSaveCommand, ValidatePart);

            UsageTypesList = new List<string>();
            foreach (UsageType u in Enum.GetValues(typeof(UsageType)))
                UsageTypesList.Add(u.ToNameString());

            PivotPointList = new List<string>();
            foreach (PivotPoint p in Enum.GetValues(typeof(PivotPoint)))
                PivotPointList.Add(p.ToNameString());
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
            _partWindow.Close();
        }
        
    }
}
