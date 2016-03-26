using System;
using System.Collections.Generic;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuildingResult : IEqualityComparer<Part>
    {
        public Dictionary<Part, int> PartsCountTable { get; set; }

        public SlabBuildingResult()
        {
            PartsCountTable = new Dictionary<Part, int>(this);
        }

        public void CountNewPart(Part p)
        {
            if (PartsCountTable.ContainsKey(p))
                PartsCountTable[p] += 1;
            else
                PartsCountTable[p] = 1;
        }

        bool IEqualityComparer<Part>.Equals(Part x, Part y)
        {
            return x.Id == y.Id;
        }

        int IEqualityComparer<Part>.GetHashCode(Part obj)
        {
            return obj.Id;
        }
    }
}