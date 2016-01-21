using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Urbbox.AutoCAD.ProtentionBuilder.Manufacture
{
    public class Modulation
    {
        public int Size { get; set; }
        [JsonIgnore]
        public string Value {
            get { return Size.ToString(); }
            set { Size = int.Parse(value); }
        }

        public Modulation(int size)
        {
            Size = size;
        }
    }
}
