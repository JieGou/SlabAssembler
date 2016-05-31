using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using System;
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
        private readonly IAlgorythimRepository _algorythimRepository;

        public double OrientationAngle => (SelectedOrientation == Orientation.Vertical) ? 90 : 0;
        public AssemblyOptions Options => _algorythimRepository.Get();

        public int SelectedModulation { get; set; }
        public ReactiveList<Part> StartLpList { get; }
        public ReactiveCommand<object> Reset { get; }
        public ReactiveCommand<object> Update { get; }

        public Orientation SelectedOrientation {
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
            _algorythimRepository = algorythimRepository;

            _partRepository.PartsChanged.Subscribe(_ =>
            {
                StartLpList.Clear();
                StartLpList.AddRange(_partRepository.GetByModulaton(SelectedModulation).WhereType(UsageType.StartLp));
            });

            StartLpList = new ReactiveList<Part>();
            Reset = ReactiveCommand.Create();
            Update = ReactiveCommand.Create();

            Reset.Subscribe(_ => _algorythimRepository.Reset());
            Update.Subscribe(_ => _algorythimRepository.SaveChanges());
        }
    }
}
