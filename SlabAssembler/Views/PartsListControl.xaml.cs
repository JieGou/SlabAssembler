using System;
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
        private IPartRepository _partRepository;
        private AutoCadManager _acad;

        public PartsListControl(ConfigurationsManager manager, AutoCadManager acad)
        {
            _partRepository = manager;
            _acad = acad;

            ViewModel = new PartsViewModel();
            ViewModel.Reset.Subscribe(x => manager.ResetDefaults());
            ViewModel.DeleteSelectedPart.Subscribe(p => {
                var part = p as Part;
                _partRepository.RemovePart(part.Id);
            });
            ViewModel.EditSelectedPart.Subscribe(p => OpenPartWindow(p as Part));
            ViewModel.CreatePart.Subscribe(_ => OpenPartWindow(new Part()));
            ViewModel.Analyze.Subscribe(_ => AnalyzeImpl());

            manager.Config.Parts.ItemChanged.Subscribe(_ =>
            {
                ViewModel.Parts.Clear();
                foreach (var p in _partRepository.GetParts())
                    ViewModel.Parts.Add(p);
            });

            DataContext = ViewModel;
            InitializeComponent();
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

            string log = "";
            foreach (var part in ViewModel.Parts)
            {
                bool e = _acad.CheckBlockExists(part.ReferenceName);
                bool l = _acad.CheckLayerExists(part.Layer);
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

            logWindow.SetResultTitle(String.Format("{0} peças analizadas.", ViewModel.Parts.Count));
        }
    }
}
