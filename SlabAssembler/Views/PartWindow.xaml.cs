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
        private ConfigurationsRepository _manager;

        public PartWindow(ConfigurationsRepository configurationsManager, Part part)
        {
            _manager = configurationsManager;
            ViewModel = part;
            ViewModel.Save.Subscribe(x => {
                configurationsManager.SavePart(ViewModel);
                Close();
            });

            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
