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
    public class ConfigurationManager
    {
        private JObject _dataJObject;

        public ConfigurationManager(string configurationFile)
        {
            LoadData(configurationFile);
        }

        private void LoadData(string configurationFile)
        {
            using (var file = File.OpenText(configurationFile))
            using (var reader = new JsonTextReader(file))
            {
                _dataJObject = (JObject) JToken.ReadFrom(reader);
            }
        }


        public IEnumerable<Part> GetParts()
        {
            try
            {
                var parts = _dataJObject["parts"] as JArray;
                return parts?.Select(p => JsonConvert.DeserializeObject<Part>(p.ToString())).ToList();
            }
            catch (Exception)
            {
                InvalidJson();
                return new List<Part>();
            }
        }

        private void InvalidJson()
        {
            MessageBox.Show("O arquivo de configurações está inválido.", "Configurações", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
