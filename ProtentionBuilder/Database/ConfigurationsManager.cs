using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;

namespace Urbbox.AutoCAD.ProtentionBuilder.Database
{
    public class ConfigurationsManager
    {
        private JObject _configurationData;

        public ConfigurationsManager(string configurationFile)
        {
            LoadData(configurationFile);
        }

        private void LoadData(string configurationFile)
        {
            _configurationData = JObject.Parse(File.ReadAllText(configurationFile));
        }

        public List<Part> GetParts()
        {
            var parts = _configurationData["parts"] as JArray;
            return parts?.Select(p => new Part(p as JObject)).ToList<Part>() ?? new List<Part>();
        }

    }
}
