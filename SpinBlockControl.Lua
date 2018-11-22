AllSpinBlockStates = {} 

function SetStates(ID, Min, Max)
  local States = {}
  States.ID = ID
  States.MinAngle = Min
  States.MaxAngle = Max

  table.insert(AllSpinBlockStates, States)
end



VectorInvestigation = false

SetStates(702, -90, 90)
SetStates(703, nil, nil)
SetStates(704, -90, 90)

ZAVT = {4, 4, 4}



function ZAVTCalculation(I)
  local String

  for Index, States in ipairs(AllSpinBlockStates) do
    I:SetSpinBlockRotationAngle(States.ID, 0)
    local FV = I:GetSubConstructInfo(States.ID).LocalForwards

    local Type = (FV == Vector3.right)   and 0 or
                 (FV == Vector3.left)    and 1 or
                 (FV == Vector3.up)      and 2 or
                 (FV == Vector3.down)    and 3 or
                 (FV == Vector3.forward) and 4 or
                 (FV == Vector3.back)    and 5 or "Calculating......"

    String = (Index == 1) and Type or String..", "..Type
  end

  I:Log("ZAVT = {"..String.."}")
end



function ZeroAngleRotationCalculation(I)
  for Index = 1, #AllSpinBlockStates do
    local Type = ZAVT[Index]

    local FV = (Type == 0) and Vector3.right or
               (Type == 1) and Vector3.left or
               (Type == 2) and Vector3.up or
               (Type == 3) and Vector3.down or
               (Type == 4) and Vector3.forward or
               (Type == 5) and Vector3.back or Vector3.zero

    local SBUV = I:GetSubConstructInfo(AllSpinBlockStates[Index].ID).LocalRotation * Vector3.up
    AllSpinBlockStates[Index].ZAR = I:Maths_CreateQuaternion(FV, SBUV)
  end
end

function SpinBlockControl(I, Index, TargetAngle)
  local Stats = AllSpinBlockStates[Index]
  local Min = Stats.MinAngle
  local Max = Stats.MaxAngle

  if Min ~= nil and Max ~= nil then
    TargetAngle = Mathf.Min(Mathf.Max(TargetAngle, Min), Max)
  end

  local SBFV = Quaternion.Inverse(Stats.ZAR) * I:GetSubConstructInfo(Stats.ID).LocalForwards
  local AngularDifference = Mathf.DeltaAngle(0, TargetAngle - math.deg(math.atan2(SBFV.x, SBFV.z)))

  if Mathf.Abs(AngularDifference) < 1 then
    I:SetSpinBlockRotationAngle(Stats.ID, TargetAngle)
  else
    I:SetSpinBlockContinuousSpeed(Stats.ID, AngularDifference * Mathf.Deg2Rad * 40)
  end
end



function Update(I)
  I:ClearLogs()

  if VectorInvestigation then
    ZAVTCalculation(I)
  elseif #AllSpinBlockStates > 0 then
    if AllSpinBlockStates[1].ZAR == nil then
      ZeroAngleRotationCalculation(I)
    end

    SpinBlockControl(I, 1, 60)
    SpinBlockControl(I, 2, 120)
    SpinBlockControl(I, 3, 120)
  end
end
