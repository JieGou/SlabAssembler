using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.Properties;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Urbbox.SlabAssembler.Core;

namespace Urbbox.SlabAssembler
{
    public class Plugin
    {
        private static PaletteSet _mainPallet;
        private static ConfigurationsRepository _configRepository;
        private static AutoCadManager _acManager;

        [CommandMethod("URBLAJE")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _configRepository = new ConfigurationsRepository(Resources.ConfigurationsFile, Resources.DefaultsConfigurationFile);
            _acManager = new AutoCadManager(AcApplication.DocumentManager.MdiActiveDocument);

            var builder = new SlabBuilder(_acManager);
            var especificationsView = new Views.EspecificationsControl(_configRepository);
            var algorythimView = new Views.AlgorythimControl(especificationsView.ViewModel, _configRepository);
            var partsView = new Views.PartsControl(_configRepository, _acManager);

            foreach (var l in _acManager.GetLayers())
                especificationsView.ViewModel.Layers.Add(l);

            builder.EspecificationsViewModel = especificationsView.ViewModel;
            builder.AlgorythimViewModel = algorythimView.ViewModel;
            _mainPallet.Add("Especificações", GetElementHost(especificationsView));
            _mainPallet.Add("Algoritmo", GetElementHost(algorythimView));
            _mainPallet.Add("Peças", GetElementHost(partsView));
            _configRepository.LoadData();
        }

        private static ElementHost GetElementHost(System.Windows.UIElement element)
        {
            return new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = element
            };
        }

        private static void _mainPallet_StateChanged(object sender, PaletteSetStateEventArgs e)
        {
            if (e.NewState == StateEventIndex.Hide) { 
                _mainPallet = null;
                _acManager = null;
                _configRepository = null;
            }
        }

        private static PaletteSet InitializeMainPallet()
        {
            return new PaletteSet("Urbbox | Construtor de Lajes")
            {
                Size = new Size(325, 600),
                DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right),
                MinimumSize = new Size(325, 500),
                Visible = true,
                KeepFocus = true,
            };
        }
    }
}
