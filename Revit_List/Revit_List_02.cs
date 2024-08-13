using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_List
{
    [Transaction(TransactionMode.ReadOnly)]
    class Revit_List_02 : IExternalCommand
    {
        // --- IExternalCommand Beginning ---
        public readonly double unit = 304.8;  // Revit Internal Unit: feet, 1 feet = 304.8 mm
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // --- Execute  Beginning ---
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.IncludeNonVisibleObjects = false;
            options.ComputeReferences = false;
            // ---------- Main Program Beginning ----------


            // Write Info to TXT
            string path = @"D:\Study\Serious\Program\Revit\r2o\Test.txt";
            //
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {

                // ---1.Grid---
                AddTextLine(fs, "---Grid---");
                FilteredElementCollector collector_Grid = new FilteredElementCollector(doc);
                collector_Grid.OfCategory(BuiltInCategory.OST_Grids).OfClass(typeof(Grid));
                int num = 0;
                foreach (var item in collector_Grid)
                {
                    AddTextLine(fs, "Grid ID: " + item.Id);
                    AddTextLine(fs, "Grid Name: " + item.Name);
                    XYZ start_point = ((Grid)item).Curve.GetEndPoint(0) * unit;
                    XYZ end_point = ((Grid)item).Curve.GetEndPoint(1) * unit;
                    double start_point_X = Math.Round(start_point.X, 2);
                    double start_point_Y = Math.Round(start_point.Y, 2);
                    double end_point_X = Math.Round(end_point.X, 2);
                    double end_point_Y = Math.Round(end_point.Y, 2);
                    if (start_point_X == end_point_X)
                    {
                        AddTextLine(fs, "StartPoint.X: " + start_point.X);
                    }
                    if (start_point_Y == end_point_Y)
                    {
                        AddTextLine(fs, "StartPoint.Y: " + start_point.Y);
                    }
                    num = num + 1;
                }
                string show_info = "轴网信息写入完毕，轴线数量：" + num.ToString();
                TaskDialog.Show("Revit List", show_info);

                ////// ---2.Level---
                ////AddTextLine(fs, "---Level---");
                ////FilteredElementCollector collector_Level = new FilteredElementCollector(doc);
                ////collector_Level.OfCategory(BuiltInCategory.OST_Levels).OfClass(typeof(Level));
                ////num = 0;
                ////foreach (var item in collector_Level)
                ////{
                ////    double elevation = ((Level)item).Elevation * unit;
                ////    AddTextLine(fs, "Level ID: " + item.Id);
                ////    AddTextLine(fs, "Level Name: " + item.Name);
                ////    AddTextLine(fs, "Elevation: " + elevation);
                ////    num = num + 1;
                ////}
                ////show_info = "标高信息写入完毕，标高数量：" + num.ToString();
                ////TaskDialog.Show("Revit List", show_info);

                // ---3.Beam---
                AddTextLine(fs, "---Beam---");
                FilteredElementCollector collector_Beam = new FilteredElementCollector(doc);
                collector_Beam.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilyInstance));
                num = 0;
                foreach (var item in collector_Beam)
                {
                    FamilyInstance beam = (FamilyInstance)item;
                    double length = beam.LookupParameter("长度").AsDouble() * unit;                   
                    Location loc = beam.Location;
                    LocationCurve loc_curve = loc as LocationCurve;
                    Line curve = (Line)loc_curve.Curve;
                    XYZ origin = curve.Origin * unit;
                    string Direction = curve.Direction.ToString();
                    double direction_X = Math.Round(curve.Direction.X, 2);
                    double direction_Y = Math.Round(curve.Direction.Y, 2);
                    if (direction_Y == 0)
                    {
                        double start_point_X = Math.Round(origin.X, 2);
                        double start_point_Y = Math.Round(origin.Y, 2);
                        double start_point_Z = Math.Round(origin.Z, 2);
                        double end_point_X = start_point_X + direction_X * length;
                        AddTextLine(fs, "Beam ID: " + item.Id);
                        AddTextLine(fs, "StartPoint.X: " + start_point_X);
                        AddTextLine(fs, "StartPoint.Y: " + start_point_Y);
                        AddTextLine(fs, "StartPoint.Z: " + start_point_Z);
                        AddTextLine(fs, "EndPoint.X: " + end_point_X);
                        AddTextLine(fs, "Direction: " + Direction);
                        AddTextLine(fs, "Length: " + length);
                    }
                    else if (direction_X == 0)
                    {
                        double start_point_X = Math.Round(origin.X, 2);
                        double start_point_Y = Math.Round(origin.Y, 2);
                        double start_point_Z = Math.Round(origin.Z, 2);
                        double end_point_Y = start_point_Y + direction_Y * length;
                        AddTextLine(fs, "Beam ID: " + item.Id);
                        AddTextLine(fs, "StartPoint.X: " + start_point_X);
                        AddTextLine(fs, "StartPoint.Y: " + start_point_Y);
                        AddTextLine(fs, "StartPoint.Z: " + start_point_Z);
                        AddTextLine(fs, "EndPoint.Y: " + end_point_Y);
                        AddTextLine(fs, "Direction: " + Direction);
                        AddTextLine(fs, "Length: " + length);
                    }

                        num = num + 1;

                }
                show_info = "梁信息写入完毕，梁数量：" + num.ToString();
                TaskDialog.Show("Revit List", show_info);
                // ---4.Column---
                AddTextLine(fs, "---Column---");
                FilteredElementCollector collector_Column = new FilteredElementCollector(doc);
                collector_Column.OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(FamilyInstance));
                num = 0;
                foreach (var item in collector_Column)
                {
                    FamilyInstance column = (FamilyInstance)item;
                    double length = column.LookupParameter("长度").AsDouble() * unit;
                    double width = column.Symbol.LookupParameter("b").AsDouble() * unit;
                    double height = column.Symbol.LookupParameter("h").AsDouble() * unit;
                    ElementId material_id = column.StructuralMaterialId;
                    string material_name = doc.GetElement(material_id).Name;
                    string rebar_cover = column.LookupParameter("钢筋保护层 - 底面").AsString();
                    Location loc = column.Location;
                    LocationPoint loc_point = loc as LocationPoint;
                    XYZ origin = loc_point.Point * unit;
                    AddTextLine(fs, "Column ID: " + item.Id);
                    AddTextLine(fs, "Column Name: " + item.Name);
                    AddTextLine(fs, "Column Type: " + column.Symbol.FamilyName);
                    AddTextLine(fs, "StartPoint.X: " + origin.X);
                    AddTextLine(fs, "StartPoint.Y: " + origin.Y);
                    AddTextLine(fs, "StartPoint.Z: " + origin.Z);
                    AddTextLine(fs, "Length: " + length);
                    AddTextLine(fs, "Width: " + width);
                    AddTextLine(fs, "Height: " + height);
                    AddTextLine(fs, "Material: " + material_name);
                    // AddTextLine(fs, "RebarCover: " + rebar_cover);
                    num = num + 1;

                }
                show_info = "柱信息写入完毕，柱数量：" + num.ToString();
                TaskDialog.Show("Revit List", show_info);
                // ---5.Wall---


                // Finish Writing TXT File and Close File Stream 
                fs.Close();
                show_info = "文件写入完毕，文件路径：" + path;
                TaskDialog.Show("Revit List", show_info);
            }
            // ---------- Main Program End ---------- 
            return Result.Succeeded;
        }

        // Others
        // A Method to Write Text into TXT File
        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);

        }// A Method to Write Text into TXT File in Newline
        private static void AddTextLine(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value + "\n");
            fs.Write(info, 0, info.Length);
        }

        // --- IExternalCommand End ---
    }
}
