using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Urbbox.AutoCAD.ProtentionBuilder.Database;

namespace Urbbox.AutoCAD.ProtentionBuilder
{
    public class Plugin
    {
        private static PaletteSet _mainPallet;
        private static ConfigurationManager _configurationManager;

        [CommandMethod("URBPROTENSION")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _configurationManager = new ConfigurationManager(@"Resources\Configurations.json");
                
            var especificationsControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new EspecificationsControl(_configurationManager)
            };

            _mainPallet.Add("Especificações", especificationsControl);
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
                Size = new Size(300, 600),
                DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right),
                MinimumSize = new Size(300, 500),
                Visible = true,
                KeepFocus = true,
            };
        }
    }
}
