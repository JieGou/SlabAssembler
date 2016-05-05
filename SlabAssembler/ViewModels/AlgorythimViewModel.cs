using System.Collections.Generic;
using System.Linq;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using System;
using ReactiveUI;
using System.Reactive.Linq;
using Autodesk.AutoCAD.Customization;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class AlgorythimViewModel : ReactiveObject
    {
        public ReactiveList<Part> StartLpList { get; }
        public ReactiveCommand<object> Reset { get; private set; }

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

        private Part _selectedStartLp;
        public Part SelectedStartLp
        {
            get { return _selectedStartLp; }
            set { this.RaiseAndSetIfChanged(ref _selectedStartLp, value); }
        }

        public AssemblyOptions Options { get; set; }
        public ReactiveCommand<object> Update { get; private set; }

        private ConfigurationsManager _configManager;
        private EspecificationsViewModel _especifications;

        public AlgorythimViewModel(EspecificationsViewModel especifications, ConfigurationsManager manager)
        {
            _configManager = manager;
            _especifications = especifications;

            Options = manager.Config.Options;
            StartLpList = new ReactiveList<Part>();
            Reset = ReactiveCommand.Create();
            Reset.Subscribe(x => _configManager.ResetDefaults());

            Update = ReactiveCommand.Create();
            Update.Subscribe(x => _configManager.SaveConfig());

            _especifications.ObservableForProperty(x => x.SelectedModulation)
                .Subscribe(x => {
                    var parts = _configManager.GetPartsByModulaton(x.GetValue()).Where(p => p.UsageType == UsageType.StartLp);
                    StartLpList.Clear();
                    foreach (var p in parts)
                        StartLpList.Add(p);
                });
        }

    }
}
