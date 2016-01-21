using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.AutoCAD.ProtentionBuilder
{
    public class Plugin
    {
        private static PaletteSet _mainPallet;
        private static ConfigurationsManager _configurationsManager;
        private static AcManager _acManager;

        [CommandMethod("URBPROTENSION")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _configurationsManager = new ConfigurationsManager(@"Resources\Configurations.json");
            _acManager = new AcManager(AcApplication.DocumentManager.MdiActiveDocument);

            var especificationsControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new EspecificationsControl(_configurationsManager, _acManager)
            };

            var algorythimControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new AlgorythimControl(_configurationsManager)
            };

            var partsControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new PartsControl()
            };

            _mainPallet.Add("Especificações", especificationsControl);
            _mainPallet.Add("Algoritmo", algorythimControl);
            _mainPallet.Add("Peças", partsControl);
        }

        private static void _mainPallet_StateChanged(object sender, PaletteSetStateEventArgs e)
        {
            if (e.NewState == StateEventIndex.Hide)
                _mainPallet = null;
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
