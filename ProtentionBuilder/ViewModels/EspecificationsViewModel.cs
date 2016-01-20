using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Urbbox.AutoCAD.ProtentionBuilder.Annotations;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class EspecificationsViewModel : ViewModel
    {
        private Modulation _selectedModulation;
        public Modulation SelectedModulation
        {
            get { return _selectedModulation; }
            set { _selectedModulation = value; OnPropertyChanged(); }
        }

        public List<Modulation> Modulations { get; }

        public EspecificationsViewModel()
        {
            Modulations = new List<Modulation>()
            {
                new Modulation(61),
                new Modulation(80)
            };
            SelectedModulation = Modulations.First();
        }
    }
}
