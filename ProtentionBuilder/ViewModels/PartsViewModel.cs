using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;
using Urbbox.AutoCAD.ProtentionBuilder.Views;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class PartsViewModel : ViewModelBase
    {
        public ObservableCollection<Part> Parts { get; set; }
        public ICommand CreatePartCommand { get; set; }
        public ICommand EditSelectedPartCommand { get; set; }
        public ICommand DeleteSelectedPartCommand { get; set; }

        private Part _selectedPart;
        public Part SelectedPart {
            get { return _selectedPart; }
            set { _selectedPart = value; OnPropertyChanged(); }
        }

        ConfigurationsManager _manager;

        public PartsViewModel(ConfigurationsManager configurationsManager)
        {
            Parts = new ObservableCollection<Part>();
            CreatePartCommand = new RelayCommand(CreatePart);
            EditSelectedPartCommand = new RelayCommand(EditSelectedPart, CanEditSelectedPart);
            DeleteSelectedPartCommand = new RelayCommand(DeleteSelectedPart, CanDeleteSelectedPart);

            _manager = configurationsManager;
            _manager.DataLoaded += _configurationsManager_DataLoaded;
            PropertyChanged += (o, e) => CommandManager.InvalidateRequerySuggested();
        }

        private bool CanDeleteSelectedPart()
        {
            return SelectedPart != null;
        }

        private bool CanEditSelectedPart()
        {
            return SelectedPart != null;
        }

        private void _configurationsManager_DataLoaded(ConfigurationData data)
        {
            Parts.Clear();
            foreach (var part in data.Parts) Parts.Add(part);
        }

        private void OpenPartWindow(Part p)
        {
            var window = new PartWindow(p);
            window.Show();
        }

        public void CreatePart()
        {
            OpenPartWindow(new Part());   
        }

        public void EditSelectedPart()
        {
            OpenPartWindow(SelectedPart);
        }

        public void DeleteSelectedPart()
        {
            _manager.DeletePart(SelectedPart.GetHashCode());
        }

    }
}
