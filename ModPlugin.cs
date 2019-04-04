using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.SteamworksIntegration;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Containers;
using BrilliantSkies.Modding.Types;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.IO;
using static BrilliantSkies.Core.Timing.GameEvents;

public class ModPlugin : GamePlugin
{
    public string name
    {
        get { return "TestMod"; }
    }

    public System.Version version
    {
        get { return new System.Version(0, 0, 0); }
    }

    public ulong PublishedFileId
    {
        get { return 0; }
    }

    private bool SWS;

    private SteamAPICall_t UGCDetails;

    private string FilePath;



    public void OnLoad()
    {
        SWS = SteamInterface.UseSteam && PublishedFileId != 0;
        if (SWS) UGCDetails = SteamUGC.RequestUGCDetails(new PublishedFileId_t(PublishedFileId), 0);

        FilePath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name);
        UpdateJSON(Path.Combine(FilePath, "plugin.json"));
        ModProblemOverwriting(name + "  v" + version, FilePath, string.Empty, false);
        StartEvent += GameStart;
    }

    public void OnStart()
    {
        ItemDefinition Item0 = Configured.i.Get<ModificationComponentContainerItem>().Find(new Guid("cdca5dec-eeee-4849-9b2e-73b23e216465"), out bool F0);
        if (F0) Item0.Code.ClassName = "DGA_PrecisionSpinBlock";

        ItemDefinition Item1 = Configured.i.Get<ModificationComponentContainerItem>().Find(new Guid("564a75cd-8d7c-469b-a4b3-053d772b7d9d"), out bool F1);
        if (F1) Item1.Code.ClassName = "DGA_HelicopterSpinner";
    }

    public void OnSave()
    {

    }

    private void UpdateJSON(string FilePath)
    {
        if (File.Exists(FilePath))
        {
            JObject jObject = JObject.Parse(File.ReadAllText(FilePath));
            string ModVersion = version.ToString();
            string FTDGameVersion = Get.Game.VersionString;

            bool F0 = jObject["name"].ToString() != name;
            bool F1 = jObject["version"].ToString() != ModVersion;
            bool F2 = jObject["gameversion"].ToString() != FTDGameVersion;

            if (F0 || F1 || F2)
            {
                if (F0) jObject["name"] = name;
                if (F1) jObject["version"] = ModVersion;
                if (F2) jObject["gameversion"] = FTDGameVersion;

                File.WriteAllText(FilePath, jObject.ToString());
            }
        }
    }

    public void ModProblemOverwriting(string InitModName, string InitModPath, string InitDescription, bool InitIsError)
    {
        ModProblem.AllModProblems.Remove(InitModPath);
        ModProblem.AddModProblem(InitModName, InitModPath, InitDescription, InitIsError);
    }

    public void GameStart()
    {
        if (SWS) new CallResult<SteamUGCRequestUGCDetailsResult_t>(Callback).Set(UGCDetails);

        OnStart();

        StartEvent -= GameStart;
    }

    public void Callback(SteamUGCRequestUGCDetailsResult_t param, bool bIOFailure)
    {
        string Description = param.m_details.m_rgchDescription;

        if (!string.IsNullOrEmpty(Description))
        {
            string InputLine = string.Empty;
            StringReader Reader = new StringReader(Description);
            System.Version LatestVersion = version;

            while ((InputLine = Reader.ReadLine()) != null)
            {
                if (InputLine.StartsWith("Mod Version "))
                {
                    System.Version.TryParse(InputLine.Remove(0, 11), out LatestVersion);
                    break;
                }
            }

            if (version.CompareTo(LatestVersion) == -1) ModProblemOverwriting(name + "  v" + version, FilePath, " Update! v" + LatestVersion, false);
        }
    }
}
