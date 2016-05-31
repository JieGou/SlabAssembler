using System;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using ReactiveUI;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class PartsViewModel : ReactiveObject
    {
        public ReactiveList<Part> Parts { get; set; }
        public ReactiveCommand<object> CreatePart { get; }
        public ReactiveCommand<object> EditSelectedPart { get; }
        public ReactiveCommand<object> DeleteSelectedPart { get; }
        public ReactiveCommand<object> Reset { get; }
        public ReactiveCommand<object> Analyze { get; }

        private Part _selectedPart;
        public Part SelectedPart {
            get { return _selectedPart; }
            set { this.RaiseAndSetIfChanged(ref _selectedPart, value); }
        }
   
        public PartsViewModel(IPartRepository partRepository)
        {
            Parts = new ReactiveList<Part>();
            CreatePart = ReactiveCommand.Create();

            var canSelectPart = this.WhenAny(x => x.SelectedPart, s => s.Value != null);
            EditSelectedPart = canSelectPart.ToCommand();
            DeleteSelectedPart = canSelectPart.ToCommand();
            Reset = ReactiveCommand.Create();
            Analyze = this.WhenAny(x => x.Parts.Count, x => x.Value > 0).ToCommand();

            partRepository.PartsChanged.Subscribe(_ =>
            {
                Parts.Clear();
                Parts.AddRange(partRepository.GetAll());
            });
        }

    }
}
