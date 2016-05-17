using System.Linq;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using System;
using ReactiveUI;
using Autodesk.AutoCAD.Customization;
using Urbbox.SlabAssembler.Managers;
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
        public AssemblyOptions Options => _algorythimRepository.GetAssemblyOptions();

        public ReactiveList<Part> StartLpList { get; }
        public ReactiveCommand<object> Reset { get; private set; }
        public ReactiveCommand<object> Update { get; private set; }

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

        public AlgorythimViewModel(IAlgorythimRepository algorythimRepository)
        {
            _algorythimRepository = algorythimRepository;
            StartLpList = new ReactiveList<Part>();
            Reset = ReactiveCommand.Create();
            Update = ReactiveCommand.Create();

            Reset.Subscribe(_ => _algorythimRepository.ResetAssemblyOptions());
            Update.Subscribe(_ => _algorythimRepository.SetAssemblyOptions(Options));
        }

        public AlgorythimViewModel(IPartRepository partRepository, IAlgorythimRepository algorythimRepository)
        {
            this._partRepository = partRepository;
            this._algorythimRepository = algorythimRepository;

            _partRepository.GetPartsObservable().Subscribe(e =>
            {
                StartLpList.Clear();
                foreach (Part p in e.NewItems)
                    StartLpList.Add(p);
            });

            StartLpList = new ReactiveList<Part>();
            Reset = ReactiveCommand.Create();
            Update = ReactiveCommand.Create();

            Reset.Subscribe(_ => _algorythimRepository.ResetAssemblyOptions());
            Update.Subscribe(_ => _algorythimRepository.SetAssemblyOptions(Options));
        }
    }
}
