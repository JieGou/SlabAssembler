using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
{
    /// <summary>
    /// Interaction logic for PartsControl.xaml
    /// </summary>
    public partial class PartsControl : UserControl
    {
        private List<Part> _parts;

        public PartsControl(ConfigurationsManager configurationsManager)
        {
            InitializeComponent();

            _parts = configurationsManager.Data.Parts;

            foreach (var modulationGroup in _parts.GroupBy(p => p.Modulation)) {
                var modulationTree = new TreeViewItem {Header = $"Modulação {modulationGroup.Key}"};

                foreach (var usageTypeGroup in modulationGroup.ToList().GroupBy(p => p.UsageType))
                {
                    var usageTypeTree = new TreeViewItem { Header = usageTypeGroup.Key.ToNameString() };

                    foreach (var part in usageTypeGroup.ToList())
                        usageTypeTree.Items.Add(part.Name);

                    modulationTree.Items.Add(usageTypeTree);
                }

                partsTreeView.Items.Add(modulationTree);
            }
        }

    }
}
