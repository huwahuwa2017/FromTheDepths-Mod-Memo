using BrilliantSkies.Core.Constants;
using BrilliantSkies.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

public class ModPlugin : GamePlugin
{
    public string name
    {
        get { return "ManualAlignment"; }
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
            
            bool Result0 = jObject["name"].ToString() != name;
            bool Result1 = jObject["version"].ToString() != ModVersion;
            bool Result2 = jObject["gameversion"].ToString() != FTDGameVersion;

            if (Result0 || Result1 || Result2)
            {
                if (Result0) jObject["name"] = name;
                if (Result1) jObject["version"] = ModVersion;
                if (Result2) jObject["gameversion"] = FTDGameVersion;

                File.WriteAllText(FilePath, jObject.ToString());
            }
        }
    }
}
