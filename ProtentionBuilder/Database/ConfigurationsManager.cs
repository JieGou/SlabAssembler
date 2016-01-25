using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Urbbox.AutoCAD.ProtentionBuilder.Manufacture;

namespace Urbbox.AutoCAD.ProtentionBuilder.Database
{
    public class ConfigurationsManager
    {
        [XmlRoot(nameof(ConfigurationData), IsNullable = false)]
        public class ConfigurationData
        {
            [XmlArray(IsNullable = true)]
            [XmlArrayItem(nameof(Part), typeof(Part))]
            public List<Part> Parts { get; set; }
            public float OutlineDistance { get; set; }
            public float DistanceBetweenLp { get; set; }
            public float DistanceBetweenLpAndLd { get; set; }
            public bool UseLds { get; set; }
            public bool UseEndLp { get; set; }
            public bool UseStartLp { get; set; }
        }

        public ConfigurationData Data { get; private set; }

        public ConfigurationsManager(string configurationFile)
        {
            LoadData(configurationFile);
        }

        private void LoadData(string configurationFile)
        {
            var deserializer = new XmlSerializer(typeof(ConfigurationData));
            TextReader reader = new StreamReader(configurationFile);
            Data = (ConfigurationData) deserializer.Deserialize(reader);
            reader.Close();
        }

        private void SaveData(ConfigurationData data, string configurationFile)
        {
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            using (TextWriter writer = new StreamWriter(configurationFile))
            {
                serializer.Serialize(writer, data);
            }
        }

    }
}
