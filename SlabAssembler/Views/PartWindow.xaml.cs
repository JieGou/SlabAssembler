using System.Windows;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for PartWindow.xaml
    /// </summary>
    public partial class PartWindow : Window
    {
        public PartWindow(ConfigurationsRepository configurationsManager, Part part)
        {
            DataContext = new PartViewModel(this, configurationsManager, part);
            InitializeComponent();
        }
    }
}
