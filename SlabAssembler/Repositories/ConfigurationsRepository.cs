using System;
using System.IO;
using System.Xml.Serialization;
using Urbbox.SlabAssembler.Core;
using System.Collections.Generic;
using System.Windows;
using Urbbox.SlabAssembler.Core.Variations;
using System.Linq;

namespace Urbbox.SlabAssembler.Repositories
{
    [XmlRoot(nameof(ConfigurationData), IsNullable = false)]
    public class ConfigurationData
    {
        [XmlArray(IsNullable = true)]
        [XmlArrayItem(nameof(Part), typeof(Part))]
        public List<Part> Parts { get; private set; }
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
    public delegate void PartsChangedEventHandler(List<Part> parts);

    public class ConfigurationsRepository
    {
        public ConfigurationData Data { get; set; }
        public event DataLoadedEventHandler DataLoaded;
        public event PartsChangedEventHandler PartsChanged;

        private readonly string _file;
        private readonly string _defaults;

        public ConfigurationsRepository(string configurationFile, string defaults)
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

            PartsChanged?.Invoke(Data.Parts);
            DataLoaded?.Invoke(Data);
        }

        public void SavePart(Part part)
        {
            for (int i = 0; i < Data.Parts.Count; i++)
            {
                if (Data.Parts[i].Id == part.Id)
                {
                    Data.Parts[i] = part;
                    SaveData();
                    return;
                }
            }

            Data.Parts.Add(part);
            SaveData();
            PartsChanged?.Invoke(Data.Parts);
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
        }

        public bool DeletePart(int id)
        {
            foreach (Part p in Data.Parts)
            {
                if (p.Id == id)
                {
                    if (Data.Parts.Remove(p)) SaveData();
                    PartsChanged?.Invoke(Data.Parts);
                    return true;
                }
            }

            return false;
        }

        public void ResetDefaults()
        {
            var result = MessageBox.Show(
                "Deseja realmente resetar as configurações?",
                "RESETAR PERMANENTEMENTE",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                File.Delete(_file);
                LoadData();
                PartsChanged?.Invoke(Data.Parts);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public Part GetNextSmallerPart(Part part, UsageType? usage = null)
        {
            if (usage == null) usage = part.UsageType;
            foreach (var p in Data.Parts.Where(p => p.UsageType == usage).OrderByDescending(p => p.Width))
            {
                if (p.Width < part.Width)
                    return p;
            }

            return null;
        }

        public Part GetRespectiveOfUsageType(Part part, UsageType usage)
        {
            if (part == null) return null;

            foreach (var p in Data.Parts.Where(p => p.UsageType == usage))
                if (p.Width == part.Width) return p;

            return GetNextSmallerPart(part, usage);
        }
    }
}
