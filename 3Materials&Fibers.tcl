#---------3Materials&Fibers.tcl---------
## Source: Prof. Li Shuang, Harbin Institute of Technology (2004)
## Modified by: Liu Wen (2023)
## Basic Units: mm, kg, sec

# =============================================================
######################## MATERIAL ########################
# =============================================================
# Material tag: concrete 1~10; steel 11~20; others 21~30
# 1  - Concrete cover
# 2  - Concrete core in column 500mm*500mm
# 3  - Concrete core in beam   500mm*300mm
# 11 - Steel HRB400
# ==============
# Define a procedure to calculate the parameters of Kent-Scott-Park Envelop
# ==============
# Kent-Scott-Park Envelop ▼
# Reference:  http://www.jdcui.com/?p=1629
# Unconffpc0     compressive strength of unconfined concrete (input negative value)
# Unconfec0      strain at maximum strength of unconfined concrete (0.002)
# H              section height (not used)
# B              section width (used to calculate the centerline length of hoop reinforcement)
# Sh             centerline distance of hoop reinforcements 【箍筋间距】
# cover          cover thickness 【保护层厚度】
# rous           ratio of volume of hoop reinforcement to volume of core concrete (measured to outside of hoops) 【体积配箍率】
# hoopfy         yield stress of hoop reinforcement 【箍筋屈服强度】
proc ConcreteMPK {Unconffpc0 Unconfec0 H B rous Sh cover hoopfy } {
    set K        [expr 1.0+$rous*$hoopfy/abs($Unconffpc0)];
    set e50u     [expr (3.0+0.29*abs($Unconffpc0/1000))/(145.0*abs($Unconffpc0/1000)-1000)];  # /1000 is to convert to MPa
    set e50h     [expr 0.75*$rous*pow([expr ($B-2*$cover)/$Sh],0.5)];
    set Z        [expr 0.5/($e50u+$e50h-abs($Unconfec0)*$K)];
    set Conffpc  [expr $K*$Unconffpc0];
    set Confec0  [expr $Unconfec0*$K];
    set Conffpcu [expr 0.2*$Conffpc];
    set Confecu  [expr -(0.8/$Z+abs($Unconfec0)*$K)];
    # # Define global variable
    # global concreteProp
    set concreteProp [list $Conffpc $Confec0 $Conffpcu $Confecu]
    return  $concreteProp
}

# Tag 1: C30-unconfined concrete, both for column and beam
set fpc_1    [expr -30.0*$MPa]
# ▲ compressive strength of unconfined concrete
set epsc0_1  -0.002
# ▲ strain at maximum strength of unconfined concrete
set fpcu_1   [expr 0.2*$fpc_1]
# ▲ ultimate compressive strength of unconfined concrete
set epscu_1  [expr -1.6*(3.0+0.29*abs($fpc_1))/(145.0*abs($fpc_1)-1000)-0.6*$epsc0_1]
# ▲ strain at ultimate compressive strength of unconfined concrete
set lambda_1 0.25
set ft_1     [expr -0.1*$fpc_1]
# ▲ tensile strength
set Ets_1    [expr 0.1*$fpc_1/$epsc0_1]

# Tag 2: C30-confined concrete, column with section 500mm*500mm
#                        Unconffpc0  Unconfec0    H    B   rous   Sh  cover   hoopfy
set subtiV   [ConcreteMPK  $fpc_1    $epsc0_1    500  500  0.005  75   15    400*$MPa]
set fpc_2    [lindex $subtiV 0]
set epsc0_2  [lindex $subtiV 1]
set fpcu_2   [lindex $subtiV 2]
set epscu_2  [lindex $subtiV 3]
set lambda_2 0.25
set ft_2     [expr -0.1*$fpc_2]
set Ets_2    [expr 0.1*$fpc_2/$epsc0_2]

