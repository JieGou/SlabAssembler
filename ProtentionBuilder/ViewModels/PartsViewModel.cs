using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;
using Urbbox.AutoCAD.ProtentionBuilder.Views;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class PartsViewModel : ViewModelBase
    {
        private class PartCommand : BaseCommand
        {
            private Part _part;

            public PartCommand(Part part)
            {
                _part = part;
            }

            public override void Execute(object parameter)
            {
                var view = new PartView(_part);
                view.Show();
            }
        }

        public ObservableCollection<Part> Parts { get; set; }
        public ICommand CreatePartCommand { get; set; }

        public PartsViewModel(ConfigurationsManager configurationsManager)
        {
            Parts = new ObservableCollection<Part>();
            CreatePartCommand = new PartCommand(new Part());
            configurationsManager.DataLoaded += _configurationsManager_DataLoaded;
        }

        private void _configurationsManager_DataLoaded(ConfigurationData data)
        {
            Parts.Clear();
            foreach (var part in data.Parts) {
                part.EditCommand = new PartCommand(part);
                Parts.Add(part);
            }
        }
    }
}
