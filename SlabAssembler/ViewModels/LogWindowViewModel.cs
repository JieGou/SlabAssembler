using System;
using ReactiveUI;

namespace Urbbox.SlabAssembler.ViewModels
{
    class LogWindowViewModel : ReactiveObject
    {
        private string _logMessage;
        public string LogMessage {
            get { return _logMessage; }
            set { this.RaiseAndSetIfChanged(ref _logMessage, value); }
        }

        private string _resultsMessage;
        public string ResultsMessage {
            get { return _resultsMessage; }
            set { this.RaiseAndSetIfChanged(ref _resultsMessage, value); }
        }

        public LogWindowViewModel()
        {
            _resultsMessage = "Analisando...";
            _logMessage = String.Empty;
        }
    }
}
