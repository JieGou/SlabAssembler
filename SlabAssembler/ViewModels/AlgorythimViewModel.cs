using System.Collections.Generic;
using System.Linq;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Repositories;
using System;
using ReactiveUI;
using System.Reactive.Linq;
using Autodesk.AutoCAD.Customization;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class AlgorythimViewModel : ReactiveObject
    {
        public ReactiveList<Part> StartLpList { get; }
        public ReactiveList<Part> Parts { get; set; }
        public ReactiveCommand<object> Reset { get; private set; }

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

        private Orientation _selectedOrientation = Orientation.Vertical;
        public Orientation SelectedOrientation {
            get { return _selectedOrientation; }
            set { this.RaiseAndSetIfChanged(ref _selectedOrientation, value); }
        }

        public double OrientationAngle => (SelectedOrientation == Orientation.Vertical) ? 90 : 0;

        private bool _onlyCimbrament;
        public bool OnlyCimbrament
        {
            get { return _onlyCimbrament; }
            set { this.RaiseAndSetIfChanged(ref _onlyCimbrament, value); }
        }

        public ReactiveCommand<object> Update { get; private set; }

        private ConfigurationsRepository _config;
        private EspecificationsViewModel _especifications;
        private bool _canUpdateConfig;

        public AlgorythimViewModel(ref EspecificationsViewModel especifications, ConfigurationsRepository config)
        {
            _config = config;
            _especifications = especifications;

            Parts = new ReactiveList<Part>();
            StartLpList = new ReactiveList<Part>();
            Reset = ReactiveCommand.Create();
            Reset.Subscribe(x => _config.ResetDefaults());

            Update = this.WhenAny(x => x._canUpdateConfig, u => u.Value).ToCommand();
            Update.Subscribe(x => UpdateConfigurations());

            _especifications.ObservableForProperty(x => x.SelectedModulation)
                .Subscribe(x => RefreshParts());

            _config.DataLoaded += ConfigurationsManager_DataLoaded;
            this.WhenAnyValue(
                x => x.UseStartLp,
                x => x.UseEndLp,
                x => x.UseLds,
                x => x.DistanceBetweenLp,
                x => x.DistanceBetweenLpAndLd,
                x => x.OutlineDistance)
                .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .InvokeCommand(this, x => x.Update);

            config.PartsChanged += Config_PartsChanged;
        }

        private void Config_PartsChanged(List<Part> parts)
        {
            Parts.Clear();
            foreach (var p in parts)
                Parts.Add(p);
            RefreshParts();
        }

        private void UpdateConfigurations()
        {
            _config.Data.UseStartLp = UseStartLp;
            _config.Data.UseEndLp = UseEndLp;
            _config.Data.UseLds = UseLds;
            _config.Data.OutlineDistance = OutlineDistance;
            _config.Data.DistanceBetweenLp = DistanceBetweenLp;
            _config.Data.DistanceBetweenLpAndLd = DistanceBetweenLpAndLd;
            _config.SaveData();
        }

        public void ConfigurationsManager_DataLoaded(ConfigurationData data)
        {
            _canUpdateConfig = false;
            OutlineDistance = data.OutlineDistance;
            DistanceBetweenLp = data.DistanceBetweenLp;
            DistanceBetweenLpAndLd = data.DistanceBetweenLpAndLd;
            UseLds = data.UseLds;
            UseEndLp = data.UseEndLp;
            UseStartLp = data.UseStartLp;
            _canUpdateConfig = true;
            RefreshParts();
        }

        public void RefreshParts()
        {
            StartLpList.Clear();
            foreach (var p in Parts.Where(p => p.UsageType == UsageType.StartLp && p.Modulation == _especifications.SelectedModulation))
                StartLpList.Add(p);
        }
    }
}
