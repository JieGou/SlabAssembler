using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for PartsControl.xaml
    /// </summary>
    public partial class PartsControl
    {
        public PartsViewModel ViewModel { get; protected set; }
        private ConfigurationsRepository _config;
        private AutoCadManager _acad;

        public PartsControl(ConfigurationsRepository config, AutoCadManager acad)
        {
            _config = config;
            _acad = acad;

            ViewModel = new PartsViewModel(config);
            ViewModel.Reset.Subscribe(x => config.ResetDefaults());
            ViewModel.DeleteSelectedPart.Subscribe(p => {
                config.DeletePart((p as Part).Id);
            });
            ViewModel.EditSelectedPart.Subscribe(p => OpenPartWindow(p as Part));
            ViewModel.CreatePart.Subscribe(x => OpenPartWindow(new Part()));
            ViewModel.Analyze.Subscribe(x => AnalyzeImpl());

            DataContext = ViewModel;
            InitializeComponent();
        }

        private void OpenPartWindow(Part part)
        {
            var window = new PartWindow(_config, part);
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
