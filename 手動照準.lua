-----------------------------------------------
--手動照準Lua Ver3.8 2018/10/29 製作者：huwahuwa--
-----------------------------------------------


function CCCode(Key)
  local Number = (Key == "T") and 1  or (Key == "G") and 2  or (Key == "Y") and 3  or (Key == "H") and 4  or
                 (Key == "U") and 5  or (Key == "J") and 6  or (Key == "I") and 7  or (Key == "K") and 8  or
                 (Key == "O") and 9  or (Key == "L") and 10 or (Key == "↑") and 11 or (Key == "→") and 12 or
                 (Key == "↓") and 13 or (Key == "←") and 14 or 0

  return Number
end

Vertical = {}
FHorizontal = {}
BHorizontal = {}

VOutput = 0
FHOutput = 0
BHOutput = 0


-----------------------------------------------------------------------------------------------------------------------
--設定ここから


--前部砲塔のWeaponSlot
ForwardWeaponSlot = 1

--後部砲塔のWeaponSlot
BackwardWeaponSlot = 2

--最高回転速度に達するまでの時間 (1秒 = 40)
Vertical.ReactionTime    = 20
FHorizontal.ReactionTime = 20
BHorizontal.ReactionTime = 20

--最高回転速度 (deg/s)
Vertical.MaxSpeed    = 10
FHorizontal.MaxSpeed = 20
BHorizontal.MaxSpeed = 20

--仰角制限
MaxVertical = 80
MinVertical = -5

--キーコンフィグ
FireKey   = CCCode("G")
ResetKey  = CCCode("O")
UpKey   = CCCode("U")
DownKey = CCCode("J")
Clockwise     = CCCode("Y")
Anticlockwise = CCCode("I")
LeftKey  = CCCode("H")
RightKey = CCCode("K")


--設定ここまで
-----------------------------------------------------------------------------------------------------------------------


function CheckCCDP(I, PropertyIndex, ComponentIndex)
  local Input = {}

  for Count, PI in pairs(PropertyIndex) do
    Input[Count] = (I:Component_GetFloatLogic_1(12, ComponentIndex, PI) ~= 0) and true or false
    if Input[Count] then I:Component_SetFloatLogic_1(12, ComponentIndex, PI, 0) end
  end

  return Input
end

function CheckCC(I)
  InputCC = {}

  for Count1 = 0, I:Component_GetCount(12) - 1 do
    local Name = I:Component_GetBlockInfo(12, Count1).CustomName

    if Name == "CCDSLF_0" or Name == "CCDSLF_1" or Name == "CCDSLF_2" or Name == "CCDSLF_3" then
      local Input
      local num

      if Name == "CCDSLF_0" then
        Input = CheckCCDP(I, {1, 2, 3, 4}, Count1)
        num = 0
      elseif Name == "CCDSLF_1" then
        Input = CheckCCDP(I, {1, 2, 3, 4}, Count1)
        num = 4
      elseif Name == "CCDSLF_2" then
        Input = CheckCCDP(I, {1, 2, 3, 4}, Count1)
        num = 8
      elseif Name == "CCDSLF_3" then
        Input = CheckCCDP(I, {1, 4}, Count1)
        num = 12
      end

      for Count, I in pairs(Input) do
        InputCC[Count + num] = I
      end
    end
  end
end



function AxisOutput(Axis, PCode, NCode)
  local InputP = false
  local InputN = false
  local RT = Axis.ReactionTime
  local V = Axis.Variation or 0

  for Count, P in pairs(PCode) do
    if (InputCC[P]) then
      InputP = true
      break
    end
  end

  for Count, N in pairs(NCode) do
    if (InputCC[N]) then
      InputN = true
      break
    end
  end

  if InputCC[ResetKey] then
    V = 0
  else
    if InputN and not InputP or InputP and not InputN then
      V = (InputP and V < RT) and V + 1 or (InputN and V > -RT) and V - 1 or V
      V = (InputP and V < 0) and V + 1 or (InputN and V > 0) and V - 1 or V
    else
      V = (V < 0) and V + 1 or (V > 0) and V - 1 or 0
    end
  end

  Axis.Variation = V
  return Axis.MaxSpeed * V / RT / 40
end

function AimPosition(I)
  if InputCC[ResetKey] then
    VOutput = 0
    FHOutput = 0
    BHOutput = 0
  end

  VOutput = Mathf.DeltaAngle(0, VOutput + AxisOutput(Vertical, {UpKey}, {DownKey}))
  VOutput = Mathf.Min(Mathf.Max(VOutput, MinVertical), MaxVertical)
  FHOutput = Mathf.DeltaAngle(0, FHOutput + AxisOutput(FHorizontal, {Anticlockwise, RightKey}, {Clockwise, LeftKey}))
  BHOutput = Mathf.DeltaAngle(0, BHOutput + AxisOutput(BHorizontal, {Anticlockwise, LeftKey}, {Clockwise, RightKey}))

  local VehicleRotation = Quaternion.LookRotation(I:GetConstructForwardVector(), I:GetConstructUpVector())
  local FAim = VehicleRotation * Quaternion.Euler(-VOutput, FHOutput, 0) * Vector3.forward
  local BAim = VehicleRotation * Quaternion.Euler(VOutput, BHOutput, 0) * Vector3.back

  for Count = 0, I:GetWeaponCount() - 1 do
    if I:GetWeaponInfo(Count).WeaponSlot == ForwardWeaponSlot then
      I:AimWeaponInDirection(Count, FAim.x, FAim.y, FAim.z, 0)
      if InputCC[2] then I:FireWeapon(Count, 0) end
    elseif I:GetWeaponInfo(Count).WeaponSlot == BackwardWeaponSlot then
      I:AimWeaponInDirection(Count, BAim.x, BAim.y, BAim.z, 0)
      if InputCC[2] then I:FireWeapon(Count, 0) end
    end
  end

  I:Log("VOutput : "..VOutput)
end

function Update(I)
  I:ClearLogs()
  CheckCC(I)
  AimPosition(I)
end
