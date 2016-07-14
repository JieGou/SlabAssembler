using Urbbox.SlabAssembler.Core.Strategies;
using Urbbox.SlabAssembler.Core.Strategies.LD;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Core.Assemblers
{
    public abstract class VerticalSlabAssembler : SlabAssembler
    {
        public VerticalSlabAssembler(IPartRepository partRepository, SlabProperties properties)
            : base (partRepository, properties)
        {
        }

        protected override IStrategy NextStrategy
        {
            get
            {
                if (CurrentStrategy == null)
                    return new VerticalLDStrategy(Properties, PartRepository, Environment);
                else
                    return null;
            }
        }
    }
}
