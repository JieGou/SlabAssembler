using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;
using Urbbox.AutoCAD.ProtentionBuilder.Database;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class AlgorythimViewModel : ModelBase
    {
        public ObservableCollection<Part> StartLpList { get; }

        public float OutlineDistance
        {
            get { return _outlineDistance; }
            set
            {
                if (value.Equals(_outlineDistance)) return;
                _outlineDistance = value;
                OnPropertyChanged();
            }
        }

        public float DistanceBetweenLp
        {
            get { return _distanceBetweenLp; }
            set
            {
                if (value.Equals(_distanceBetweenLp)) return;
                _distanceBetweenLp = value;
                OnPropertyChanged();
            }
        }

        public float DistanceBetweenLpAndLd
        {
            get { return _distanceBetweenLpAndLd; }
            set
            {
                if (value.Equals(_distanceBetweenLpAndLd)) return;
                _distanceBetweenLpAndLd = value;
                OnPropertyChanged();
            }
        }

        public bool UseLds
        {
            get { return _useLds; }
            set
            {
                if (value == _useLds) return;
                _useLds = value;
                OnPropertyChanged();
            }
        }

        public bool UseEndLp
        {
            get { return _useEndLp; }
            set
            {
                if (value == _useEndLp) return;
                _useEndLp = value;
                OnPropertyChanged();
            }
        }

        public bool UseStartLp
        {
            get { return _useStartLp; }
            set
            {
                if (value == _useStartLp) return;
                _useStartLp = value;
                OnPropertyChanged();
            }
        }

        private readonly EspecificationsViewModel _especificationsViewModel;
        private List<Part> _parts;
        private float _outlineDistance;
        private float _distanceBetweenLp;
        private float _distanceBetweenLpAndLd;
        private bool _useLds;
        private bool _useEndLp;
        private bool _useStartLp;
        private ConfigurationsManager _manager;

        public AlgorythimViewModel(ref EspecificationsViewModel especifications, ConfigurationsManager configurationsManager)
        {
            _especificationsViewModel = especifications;
            _manager = configurationsManager;
            _parts = _manager.Data.Parts;
            StartLpList = new ObservableCollection<Part>();

            _especificationsViewModel.PropertyChanged += EspecificationsViewModel_PropertyChanged;
            _manager.DataLoaded += ConfigurationsManager_DataLoaded;
        }

        private void ConfigurationsManager_DataLoaded(ConfigurationData data)
        {
            OutlineDistance = data.OutlineDistance;
            DistanceBetweenLp = data.DistanceBetweenLp;
            DistanceBetweenLpAndLd = data.DistanceBetweenLpAndLd;
            UseLds = data.UseLds;
            UseEndLp = data.UseEndLp;
            UseStartLp = data.UseStartLp;
            SetParts();
        }

        private void SetParts()
        {
            _parts = _manager.Data.Parts;
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
