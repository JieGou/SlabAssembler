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
        private AutoCadManager _acad;
        private ObjectId _selectedOutline;
        private List<Part> _parts;
        private int _selectedModulation;
        private bool _selecting;
        private string _selectionStatus;
        private Part _selectedCast;
        private Part _selectedLd;
        private Part _selectedLp;
        private string _selectedPillarsLayer;
        private string _selectedGirdersLayer;
        private string _selectedEmptiesLayer;

        public int SelectedModulation {
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
        public ICommand SelectCommand { get; }
        public ICommand DrawCommand { get; }

        public Part SelectedCast {
            get { return _selectedCast; }
            set { _selectedCast = value; OnPropertyChanged(); }
        }

        public Part SelectedLd {
            get { return _selectedLd; }
            set { _selectedLd = value; OnPropertyChanged(); }
        }

        public Part SelectedLp {
            get { return _selectedLp; }
            set { _selectedLp = value; OnPropertyChanged(); }
        }

        public string SelectedPillarsLayer {
            get { return _selectedPillarsLayer; }
            set { _selectedPillarsLayer = value; }
        }

        public string SelectedGirdersLayer {
            get { return _selectedGirdersLayer; }
            set { _selectedGirdersLayer = value; }
        }

        public string SelectedEmptiesLayer {
            get { return _selectedEmptiesLayer; }
            set { _selectedEmptiesLayer = value; }
        }

        public string SelectionStatus {
            get { return _selectionStatus; }
            set { _selectionStatus = value; OnPropertyChanged(); }
        }

        public ObjectId SelectedOutline {
            get { return _selectedOutline; }
            set { _selectedOutline = value; OnPropertyChanged(); }
        }

        public EspecificationsViewModel(ConfigurationsManager configurationsManager, AutoCadManager acad)
        {
            this._acad = acad;
            this._parts = configurationsManager.Data.Parts;
            this._selecting = false;
            this._selectedOutline = ObjectId.Null;
            this.Modulations = new ObservableCollection<int>() { 0 };
            this.FormsAndBoxes = new ObservableCollection<Part>();
            this.LdList = new ObservableCollection<Part>();
            this.LpList = new ObservableCollection<Part>();
            this.Layers = new ObservableCollection<string>(acad.GetLayers());
            this.SelectCommand = new RelayCommand(ExecuteSelectCommand, () => !_selecting);
            this.DrawCommand = new RelayCommand(ExecuteDrawCommand, CanExecuteDrawCommand);
            this.SelectedModulation = 0;
            this.SelectionStatus = "Nenhum contorno selecionado.";

            configurationsManager.DataLoaded += ConfigurationsManager_DataLoaded;
            PropertyChanged += Especifications_PropertyChanged;
        }

        private bool CanExecuteDrawCommand()
        {
            return SelectedOutline != ObjectId.Null
                && SelectedModulation > 0
                && SelectedCast != null
                && SelectedLp != null
                && SelectedLd != null;
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
            else if (e.PropertyName == nameof(SelectedOutline))
                SelectionStatus = String.Format("Contorno selecionado: #{0}", _selectedOutline.GetHashCode());

            CommandManager.InvalidateRequerySuggested();
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
