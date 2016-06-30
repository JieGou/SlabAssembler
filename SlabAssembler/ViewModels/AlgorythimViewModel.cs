using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using System;
using System.Reactive.Linq;
using ReactiveUI;
using Autodesk.AutoCAD.Customization;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class AlgorythimViewModel : ReactiveObject
    {
        private Orientation _selectedOrientation = Orientation.Vertical;
        private bool _onlyCimbrament;
        private Part _selectedStartLp;
        private readonly IPartRepository _partRepository;
        private int _selectedModulation;
        private bool _hasChanges;
        private bool HasChanges
        {
            get { return _hasChanges; }
            set { this.RaiseAndSetIfChanged(ref _hasChanges, value); }
        }

        public int OrientationAngle => SelectedOrientation == Orientation.Vertical ? 90 : 0;
        public AssemblyOptions Options { get; }
        public ReactiveList<Part> StartLpList { get; }
        public ReactiveCommand<object> Reset { get; }
        public ReactiveCommand<object> Update { get; }

        public int SelectedModulation
        {
            get { return _selectedModulation; }
            set { this.RaiseAndSetIfChanged(ref _selectedModulation, value); }
        }

        public Orientation SelectedOrientation
        {
            get { return _selectedOrientation; }
            set { this.RaiseAndSetIfChanged(ref _selectedOrientation, value); }
        }

        public bool OnlyCimbrament
        {
            get { return _onlyCimbrament; }
            set { this.RaiseAndSetIfChanged(ref _onlyCimbrament, value); }
        }

        public Part SelectedStartLp
        {
            get { return _selectedStartLp; }
            set { this.RaiseAndSetIfChanged(ref _selectedStartLp, value); }
        }

        public AlgorythimViewModel(IPartRepository partRepository, IAlgorythimRepository algorythimRepository)
        {
            _partRepository = partRepository;
            HasChanges = false;

            Options = algorythimRepository.Get();
            StartLpList = new ReactiveList<Part>();
            Reset = ReactiveCommand.Create();
            Update = this.WhenAny(x => x.HasChanges, c => c.Value).ToCommand();

            Options.Changed.Subscribe(x => HasChanges = true);
            this.WhenAnyValue(x => x.SelectedModulation).Subscribe(_ => ResetParts());
            _partRepository.PartsChanged.Subscribe(_ => ResetParts());
            Reset.Subscribe(_ => algorythimRepository.Reset());
            Update.Subscribe(_ =>
            {
                algorythimRepository.SaveChanges();
                HasChanges = false;
            });
        }

        private void ResetParts()
        {
            if (SelectedModulation == 0) return;

            StartLpList.Clear();
            foreach (var p in _partRepository.GetByModulaton(SelectedModulation).WhereType(UsageType.StartLp))
                StartLpList.Add(p);
        }
    }
}
