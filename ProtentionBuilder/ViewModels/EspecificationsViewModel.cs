using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class EspecificationsViewModel : ViewModel
    {
        private int _selectedModulation;
        public int SelectedModulation
        {
            get { return _selectedModulation; }
            set { _selectedModulation = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> Modulations { get; }
        public ObservableCollection<Part> FormsAndBoxes { get; }
        public ObservableCollection<Part> LpList { get; }
        public ObservableCollection<Part> LdList { get; }
        public ObservableCollection<string> Layers { get; } 

        private readonly List<Part> _parts;

        public EspecificationsViewModel(ConfigurationsManager configurationsManager, AcManager ac)
        {
            _parts = configurationsManager.GetParts();

            Modulations = new ObservableCollection<int>(_parts.GroupBy(p => p.Modulation).Select(g => g.Key));

            FormsAndBoxes = new ObservableCollection<Part>();
            LdList = new ObservableCollection<Part>();
            LpList = new ObservableCollection<Part>();
            Layers = new ObservableCollection<string>(ac.GetLayers());

            SelectedModulation = Modulations.First();
        }


        private void SetParts()
        {
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

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(SelectedModulation))
                SetParts();
        }
    }
}
