using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Urbbox.AutoCAD.ProtentionBuilder.Building;

namespace Urbbox.AutoCAD.ProtentionBuilder.Database
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

        public ConfigurationData()
        {
            Parts = new List<Part>();
            OutlineDistance = 0;
            DistanceBetweenLp = 0;
            DistanceBetweenLpAndLd = 0;
            UseLds = false;
            UseEndLp = false;
            UseStartLp = false;
        }
    }

    public delegate void DataLoadedEventHandler(ConfigurationData data);

    public class ConfigurationsManager
    {
        public ConfigurationData Data { get; set; }
        public event DataLoadedEventHandler DataLoaded;
        private readonly string _file;

        public ConfigurationsManager(string configurationFile)
        {
            _file = configurationFile;
            Data = new ConfigurationData();
        }

        public void LoadData()
        {
            var deserializer = new XmlSerializer(typeof(ConfigurationData));
            TextReader reader = new StreamReader(_file);
            Data = (ConfigurationData) deserializer.Deserialize(reader);
            reader.Close();

            DataLoaded?.Invoke(Data);
        }

        public void SaveData()
        {
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            using (TextWriter writer = new StreamWriter(_file))
            {
                serializer.Serialize(writer, Data);
            }

            DataLoaded?.Invoke(Data);
        }

    }
}
