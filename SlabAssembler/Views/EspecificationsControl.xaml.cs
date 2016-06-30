using System.Collections.Generic;
using System.Windows.Controls;
using Urbbox.SlabAssembler.Managers;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for EspecificationsControl.xaml
    /// </summary>
    public partial class EspecificationsControl : UserControl
    {
        public EspecificationsViewModel ViewModel { get; protected set; }

        public EspecificationsControl(IPartRepository partRepository)
        {
            ViewModel = new EspecificationsViewModel(partRepository);
            var acad = new AutoCadManager();
            AcApplication.DocumentManager.DocumentActivationChanged += (e, a) => UpdateLayers(acad.GetLayers());
            UpdateLayers(acad.GetLayers());

            DataContext = ViewModel;
            InitializeComponent();
        }

        public void UpdateLayers(IEnumerable<string> layers)
        {
            foreach (var l in layers)
                ViewModel.Layers.Add(l);
        }

    }
}
