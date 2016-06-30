using System.Linq;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    public sealed class AlgorythimRepository : XMLRepository<AssemblyOptions>, IAlgorythimRepository
    {
        public AlgorythimRepository() : base(Properties.Resources.OptionsDataFile)
        {
            if (!GetAll().Any())
                Reset();
                if (!GetAll().Any()) Add(new AssemblyOptions());
        }

        public AssemblyOptions Get()
        {
            return Get(0);
        }

        public void SaveChanges()
        {
            using (var transaction = StartTransaction())
                transaction.Commit();
        }

        public void Reset()
        {
            var defaultRepo = new XMLRepository<AssemblyOptions>(Properties.Resources.DefaultOptionsDataFile);

            Clear();
            using (var t = StartTransaction())
            {
                if (!defaultRepo.GetAll().Any())
                    Add(new AssemblyOptions());
                else
                    AddRange(defaultRepo.GetAll());
                t.Commit();
            }
        }
    }
}
