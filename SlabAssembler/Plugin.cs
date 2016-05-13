using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Urbbox.SlabAssembler.Properties;
using Urbbox.SlabAssembler.Core;
using System.Reactive.Linq;
using System;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler
{
    public static class Plugin
    {
        private static PaletteSet _mainPallet;
        private static ConfigurationsManager _configManager;
        private static AutoCadManager _acManager;

        [CommandMethod("URBLAJE")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _configManager = new ConfigurationsManager(Resources.ConfigurationsFile, Resources.DefaultsConfigurationFile);
            _acManager = new AutoCadManager();

            var especificationsView = new Views.EspecificationsControl(_configManager, _acManager);
            var algorythimView = new Views.AlgorythimControl(especificationsView.ViewModel, _configManager);
            var partsView = new Views.PartsListControl(_configManager, _acManager);
            var helper = new BuildingProcessHelper(_acManager);
            var prop = new SlabProperties() {
                Algorythim = algorythimView.ViewModel,
                Parts = especificationsView.ViewModel
            };

            especificationsView.ViewModel.DrawSlab.Subscribe(_ => {
                using (var builder = new SlabBuilder(_acManager, _configManager))
                {
                    prop.MaxPoint = helper.GetMaxPoint(especificationsView.ViewModel.SelectedOutline);
                    prop.StartPoint = helper
                        .GetStartPoint(especificationsView.ViewModel.SelectedOutline, especificationsView.ViewModel.SpecifyStartPoint)
                        .Add(prop.StartPointDeslocation);
                    builder.Start(prop);
                }
            });

            especificationsView.ViewModel.SelectOutline.Subscribe(_ => {
                especificationsView.ViewModel.SelectedOutline = helper.SelectOutline();
            });

            _mainPallet.Add("Especificações", GetElementHost(especificationsView));
            _mainPallet.Add("Algoritmo", GetElementHost(algorythimView));
            _mainPallet.Add("Peças", GetElementHost(partsView));
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
                _configManager = null;
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
