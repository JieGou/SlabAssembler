using System;
using Urbbox.SlabAssembler.ViewModels;
using System.Windows.Controls;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for AlgorythimControl.xaml
    /// </summary>
    public partial class AlgorythimControl : UserControl
    {
        public AlgorythimViewModel ViewModel { get; }

        public AlgorythimControl(IPartRepository partRepository, IAlgorythimRepository algorythimRepository)
        {
            ViewModel = new AlgorythimViewModel(partRepository, algorythimRepository);
            DataContext = ViewModel;
            InitializeComponent();
        }
      
    }
}
