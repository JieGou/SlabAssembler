using System.Windows;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
{
    /// <summary>
    /// Interaction logic for PartWindow.xaml
    /// </summary>
    public partial class PartWindow : Window
    {
        public PartWindow(ConfigurationsManager configurationsManager, Part part)
        {
            DataContext = new PartViewModel(configurationsManager, part);
            InitializeComponent();
        }
    }
}
