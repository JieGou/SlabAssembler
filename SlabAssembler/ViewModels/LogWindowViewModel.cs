using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.ViewModels
{
    class LogWindowViewModel : ModelBase
    {
        private string _logMessage;
        public string LogMessage {
            get { return _logMessage; }
            set { _logMessage = value; OnPropertyChanged(); }
        }

        private string _resultsMessage;
        public string ResultsMessage {
            get { return _resultsMessage; }
            set { _resultsMessage = value; OnPropertyChanged(); }
        }

        public LogWindowViewModel()
        {
            _resultsMessage = "Analisando...";
            _logMessage = String.Empty;
        }
    }
}
