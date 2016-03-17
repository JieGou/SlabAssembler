using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabEspecifications
    {
        //Caixas e Fôrmas
        public EspecificationsViewModel PartsEspecifications { get; set; }
        public AlgorythimViewModel AlgorythimEspecifications { get; set; }

        public Orientation? Orientation { get; set; }
        public ObjectId Outline { get; set; } = ObjectId.Null;
    }
}
