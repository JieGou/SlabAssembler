using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.Repositories.Core
{
    public interface IOperableByTransaction<in TElement> where TElement : class
    {
        ITransaction<TElement> CurrentTransaction { get; }
    }
}
