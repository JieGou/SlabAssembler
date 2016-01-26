using System.Windows.Controls;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
{
    /// <summary>
    /// Interaction logic for AlgorythimControl.xaml
    /// </summary>
    public partial class AlgorythimControl : UserControl
    {
        public AlgorythimViewModel ViewModel { get; private set; }

        public AlgorythimControl(EspecificationsControl especificationsControl,  ConfigurationsManager configurations)
        {
            InitializeComponent();
            ViewModel = new AlgorythimViewModel(especificationsControl.ViewModel, configurations);
            DataContext = ViewModel;
        }

    }
}
