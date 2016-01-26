using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.Properties;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.AutoCAD.ProtentionBuilder
{
    public class Plugin
    {
        private static PaletteSet _mainPallet;
        private static ConfigurationsManager _configurationsManager;
        private static AcManager _acManager;

        [CommandMethod("URBPROTENSION")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _configurationsManager = new ConfigurationsManager(Resources.ConfigurationsFile);
            _acManager = new AcManager(AcApplication.DocumentManager.MdiActiveDocument);

            var especifications = new Views.EspecificationsControl(_configurationsManager, _acManager);
            var especificationsControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = especifications
            };

            var algorythimControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new Views.AlgorythimControl(especifications, _configurationsManager)
            };

            var partsControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new Views.PartsControl(_configurationsManager)
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
