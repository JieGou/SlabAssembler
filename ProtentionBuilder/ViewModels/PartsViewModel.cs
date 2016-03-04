using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Urbbox.AutoCAD.ProtentionBuilder.Building;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Commands;
using Urbbox.AutoCAD.ProtentionBuilder.Views;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class PartsViewModel : ModelBase
    {
        public ObservableCollection<Part> Parts { get; set; }
        public ICommand CreatePartCommand { get; set; }
        public ICommand EditSelectedPartCommand { get; set; }
        public ICommand DeleteSelectedPartCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand AnalyzeCommand { get; set; }

        private Part _selectedPart;
        public Part SelectedPart {
            get { return _selectedPart; }
            set { _selectedPart = value; OnPropertyChanged(); }
        }

        private ConfigurationsManager _manager;
        private AutoCadManager _acad;

        public PartsViewModel(ConfigurationsManager configurationsManager, AutoCadManager acad)
        {
            _manager = configurationsManager;
            _acad = acad;
            Parts = new ObservableCollection<Part>();
            CreatePartCommand = new RelayCommand(() => OpenPartWindow(new Part()));
            EditSelectedPartCommand = new RelayCommand(() => OpenPartWindow(SelectedPart), HasSelectedPart);
            DeleteSelectedPartCommand = new RelayCommand(() => _manager.DeletePart(SelectedPart.GetHashCode()), HasSelectedPart);
            ResetCommand = new RelayCommand(() => _manager.ResetDefaults());
            AnalyzeCommand = new RelayCommand(ExecuteAnalyzeCommand, () => Parts.Count > 0);

            _manager.DataLoaded += _manager_DataLoaded;
            PropertyChanged += (o, e) => CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteAnalyzeCommand()
        {
            var logWindow = new LogWindow();
            logWindow.Show();

            string log = "";
            foreach (var part in Parts)
            {
                bool e = _acad.CheckBlockExists(part.ReferenceName);
                bool l = _acad.CheckLayerExists(part.Layer);
                log += String.Format("{0}\t-> {1}{2}\n", part.ReferenceName, (e)? "OK": "NAO ENCONTRADO", (l)? "":", SEM CAMADA");
                logWindow.SetLogMessage(log);
            }

            logWindow.SetResultTitle(String.Format("{0} peças analizadas.", Parts.Count));
        }

        private bool HasSelectedPart()
        {
            return SelectedPart != null;
        }

        private void _manager_DataLoaded(ConfigurationData data)
        {
            Parts.Clear();
            foreach (var part in data.Parts) Parts.Add(part);
        }

        private void OpenPartWindow(Part p)
        {
            var window = new PartWindow(_manager, p);
            window.Show();
        }

    }
}
