using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Repositories;
using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class EspecificationsViewModel : ReactiveObject
    {
        private List<Part> _parts;

        private bool _selecting;
        private bool _drawing;
        public ReactiveList<int> Modulations { get; }
        public ReactiveList<Part> FormsAndBoxes { get; }
        public ReactiveList<Part> LpList { get; }
        public ReactiveList<Part> LdList { get; }
        public ReactiveList<string> Layers { get; }

        public ReactiveCommand<object> SelectOutline { get; private set; }
        public ReactiveCommand<object> DrawSlab { get; private set; }

        private int _selectedModulation;
        public int SelectedModulation {
            get { return _selectedModulation; }
            set { this.RaiseAndSetIfChanged(ref _selectedModulation, value); }
        }

        private Part _selectedCast;
        public Part SelectedCast {
            get { return _selectedCast; }
            set { this.RaiseAndSetIfChanged(ref _selectedCast, value); }
        }

        private Part _selectedLd;
        public Part SelectedLd {
            get { return _selectedLd; }
            set { this.RaiseAndSetIfChanged(ref _selectedLd, value); }
        }

        private Part _selectedLp;
        public Part SelectedLp {
            get { return _selectedLp; }
            set { this.RaiseAndSetIfChanged(ref _selectedLp, value); }
        }

        private string _selectedColumnsLayer;
        public string SelectedColumnsLayer {
            get { return _selectedColumnsLayer; }
            set { this.RaiseAndSetIfChanged(ref _selectedColumnsLayer, value); }
        }

        private string _selectedGirdersLayer;
        public string SelectedGirdersLayer {
            get { return _selectedGirdersLayer; }
            set { this.RaiseAndSetIfChanged(ref _selectedGirdersLayer, value); }
        }

        private string _selectedEmptiesLayer;
        public string SelectedEmptiesLayer {
            get { return _selectedEmptiesLayer; }
            set { this.RaiseAndSetIfChanged(ref _selectedEmptiesLayer, value); }
        }

        private string _selectionStatus;
        public string SelectionStatus {
            get { return _selectionStatus; }
            set { this.RaiseAndSetIfChanged(ref _selectionStatus, value); }
        }

        private ObjectId _selectedOutline;
        public ObjectId SelectedOutline {
            get { return _selectedOutline; }
            set { this.RaiseAndSetIfChanged(ref _selectedOutline, value); }
        }

        public EspecificationsViewModel(ConfigurationsRepository config)
        {
            _parts = new List<Part>();
            _selecting = false;
           
            Modulations = new ReactiveList<int>() { 0 };
            FormsAndBoxes = new ReactiveList<Part>();
            LdList = new ReactiveList<Part>();
            LpList = new ReactiveList<Part>();
            Layers = new ReactiveList<string>();
            SelectionStatus = "Selecione um contorno.";
            SelectedModulation = 0;
            SelectOutline = this.WhenAny(x => x._selecting, x => x._drawing, (s, d) => !s.Value && !d.Value).ToCommand();
            SelectOutline.IsExecuting.ToProperty(this, x => x._selecting, false);

            DrawSlab = this.WhenAnyValue(
                x => x.SelectedOutline,
                x => x.SelectedModulation,
                x => x.SelectedCast,
                x => x.SelectedLp, 
                x => x.SelectedLd,
                x => x._drawing)
                .Select(x => 
                    x.Item1 != ObjectId.Null 
                    && x.Item2 > 0
                    && x.Item3 != null
                    && x.Item4 != null
                    && x.Item5 != null
                    && !x.Item6)
                    .ToCommand();
            DrawSlab.IsExecuting.ToProperty(this, x => x._drawing, false);

            this.ObservableForProperty(x => x.SelectedModulation)
                .Subscribe(x => RefreshParts());
            this.ObservableForProperty(x => x.SelectedOutline)
                .Subscribe(s => {
                    SelectionStatus = (s.Value != ObjectId.Null)? $"Contorno selecionado: #{s.Value.GetHashCode()}" : "Nenhum contorno selecionado.";
                });

            config.PartsChanged += Config_PartsChanged;
        }

        private void Config_PartsChanged(List<Part> parts)
        {
            _parts = parts;
            RefreshParts();
        }

        private void RefreshParts()
        {
            Modulations.Clear();
            foreach (var p in _parts.GroupBy(p => p.Modulation).Select(g => g.Key))
                Modulations.Add(p);

            FormsAndBoxes.Clear();
            foreach (var p in _parts.Where(p => (p.UsageType == UsageType.Box || p.UsageType == UsageType.Form) && p.Modulation == SelectedModulation))
                FormsAndBoxes.Add(p);

            LpList.Clear();
            foreach (var p in _parts.Where(p => (p.UsageType == UsageType.Lp) && p.Modulation == SelectedModulation))
                LpList.Add(p);

            LdList.Clear();
            foreach (var p in _parts.Where(p => (p.UsageType == UsageType.Ld) && p.Modulation == SelectedModulation))
                LdList.Add(p);
        }

    }
}