# Tag 3: C40-confined concrete, beam with section 500mm*300mm
#                        Unconffpc0  Unconfec0    H    B   rous   Sh  cover   hoopfy
set subtiV   [ConcreteMPK  $fpc_1    $epsc0_1    500  300  0.005  75   15    400*$MPa]
set fpc_3    [lindex $subtiV 0]
set epsc0_3  [lindex $subtiV 1]
set fpcu_3   [lindex $subtiV 2]
set epscu_3  [lindex $subtiV 3]
set lambda_3 0.25
set ft_3     [expr -0.1*$fpc_3]
set Ets_3    [expr 0.1*$fpc_3/$epsc0_3]

# Reference:  https://opensees.berkeley.edu/wiki/index.php/Concrete02_Material_--_Linear_Tension_Softening
# uniaxialMaterial Concrete02 $matTag $fpc $epsc0 $fpcu $epsU $lambda $ft $Ets
# $matTag	integer tag identifying material
# $fpc	concrete compressive strength at 28 days (compression is negative)*
# $epsc0	concrete strain at maximum strength*
# $fpcu	concrete crushing strength *
# $epsU	concrete strain at crushing strength*
# $lambda	ratio between unloading slope at $epscu and initial slope
# $ft	tensile strength
# $Ets	tension softening stiffness (absolute value) (slope of the linear tension softening branch)
###############################################################
# Concrete material       matTag  fpc    epsc0     fpcu     epscu     lambda      ft     Ets
uniaxialMaterial Concrete02  1  $fpc_1  $epsc0_1  $fpcu_1  $epscu_1  $lambda_1   $ft_1  $Ets_1
uniaxialMaterial Concrete02  2  $fpc_2  $epsc0_2  $fpcu_2  $epscu_2  $lambda_2   $ft_2  $Ets_2
uniaxialMaterial Concrete02  3  $fpc_3  $epsc0_3  $fpcu_3  $epscu_3  $lambda_3   $ft_3  $Ets_3
puts " $fpc_1  $epsc0_1  $fpcu_1  $epscu_1  $lambda_1   $ft_1  $Ets_1"
puts " $fpc_2  $epsc0_2  $fpcu_2  $epscu_2  $lambda_2   $ft_2  $Ets_2"
puts " $fpc_3  $epsc0_3  $fpcu_3  $epscu_3  $lambda_3   $ft_3  $Ets_3"
###############################################################
# HRB400
# Reference:  http://www.jdcui.com/?p=2567
set Fy          [expr 400.0*$MPa]
# ▲ yield stress
set Es          2.0e5
# ▲ initial modual
set b           0.01
set R0          18.5
set cR1         0.925
set cR2         0.15
set a1          0.0
set a2          1.0
set a3          0.0
set a4          1.0
# Reference:  https://opensees.berkeley.edu/wiki/index.php/Steel02_Material_--_Giuffr%C3%A9-Menegotto-Pinto_Model_with_Isotropic_Strain_Hardening
# uniaxialMaterial Steel02 $matTag $Fy $E $b $R0 $cR1 $cR2 <$a1 $a2 $a3 $a4 $sigInit>
# $matTag	integer tag identifying material
# $Fy	yield strength
# $E0	initial elastic tangent
# $b	strain-hardening ratio (ratio between post-yield tangent and initial elastic tangent)
# $R0 $CR1 $CR2	parameters to control the transition from elastic to plastic branches.
# Recommended values: $R0=between 10 and 20, $cR1=0.925, $cR2=0.15
# $a1	isotropic hardening parameter, increase of compression yield envelope as proportion of yield strength after a plastic strain of $a2*($Fy/E0). (optional)
# $a2	isotropic hardening parameter (see explanation under $a1). (optional default = 1.0).
# $a3	isotropic hardening parameter, increase of tension yield envelope as proportion of yield strength after a plastic strain of $a4*($Fy/E0). (optional default = 0.0)
# $a4	isotropic hardening parameter (see explanation under $a3). (optional default = 1.0)
# $sigInit	Initial Stress Value (optional, default: 0.0) the strain is calculated from epsP=$sigInit/$E
# if (sigInit!= 0.0) { double epsInit = sigInit/E; eps = trialStrain+epsInit; } else eps = trialStrain;
###############################################################
# Steel material          matTag  Fy    E    b  R0   cR1   cR2   a1   a2   a3   a4
uniaxialMaterial Steel02    11   $Fy   $Es  $b $R0  $cR1  $cR2  $a1  $a2  $a3  $a4
puts " $Fy   $Es  $b $R0  $cR1  $cR2  $a1  $a2  $a3  $a4"

