using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    public static class PartsCollectionExtensions
    {
        public static IEnumerable<Part> WhereType(this IEnumerable<Part> parts, UsageType usage)
        {
            return parts.Where(p => p.UsageType == usage);
        }

        public static IEnumerable<Part> WhereModulation(this IEnumerable<Part> parts, int modulation)
        {
            return parts.Where(p => p.Modulation == modulation);
        }
    }

    public class PartRepository : XMLRepository<Part>, IPartRepository
    {
        public ReactiveCommand<object> PartsChanged { get; }

        public PartRepository() : base(Properties.Resources.PartsDataFile)
        {
            PartsChanged = ReactiveCommand.Create();
            PartsChanged.Execute(null);

            if (GetAll().Count() == 0)
                ResetParts();
        }

        public Part GetById(string id)
        {
            return Find(p => p.Id == id).FirstOrDefault();
        }

        public IEnumerable<Part> GetByType(UsageType usage)
        {
            return GetAll().WhereType(usage);
        }

        public override void Add(Part entity)
        {
            base.Add(entity);
            PartsChanged?.Execute(null);
        }

        public override void Remove(Part entity)
        {
            base.Remove(entity);
            PartsChanged?.Execute(null);
        }

        public IEnumerable<Part> GetByModulaton(int modulation)
        {
            return GetAll().WhereModulation(modulation);
        }

        public Part GetNextSmaller(Part currentPart, UsageType necessaryUsageType)
        {
            return GetByModulaton(currentPart.Modulation)
                .WhereType(necessaryUsageType)
                .OrderByDescending(p => p.Width)
                .FirstOrDefault(p => p.Width < currentPart.Width);
        }

        public Part GetRespectiveOfType(Part currentPart, UsageType usage)
        {
            return GetByModulaton(currentPart.Modulation)
                .WhereType(usage)
                .FirstOrDefault(p => p.Width == currentPart.Width);
        }

        public void ResetParts()
        {
            var defaultRepo = new XMLRepository<Part>(Properties.Resources.DefaultsPartsDataFile);

            Clear();
            using (var t = StartTransaction())
            { 
                AddRange(defaultRepo.GetAll());
                t.Commit();
            }
        }
    }
}
