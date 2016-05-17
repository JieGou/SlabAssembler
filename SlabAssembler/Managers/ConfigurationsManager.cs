using System.IO;
using Urbbox.SlabAssembler.Core;
using System.Collections.Generic;
using System.Windows;
using Urbbox.SlabAssembler.Core.Variations;
using System.Linq;
using Urbbox.SlabAssembler.Repositories;
using System;
using System.Collections.Specialized;

namespace Urbbox.SlabAssembler.Managers
{
    public class ConfigurationsManager : IPartRepository, IAlgorythimRepository
    {
        public XmlModelDatabase<ConfigurationData> DataManager;

        protected ConfigurationData DefaultConfiguration;

        private ConfigurationData _config;
        protected ConfigurationData Config {
            get { return _config ?? (_config = DataManager.LoadData()); }
            set { _config = value; }
        }

        public ConfigurationsManager(string configurationFile, string defaults)
        {
            if (!File.Exists(configurationFile))
                File.Copy(defaults, DataManager.FilePathName);

            var defaultManager = new XmlModelDatabase<ConfigurationData>(defaults);
            var mainManager = new XmlModelDatabase<ConfigurationData>(configurationFile);
            DataManager = mainManager;
            DefaultConfiguration = defaultManager.LoadData();
        }

        protected void SaveConfig()
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
            return Config.Parts;
        }

        public IEnumerable<Part> GetPartsByType(UsageType usage)
        {
            return Config.Parts.Where(p => p.UsageType == usage);
        }

        public IEnumerable<Part> GetPartsByModulaton(int modulation)
        {
            return Config.Parts.Where(p => p.Modulation == modulation);
        }

        public Part GetPart(Guid id)
        {
            return GetParts()
                .First(p => p.Id == id);
        }

        public Guid SavePart(Part part)
        {
            if (GetPart(part.Id) != null) RemovePart(part.Id);
            
            Config.Parts.Add(part);
            DataManager.SaveData(Config);
            return part.Id;
        }

        public void RemovePart(Guid id)
        {
            Config.Parts.Remove(GetPart(id));
            DataManager.SaveData(Config);
        }

        public void ResetParts()
        {
            Config.Parts.Clear();
            Config.Parts.AddRange(DefaultConfiguration.Parts);
            DataManager.SaveData(Config);
        }

        public IObservable<NotifyCollectionChangedEventArgs> GetPartsObservable()
        {
            return Config.Parts.Changed;
        }

        public Part GetNextSmaller(Part currentPart, UsageType necessaryUsageType)
        {
            return GetPartsByModulaton(currentPart.Modulation)
                .Where(p => p.UsageType == necessaryUsageType)
                .OrderByDescending(p => p.Width)
                .First(p => p.Width < currentPart.Width);
        }

        public Part GetRespectiveOfType(Part actual, UsageType expectedType)
        {
            return GetPartsByType(expectedType)
                .First(p => Math.Abs(p.Width - actual.Width) < 0.1f);
        }

        public AssemblyOptions GetAssemblyOptions()
        {
            return Config.Options;
        }

        public void SetAssemblyOptions(AssemblyOptions options)
        {
            Config.Options = options;
            DataManager.SaveData(Config);
        }

        public void ResetAssemblyOptions()
        {
            Config.Options = DefaultConfiguration.Options;
            DataManager.SaveData(Config);
        }

        public void SaveOptions()
        {
            DataManager.SaveData(Config);
        }

        public void ResetOptions()
        {
            Config.Options = DefaultConfiguration.Options;
            DataManager.SaveData(Config);
        }
    }
}
