using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Urbbox.AutoCAD.ProtentionBuilder.Manufacture
{
    public class Modulation
    {
        public int Size { get; set; }
        public List<Part> Parts { get; set; }
        public string Value => this.ToString();

        public Modulation(int size)
        {
            Size = size;
        }

        public override string ToString()
        {
            return Size.ToString();
        }
    }
}
