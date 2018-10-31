using System;
using System.Linq;
using UnityEngine;

namespace EndlessShapes.Blocks
{
    public class SyncroniseBlock : Block, IBlockWithText
    {
        private string SyncroniseData = string.Empty;

        private string SyncroniseDataDelimiter = "<Delimiter 93529703>";

        private int StartTime;

        private int uniqueId;

        public bool BlockWithText;



        public int UniqueId
        {
            set { if (Time.frameCount == StartTime) uniqueId = value; }
            get { return uniqueId; }
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



        public virtual string SetText(string str, bool sync = true)
        {
            return string.Empty;
        }

        public virtual string GetText()
        {
            return string.Empty;
        }



        public override void SyncroniseUpdate(string NewSyncroniseData)
        {
            base.SyncroniseUpdate(NewSyncroniseData);

            if (NewSyncroniseData != SyncroniseData)
            {
                SyncroniseData = NewSyncroniseData;
                string[] SDA = SyncroniseData.Split(new string[] { SyncroniseDataDelimiter }, StringSplitOptions.None);
                ExtraInfoArrayReadPackage.DataArray = SDA[0].Split(',').Select(S => double.Parse(S)).ToArray();
                SetExtraInfo(new ExtraInfoArrayReadPackage(ExtraInfoArrayReadPackage.DataArray.Length, false));
                SetText(SDA[1]);
                StuffChangedSyncIt();
            }
        }

        public override void StateChanged(IBlockStateChange change)
        {
            base.StateChanged(change);

            if (BlockWithText)
            {
                if (change.InitiatedOrInitiatedInUnrepairedState_OnlyCalledOnce)
                {
                    StartTime = Time.frameCount;
                    UniqueId = MainConstruct.UniqueIdsRestricted.CheckOutANewUniqueId();
                    GetConstructableOrSubConstructable().iBlocksWithText.BlocksWithText.Add(this);
                }
                else if (change.IsPerminentlyRemovedOrConstructDestroyed)
                {
                    GetConstructableOrSubConstructable().iBlocksWithText.BlocksWithText.Remove(this);
                }
            }
        }
    }
}
