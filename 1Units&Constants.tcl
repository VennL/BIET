#---------1Units&Constants.tcl---------
# Basic units
global mm sec kg m N kN m2 mm2 MPa pi g max min;
set mm   1.;  # basic unit of length: millimeter
set sec  1.;  # basic unit of time: second
set kg   1.;  # basic unit of mass: kilogram
# Derived units
set m   [expr $mm*1000];
set N   [expr $kg*$m/$sec/$sec];
set kN  [expr $N*1000.];
set m2  [expr pow($m,2)];
set mm2 [expr pow($mm,2)];
set MPa [expr $N/$mm2];          # basic unit of stress: Mega Pascal
# Constants
set pi   3.1416;                        # pi
set g    [expr 9.81*$m/$sec/$sec];    # gravity acceleration
set max  1.e20;                       # a large number
set min  [expr 1/$max];               # a small number

#---------1Units&Constants.tcl---------