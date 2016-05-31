using System.Collections.Generic;
using ReactiveUI;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    public interface IPartRepository : IRepository<Part>, IOperableByTransaction<Part>
    {
        IReactiveCommand PartsChanged { get; }

        IEnumerable<Part> GetByType(UsageType usage);
        IEnumerable<Part> GetByModulaton(int modulation);
        Part GetNextSmaller(Part currentPart, UsageType necessaryUsageType);
        Part GetRespectiveOfType(Part part, UsageType usage, float tolerance);
        void ResetParts();
    }

}
