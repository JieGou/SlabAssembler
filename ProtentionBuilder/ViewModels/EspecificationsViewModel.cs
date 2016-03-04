using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;
using System;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class EspecificationsViewModel : ModelBase
    {
        public int SelectedModulation
        {
            get { return _selectedModulation; }
            set {
                if (value != _selectedModulation) { 
                    _selectedModulation = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<int> Modulations { get; }
        public ObservableCollection<Part> FormsAndBoxes { get; }
        public ObservableCollection<Part> LpList { get; }
        public ObservableCollection<Part> LdList { get; }
        public ObservableCollection<string> Layers { get; }
        public ICommand SelectCommand { get; set; }
        public ICommand DrawCommand { get; set; }
        public string SelectionStatus { get; set; }
        public ObjectId SelectedOutline
        {
            get { return _selectedObject; }
            set {
                _selectedObject = value;
                SelectionStatus = String.Format("Contorno selecionado. #{0}", _selectedObject.GetHashCode());
                OnPropertyChanged();
            }
        }

        private AutoCadManager _acad;
        private ObjectId _selectedObject;
        private List<Part> _parts;
        private int _selectedModulation;
        private bool _selecting;

        public EspecificationsViewModel(ConfigurationsManager configurationsManager, AutoCadManager acad)
        {
            this._acad = acad;
            this._parts = configurationsManager.Data.Parts;
            this._selecting = false;
            this.Modulations = new ObservableCollection<int>() { 0 };
            this.FormsAndBoxes = new ObservableCollection<Part>();
            this.LdList = new ObservableCollection<Part>();
            this.LpList = new ObservableCollection<Part>();
            this.Layers = new ObservableCollection<string>(acad.GetLayers());
            this.SelectCommand = new RelayCommand(ExecuteSelectCommand, () => !_selecting);
            this.SelectCommand = new RelayCommand(ExecuteDrawCommand, () => SelectedOutline != null);
            this.SelectedModulation = 0;
            this.SelectionStatus = "Nenhum contorno selecionado.";

            configurationsManager.DataLoaded += ConfigurationsManager_DataLoaded;
            PropertyChanged += Especifications_PropertyChanged;
        }

        private void ExecuteDrawCommand()
        {
            //Do nothing
        }

        private void ExecuteSelectCommand()
        {
            _selecting = true;
            var result = _acad.SelectSingle("Selecione o contorno da laje");
            if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK) {
                ObjectId selected = result.Value.GetObjectIds().First();
                if (_acad.ValidateOutline(selected)) { 
                    SelectedOutline = selected;
                } else
                    Application.ShowAlertDialog("Selecione um contorno válido!");
            }
            _selecting = false;
        }

        private void Especifications_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedModulation))
                SetParts();
        }

        private void ConfigurationsManager_DataLoaded(ConfigurationData data)
        {
            _parts = data.Parts;
            SetParts();
        }

        private void SetParts()
        {
            Modulations.Clear();
            foreach (var modulation in _parts.GroupBy(p => p.Modulation).Select(g => g.Key))
                Modulations.Add(modulation);

            FormsAndBoxes.Clear();
            foreach (var part in _parts.Where(p => (p.UsageType == UsageType.Box || p.UsageType == UsageType.Form) && p.Modulation == SelectedModulation))
                FormsAndBoxes.Add(part);

            LpList.Clear();
            foreach (var part in _parts.Where(p => (p.UsageType == UsageType.Lp) && p.Modulation == SelectedModulation))
                LpList.Add(part);

            LdList.Clear();
            foreach (var part in _parts.Where(p => (p.UsageType == UsageType.Ld) && p.Modulation == SelectedModulation))
                LdList.Add(part);
        }

    }
}
