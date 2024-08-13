geomTransf Linear 1  1  0  0
geomTransf Linear 2  0  1  0
geomTransf Linear 3  0  0  1
geomTransf Linear 4 -1  0  0
geomTransf Linear 5  0 -1  0
geomTransf Linear 6  0  0 -1
geomTransf PDelta 7  0 -1  0
# Beam Number: 4
element nonlinearBeamColumn 339992 1001002 2001002 4 201 5; # Beam 
element nonlinearBeamColumn 340070 2001002 2002002 4 201 1; # Beam 
element nonlinearBeamColumn 340098 2002002 1002002 4 201 2; # Beam 
element nonlinearBeamColumn 340115 1002002 1001002 4 201 4; # Beam 
# Column Number: 4
element nonlinearBeamColumn 338515 1001001 1001002 4 101 7; # Column 
element nonlinearBeamColumn 338669 2001001 2001002 4 101 7; # Column 
element nonlinearBeamColumn 338782 2002001 2002002 4 101 7; # Column 
element nonlinearBeamColumn 338865 1002001 1002002 4 101 7; # Column 
# Wall Number: 0
if {$WallSwitch == 1} {
}
# Floor Number: 1
node 340212 -4100.85 -351.86 4000
fix 340212 0 0 1 1 1 0
rigidDiaphragm 3 340212 2002002 2001002 1001002 1002002
# mass 340212 360 360 360 0 0 0