########################################################################################
########################################################################################
########################################################################################

# =============================================================
######################## FIBER ########################
# =============================================================
# Fiber tag: column 101~200; beam 201~300; others 301~400
# 101 - Column: 500mm*500mm, cover 15mm
# 201 - Beam:   500mm*300mm, cover 15mm
# ==============
# Column 101
###########################################
# !!!The section rotate in the Z axis!!!  #
#                                         #
#-----------------COLUMN------------------#
#                                         #
#        LEFT         RIGHT               #
#        SIDE         SIDE                #           ------>---------->---------->------
#        REBAR        REBER               #          /|\          /|\     /|\          /|\
#          -------------           ---    #           |            |       |            |
#          | *       * |            ^     #           |            |       |            |
#    <-----------|     |            B     #           ------>---------->---------->------
#  Y(local)| *   |   * |            v     #          /|\          /|\     /|\          /|\
#          ------|------           ---    #           |            |       |            |
#                |                        #           |            |       |            |
#                V  Z(local)              #           ------>---------->---------->------
#                                         #          /|\          /|\     /|\          /|\
#          |<----H---->|                  #           |            |       |            |
#      X: out of screen (up)              #           |            |       |            |
#                                         #          ===          ===     ===          ===
#    /|\ Y(global)                        #    Z(global) : up
#     |                                   #             /|\  ^  Y(global) : inward perpendicular to the screen
#     |                                   #              |  /
#     |------->  X(global)                #              | /
#                                         #              |/------->  X(global) : right
#       ELEMENT "GROWTH" DIRECTION        #
#        (POSITIVE of X IN LOCAL)         #
#              IS SAME AS                 #      Make sure the direction of the element construction
#     THE POSITIVE GLOBAL DIRECTION       #
#    (BOTH of THE COLUMN AND THE BEAM)    #
###########################################
# set GJ        1e17;		# torsional stiffness
# set secH      500;
# set secB      500;
# set coverH    15;
# set coverB    15;
# set edgeH     [expr $secH/2.0];
# set edgeB     [expr $secB/2.0];
# set edgeHng   [expr -$edgeH];
# set edgeBng   [expr -$edgeB];
# set coreH     [expr $edgeH-$coverH];
# set coreB     [expr $edgeB-$coverB];
# set coreHng   [expr -$coreH];
# set coreBng   [expr -$coreB];
# set numbarH   4;
# set numbarB   4;
# set rbar      16;
# set areabar   [expr $pi*$rbar*$rbar];
# section Fiber 101 -GJ $GJ {
#     # Reference:  https://opensees.berkeley.edu/wiki/index.php/Patch_Command
#     # patch rect $matTag $numSubdivY $numSubdivZ    $yI      $zI      $yJ      $zJ
#     patch rect      2        10         10       $coreHng $coreBng $coreH   $coreB
#     patch rect      1        10          1       $edgeHng $edgeBng $coreHng $edgeB
#     patch rect      1        10          1       $coreH   $edgeBng $edgeH   $edgeB
#     patch rect      1         1         10       $coreHng $edgeBng $coreH   $coreB
#     patch rect      1         1         10       $coreHng $coreB   $coreH   $edgeB
#     # Reference:  https://opensees.berkeley.edu/wiki/index.php/Layer_Command
#     # layer straight $matTag $numFiber $areaFiber $yStart $zStart  $yEnd  $zEnd
#     layer straight      11   $numbarH  $areabar  $coreHng $coreBng $coreH $coreBng
#     layer straight      11   $numbarH  $areabar  $coreHng $coreB   $coreH $coreB
# }
proc RcColumnSection {id HSec BSec coverH coverB coreID coverID steelID numBarsLCol barAreaLCol numBarsRCol barAreaRCol numBarsIntTotCol barAreaIntCol {nfCoreY 10} {nfCoreZ 10} {nfCoverY 12} {nfCoverZ 12} }   {
    set coverY  [expr $HSec/2.0];
    set coverZ  [expr $BSec/2.0];
    set ncoverY [expr -$coverY];
    set ncoverZ [expr -$coverZ];
    set coreY   [expr $coverY-$coverH];
    set coreZ   [expr $coverZ-$coverB];
    set ncoreY  [expr -$coreY]
    set ncoreZ  [expr -$coreZ]
    set numBarsIntCol [expr $numBarsIntTotCol/2];	# number of intermediate bars per side
    set GJ 1e17;		# torsional stiffness
    set tag     [expr $id+1000]
    section Fiber $tag -GJ $GJ {
        #            patch quad  $matTag   $numSubdivIJ $numSubdivJK   $yI      $zI      $yJ     $zJ      $yK     $zK     $yL      $zL
        #            Define the core patch
        patch quad  $coreID   $nfCoreY     $nfCoreZ       $ncoreY  $ncoreZ  $coreY  $ncoreZ  $coreY  $coreZ  $ncoreY  $coreZ

        #            Define the four cover patches
        patch quad  $coverID  1            $nfCoverZ      $ncoverY $ncoverZ $ncoreY $ncoreZ  $ncoreY $coreZ  $ncoverY $coverZ
        patch quad  $coverID  1            $nfCoverZ      $coreY   $ncoreZ  $coverY $ncoverZ $coverY $coverZ $coreY   $coreZ
        patch quad  $coverID  $nfCoverY    1              $ncoreY  $coreZ   $coreY  $coreZ   $coverY $coverZ $ncoverY $coverZ
        patch quad  $coverID  $nfCoverY    1              $ncoverY $ncoverZ $coverY $ncoverZ $coreY  $ncoreZ $ncoreY  $ncoreZ

        layer straight   $steelID      $numBarsLCol  $barAreaLCol   $coreY     $ncoreZ    $coreY  $coreZ
        layer straight   $steelID      $numBarsRCol  $barAreaRCol   $ncoreY    $ncoreZ    $ncoreY $coreZ
        set spacingY [expr ($coreY-$ncoreY)/($numBarsIntCol-1)]
        set numBarsIntCol1 [expr $numBarsIntCol-2]
        layer straight   $steelID      $numBarsIntCol1  $barAreaIntCol  [expr $coreY-$spacingY]  $coreZ  [expr $ncoreY+$spacingY]  $coreZ
        layer straight   $steelID      $numBarsIntCol1  $barAreaIntCol  [expr $coreY-$spacingY]  $ncoreZ  [expr $ncoreY+$spacingY]  $ncoreZ
    }
    uniaxialMaterial Elastic [expr $id+1001] 541556619
    section Aggregator $id [expr $id+1001] T -section $tag
}
#                 id HSec BSec coverH coverB coreID coverID steelID numBarsLCol barAreaLCol numBarsRCol barAreaRCol numBarsIntTotCol barAreaIntCol
RcColumnSection  101  500  500   35     35      2      1       11        4          380          4          380            4            314;
# ==============
# Beam 201
###########################################
# !!!The section rotate in the Z axis!!!  #
#                                         #
#------------------BEAM-------------------#
#                                         #
#                     /|\ Y(local)        #
#                      |                  #
#    TOP          -----|-----      ---    #
#    LAYER        | *  |  * |      /|\    #
#    REBAR        |    |    |       |     #
#                 |    |    |       |     #
#      Z(local)   |    |    |       |     #
#          <-----------|    |       H     #
#                 |         |       |     #
#                 |         |       |     #
#                 |         |       |     #
#    BOTTOM       | *     * |      \|/    #
#    LAYER        -----------      ---    #
#    REBAR                                #
#                 |<---B--->|             #
#                                         #
#    /|\                                  #
#     | Z(global)                         #
#     |                                   #
#     |                                   #
#     |------->  X/Y(global)              #
#                                         #
#       ELEMENT "GROWTH" DIRECTION        #
#        (POSITIVE of X IN LOCAL)         #
#              IS SAME AS                 #
#     THE POSITIVE GLOBAL DIRECTION       #
#    (BOTH of THE COLUMN AND THE BEAM)    #
###########################################
# Z(global) : up
#     /|\  ^  Y(global) : inward perpendicular to the screen
#      |  /
#      | /
#      |/------->  X(global) : right
#
# set GJ        1e17;		# torsional stiffness
# set secH      500;
# set secB      300;
# set coverH    15;
# set coverB    15;
# set edgeH     [expr $secH/2.0];
# set edgeB     [expr $secB/2.0];
# set edgeHng   [expr -$edgeH];
# set edgeBng   [expr -$edgeB];
# set coreH     [expr $edgeH-$coverH];
# set coreB     [expr $edgeB-$coverB];
# set coreHng   [expr -$coreH];
# set coreBng   [expr -$coreB];
# set numbarH   4;
# set numbarB   4;
# set rbar      16;
# set areabar   [expr $pi*$rbar*$rbar];
# section Fiber 201 -GJ $GJ {
#     # Reference:  https://opensees.berkeley.edu/wiki/index.php/Patch_Command
#     # patch rect $matTag $numSubdivY $numSubdivZ    $yI      $zI      $yJ      $zJ
#     patch rect      2        10         10       $coreHng $coreBng $coreH   $coreB
#     patch rect      1        10          1       $edgeHng $edgeBng $coreHng $edgeB
#     patch rect      1        10          1       $coreH   $edgeBng $edgeH   $edgeB
#     patch rect      1         1         10       $coreHng $edgeBng $coreH   $coreB
#     patch rect      1         1         10       $coreHng $coreB   $coreH   $edgeB
#     # Reference:  https://opensees.berkeley.edu/wiki/index.php/Layer_Command
#     # layer straight $matTag $numFiber $areaFiber $yStart $zStart  $yEnd  $zEnd
#     layer straight      11   $numbarH  $areabar  $coreHng $coreBng $coreH $coreBng
#     layer straight      11   $numbarH  $areabar  $coreHng $coreB   $coreH $coreB
# }
proc  RcBeamSection  {id HSec BSec coverH coverB  coreID coverID steelID numBarsTOPBeam barAreaTOPBeam numBarsBOTBeam barAreaBOTBeam numBarsIntTotBeam barAreaIntBeam {nfCoreY 10} {nfCoreZ 10} {nfCoverY 12} {nfCoverZ 12} }   {
    set coverY  [expr $HSec/2.0];
    set coverZ  [expr $BSec/2.0];
    set ncoverY [expr -$coverY];
    set ncoverZ [expr -$coverZ];
    set coreY   [expr $coverY-$coverH];
    set coreZ   [expr $coverZ-$coverB];
    set ncoreY  [expr -$coreY]
    set ncoreZ  [expr -$coreZ]
    set numBarsIntBeam [expr $numBarsIntTotBeam/2];	# number of intermediate bars per side
    set GJ 1e17;		# torsional stiffness
    set tag     [expr $id+1000]
    section Fiber $tag -GJ $GJ {
        #            patch quad  $matTag   $numSubdivIJ $numSubdivJK   $yI      $zI      $yJ     $zJ      $yK     $zK     $yL      $zL
        #            Define the core patch
        patch quad  $coreID   $nfCoreY     $nfCoreZ       $ncoreY  $ncoreZ  $coreY  $ncoreZ  $coreY  $coreZ  $ncoreY  $coreZ

        #            Define the four cover patches
        patch quad  $coverID  1            $nfCoverZ      $ncoverY $ncoverZ $ncoreY $ncoreZ  $ncoreY $coreZ  $ncoverY $coverZ
        patch quad  $coverID  1            $nfCoverZ      $coreY   $ncoreZ  $coverY $ncoverZ $coverY $coverZ $coreY   $coreZ
        patch quad  $coverID  $nfCoverY    1              $ncoreY  $coreZ   $coreY  $coreZ   $coverY $coverZ $ncoverY $coverZ
        patch quad  $coverID  $nfCoverY    1              $ncoverY $ncoverZ $coverY $ncoverZ $coreY  $ncoreZ $ncoreY  $ncoreZ

        #            layer straight   $matTag       $numFiber        $areaFiber        $yStart    $zStart    $yEnd    $zEnd
        #            Define the Top layers rebar
        layer straight   $steelID      $numBarsTOPBeam  $barAreaTOPBeam   $coreY     $coreZ     $coreY   $ncoreZ
        #            Define the Bottom layers rebar
        layer straight   $steelID      $numBarsBOTBeam  $barAreaBOTBeam   $ncoreY    $coreZ    $ncoreY   $ncoreZ
        set spacingY [expr ($coreY-$ncoreY)/($numBarsIntBeam-1)]
        set numBarsIntBeam1 [expr $numBarsIntBeam-2]
        layer straight   $steelID      $numBarsIntBeam1  $barAreaIntBeam  [expr $coreY-$spacingY]  $coreZ  [expr $ncoreY+$spacingY]  $coreZ
        layer straight   $steelID      $numBarsIntBeam1  $barAreaIntBeam  [expr $coreY-$spacingY]  $ncoreZ  [expr $ncoreY+$spacingY]  $ncoreZ
    }
    uniaxialMaterial Elastic [expr $id+1001] 541556619
    section Aggregator $id [expr $id+1001] T -section $tag
}
#                  id HSec BSec coverH coverB  coreID coverID steelID numBarsTOPBeam barAreaTOPBeam numBarsBOTBeam barAreaBOTBeam numBarsIntTotBeam barAreaIntBeam
RcBeamSection     201  500  300   35     35       3      1       11        4             380              4             254               4              113;
########################################################################################
########################################################################################
########################################################################################

