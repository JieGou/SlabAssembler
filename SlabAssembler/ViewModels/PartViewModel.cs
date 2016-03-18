using System;
using System.Collections.Generic;
using System.Windows.Input;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels.Commands;
using Urbbox.SlabAssembler.Views;

namespace Urbbox.SlabAssembler.ViewModels
{
    class PartViewModel : ModelBase
    {
        public Part Part { get; set; }
        public List<string> UsageTypesList { get; set; }
        public List<string> PivotPointList { get; set; }
        public ICommand SaveCommand { get; set; }
        private ConfigurationsRepository _manager;
        private PartWindow _partWindow;

        public PartViewModel(PartWindow partWindow, ConfigurationsRepository configurationsManager, Part part)
        {
            this._partWindow = partWindow;
            this._manager = configurationsManager;
            this.Part = part;
            this.Part.PropertyChanged += Part_PropertyChanged;
            this.SaveCommand = new RelayCommand(ExecuteSaveCommand, ValidatePart);
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
