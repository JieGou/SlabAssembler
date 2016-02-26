using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
{
    /// <summary>
    /// Interaction logic for PartsControl.xaml
    /// </summary>
    public partial class PartsControl
    {

        public PartsControl(ConfigurationsManager configurationsManager)
        {
            DataContext = new PartsViewModel(configurationsManager);
            InitializeComponent();
        }

    }
}
