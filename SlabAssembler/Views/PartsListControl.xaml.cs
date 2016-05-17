using System;
using System.Windows;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Managers;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for PartsListControl.xaml
    /// </summary>
    public partial class PartsListControl
    {
        public PartsViewModel ViewModel { get; protected set; }
        private readonly IPartRepository _partRepository;
        private readonly AutoCadManager _acad;

        public PartsListControl(IPartRepository partRepository, AutoCadManager acad)
        {
            _partRepository = partRepository;
            _acad = acad;

            ViewModel = new PartsViewModel(_partRepository);
            ViewModel.Reset.Subscribe(ResetImpl);

            ViewModel.DeleteSelectedPart.Subscribe(DeleteSelectedPartImpl);
            ViewModel.EditSelectedPart.Subscribe(p => OpenPartWindow(p as Part));
            ViewModel.CreatePart.Subscribe(_ => OpenPartWindow(new Part()));
            ViewModel.Analyze.Subscribe(_ => AnalyzeImpl());

            DataContext = ViewModel;
            InitializeComponent();
        }

        private void DeleteSelectedPartImpl(object p)
        {
            var part = p as Part;
            if (part != null) _partRepository.RemovePart(part.Id);
        }

        private void ResetImpl(object x)
        {
            var result = MessageBox.Show("Deseja realmente resetar todas as peças para o padrão?", "Resetar Permanentemente", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                _partRepository.ResetParts();
        }

        private void OpenPartWindow(Part part)
        {
            var window = new PartWindow(_partRepository, part);
            AcApplication.ShowModelessWindow(window);
        }

        private void AnalyzeImpl()
        {
            var logWindow = new LogWindow();
            AcApplication.ShowModelessWindow(logWindow);

            var log = "";
            foreach (var part in ViewModel.Parts)
            {
                var e = _acad.CheckBlockExists(part.ReferenceName);
                var l = _acad.CheckLayerExists(part.Layer);
                log += $"{part.ReferenceName}\t -> ";
                if (e && l)
                    log += "OK";
                else
                {
                    if (!e) log += "REFERÊNCIA INEXISTENTE, ";
                    if (!l) log += "CAMADA INEXISTENTE";
                }

                log += "\n";
                logWindow.SetLogMessage(log);
            }

            logWindow.SetResultTitle($"{ViewModel.Parts.Count} peças analizadas.");
        }
    }
}
