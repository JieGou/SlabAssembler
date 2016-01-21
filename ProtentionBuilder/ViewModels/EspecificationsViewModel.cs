using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class EspecificationsViewModel : ViewModel
    {
        private Modulation _selectedModulation;
        public Modulation SelectedModulation
        {
            get { return _selectedModulation; }
            set { _selectedModulation = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Modulation> Modulations { get; }
        public ObservableCollection<Part> FormsAndBoxes { get; }
        public ObservableCollection<Part> LpList { get; }
        public ObservableCollection<Part> LdList { get; }

        public EspecificationsViewModel(ConfigurationManager configurationManager)
        {
            Collection<Part> parts = configurationManager.GetParts() as Collection<Part>;

            Modulations = parts?.Select(p => p.Modulation) as ObservableCollection<Modulation>;
            FormsAndBoxes = parts?.Where(p => p.UsageType == UsageType.Box || p.UsageType == UsageType.Form) as ObservableCollection<Part>;
            LpList = parts?.Where(p => p.UsageType == UsageType.Lp) as ObservableCollection<Part>;
            LdList = parts?.Where(p => p.UsageType == UsageType.Ld) as ObservableCollection<Part>;

            SelectedModulation = Modulations?.First();
        }
    }
}
