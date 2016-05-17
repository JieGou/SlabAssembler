using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for PartWindow.xaml
    /// </summary>
    public partial class PartWindow : Window
    {
        public Part ViewModel { get; protected set; }
        private IPartRepository _partRepository;

        public PartWindow(IPartRepository repo, Part part)
        {
            _partRepository = repo;
            ViewModel = part;
            ViewModel.Save.Subscribe(x => {
                _partRepository.SavePart(ViewModel);
                Close();
            });

            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
