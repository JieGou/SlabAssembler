using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urbbox.AutoCAD.ProtentionBuilder.Database;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    class AlgorythimViewModel : ViewModel
    {
        public ObservableCollection<Part> StartLpList;

        public AlgorythimViewModel(ConfigurationsManager configurations)
        {
            var parts = configurations.GetParts();

            StartLpList = new ObservableCollection<Part>(parts.Where(p => p.UsageType == UsageType.StartLp));
        }
    }
}
