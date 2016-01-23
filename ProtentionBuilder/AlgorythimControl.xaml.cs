using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;

namespace Urbbox.AutoCAD.ProtentionBuilder
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
