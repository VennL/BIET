//********************************************
// Revit to Opensees v0.3
// Created by: Liu Wen
// Date       : 2024.6.4
// Version    : v0.3
// Code       : C# (Revit API)
// Coding     : UTF-8 with BOM
// Unit       : mm
// Description: This program is used to convert Revit model to .tcl file for Opensees.
//              2Node.tcl: define node
//              4Elements.tcl: define column, beam, wall, floor
//              5GravityLoad.tcl: define gravity load and mass
//              6Recorders.tcl: define recorders
//********************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace R2O
{
    [Transaction(TransactionMode.ReadOnly)]
    class R2O_v03 : IExternalCommand
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
            // --- 2Node.tcl ---
            double[] list_X = { -99999 };
            double[] list_Y = { -99999 };
            double[] list_Z = { -99999 };
            int[] usage = { 0 };
            // ▲ save node coordinate XYZ in 3 lists
            string path_Node = @"D:\Study\Serious\Program\Revit\r2o\R2O_Output\2Node.tcl";
            using (FileStream fs = File.Open(path_Node, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                // ▼ Grid -> X, Y coordinate
                FilteredElementCollector collector_Grid = new FilteredElementCollector(doc);
                collector_Grid.OfCategory(BuiltInCategory.OST_Grids).OfClass(typeof(Grid));
                foreach (var item in collector_Grid)
                {
                    XYZ start_point = ((Grid)item).Curve.GetEndPoint(0) * unit;
                    XYZ end_point = ((Grid)item).Curve.GetEndPoint(1) * unit;
                    // ▲ get start point and end point of each grid line
                    double start_point_X = Math.Round(start_point.X, 2);
                    // ▲ round to 2 decimal places
                    double start_point_Y = Math.Round(start_point.Y, 2);
                    double end_point_X = Math.Round(end_point.X, 2);
                    double end_point_Y = Math.Round(end_point.Y, 2);
                    if (start_point_X == end_point_X) // this grid line is in X-direction
                    {
                        int index = Array.IndexOf(list_X, start_point_X);
                        if (index == -1) // this X coordinate is not in the list
                        {
                            Array.Resize(ref list_X, list_X.Length + 1);
                            list_X[list_X.Length - 1] = start_point_X;
                            AddTextLine(fs, "# X: " + start_point_X);
                        }
                    }
                    if (start_point_Y == end_point_Y) // this grid line is in Y-direction
                    {
                        int index = Array.IndexOf(list_Y, start_point_Y);
                        if (index == -1) // this Y coordinate is not in the list
                        {
                            Array.Resize(ref list_Y, list_Y.Length + 1);
                            list_Y[list_Y.Length - 1] = start_point_Y;
                            AddTextLine(fs, "# Y: " + start_point_Y);
                        }
                    }
                }
                // ▼ Level -> Z coordinate
                FilteredElementCollector collector_Level = new FilteredElementCollector(doc);
                collector_Level.OfCategory(BuiltInCategory.OST_Levels).OfClass(typeof(Level));
                foreach (var item in collector_Level)
                {
                    double elevation = Math.Round(((Level)item).Elevation * unit, 2);
                    int index = Array.IndexOf(list_Z, elevation);
                    if (index == -1) // this Z coordinate is not in the list
                    {
                        Array.Resize(ref list_Z, list_Z.Length + 1);
                        list_Z[list_Z.Length - 1] = elevation;
                    }
                }
                // ▼ Write Node File
                for (int i = 1; i < list_X.Length; i++)
                {
                    for (int j = 1; j < list_Y.Length; j++)
                    {
                        for (int k = 1; k < list_Z.Length; k++)
                        {
                            Array.Resize(ref usage, usage.Length + 1);
                            usage[(i-1) * (list_Y.Length - 1) * (list_Z.Length - 1) + (j-1) * (list_Z.Length - 1) + k-1] = 0;
                            AddTextLine(fs, "node " + (i * 1000000 + j * 1000 + k).ToString() + " " + list_X[i].ToString() + " " + list_Y[j].ToString() + " " + list_Z[k].ToString() + "; # floor: " + k.ToString());
                            // ▲ node tag = x00y00z (1000000 * X + 1000 * Y + Z)
                            AddTextLine(fs, "set node" + (i * 1000000 + j * 1000 + k).ToString() + "X " + list_X[i].ToString());
                            AddTextLine(fs, "set node" + (i * 1000000 + j * 1000 + k).ToString() + "Y " + list_Y[j].ToString());
                            AddTextLine(fs, "set node" + (i * 1000000 + j * 1000 + k).ToString() + "Z " + list_Z[k].ToString());
                            if (list_Z[k] == 0)  // fix nodes on ground
                            {
                                AddTextLine(fs, "fix " + (i * 1000000 + j * 1000 + k).ToString() + " 1 1 1 1 1 1");
                            }
                        }
                    }
                }
                AddTextLine(fs, "set numNodes " + ((list_X.Length - 1) * (list_Y.Length - 1) * (list_Z.Length - 1)).ToString());
                fs.Close();
                string show_info = "Node文件写入完毕，文件路径：" + path_Node;
                TaskDialog.Show("Revit2Opensees", show_info);
            }
            // --- 4Elements.tcl ---
            int[] list_floor = { 0 };
            string path_Element = @"D:\Study\Serious\Program\Revit\r2o\R2O_Output\4Elements.tcl";
            using (FileStream fs = File.Open(path_Element, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                AddTextLine(fs, "geomTransf Linear 1  1  0  0");  // beam, Y-in(+)
                AddTextLine(fs, "geomTransf Linear 2  0  1  0");  // beam, X-left(-)
                AddTextLine(fs, "geomTransf Linear 3  0  0  1");
                AddTextLine(fs, "geomTransf Linear 4 -1  0  0");  // beam, Y-out(-)
                AddTextLine(fs, "geomTransf Linear 5  0 -1  0");  // beam, X-right(+)
                AddTextLine(fs, "geomTransf Linear 6  0  0 -1");
                AddTextLine(fs, "geomTransf PDelta 7  0 -1  0");  // column, up(+)
                AddTextLine(fs, "set gravitysum 0");
                AddTextLine(fs, "set rouRCg 25*$kN/$m/$m/$m");
                AddTextLine(fs, "set FloorAreaLoad 6*$kN/$m/$m");
                int geomtransf = 0;
                // ▲ define 3D linear transformation from local to global coordinate
                // Reference: https://opensees.berkeley.edu/wiki/index.php/Linear_Transformation
                // --- Beam ---
                FilteredElementCollector collector_Beam = new FilteredElementCollector(doc);
                collector_Beam.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilyInstance));
                int numofbeams = collector_Beam.Count();
                AddTextLine(fs, "# Beam Number: " + numofbeams.ToString());
                foreach (var item in collector_Beam)
                {
                    FamilyInstance beam = (FamilyInstance)item;
                    double length = beam.LookupParameter("长度").AsDouble() * unit;
                    // ▲ get beam length
                    Location loc = beam.Location;
                    LocationCurve loc_curve = loc as LocationCurve;
                    Line curve = (Line)loc_curve.Curve;
                    XYZ origin = curve.Origin * unit;
                    // ▲ get beam start point
                    string Direction = curve.Direction.ToString();
                    // ▲ get beam direction
                    double direction_X = Math.Round(curve.Direction.X, 2);
                    double direction_Y = Math.Round(curve.Direction.Y, 2);
                    if (direction_Y == 0) // this beam is in X-direction
                    {
                        double start_point_X = Math.Round(origin.X, 2);
                        double start_point_Y = Math.Round(origin.Y, 2);
                        double start_point_Z = Math.Round(origin.Z, 2);
                        double end_point_X = Math.Round(start_point_X + direction_X * length, 2);
                        // ▲ get beam end point (only X coordinate changes)
                        int index_start_X = Array.IndexOf(list_X, start_point_X);
                        int index_end_X = Array.IndexOf(list_X, end_point_X);
                        int index_Y = Array.IndexOf(list_Y, start_point_Y);
                        int index_Z = Array.IndexOf(list_Z, start_point_Z);
                        if (start_point_X < end_point_X)  // beam, X-right(+)
                        {
                            geomtransf = 5;
                        }
                        else  // beam, X-left(-)
                        {
                            geomtransf = 2;
                        }
                        AddTextLine(fs, "element nonlinearBeamColumn " + item.Id + " " + (index_start_X * 1000000 + index_Y * 1000 + index_Z).ToString() + " " + (index_end_X * 1000000 + index_Y * 1000 + index_Z).ToString() + " 4 201 " + geomtransf.ToString() + "; # Beam ");
                        // ▲ element type = nonlinearBeamColumn, beam tag = beam.Id, node tag = x00y00z (1000000 * X + 1000 * Y + Z), number of integration points = 4, section tag = 201, transformation tag = 5/2
                        // https://opensees.berkeley.edu/wiki/index.php/Force-Based_Beam-Column_Element
                        AddTextLine(fs, "set length" + item.Id + " " + length.ToString());
                        AddTextLine(fs, "set volume" + item.Id + " [expr $length" + item.Id + "*300*500]");
                        AddTextLine(fs, "set weight" + item.Id + " [expr $volume" + item.Id + "*$rouRCg]");
                        AddTextLine(fs, "set gravitysum [expr $gravitysum + $weight" + item.Id + "]");
                    }
                    else if (direction_X == 0) // this beam is in Y-direction
                    {
                        double start_point_X = Math.Round(origin.X, 2);
                        double start_point_Y = Math.Round(origin.Y, 2);
                        double start_point_Z = Math.Round(origin.Z, 2);
                        double end_point_Y = Math.Round(start_point_Y + direction_Y * length, 2);
                        // ▲ get beam end point (only Y coordinate changes)
                        int index_start_Y = Array.IndexOf(list_Y, start_point_Y);
                        int index_end_Y = Array.IndexOf(list_Y, end_point_Y);
                        int index_X = Array.IndexOf(list_X, start_point_X);
                        int index_Z = Array.IndexOf(list_Z, start_point_Z);
                        if (start_point_Y < end_point_Y)  // beam, Y-in(+)
                        {
                            geomtransf = 1;
                        }
                        else  // beam, Y-out(-)
                        {
                            geomtransf = 4;
                        }
                        AddTextLine(fs, "element nonlinearBeamColumn " + item.Id + " " + (index_X * 1000000 + index_start_Y * 1000 + index_Z).ToString() + " " + (index_X * 1000000 + index_end_Y * 1000 + index_Z).ToString() + " 4 201 " + geomtransf.ToString() + "; # Beam ");
                        // ▲ element type = nonlinearBeamColumn, beam tag = beam.Id, node tag = x00y00z (1000000 * X + 1000 * Y + Z), number of integration points = 4, section tag = 201, transformation tag = 1/4
                        AddTextLine(fs, "set length" + item.Id + " " + length.ToString());
                        AddTextLine(fs, "set volume" + item.Id + " [expr $length" + item.Id + "*300*500]");
                        AddTextLine(fs, "set weight" + item.Id + " [expr $volume" + item.Id + "*$rouRCg]");
                        AddTextLine(fs, "set gravitysum [expr $gravitysum + $weight" + item.Id + "]");
                    }
                }
                // --- Column ---
                FilteredElementCollector collector_Column = new FilteredElementCollector(doc);
                collector_Column.OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(FamilyInstance));
                int numofcolumns = collector_Column.Count();
                AddTextLine(fs, "# Column Number: " + numofcolumns.ToString());
                foreach (var item in collector_Column)
                {
                    FamilyInstance column = (FamilyInstance)item;
                    XYZ bounding_box_max = column.get_BoundingBox(null).Max * unit;
                    XYZ bounding_box_min = column.get_BoundingBox(null).Min * unit;
                    double bounding_box_max_X = Math.Round(bounding_box_max.X, 2);
                    double bounding_box_max_Y = Math.Round(bounding_box_max.Y, 2);
                    double bounding_box_max_Z = Math.Round(bounding_box_max.Z, 2);
                    double bounding_box_min_X = Math.Round(bounding_box_min.X, 2);
                    double bounding_box_min_Y = Math.Round(bounding_box_min.Y, 2);
                    double bounding_box_min_Z = Math.Round(bounding_box_min.Z, 2);
                    double start_point_X = Math.Round((bounding_box_max_X + bounding_box_min_X) / 2, 2);
                    double start_point_Y = Math.Round((bounding_box_max_Y + bounding_box_min_Y) / 2, 2);
                    double start_point_Z = Math.Round(bounding_box_min_Z, 2);
                    double end_point_Z = Math.Round(bounding_box_max_Z, 2);
                    int index_X = Array.IndexOf(list_X, start_point_X);
                    int index_Y = Array.IndexOf(list_Y, start_point_Y);
                    int index_start_Z = Array.IndexOf(list_Z, start_point_Z);
                    int index_end_Z = Array.IndexOf(list_Z, end_point_Z);
                    double length = Math.Round(bounding_box_max_Z - bounding_box_min_Z, 2);
                    usage[(index_X - 1) * (list_Y.Length - 1) * (list_Z.Length - 1) + (index_Y - 1) * (list_Z.Length - 1) + index_start_Z - 1] = 1;
                    usage[(index_X - 1) * (list_Y.Length - 1) * (list_Z.Length - 1) + (index_Y - 1) * (list_Z.Length - 1) + index_end_Z - 1] = 1;
                    geomtransf = 7;
                    AddTextLine(fs, "element nonlinearBeamColumn " + item.Id + " " + (index_X * 1000000 + index_Y * 1000 + index_start_Z).ToString() + " " + (index_X * 1000000 + index_Y * 1000 + index_end_Z).ToString() + " 4 101 " + geomtransf.ToString() + "; # Column ");
                    // ▲ element type = nonlinearBeamColumn, column tag = column.Id, node tag = x00y00z (1000000 * X + 1000 * Y + Z), number of integration points = 4, section tag = 101, transformation tag = 5/2
                    AddTextLine(fs, "set length" + item.Id + " " + length.ToString());
                    AddTextLine(fs, "set volume" + item.Id + " [expr $length" + item.Id + "*500*500]");
                    AddTextLine(fs, "set weight" + item.Id + " [expr $volume" + item.Id + "*$rouRCg]");
                    AddTextLine(fs, "set gravitysum [expr $gravitysum + $weight" + item.Id + "]");
                }
                // --- Wall ---
                FilteredElementCollector collector_Wall = new FilteredElementCollector(doc).OfClass(typeof(Wall));
                int numofwalls = collector_Wall.Count();
                AddTextLine(fs, "# Wall Number: " + numofwalls.ToString());
                AddTextLine(fs, "if {$WallSwitch == 1} {");
                int tag = 77001;
                foreach (Wall wall in collector_Wall)
                {
                    double length = wall.LookupParameter("长度").AsDouble() * unit;
                    // ▲ get wall length
                    Location loc = wall.Location;
                    LocationCurve loc_curve = loc as LocationCurve;
                    Line curve = (Line)loc_curve.Curve;
                    XYZ origin = curve.Origin * unit;
                    // ▲ get wall start point
                    string Direction = curve.Direction.ToString();
                    // ▲ get wall direction
                    double direction_X = Math.Round(curve.Direction.X, 2);
                    double direction_Y = Math.Round(curve.Direction.Y, 2);
                    XYZ bounding_box_max = wall.get_BoundingBox(null).Max * unit;
                    XYZ bounding_box_min = wall.get_BoundingBox(null).Min * unit;
                    double bounding_box_max_Z = Math.Round(bounding_box_max.Z, 2);
                    double bounding_box_min_Z = Math.Round(bounding_box_min.Z, 2);
                    int index_start_Z = Array.IndexOf(list_Z, bounding_box_min_Z);
                    int index_end_Z = Array.IndexOf(list_Z, bounding_box_max_Z);
                    // ▲ use bounding box to get wall elevation
                    if (direction_Y == 0) // this wall is in X-direction
                    {
                        double start_point_X = Math.Round(origin.X, 2);
                        double start_point_Y = Math.Round(origin.Y, 2);
                        double end_point_X = Math.Round(start_point_X + direction_X * length, 2);
                        // ▲ get wall end point (only X coordinate changes)
                        int index_start_X = Array.IndexOf(list_X, start_point_X);
                        int index_end_X = Array.IndexOf(list_X, end_point_X);
                        int index_Y = Array.IndexOf(list_Y, start_point_Y);
                        AddTextLine(fs, "node " + wall.Id + " " + ((start_point_X + end_point_X) / 2).ToString() + " " + start_point_Y.ToString() + " " + ((bounding_box_max_Z + bounding_box_min_Z) / 2).ToString());
                        // ▲ node tag = wall.Id, node coordinate = center of wall
                        AddTextLine(fs, "Wall2Section " + (index_start_X * 1000000 + index_Y * 1000 + index_start_Z).ToString() + " " + wall.Id + " " + (index_end_X * 1000000 + index_Y * 1000 + index_end_Z).ToString() + " " + tag);
                        // ▲ element type = beamWithHinges, mid point tag = wall.Id, wall tag = tag
                        // https://opensees.berkeley.edu/wiki/index.php/Beam_With_Hinges_Element
                        // Wall2Section {start_point mid_point end_point wall_tag}
                        tag++;
                    }
                    else if (direction_X == 0) // this wall is in Y-direction
                    {
                        double start_point_X = Math.Round(origin.X, 2);
                        double start_point_Y = Math.Round(origin.Y, 2);
                        double end_point_Y = Math.Round(start_point_Y + direction_Y * length, 2);
                        // ▲ get wall end point (only Y coordinate changes)
                        int index_start_Y = Array.IndexOf(list_Y, start_point_Y);
                        int index_end_Y = Array.IndexOf(list_Y, end_point_Y);
                        int index_X = Array.IndexOf(list_X, start_point_X);
                        AddTextLine(fs, "node " + wall.Id + " " + start_point_X.ToString() + " " + ((start_point_Y + end_point_Y) / 2).ToString() + " " + ((bounding_box_max_Z + bounding_box_min_Z) / 2).ToString());
                        AddTextLine(fs, "Wall2Section " + (index_X * 1000000 + index_start_Y * 1000 + index_start_Z).ToString() + " " + wall.Id + " " + (index_X * 1000000 + index_end_Y * 1000 + index_end_Z).ToString() + " " + tag);
                        tag++;
                    }
                }
                AddTextLine(fs, "}");
                // --- Slab (Diaphragm) ---
                FilteredElementCollector collector_Floor = new FilteredElementCollector(doc);
                collector_Floor.OfCategory(BuiltInCategory.OST_Floors).OfClass(typeof(Floor));
                int numoffloors = collector_Floor.Count();
                AddTextLine(fs, "# Slab Number: " + numoffloors.ToString());
                foreach (var item in collector_Floor)
                {
                    Floor floor = (Floor)item;
                    Array.Resize(ref list_floor, list_floor.Length + 1);
                    list_floor[list_floor.Length - 1] = item.Id.IntegerValue;
                    XYZ bounding_box_max = floor.get_BoundingBox(null).Max * unit;
                    XYZ bounding_box_min = floor.get_BoundingBox(null).Min * unit;
                    double bounding_box_max_X = Math.Round(bounding_box_max.X, 2);
                    double bounding_box_max_Y = Math.Round(bounding_box_max.Y, 2);
                    double bounding_box_max_Z = Math.Round(bounding_box_max.Z, 2);
                    double bounding_box_min_X = Math.Round(bounding_box_min.X, 2);
                    double bounding_box_min_Y = Math.Round(bounding_box_min.Y, 2);
                    // ▲ get bounding box of floor, the max_Z equals to the floor elevation
                    int index_max_X = Array.IndexOf(list_X, bounding_box_max_X);
                    int index_max_Y = Array.IndexOf(list_Y, bounding_box_max_Y);
                    int index_max_Z = Array.IndexOf(list_Z, bounding_box_max_Z);
                    int index_min_X = Array.IndexOf(list_X, bounding_box_min_X);
                    int index_min_Y = Array.IndexOf(list_Y, bounding_box_min_Y);
                    AddTextLine(fs, "node " + item.Id + " " + ((bounding_box_max_X + bounding_box_min_X) / 2).ToString() + " " + ((bounding_box_max_Y + bounding_box_min_Y) / 2).ToString() + " " + bounding_box_max_Z.ToString());
                    // ▲ node tag = floor.Id, node coordinate = center of floor
                    AddTextLine(fs, "fix " + item.Id + " 0 0 1 1 1 0");
                    // ▲ fix nodes on floor
                    AddTextLine(fs, "rigidDiaphragm 3 " + item.Id + " " + (index_max_X * 1000000 + index_max_Y * 1000 + index_max_Z).ToString() + " " + (index_max_X * 1000000 + index_min_Y * 1000 + index_max_Z).ToString() + " " + (index_min_X * 1000000 + index_min_Y * 1000 + index_max_Z).ToString() + " " + (index_min_X * 1000000 + index_max_Y * 1000 + index_max_Z).ToString());
                    // ▲ direction = 3 (Z-direction), the retained node tag = floor.Id, the constrained nodes tag = x00y00z (1000000 * X + 1000 * Y + Z)
                    // https://opensees.berkeley.edu/wiki/index.php/RigidDiaphragm_command
                    AddTextLine(fs, "set area" + item.Id + " " + (bounding_box_max_X - bounding_box_min_X) * (bounding_box_max_Y - bounding_box_min_Y));
                    AddTextLine(fs, "set gravity" + item.Id + " [expr $area" + item.Id + "*$FloorAreaLoad]");
                    AddTextLine(fs, "set gravitysum [expr $gravitysum + $gravity" + item.Id + "]");          
                }
                // --- Remove Unused Nodes ---
                AddTextLine(fs, "# Remove Unused Nodes");
                for (int i = 1; i < list_X.Length; i++)
                {
                    for (int j = 1; j < list_Y.Length; j++)
                    {
                        for (int k = 1; k < list_Z.Length; k++)
                        {
                            if (usage[(i - 1) * (list_Y.Length - 1) * (list_Z.Length - 1) + (j - 1) * (list_Z.Length - 1) + k - 1] == 0)
                            {
                                AddTextLine(fs, "remove node " + (i * 1000000 + j * 1000 + k).ToString());
                                AddTextLine(fs, "set numNodes [expr $numNodes - 1]");
                            }
                        }
                    }
                }
                fs.Close();
                string show_info = "Element文件写入完毕，文件路径：" + path_Element;
                TaskDialog.Show("Revit2Opensees", show_info);
            }
            // --- 5GravityLoad.tcl ---
            string path_Gravity = @"D:\Study\Serious\Program\Revit\r2o\R2O_Output\5GravityLoad.tcl";
            using (FileStream fs = File.Open(path_Gravity, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                AddTextLine(fs, "set gravityEachNode [expr $gravitysum/$numNodes]");
                AddTextLine(fs, "puts \"gravityEachNode: $gravityEachNode\"");
                AddTextLine(fs, "pattern Plain 1 Constant {");
                // ▲ define load pattern
                for (int i = 1; i < list_X.Length; i++)
                {
                    for (int j = 1; j < list_Y.Length; j++)
                    {
                        for (int k = 1; k < list_Z.Length; k++)
                        {
                            if (usage[(i - 1) * (list_Y.Length - 1) * (list_Z.Length - 1) + (j - 1) * (list_Z.Length - 1) + k - 1] == 1)
                            {
                            AddTextLine(fs, "load " + (i * 1000000 + j * 1000 + k).ToString() + " 0.0 0.0 -$gravityEachNode 0.0 0.0 0.0");
                            }
                            // ▲ load node tag = x00y00z (1000000 * X + 1000 * Y + Z), load = -10800 kg * g * N
                        }
                    }
                }
                AddTextLine(fs, "}");
                AddTextLine(fs, "set massEachNode [expr $gravityEachNode/$g]");
                AddTextLine(fs, "puts \"massEachNode: $massEachNode kg\"");
                for (int i = 1; i < list_X.Length; i++)
                {
                    for (int j = 1; j < list_Y.Length; j++)
                    {
                        for (int k = 1; k < list_Z.Length; k++)
                        {
                            if (usage[(i - 1) * (list_Y.Length - 1) * (list_Z.Length - 1) + (j - 1) * (list_Z.Length - 1) + k - 1] == 1)
                            {
                            AddTextLine(fs, "mass " + (i * 1000000 + j * 1000 + k).ToString() + " $massEachNode $massEachNode $massEachNode 0.0 0.0 0.0");
                            }
                        }
                    }
                }
                fs.Close();
                string show_info = "GravityLoad文件写入完毕，文件路径：" + path_Gravity;
                TaskDialog.Show("Revit2Opensees", show_info);
            }
            // --- 6Recorders.tcl ---
            string path_Recorder = @"D:\Study\Serious\Program\Revit\r2o\R2O_Output\6Recorders.tcl";
            using (FileStream fs = File.Open(path_Recorder, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                for (int i = 1; i < list_X.Length; i++)
                {
                    for (int j = 1; j < list_Y.Length; j++)
                    {
                        for (int k = 1; k < list_Z.Length; k++)
                        {
                            // https://opensees.berkeley.edu/wiki/index.php/Node_Recorder
                            // recorder Node<-file $fileName > < -xml $fileName > < -binary $fileName > < -tcp $inetAddress $port > < -precision $nSD > < -timeSeries $tsTag > < -time > < -dT $deltaT > < -closeOnWrite > < -node $node1 $node2...> < -nodeRange $startNode $endNode > < -region $regionTag > -dof($dof1 $dof2...) $respType'
                            AddTextLine(fs, "recorder Node -file ./Output/disp/" + (i * 1000000 + j * 1000 + k).ToString() + "Disp.txt -time -node " + (i * 1000000 + j * 1000 + k).ToString() + " -dof 1 2 3 disp");
                            // ▲ recorder type = Node, output file = x00y00zDisp.txt, time, node tag = x00y00z (1000000 * X + 1000 * Y + Z), degree of freedom = 1 2 3 (X Y Z), output type = disp
                            AddTextLine(fs, "recorder Node -file ./Output/acce/" + (i * 1000000 + j * 1000 + k).ToString() + "Acce.txt -time -node " + (i * 1000000 + j * 1000 + k).ToString() + " -dof 1 2 3 accel");
                            // ▲ recorder type = Node, output file = x00y00zAcce.txt, time, node tag = x00y00z (1000000 * X + 1000 * Y + Z), degree of freedom = 1 2 3 (X Y Z), output type = accel
                        }
                    }
                }
                fs.Close();
                string show_info = "Recorder文件写入完毕，文件路径：" + path_Recorder;
                TaskDialog.Show("Revit2Opensees", show_info);
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
