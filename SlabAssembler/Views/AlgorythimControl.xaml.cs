using Urbbox.SlabAssembler.ViewModels;
using System.Windows.Controls;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for AlgorythimControl.xaml
    /// </summary>
    public partial class AlgorythimControl : UserControl
    {
        public AlgorythimViewModel ViewModel { get; private set; }

        public AlgorythimControl(EspecificationsViewModel especifications, ConfigurationsManager manager)
        {
            ViewModel = new AlgorythimViewModel(especifications, manager);
            DataContext = ViewModel;
            InitializeComponent();
        }
      
    }
}
