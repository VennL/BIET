#################################################################
## 0Main.tcl
## Coding: UTF-8
## Author: Liu Wen
## Description: Calculate the response of RC frame exported from Revit.
## Basic Units: mm, kg, sec
#################################################################

wipe
wipe;                                          # clear memory of all past model definitions
model BasicBuilder -ndm 3 -ndf 6;              # Define the model builder, ndm=#dimension, ndf=#dofs

#  Z(global) : up 
#           /|\  ^  Y(global) : inward perpendicular to the screen              
#            |  /                                
#            | /                                  
#            |/------->  X(global) : right 
#      

puts "**********Program Start**********";      # print the start information

# Step1. Define units and constants ▼
# source 1Units&Constants.tcl;
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
set pi   3.1416;                      # pi
set g    [expr 9.81*$m/$sec/$sec];    # gravity acceleration
set max  1.e20;                       # a large number
set min  [expr 1/$max];               # a small number
puts "Step1. Units & Constants Defined Successfully";
puts "*********************************";

# Step2. Define nodes from grids and levels ▼
source 2Node.tcl;
puts "Step2. Nodes Defined Successfully";
puts "*********************************";

# Step3. Define materials and fibers ▼
source 3Materials&Fibers.tcl;
puts "Step3. Materials & Fibers Defined Successfully";
puts "*********************************";

# Step4. Define columns, beams and rigid diaphragms ▼
set WallSwitch 0; # 1: Wall; 0: No Wall
source 4Elements.tcl;
puts "Step4. Elements Defined Successfully";
puts "*********************************";

# Step5. Define gravity loads ▼
source 5GravityLoad.tcl;
puts "Step5. Gravity Loads Defined Successfully";
puts "*********************************";

# Step6. Define recorders ▼
source 6Recorders.tcl;
puts "Step6. Recorders Defined Successfully";
puts "*********************************";

# Step7. Analysis ▼
wipeAnalysis
wipeAnalysis;
# Period
set a [eigen 10] ;
set W12 [lindex $a 0] ;
set W22 [lindex $a 1] ;
set W32 [lindex $a 2] ;
set W42 [lindex $a 3] ;
set W52 [lindex $a 4] ;
set W62 [lindex $a 5] ;
set W72 [lindex $a 6] ;
set W82 [lindex $a 7] ;
set W92 [lindex $a 8] ;
set W1 [expr pow($W12,0.5)] ;
set W2 [expr pow($W22,0.5)] ;
set W3 [expr pow($W32,0.5)] ;
set W4 [expr pow($W42,0.5)] ;
set W5 [expr pow($W52,0.5)] ;
set W6 [expr pow($W62,0.5)] ;
set W7 [expr pow($W72,0.5)] ;
set W8 [expr pow($W82,0.5)] ;
set W9 [expr pow($W92,0.5)] ;
set T1 [expr 2.0*$pi/$W1] ;
set T2 [expr 2.0*$pi/$W2] ;
set T3 [expr 2.0*$pi/$W3] ;
set T4 [expr 2.0*$pi/$W4] ;
set T5 [expr 2.0*$pi/$W5] ;
set T6 [expr 2.0*$pi/$W6] ;
set T7 [expr 2.0*$pi/$W7] ;
set T8 [expr 2.0*$pi/$W8] ;
set T9 [expr 2.0*$pi/$W9] ;
set PeriodFileDir "./Output/PeriodFile.txt"
set PeriodFile [open $PeriodFileDir "w"]
puts "W1=$W1 Rad/Sec"
puts "T1=$T1 Sec"
puts "W2=$W2 Rad/Sec"
puts "T2=$T2 Sec"
puts "W3=$W3 Rad/Sec"
puts "T3=$T3 Sec"
puts "W4=$W4 Rad/Sec"
puts "T4=$T4 Sec"
puts "W5=$W5 Rad/Sec"
puts "T5=$T5 Sec"
puts "W6=$W6 Rad/Sec"
puts "T6=$T6 Sec"
puts "W7=$W7 Rad/Sec"
puts "T7=$T7 Sec"
puts "W8=$W8 Rad/Sec"
puts "T8=$T8 Sec"
puts "W9=$W9 Rad/Sec"
puts "T9=$T9 Sec"
puts "*********************************";
puts $PeriodFile "WallSwitch=$WallSwitch"
puts $PeriodFile "W1=$W1 Rad/Sec"
puts $PeriodFile "T1=$T1 Sec"
puts $PeriodFile "W2=$W2 Rad/Sec"
puts $PeriodFile "T2=$T2 Sec"
puts $PeriodFile "W3=$W3 Rad/Sec"
puts $PeriodFile "T3=$T3 Sec"
puts $PeriodFile "W4=$W4 Rad/Sec"
puts $PeriodFile "T4=$T4 Sec"
puts $PeriodFile "W5=$W5 Rad/Sec"
puts $PeriodFile "T5=$T5 Sec"
close $PeriodFile

# static analysis
set StaticSwitch 0;
if {$StaticSwitch == 1} {
    wipeAnalysis
    wipeAnalysis;
    constraints Transformation
    numberer RCM
    system SparseGeneral
    test NormDispIncr 1e-5 100 2
    algorithm Newton
    integrator LoadControl 0.01
    analysis Static
    analyze 100
    puts "Static Analysis Finished"
    puts "*********************************";
}

# rayleigh damping
set xDamp 0.05;
set nEigenI 1;
set nEigenJ 2;
set lambdaN [eigen [expr $nEigenJ]]; 
set lambdaI [lindex $lambdaN [expr $nEigenI-1]];
set lambdaJ [lindex $lambdaN [expr $nEigenJ-1]];
set omegaI [expr pow($lambdaI,0.5)]; 
set omegaJ [expr pow($lambdaJ,0.5)];
set alphaM [expr $xDamp*(2*$omegaI*$omegaJ)/($omegaI+$omegaJ)]; 
set betaKcurr [expr 2.*$xDamp/($omegaI+$omegaJ)];   
rayleigh $alphaM $betaKcurr 0 0
puts "Rayleigh Damping Defined Successfully";
set IDloadTag 1001;
set GroundMotionFile "GM1X.txt";
set iGMdirection "1"; 
set iGMfact "200";  
set dt 0.02;   
foreach GMdirection $iGMdirection GMfile $GroundMotionFile GMfact $iGMfact { 
incr IDloadTag; 
set GMfatt [expr 1*$GMfact];  
set AccelSeries "Series -dt $dt -filePath $GroundMotionFile -factor $GMfatt";
pattern UniformExcitation $IDloadTag $GMdirection -accel $AccelSeries; 
}  
puts "Ground Motion Defined Successfully";
wipeAnalysis
wipeAnalysis;
constraints Transformation
numberer RCM
system SparseGeneral
test NormDispIncr 1.0e-3 500 2
algorithm Newton
integrator Newmark 0.5 0.25
analysis Transient
analyze 1000 0.02
puts "Transient Analysis Finished"
wipe 