# =============================================================
######################## Infill Wall ########################
# =============================================================
# Reference:  https://opensees.berkeley.edu/wiki/index.php/Infill_Wall_Model_and_Element_Removal
# Model_IR.tcl, Line 326~438
# source 1Units&Constants.tcl;
# inelastic section for the infill wall
set EminfM [expr 5000*$MPa];    # masonry modulus of elasticity
set sectioninf 1000;
section fiberSec $sectioninf -GJ 1e10 {
    set infmattag 1011;
    set fyfibinf [expr 0.756/0.2248*$kN]; # 1 kN = 0.2248 kip
    set areafibinf [expr 2.214145*645.16]; # 1 in^2 = 645.16 mm^2
    set zfibinf [expr 17.967402*25.4]; # 1 in = 25.4 mm
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1012;
    set fyfibinf [expr 0.611/0.2248*$kN];
    set areafibinf [expr 5.260807*645.16];
    set zfibinf [expr 9.367025*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1013;
    set fyfibinf [expr 0.545/0.2248*$kN];
    set areafibinf [expr 8.324044*645.16];
    set zfibinf [expr 6.63148*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1014;
    set fyfibinf [expr 0.49/0.2248*$kN];
    set areafibinf [expr 12.791111*645.16];
    set zfibinf [expr 4.799369*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1015;
    set fyfibinf [expr 0.396/0.2248*$kN];
    set areafibinf [expr 30.176721*645.16];
    set zfibinf [expr 2.515478*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1016;
    set fyfibinf [expr 0.396/0.2248*$kN];
    set areafibinf [expr 30.176721*645.16];
    set zfibinf [expr -2.515478*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1017;
    set fyfibinf [expr 0.49/0.2248*$kN];
    set areafibinf [expr 12.791111*645.16];
    set zfibinf [expr -4.799369*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1018;
    set fyfibinf [expr 0.545/0.2248*$kN];
    set areafibinf [expr 8.324044*645.16];
    set zfibinf [expr -6.63148*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1019;
    set fyfibinf [expr 0.611/0.2248*$kN];
    set areafibinf [expr 5.260807*645.16];
    set zfibinf [expr -9.367025*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    set infmattag 1020;
    set fyfibinf [expr 0.756/0.2248*$kN];
    set areafibinf [expr 2.214145*645.16];
    set zfibinf [expr -17.967402*25.4];
    uniaxialMaterial Steel01 $infmattag $fyfibinf $EminfM 0.02;
    fiber 0.0 $zfibinf $areafibinf $infmattag;

    # this fiber in y directtion with very snall area is needed to supply a very small in plane stiffness
    set infmattag 1021;
    uniaxialMaterial Steel01 $infmattag 1.e40 $EminfM 0.02;
    layer straight $infmattag 1 0.0001 1.0 0.0 1.0 0.0;
}

# aggregate with rigid torsion stiffness
set sectioninfT 11;
set Torsionmat 90;
uniaxialMaterial Elastic $Torsionmat [expr 1.e12]
section Aggregator $sectioninfT $Torsionmat T -section $sectioninf

# elastic section for pin connection
set sectionpin 12;
set AreainfM [expr 117.5337*645.16];
section Elastic $sectionpin $EminfM $AreainfM 1e-3 1e-3 [expr $EminfM/2.5] 1e-3;

proc Wall2Section {start_point mid_point end_point wall_tag}   {
    set rinfM [expr pow((pow(4267.2,2.0)+pow(7315.2,2.0)),0.5)];
    set infnum [expr $wall_tag + 99000];
    set infnum2 [expr $wall_tag + 88000];
    set fileremoval "Dispwall1-cg.tcl"; 
    # old: element beamWithHinges $eleTag $iNode $jNode $secTagI $Lpi $secTagJ $Lpj $E $A $Iz $Iy $G $J $transfTag <-mass $massDens> <-iter $maxIters $tol>
    # new: element forceBeamColumn $eleTag $iNode $jNode $transfTag "HingeRadau $secTagI $LpI $secTagJ $LpJ $secTagInterior" <-mass $massDens> <-iter $maxIters $tol>
    element forceBeamColumn $infnum $mid_point  $start_point 6 HingeRadau 12 [expr $rinfM*0.1] 11 [expr $rinfM*0.05] 11
    element forceBeamColumn $infnum2 $mid_point  $end_point 6 HingeRadau 12 [expr $rinfM*0.1] 11 [expr $rinfM*0.05] 11
    #         recorders for collapse removal
    recorder Collapse -ele $infnum   -time  -crit INFILLWALL  -file Output/Infill/CollapseSequence.out  -file_infill $fileremoval -global_gravaxis 2 -checknodes $start_point $mid_point $end_point
    recorder Collapse -ele $infnum2  -time  -crit INFILLWALL   -file_infill $fileremoval -global_gravaxis 2 -checknodes $start_point $mid_point $end_point
    recorder Collapse -ele $infnum $infnum2 -node $mid_point
    #          recorders for the displacements of the midspan nodes
    recorder Node -file Output/Infill/Midspan-$mid_point.out -time -node $mid_point -dof 1 2 3 disp;
}


#---------3Materials&Fibers.tcl---------

