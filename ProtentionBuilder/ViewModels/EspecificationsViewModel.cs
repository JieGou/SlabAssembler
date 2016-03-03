using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;
using Urbbox.AutoCAD.ProtentionBuilder.Database;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class EspecificationsViewModel : ModelBase
    {
        private int _selectedModulation;
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

        private List<Part> _parts;

        public EspecificationsViewModel(ConfigurationsManager configurationsManager, AcManager ac)
        {
            _parts = configurationsManager.Data.Parts;
            Modulations = new ObservableCollection<int>() { 0 };
            FormsAndBoxes = new ObservableCollection<Part>();
            LdList = new ObservableCollection<Part>();
            LpList = new ObservableCollection<Part>();
            Layers = new ObservableCollection<string>(ac.GetLayers());
            SelectedModulation = 0;

            configurationsManager.DataLoaded += ConfigurationsManager_DataLoaded;
            PropertyChanged += Especifications_PropertyChanged;
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
