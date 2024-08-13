using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace R2O
{
    [Transaction(TransactionMode.ReadOnly)]
    class Column : IExternalCommand
    {
        // --- IExternalCommand Beginning ---
        public readonly double unit_um = 304.8 * 1000;  // Revit Internal Unit: feet, 1 feet = 304.8 mm = 304.8 * 1000 um
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // --- Execute  Beginning ---
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.IncludeNonVisibleObjects = false;
            options.ComputeReferences = false;
            // ---------- Main Program Beginning ----------
            // --- Grid ---
            FilteredElementCollector collector_Grid = new FilteredElementCollector(doc);
            collector_Grid.OfCategory(BuiltInCategory.OST_Grids).OfClass(typeof(Grid));
            var grid_list_X = new List<long>();
            var grid_list_Y = new List<long>();
            foreach (var item in collector_Grid)
            {
                XYZ start_point = ((Grid)item).Curve.GetEndPoint(0) * unit_um;
                XYZ end_point = ((Grid)item).Curve.GetEndPoint(1) * unit_um;
                if (start_point.X - end_point.X < 0.001 && start_point.X - end_point.X > -0.001)
                {
                    long grid_X = Convert.ToInt64(start_point.X);
                    grid_list_X.Add(grid_X);
                }
                else if (start_point.Y - end_point.Y < 0.001 && start_point.Y - end_point.Y > -0.001)
                {
                    long grid_Y = Convert.ToInt64(start_point.Y);
                    grid_list_Y.Add(grid_Y);
                }
                else
                {
                    TaskDialog.Show("Node", "轴网不是水平或垂直的");
                }
            }
            // --- Level ---
            FilteredElementCollector collector_Level = new FilteredElementCollector(doc);
            collector_Level.OfCategory(BuiltInCategory.OST_Levels).OfClass(typeof(Level));
            var level_list_Z = new List<long>();
            foreach (var item in collector_Level)
            {
                double elevation = ((Level)item).Elevation * unit_um;
                long level_Z = Convert.ToInt64(elevation);
                level_list_Z.Add(level_Z);
            }
            // --- Write info to Node.tcl file ---
            string path = @"D:\Study\Serious\Program\Revit\r2o\Column.tcl";           
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                AddLine.AddTextLine(fs, "# unit: m");
                FilteredElementCollector collector_Beam = new FilteredElementCollector(doc);
                collector_Beam.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilyInstance));
                int num = 0;
                foreach (var item in collector_Beam)
                {
                    FamilyInstance beam = (FamilyInstance)item;
                    double length = beam.LookupParameter("长度").AsDouble() * unit_um;
                    double width = beam.Symbol.LookupParameter("b").AsDouble() * unit_um;
                    double height = beam.Symbol.LookupParameter("h").AsDouble() * unit_um;
                    ElementId material_id = beam.StructuralMaterialId;
                    string material_name = doc.GetElement(material_id).Name;
                    string rebar_cover = beam.LookupParameter("钢筋保护层 - 底面").AsString();
                    Location loc = beam.Location;
                    LocationCurve loc_curve = loc as LocationCurve;
                    Line curve = (Line)loc_curve.Curve;
                    XYZ origin = curve.Origin * unit_um;
                    string Direction = curve.Direction.ToString();
                    // double buttom_elevation = beam.LookupParameter("底部高程").AsDouble() * unit;
                    // double top_elevation = beam.LookupParameter("顶部高程").AsDouble() * unit;
                    // double elevation = (buttom_elevation + top_elevation) / 2; 
                    // BoundingBoxXYZ bounding_box = beam.get_BoundingBox(null);
                    // XYZ start_point = bounding_box.Min * unit;
                    // XYZ end_point = bounding_box.Max * unit;
                    // double x_length = end_point.X - start_point.X;
                    // double y_length = end_point.Y - start_point.Y;
                    // double z_length = end_point.Z - start_point.Z;      
                    AddLine.AddTextLine(fs, "Beam ID: " + item.Id);
                    AddLine.AddTextLine(fs, "Beam Name: " + item.Name);
                    AddLine.AddTextLine(fs, "Beam Type: " + beam.Symbol.FamilyName);
                    AddLine.AddTextLine(fs, "StartPoint.X: " + origin.X);
                    AddLine.AddTextLine(fs, "StartPoint.Y: " + origin.Y);
                    AddLine.AddTextLine(fs, "StartPoint.Z: " + origin.Z);
                    AddLine.AddTextLine(fs, "Direction: " + Direction);
                    // AddTextLine(fs, "x_length: " + x_length);
                    // AddTextLine(fs, "y_length: " + y_length);
                    // AddTextLine(fs, "z_length: " + z_length);
                    AddLine.AddTextLine(fs, "Length: " + length);
                    AddLine.AddTextLine(fs, "Width: " + width);
                    AddLine.AddTextLine(fs, "Height: " + height);
                    AddLine.AddTextLine(fs, "Material: " + material_name);
                    AddLine.AddTextLine(fs, "RebarCover: " + rebar_cover);
                    // AddTextLine(fs, "Elevation: " + elevation);
                    num = num + 1;

                }
                fs.Close();
                string show_info = "Column文件写入完毕，文件路径：" + path;
                TaskDialog.Show("Column", show_info);
            }
            // ---------- Main Program End ---------- 
            return Result.Succeeded;

        }
    }
}

