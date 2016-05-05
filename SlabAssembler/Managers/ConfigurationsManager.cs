using System.IO;
using System.Xml.Serialization;
using Urbbox.SlabAssembler.Core;
using System.Collections.Generic;
using System.Windows;
using Urbbox.SlabAssembler.Core.Variations;
using System.Linq;
using Urbbox.SlabAssembler.Repositories;
using System;

namespace Urbbox.SlabAssembler.Managers
{
    public class ConfigurationsManager : IPartRepository
    {
        public XmlModelDatabase<ConfigurationData> DataManager;

        private readonly string _defaults;

        private ConfigurationData _config;
        public ConfigurationData Config {
            get
            {
                if (_config == null) _config = DataManager.LoadData();
                return _config;
            }
            protected set { _config = value; }
        }

        public ConfigurationsManager(string configurationFile, string defaults)
        {
            _defaults = defaults;
            _config = new ConfigurationData();
            DataManager = new XmlModelDatabase<ConfigurationData>(configurationFile);

            if (!File.Exists(configurationFile))
                File.Copy(_defaults, DataManager.FilePathName);
        }

        public void SaveConfig()
        {
            DataManager.SaveData(Config);
        }

        public void ResetDefaults()
        {
            var result = MessageBox.Show(
                "Deseja realmente resetar as configurações?",
                "Resetar Permanentemente",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                File.Delete(DataManager.FilePathName);
        }

        public IEnumerable<Part> GetParts()
        {
            return _config.Parts;
        }

        public IEnumerable<Part> GetPartsByType(UsageType usage)
        {
            return _config.Parts.Where(p => p.UsageType == usage);
        }

        public IEnumerable<Part> GetPartsByModulaton(int modulation)
        {
            return _config.Parts.Where(p => p.Modulation == modulation);
        }

        public Part GetPart(Guid id)
        {
            return GetParts()
                .Where(p => p.Id == id)
                .First();
        }

        public Guid AddPart(Part part)
        {
            part.Id = Guid.NewGuid();
            _config.Parts.Add(part);
            DataManager.SaveData(_config);
            return part.Id;
        }

        public void RemovePart(Guid id)
        {
            _config.Parts.Remove(GetPart(id));
            DataManager.SaveData(_config);
        }

        public Part GetNextSmaller(Part currentPart, UsageType necessaryUsageType)
        {
            return GetPartsByModulaton(currentPart.Modulation)
                .Where(p => p.UsageType == necessaryUsageType)
                .OrderByDescending(p => p.Width)
                .Where(p => p.Width < currentPart.Width)
                .First();
        }

        public Part GetRespectiveOfType(Part actual, UsageType expectedType)
        {
            return GetPartsByType(expectedType)
                .Where(p => p.Width == actual.Width)
                .First();
        }
    }
}
