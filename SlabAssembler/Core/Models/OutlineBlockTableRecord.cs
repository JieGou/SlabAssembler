using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace Urbbox.SlabAssembler.Core.Models
{
    public class OutlineBlockTableRecord : BlockTableRecord
    {
        private Part _part;
        public Part Part
        {
            get { return _part; }
            set
            {
                _part = value;

                Name = Part.OutlineReferenceName;
                Origin = Part.PivotPoint;
            }
        }

        public OutlineBlockTableRecord()
        {
            Units = UnitsValue.Centimeters;
        }

        public OutlineBlockTableRecord(Part part)
        {
            Part = part;
            Units = UnitsValue.Centimeters;
        }

        public void BuildObject(Transaction transaction, float border)
        {
            DrawOutsideBorders(border, transaction);
            DrawInsideBorders(transaction);
            DrawCrossedLines(border, transaction);
        }

        private void DrawCrossedLines(float border, Transaction tr)
        {
            foreach (var line in EntityGenerator.CreateCrossLines(Part.Dimensions, border))
            {
                using (line)
                {
                    AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                }
            }
        }

        private void DrawInsideBorders(Transaction tr)
        {
            using (var poly = EntityGenerator.CreateSquare(Part.Dimensions))
            {
                AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
            }
        }

        private void DrawOutsideBorders(float distance, Transaction tr)
        {
            using (var poly = EntityGenerator.CreateSquare(Part.Dimensions, distance))
            {
                AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
            }
        }
    }
}