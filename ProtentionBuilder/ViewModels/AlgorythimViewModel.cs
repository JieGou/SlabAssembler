using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class AlgorythimViewModel : ViewModel
    {
        public ObservableCollection<Part> StartLpList { get; }
        private readonly EspecificationsViewModel _especificationsViewModel;
        private readonly List<Part> _parts; 

        public AlgorythimViewModel(EspecificationsViewModel especifications, ConfigurationsManager configurations)
        {
            _especificationsViewModel = especifications;
            _especificationsViewModel.PropertyChanged += EspecificationsViewModel_PropertyChanged;
            _parts = configurations.GetParts();

            StartLpList = new ObservableCollection<Part>();
            SetParts();
        }

        private void SetParts()
        {
            StartLpList.Clear();
            foreach (var part in _parts.Where(p => p.UsageType == UsageType.StartLp && p.Modulation == _especificationsViewModel.SelectedModulation))
                    StartLpList.Add(part);
        }

        private void EspecificationsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_especificationsViewModel.SelectedModulation))
                SetParts();
        }
    }
}
