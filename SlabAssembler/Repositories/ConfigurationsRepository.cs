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
        public XmlModelDatabase<ConfigurationData> DataManager;

        private readonly string _defaults;

        public ConfigurationsRepository(string configurationFile, string defaults)
        {
            _defaults = defaults;
            Data = new ConfigurationData();
            DataManager = new XmlModelDatabase<ConfigurationData>(configurationFile);
        }

        public void LoadData()
        {
            if (!File.Exists(DataManager.File)) CreateConfigurationFile();

            Data = DataManager.LoadData();
            PartsChanged?.Invoke(Data.Parts);
            DataLoaded?.Invoke(Data);
        }

        public void SaveData()
        {
            DataManager.SaveData(Data);
        }

        public void SavePart(Part part)
        {
            for (int i = 0; i < Data.Parts.Count; i++)
            {
                if (Data.Parts[i].ReferenceName == part.ReferenceName)
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
                File.Copy(_defaults, DataManager.File);
            } catch (IOException) { }
        }

        public bool DeletePart(string referenceName)
        {
            foreach (Part p in Data.Parts)
            {
                if (p.ReferenceName == referenceName)
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
                File.Delete(DataManager.File);
                LoadData();
                PartsChanged?.Invoke(Data.Parts);
            }
            catch (IOException) { }
        }

        public Part GetNextSmallerPart(Part part, UsageType? usage = null, int modulation = 0)
        {
            if (usage == null) usage = part.UsageType;
            if (modulation == 0) modulation = part.Modulation;

            foreach (var p in Data.Parts.Where(p => p.UsageType == usage && p.Modulation == part.Modulation).OrderByDescending(p => p.Width))
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
