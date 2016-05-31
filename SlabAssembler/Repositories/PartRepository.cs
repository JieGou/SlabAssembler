using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using Urbbox.SlabAssembler.Core;
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
        public IReactiveCommand PartsChanged { get; }

        public PartRepository() : base(Properties.Resources.PartsDataFile)
        {
            PartsChanged = ReactiveCommand.Create();
        }

        public IEnumerable<Part> GetByType(UsageType usage)
        {
            return GetAll().WhereType(usage);
        }

        public override void Add(Part entity)
        {
            base.Add(entity);
            PartsChanged.Execute(null);
        }

        public override void Remove(Part entity)
        {
            base.Remove(entity);
            PartsChanged.Execute(null);
        }

        public IEnumerable<Part> GetByModulaton(int modulation)
        {
            return GetAll().WhereModulation(modulation);
        }

        public Part GetNextSmaller(Part currentPart, UsageType necessaryUsageType)
        {
            return GetByModulaton(currentPart.Modulation)
                .OrderByDescending(p => p.Width)
                .First(p => p.Width < currentPart.Width);
        }

        public Part GetRespectiveOfType(Part currentPart, UsageType usage, float tolerance = 5)
        {
            return GetByModulaton(currentPart.Modulation)
                .WhereType(usage)
                .First(p => Math.Abs(p.Width - currentPart.Width) < tolerance);
        }

        public void ResetParts()
        {
            var defaultRepo = new PartRepository { RepositoryDataFile = Properties.Resources.DefaultsPartsDataFile };
            using (var t = StartTransaction())
            { 
                Clear();
                AddRange(defaultRepo.GetAll());
                t.Commit();
            }
        }
    }
}
