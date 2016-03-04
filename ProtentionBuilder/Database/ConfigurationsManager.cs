using System;
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
        private readonly string _defaults;

        public ConfigurationsManager(string configurationFile, string defaults)
        {
            _file = configurationFile;
            _defaults = defaults;
            Data = new ConfigurationData();
        }

        public void LoadData()
        {
            if (!File.Exists(_file)) CreateConfigurationFile();

            try { 
                var deserializer = new XmlSerializer(typeof(ConfigurationData));
                using (TextReader reader = new StreamReader(_file))
                    Data = (ConfigurationData) deserializer.Deserialize(reader);
            }
            catch (IOException) { }
            catch (StackOverflowException) { }
            catch (UnauthorizedAccessException) { }

            DataLoaded?.Invoke(Data);
        }

        public void SavePart(Part part)
        {
            int id = part.GetHashCode();
            for (int i = 0; i < Data.Parts.Count; i++)
            {
                if (Data.Parts[i].GetHashCode() == id)
                {
                    Data.Parts[i] = part;
                    SaveData();
                    return;
                }
            }

            Data.Parts.Add(part);
            SaveData();
            return;
        }

        private void CreateConfigurationFile()
        {
            try {
                File.Copy(_defaults, _file);
            } catch (IOException) { }
              catch (UnauthorizedAccessException) { }
        }

        public void SaveData()
        {
            try { 
                var serializer = new XmlSerializer(typeof(ConfigurationData));
                using (TextWriter writer = new StreamWriter(_file))
                    serializer.Serialize(writer, Data);
            }
            catch (IOException) { }
            catch (StackOverflowException) { }
            catch (UnauthorizedAccessException) { }

            DataLoaded?.Invoke(Data);
        }

        public bool DeletePart(int id)
        {
            foreach (Part p in Data.Parts)
            {
                if (p.GetHashCode() == id)
                {
                    if (Data.Parts.Remove(p)) SaveData();
                    return true;
                }
            }

            return false;
        }

        public void ResetDefaults()
        {
            try
            {
                File.Delete(_file);
                LoadData();
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
