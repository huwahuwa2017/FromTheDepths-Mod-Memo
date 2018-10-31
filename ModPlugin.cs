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
        ModProblem.AddModProblem(name, FilePath, "Mod Version " + version, false);
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
            bool Update = false;

            if (jObject["version"].ToString() != ModVersion)
            {
                jObject["version"] = ModVersion;
                Update = true;
            }

            if (jObject["gameversion"].ToString() != FTDGameVersion)
            {
                jObject["gameversion"] = FTDGameVersion;
                Update = true;
            }

            if (Update) File.WriteAllText(FilePath, jObject.ToString());
        }
    }
}
