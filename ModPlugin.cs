using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.SteamworksIntegration;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Modding;
using Newtonsoft.Json.Linq;
using Steamworks;
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



    public string FilePath;

    public DelayedFunctionCaller<ITimeStep> DFC;

    public void OnLoad()
    {
        FilePath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name + "/plugin.json");
        UpdateJSON(FilePath);
        ModProblemOverwriting(name + "  v" + version, FilePath, string.Empty, false);
        if (SteamInterface.UseSteam && PublishedFileId != 0) UpdateEvent += Update;
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

    public void Update(ITimeStep TS)
    {
        if (DFC == null)
        {
            DFC = new DelayedFunctionCaller<ITimeStep>();
            DFC.Add(new DelayedFunction<ITimeStep>
            {
                FunctionToCall = SlowUpdate,
                Period = 0.1f
            });
        }

        DFC.PassageOfTIme(TS);
    }

    public void SlowUpdate(ITimeStep TS)
    {
        CallResult<SteamUGCRequestUGCDetailsResult_t> UGCDetailsRequest = new CallResult<SteamUGCRequestUGCDetailsResult_t>(Callback);
        UGCDetailsRequest.Set(SteamUGC.RequestUGCDetails(new PublishedFileId_t(PublishedFileId), 0));
    }

    public void Callback(SteamUGCRequestUGCDetailsResult_t pCallback, bool bIOFailure)
    {
        string Description = pCallback.m_details.m_rgchDescription;

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

            if (version.CompareTo(LatestVersion) == -1) ModProblemOverwriting(name + "  v" + version, FilePath, "Old Version", false);
        }

        UpdateEvent -= Update;
    }
}
