using Urbbox.SlabAssembler.Core.Strategies;

namespace Urbbox.SlabAssembler.Core
{
    public abstract class Builder
    {
        protected IStrategy CurrentStrategy { get; private set; }

        public virtual void Start()
        {
            while ((CurrentStrategy = NextStrategy) != null)
            {
                CurrentStrategy.Run();
            }
        }

        protected abstract IStrategy NextStrategy { get; }
    }
}
