using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.SteamworksIntegration;
using BrilliantSkies.Modding;
using Newtonsoft.Json.Linq;
using Steamworks;
using System.IO;
using static BrilliantSkies.Core.Timing.GameEvents;

public class ModPlugin : GamePlugin
{
    private bool SWS;

    private string FilePath;

    private SteamAPICall_t UGCDetails;
    
    public string name
    {
        //Mod Name
        get { return "TestMod"; }
    }

    public System.Version version
    {
        //Mod Version
        get { return new System.Version(0, 0, 0); }
    }

    public ulong PublishedFileId
    {
        //Steam Workshop ID
        get { return 0; }
    }



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

            if (version.CompareTo(LatestVersion) == -1)
            {
                ModProblemOverwriting(name + "  v" + version, FilePath, " Update! v" + LatestVersion, false);
            }
        }
    }
}
