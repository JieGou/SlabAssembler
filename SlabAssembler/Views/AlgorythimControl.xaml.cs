using Urbbox.SlabAssembler.Repositories;
using System;
using System.Reactive.Linq;
using Urbbox.SlabAssembler.ViewModels;
using System.Windows.Controls;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for AlgorythimControl.xaml
    /// </summary>
    public partial class AlgorythimControl : UserControl
    {
        public AlgorythimViewModel ViewModel { get; private set; }

        public AlgorythimControl(EspecificationsViewModel especifications, ConfigurationsRepository config)
        {
            ViewModel = new AlgorythimViewModel(ref especifications, config);
            DataContext = ViewModel;
            InitializeComponent();
        }
      
    }
}
