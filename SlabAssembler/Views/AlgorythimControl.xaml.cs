using System.Windows.Controls;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for AlgorythimControl.xaml
    /// </summary>
    public partial class AlgorythimControl : UserControl
    {
        public AlgorythimViewModel ViewModel { get; private set; }

        public AlgorythimControl(EspecificationsViewModel viewModel, ConfigurationsRepository configurations, SlabBuilder builder)
        {
            ViewModel = new AlgorythimViewModel(ref viewModel, configurations);
            //builder.Especifications.AlgorythimEspecifications = ViewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
