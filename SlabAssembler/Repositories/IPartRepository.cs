using System.Collections.Generic;
using ReactiveUI;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    public interface IPartRepository : IRepository<Part>, IOperableByTransaction<Part>
    {
        ReactiveCommand<object> PartsChanged { get; }

        Part GetById(string id);
        IEnumerable<Part> GetByType(UsageType usage);
        IEnumerable<Part> GetByModulaton(int modulation);
        Part GetNextSmaller(Part currentPart, UsageType necessaryUsageType);
        Part GetRespectiveOfType(Part part, UsageType usage);
        void ResetParts();
    }

}
