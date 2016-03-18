using System.Collections.Generic;
using System.Linq;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Repositories;
using System;
using ReactiveUI;
using System.Reactive.Linq;
using Urbbox.SlabAssembler.ViewModels.Commands;
using System.Windows.Input;
using Autodesk.AutoCAD.Customization;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class AlgorythimViewModel : ReactiveObject
    {
        public ReactiveList<Part> StartLpList { get; }

        private float _outlineDistance;
        public float OutlineDistance
        {
            get { return _outlineDistance; }
            set { this.RaiseAndSetIfChanged(ref _outlineDistance, value); }
        }

        private float _distanceBetweenLp;
        public float DistanceBetweenLp
        {
            get { return _distanceBetweenLp; }
            set { this.RaiseAndSetIfChanged(ref _distanceBetweenLp, value); }
        }

        private float _distanceBetweenLpAndLd;
        public float DistanceBetweenLpAndLd
        {
            get { return _distanceBetweenLpAndLd; }
            set { this.RaiseAndSetIfChanged(ref _distanceBetweenLpAndLd, value); }
        }

        private bool _useLds;
        public bool UseLds
        {
            get { return _useLds; }
            set { this.RaiseAndSetIfChanged(ref _useLds, value); }
        }

        private bool _useEndLp;
        public bool UseEndLp
        {
            get { return _useEndLp; }
            set { this.RaiseAndSetIfChanged(ref _useEndLp, value); }
        }

        private bool _useStartLp;
        public bool UseStartLp
        {
            get { return _useStartLp; }
            set { this.RaiseAndSetIfChanged(ref _useStartLp, value); }
        }

        private bool _specifyStartPoint;
        public bool SpecifyStartPoint
        {
            get { return _specifyStartPoint; }
            set { this.RaiseAndSetIfChanged(ref _specifyStartPoint, value); }
        }

        public ICommand ResetCommand { get; private set; }

        private Orientation _selectedOrientation = Orientation.Vertical;
        public Orientation SelectedOrientation {
            get { return _selectedOrientation; }
            set { this.RaiseAndSetIfChanged(ref _selectedOrientation, value); }
        }

        public ReactiveList<Orientation> OrientationsList { get; private set; }

        private List<Part> _parts;
        private ConfigurationsRepository _manager;
        private EspecificationsViewModel _especifications;

        public AlgorythimViewModel(ref EspecificationsViewModel especifications, ConfigurationsRepository configurationsManager)
        {
            this._manager = configurationsManager;
            this._especifications = especifications;
            this._parts = _manager.Data.Parts;
            this.StartLpList = new ReactiveList<Part>();
            this.ResetCommand = new RelayCommand(() => _manager.ResetDefaults());

            _especifications.ObservableForProperty(x => x.SelectedModulation)
                .Subscribe(x => RefreshParts());

            OrientationsList = new ReactiveList<Orientation>();
            foreach (Orientation o in Enum.GetValues(typeof(Orientation)))
                OrientationsList.Add(o);

            _manager.DataLoaded += ConfigurationsManager_DataLoaded;
            PropertyChanged += AlgorythimViewModel_PropertyChanged;
        }


        private void AlgorythimViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UseStartLp)) _manager.Data.UseStartLp = UseStartLp;
            if (e.PropertyName == nameof(UseEndLp)) _manager.Data.UseEndLp = UseEndLp;
            if (e.PropertyName == nameof(UseLds)) _manager.Data.UseLds = UseLds;
            if (e.PropertyName == nameof(OutlineDistance)) _manager.Data.OutlineDistance = OutlineDistance;
            if (e.PropertyName == nameof(DistanceBetweenLp)) _manager.Data.DistanceBetweenLp = DistanceBetweenLp;
            if (e.PropertyName == nameof(DistanceBetweenLpAndLd)) _manager.Data.DistanceBetweenLpAndLd = DistanceBetweenLpAndLd;
            _manager.SaveData();
        }

        private void ConfigurationsManager_DataLoaded(ConfigurationData data)
        {
            _outlineDistance = data.OutlineDistance;
            _distanceBetweenLp = data.DistanceBetweenLp;
            _distanceBetweenLpAndLd = data.DistanceBetweenLpAndLd;
            _useLds = data.UseLds;
            _useEndLp = data.UseEndLp;
            _useStartLp = data.UseStartLp;
            RefreshParts();
        }

        public void RefreshParts()
        {
            _parts = _manager.Data.Parts;
            StartLpList.Clear();
            foreach (var p in _parts.Where(p => p.UsageType == UsageType.StartLp && p.Modulation == _especifications.SelectedModulation))
                StartLpList.Add(p);
        }
    }
}
