using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;
using Urbbox.SlabAssembler.ViewModels.Commands;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for PartsControl.xaml
    /// </summary>
    public partial class PartsControl
    {
        public PartsControl(ConfigurationsRepository configurationsManager, AutoCadManager acad)
        {
            DataContext = new PartsViewModel(configurationsManager, acad);
            InitializeComponent();
        }
    }
}
