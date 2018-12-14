using BrilliantSkies.Core.Constants;
using BrilliantSkies.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

public class ModPlugin : GamePlugin
{
    public string name
    {
        get { return "TestMod"; }
    }

    public Version version
    {
        get { return new Version(0, 0, 0); }
    }



    public void OnLoad()
    {
        string FilePath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name + "/plugin.json");
        UpdateJSON(FilePath);
        ModProblem.AddModProblem(name + "   v" + version, FilePath, string.Empty, false);
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
}
