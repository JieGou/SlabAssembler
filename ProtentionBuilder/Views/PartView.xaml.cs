using System.Windows;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
{
    /// <summary>
    /// Interaction logic for PartView.xaml
    /// </summary>
    public partial class PartView : Window
    {
        public PartView(Part part)
        {
            DataContext = new PartViewModel(part);
            InitializeComponent();
        }
    }
}
