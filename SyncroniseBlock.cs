using System;
using System.Linq;
using UnityEngine;

public class SyncroniseBlock : Block, IBlockWithText
{
    private string SyncroniseData = string.Empty;

    private string SyncroniseDataDelimiter = "#=Delimiter9128347=#";

    private int StartTime = 0;

    public int UniqueId { get; set; }

    public bool BlockWithText;



    public void SetUniqueId(int NewUniqueId)
    {
        if (Time.frameCount == StartTime) UniqueId = NewUniqueId;
    }

    public void SyncroniseDataUpLoad()
    {
        GetExtraInfo(new ExtraInfoArrayWritePackage());
        string NewSyncroniseData = String.Join(",", ExtraInfoArrayWritePackage.DataArray.Select(D => D.ToString()).ToArray()) + SyncroniseDataDelimiter + GetText();

        if (NewSyncroniseData != SyncroniseData)
        {
            SyncroniseData = NewSyncroniseData;
            GetConstructableOrSubConstructable().iMultiplayerSyncroniser.RPCRequest_SyncroniseBlock(this, SyncroniseData);
        }
    }



    public virtual string GetText()
    {
        return string.Empty;
    }

    public virtual string SetText(string str, bool sync = true)
    {
        return string.Empty;
    }



    public override void SyncroniseUpdate(string NewSyncroniseData)
    {
        if (NewSyncroniseData != SyncroniseData)
        {
            SyncroniseData = NewSyncroniseData;
            string[] SDA = SyncroniseData.Split(new string[] { SyncroniseDataDelimiter }, StringSplitOptions.None);
            ExtraInfoArrayReadPackage.DataArray = SDA[0].Split(',').Select(S => float.Parse(S)).ToArray();
            SetExtraInfo(new ExtraInfoArrayReadPackage(ExtraInfoArrayReadPackage.DataArray.Length, false));
            SetText(SDA[1]);
        }
    }

    public override void StateChanged(IBlockStateChange change)
    {
        base.StateChanged(change);

        if (BlockWithText)
        {
            if (UniqueId == 0)
            {
                UniqueId = MainConstruct.iUniqueIdentifierCreator.CheckOutANewUniqueId();
                StartTime = Time.frameCount;
            }
            if (change.InitiatedOrInitiatedInUnrepairedState_OnlyCalledOnce)
            {
                GetConstructableOrSubConstructable().iBlocksWithText.BlocksWithText.Add(this);
            }
            else if (change.IsPerminentlyRemovedOrConstructDestroyed)
            {
                GetConstructableOrSubConstructable().iBlocksWithText.BlocksWithText.Remove(this);
            }
        }
    }
}
