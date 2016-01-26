using System.Windows.Controls;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
{
    /// <summary>
    /// Interaction logic for EspecificationsControl.xaml
    /// </summary>
    public partial class EspecificationsControl : UserControl
    {
        public EspecificationsViewModel ViewModel { get; set; }


        public EspecificationsControl(ConfigurationsManager manager, AcManager ac)
        {
            InitializeComponent();
            ViewModel = new EspecificationsViewModel(manager, ac);
            DataContext = ViewModel;
        }

    }
}
