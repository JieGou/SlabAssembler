using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;

namespace Urbbox.AutoCAD.ProtentionBuilder
{
    public class Plugin
    {
        private static PaletteSet _mainPallet;

        [CommandMethod("URBPROTENSION")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
                
            var especificationsControl = new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = new EspecificationsControl()
            };

            _mainPallet.Add("Especificações", especificationsControl);
        }

        private static PaletteSet InitializeMainPallet()
        {
            return new PaletteSet("Urbbox | Construtor de Lajes")
            {
                Size = new Size(300, 600),
                DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right),
                Visible = true,
                KeepFocus = true
            };
        }
    }
}
