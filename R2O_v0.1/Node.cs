using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace R2O
{
    [Transaction(TransactionMode.ReadOnly)]
    class Node : IExternalCommand
    {
        // --- IExternalCommand Beginning ---
        public readonly double unit = 0.3048;  // Revit Internal Unit: feet, 1 feet = 0.3048 m
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // --- Execute  Beginning ---
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.IncludeNonVisibleObjects = false;
            options.ComputeReferences = false;
            // ---------- Main Program Beginning ----------
            // --- Write info to 2Node.tcl file ---
            string path = @"D:\Study\Serious\Program\Revit\r2o\2Node.tcl";
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                AddLine.AddTextLine(fs, "# unit: m");
                // --- Grid ---
                FilteredElementCollector collector_Grid = new FilteredElementCollector(doc);
                collector_Grid.OfCategory(BuiltInCategory.OST_Grids).OfClass(typeof(Grid));
                var grid_list_X = new List<lines>();
                var grid_list_Y = new List<lines>();
                int grid_count = 0;
                foreach (var item in collector_Grid)
                {
                    XYZ start_point = ((Grid)item).Curve.GetEndPoint(0) * unit;
                    XYZ end_point = ((Grid)item).Curve.GetEndPoint(1) * unit;
                    if (start_point.X - end_point.X < 0.01 && start_point.X - end_point.X > -0.01)
                    {
                        long grid_X = Convert.ToInt64(start_point.X);
                        grid_list_X.Add(grid_X);
                        grid_count++;
                        AddLine.AddTextLine(fs, "set grid" + item.Id + " " + grid_count);
                    }
                    else if (start_point.Y - end_point.Y < 0.001 && start_point.Y - end_point.Y > -0.001)
                    {
                        long grid_Y = Convert.ToInt64(start_point.Y);
                        grid_list_Y.Add(grid_Y);
                        grid_count++;
                        AddLine.AddTextLine(fs, "set grid" + item.Id + " " + grid_count);
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
                    double elevation = ((Level)item).Elevation * unit;
                    long level_Z = Convert.ToInt64(elevation);
                    level_list_Z.Add(level_Z);
                }

                foreach (var item_X in grid_list_X)
                {
                    foreach (var item_Y in grid_list_Y)
                    {
                        foreach (var item_Z in level_list_Z)
                        {
                            string node_name_X = Math.Abs(item_X).ToString();
                            string node_name_Y = Math.Abs(item_Y).ToString();
                            string node_name_Z = Math.Abs(item_Z).ToString();
                            string node_tag = node_name_X + node_name_Y + node_name_Z;
                            string node_coordinate = " " + item_X + " " + item_Y + " " + item_Z;
                            AddLine.AddTextLine(fs, "node " + node_tag + node_coordinate);
                        }
                    }
                }
                fs.Close();
                string show_info = "Node文件写入完毕，文件路径：" + path;
                TaskDialog.Show("Node", show_info);
            }
            // ---------- Main Program End ---------- 
            return Result.Succeeded;

        }
        struct lines
        {
            string id;
            int num;
            int type;  // 0 for grid X, 1 for grid Y, 2 for level
        }
    }
}

